using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;

using Badminton.Stick_Figures;
using Badminton.Attacks;

namespace Badminton.Screens
{
    class s_FreeForAll : GameScreen
    {
        World world;
        List<Wall> walls;
        Vector2[] spawnPoints;
        TrapAmmo[] ammo;
        Texture2D background;

        String playerName;
        String MAP;
        Vector2 g;
        int playerNum;
        int round;
        int roundCount = 1;
        Boolean endRound = false;
        Boolean nextRound = true;
        long nextRoundTimer;
        Color playerColor;
        Color[] Colors;
        List<LocalPlayer> players;


        public s_FreeForAll(String name, Color color, int round, String MAP, int playerNum, Vector2 g)
        {
            this.playerName = name;
            this.playerColor = color;
            this.MAP = MAP;
            this.g = g;
            this.playerNum = playerNum;
            this.round = round;
            nextRoundTimer = Environment.TickCount;


            Random ran = new Random();

            world = new World(g);
            players = new List<LocalPlayer>();

            Colors = new Color[playerNum + 2];

            Colors[0] = playerColor;

            for (int i = 1; i < playerNum; i++)
            {
                Colors[i] = new Color(ran.Next(255), ran.Next(255), ran.Next(255));
            }

            object[] map = Map.LoadCastle(world);
            background = (Texture2D)map[0];
            walls = (List<Wall>)map[1];
            spawnPoints = (Vector2[])map[2];
            Vector3[] ammoPoints = (Vector3[])map[3];
            ammo = new TrapAmmo[ammoPoints.Length];
            for (int i = 0; i < ammoPoints.Length; i++)
                ammo[i] = new TrapAmmo(world, new Vector2(ammoPoints[i].X, ammoPoints[i].Y) * MainGame.PIXEL_TO_METER, (int)ammoPoints[i].Z);


            initPlayers();
        }


        private void initPlayers()
        {
            for (int i = 0; i < playerNum; i++)
            {


                //int spawnP = ran.Next(players.Length);
                if (i == 0)
                {
                    players.Add(new LocalPlayer(world, spawnPoints[0] * MainGame.PIXEL_TO_METER, Category.Cat1, 1.5f, Colors[i], PlayerIndex.One));
                }
                else if (i == 1)
                {
                    players.Add(new LocalPlayer(world, spawnPoints[1] * MainGame.PIXEL_TO_METER, Category.Cat2, 1.5f, Colors[i], PlayerIndex.Two));
                }
                else if (i == 2)
                {
                    players.Add(new LocalPlayer(world, spawnPoints[2] * MainGame.PIXEL_TO_METER, Category.Cat3, 1.5f, Colors[i], PlayerIndex.Three));
                }
                else if (i == 3)
                {
                    players.Add(new LocalPlayer(world, spawnPoints[3] * MainGame.PIXEL_TO_METER, Category.Cat3, 1.5f, Colors[i], PlayerIndex.Four
));
                }
            }



        }
        public virtual GameScreen Update(GameTime gameTime)
        {

            foreach (LocalPlayer p in players)
            {
                p.Update();

            }

            for (int i = 0; i < players.Count; i++)
            {
  
                if (players[i] == null)
                {
                    Console.Out.WriteLine("num" + i + " null");
                }

                if (players[i].IsDead || players[i].Position.Y * MainGame.METER_TO_PIXEL > 1080)
                {
                    players[i].Destroy();

                    if (i == 0)
                    {
                        players[i] = new LocalPlayer(world, spawnPoints[i] * MainGame.PIXEL_TO_METER, Category.Cat1, 1.5f, Colors[i], PlayerIndex.One);
                    }
                    else if (i == 1)
                    {
                        players[i] = new LocalPlayer(world, spawnPoints[i] * MainGame.PIXEL_TO_METER, Category.Cat2, 1.5f, Colors[i], PlayerIndex.Two);
                    }
                    else if (i == 2)
                    {
                        players[i] = new LocalPlayer(world, spawnPoints[i] * MainGame.PIXEL_TO_METER, Category.Cat3, 1.5f, Colors[i], PlayerIndex.Three);
                    }
                    else if (i == 3)
                    {
                        players[i] = new LocalPlayer(world, spawnPoints[i] * MainGame.PIXEL_TO_METER, Category.Cat4, 1.5f, Colors[i], PlayerIndex.Four);
                    }

                    roundCount++;
                    if (roundCount > round)
                    {
                        endRound = true;
                    }
                    nextRound = true;
                    nextRoundTimer = Environment.TickCount;
                }
            }
            foreach (TrapAmmo t in ammo)
                t.Update();

            // These two lines stay here, even after we delete testing stuff
            world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
            return this;
        }

        public virtual GameScreen GoBack()
        {
            return null; //new MainMenu(); // Change this to show confirmation dialog later
        }

        public virtual void Draw(SpriteBatch sb)
        {
            sb.Draw(background, new Rectangle(0, 0, 1920, 1080), Color.White);

            foreach (LocalPlayer p in players)
            {
                p.Draw(sb);


                if (nextRound)
                {
                    if (!endRound)
                    {
                        long now = Environment.TickCount;
                        double diff = now - nextRoundTimer;

                        if (diff <= 2000)
                        {
                            string s = "-    R O U N D  " + roundCount + "    -";
                            sb.DrawString(MainGame.fnt_basicFont, s, new Vector2(800, 500), Color.White);
                        }
                        else
                        {
                            nextRound = false;

                        }
                    }
                    else
                    {
                        sb.DrawString(MainGame.fnt_basicFont, "End", new Vector2(800, 500), Color.Red);
                    }
                }

                foreach (TrapAmmo t in ammo)
                    t.Draw(sb);

                //			foreach (Wall w in walls)
                //				w.Draw(sb);
            }
        }
    }
}
