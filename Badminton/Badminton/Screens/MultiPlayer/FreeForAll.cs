using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerPhysics.Dynamics;

using Badminton.Stick_Figures;
using Badminton.Screens.Menus;
using Badminton.Attacks;

namespace Badminton.Screens.MultiPlayer
{
    class FreeForAll : GameScreen
    {
        World world;
        List<Wall> walls;

        LocalPlayer[] player;
        Vector2[] spawnPoints;
        TrapAmmo[] ammo;
        Texture2D background;

        int[] lives;

        public FreeForAll()
        {

            lives = new int[4];
            player = new LocalPlayer[4];
            world = new World(new Vector2(0, 9.8f)); // That'd be cool to have gravity as a map property, so you could play 0G levels

            walls = new List<Wall>();
            walls.Add(new Wall(world, 960 * MainGame.PIXEL_TO_METER, 1040 * MainGame.PIXEL_TO_METER, 1200 * MainGame.PIXEL_TO_METER, 32 * MainGame.PIXEL_TO_METER, 0.0f));

            object[] map = Map.LoadCastle(world);
            background = (Texture2D)map[0];
            spawnPoints = (Vector2[])map[2];
            Vector3[] ammoPoints = (Vector3[])map[3];
            ammo = new TrapAmmo[ammoPoints.Length];
            for (int i = 0; i < ammoPoints.Length; i++)
                ammo[i] = new TrapAmmo(world, new Vector2(ammoPoints[i].X, ammoPoints[i].Y) * MainGame.PIXEL_TO_METER, (int)ammoPoints[i].Z);

            player[0] = new LocalPlayer(world, spawnPoints[0] * MainGame.PIXEL_TO_METER, Category.Cat1, 1.5f, Color.Red, PlayerIndex.One);
            player[1] = new LocalPlayer(world, spawnPoints[1] * MainGame.PIXEL_TO_METER, Category.Cat2, 1.5f, Color.Blue, PlayerIndex.Two);
            player[2] = new LocalPlayer(world, spawnPoints[2] * MainGame.PIXEL_TO_METER, Category.Cat3, 1.5f, Color.Green, PlayerIndex.Three);
            player[3] = new LocalPlayer(world, spawnPoints[3] * MainGame.PIXEL_TO_METER, Category.Cat4, 1.5f, Color.Cyan, PlayerIndex.Four);

            // init lives
            for (int i = 0; i < 4; i++)
            {
                lives[i] = 2;
            }
        }

        public GameScreen Update(GameTime gameTime)
        {
            for (int i = 0; i < player.Length; i++)
            {
                if (player[i] != null)
                {
                    player[i].Update();
                }
            }

            // end condition
            for (int i = 0; i < player.Length; i++)
            {
                if (player[i] != null)
                {
                    if (player[i].IsDead || player[i].Position.Y * MainGame.METER_TO_PIXEL > 1080)
                    {

                        switch (i)
                        {
                            case 0:
                                player[0].Destroy();
                                if (lives[0] > 0)
                                {
                                    lives[0]--;
                                    if (lives[0] > 0)
                                    {
                                        player[0] = new LocalPlayer(world, spawnPoints[0] * MainGame.PIXEL_TO_METER, Category.Cat1, 1.5f, Color.Red, PlayerIndex.One);
                                    }
                                    else
                                    {
                                        player[0] = null;
                                    }
                                }
                                break;

                            case 1:
                                player[1].Destroy();
                                if (lives[1] > 0)
                                {
                                    lives[1]--;
                                    if (lives[1] > 0)
                                    {
                                        player[1] = new LocalPlayer(world, spawnPoints[1] * MainGame.PIXEL_TO_METER, Category.Cat2, 1.5f, Color.Blue, PlayerIndex.Two);
                                    }
                                    else
                                    {
                                        player[1] = null;
                                    }
                                }
                                break;

                            case 2:
                                player[2].Destroy();
                                if (lives[2] > 0)
                                {
                                    lives[2]--;
                                    if (lives[2] > 0)
                                    {
                                        player[2] = new LocalPlayer(world, spawnPoints[2] * MainGame.PIXEL_TO_METER, Category.Cat3, 1.5f, Color.Green, PlayerIndex.Three);
                                    }
                                    else
                                    {
                                        player[2] = null;
                                    }
                                }
                                break;

                            case 3:
                                player[3].Destroy();
                                if (lives[3] > 0)
                                {
                                    lives[3]--;
                                    if (lives[3] > 0)
                                    {
                                        player[3] = new LocalPlayer(world, spawnPoints[3] * MainGame.PIXEL_TO_METER, Category.Cat4, 1.5f, Color.Cyan, PlayerIndex.Four);
                                    }
                                    else
                                    {
                                        player[3] = null;
                                    }
                                }
                                break;
                        }
                    }
                }
            }

            // update ammo
            foreach (TrapAmmo t in ammo)
                t.Update();

            // These two lines stay here, even after we delete testing stuff
            world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
            return this;

        }

        public void Draw(SpriteBatch sb)
        {
            // draw background
            sb.Draw(MainGame.tex_bg_castle, new Rectangle(0, 0, 1920, 1080), Color.White);

            // draw ammo
            foreach (TrapAmmo t in ammo)
                t.Draw(sb);

            // draw players
            for (int i = 0; i < player.Length; i++)
            {
                if (player[i] != null)
                {
                    player[i].Draw(sb);
                }
            }

            // draw walls
            foreach (Wall w in walls)
                w.Draw(sb);

            // draw player status
            for (int i = 0; i < 4; i++)
	            sb.DrawString(MainGame.fnt_basicFont, "Player" + (i + 1).ToString() + ": " + lives[i], new Vector2(20, 20 + i * 20), Color.Gold);
        }

        public GameScreen GoBack()
        {
            return null;
            //        return new MainMenu();
        }
    }
}
