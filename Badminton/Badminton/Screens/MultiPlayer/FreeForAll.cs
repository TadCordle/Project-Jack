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

		StickFigure[] player;
        Vector2[] spawnPoints;
        TrapAmmo[] ammo;
        Texture2D background;

        int[] lives;

		// Params:
		// Array of player colors (length tells how many players there are)
		// gravity
		// time limit
		// lives
		// sudden death mode?
		// allow traps?
		// allow long range?
		// fill empty with bots?
        public FreeForAll()
        {
            world = new World(new Vector2(0, 9.8f)); // That'd be cool to have gravity as a map property, so you could play 0G levels

            object[] map = Map.LoadCastle(world);
			background = (Texture2D)map[0];
			walls = (List<Wall>)map[1];
            spawnPoints = (Vector2[])map[2];
            Vector3[] ammoPoints = (Vector3[])map[3];
            ammo = new TrapAmmo[ammoPoints.Length];
            for (int i = 0; i < ammoPoints.Length; i++)
                ammo[i] = new TrapAmmo(world, new Vector2(ammoPoints[i].X, ammoPoints[i].Y) * MainGame.PIXEL_TO_METER, (int)ammoPoints[i].Z);

			lives = new int[4];
			player = new LocalPlayer[4];
			
			player[0] = new LocalPlayer(world, spawnPoints[0] * MainGame.PIXEL_TO_METER, Category.Cat1, 1.5f, Color.Red, PlayerIndex.One);
			player[1] = new LocalPlayer(world, spawnPoints[1] * MainGame.PIXEL_TO_METER, Category.Cat2, 1.5f, Color.Blue, PlayerIndex.Two);
			player[2] = new LocalPlayer(world, spawnPoints[2] * MainGame.PIXEL_TO_METER, Category.Cat3, 1.5f, Color.Green, PlayerIndex.Three);
			player[3] = new LocalPlayer(world, spawnPoints[3] * MainGame.PIXEL_TO_METER, Category.Cat4, 1.5f, Color.Cyan, PlayerIndex.Four);

            // init lives
            for (int i = 0; i < 4; i++)
                lives[i] = 2;
        }

        public GameScreen Update(GameTime gameTime)
        {
			for (int i = 0; i < player.Length; i++)
			{
				if (player[i] != null && lives[i] > 0)
				{
					player[i].Update();
					if (player[i].IsDead || player[i].Position.Y * MainGame.METER_TO_PIXEL > 1080)
					{
						player[i].Destroy();
						// TODO: Add respawn timer
						lives[i]--;
						if (lives[i] > 0)
							player[i] = player[i].Respawn();
						else
							player[i] = null;
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
				if (player[i] != null)
					player[i].Draw(sb);

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
//			return new MainMenu();
        }
    }
}
