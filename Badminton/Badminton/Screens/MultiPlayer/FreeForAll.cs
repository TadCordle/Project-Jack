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
		int[] respawnTime;
		const int MAX_RESPAWN_TIME = 300;

		private static Category[] Categories = new Category[] { Category.Cat1, Category.Cat2, Category.Cat3, Category.Cat4 };
		private static PlayerIndex[] Players = new PlayerIndex[] { PlayerIndex.One, PlayerIndex.Two, PlayerIndex.Three, PlayerIndex.Four };

		// TODO: Time limit
		public FreeForAll(Color[] colors, string mapString, float gravity, int minutes, int lives, float limbStrength, bool suddenDeath, bool traps, bool longRange, bool bots)
		{
			world = new World(new Vector2(0, gravity));

			object[] map = Map.LoadMap(world, mapString);
			background = (Texture2D)map[0];
			walls = (List<Wall>)map[1];
			spawnPoints = (Vector2[])map[2];
			Vector3[] ammoPoints = (Vector3[])map[3];
			ammo = new TrapAmmo[ammoPoints.Length];
			if (traps)
				for (int i = 0; i < ammoPoints.Length; i++)
					ammo[i] = new TrapAmmo(world, new Vector2(ammoPoints[i].X, ammoPoints[i].Y) * MainGame.PIXEL_TO_METER, (int)ammoPoints[i].Z);

			StickFigure.AllowTraps = traps;
			StickFigure.AllowLongRange = longRange;

			this.lives = new int[bots ? 4 : colors.Length];
			this.respawnTime = new int[bots ? 4 : colors.Length];
			player = new StickFigure[bots ? 4 : colors.Length];

			for (int i = 0; i < colors.Length; i++)
				player[i] = new LocalPlayer(world, spawnPoints[i] * MainGame.PIXEL_TO_METER, Categories[i], 1.5f, limbStrength, suddenDeath, colors[i], Players[i]);

			if (bots && colors.Length < 4)
			{
				for (int i = colors.Length; i < 4; i++)
					player[i] = new BotPlayer(world, spawnPoints[i] * MainGame.PIXEL_TO_METER, Categories[i], 1.5f, limbStrength, suddenDeath, new Color(i * 60, i * 60, i * 60), Players[i], player[0]);
			}

			for (int i = 0; i < player.Length; i++)
			{
				this.lives[i] = lives;
				this.respawnTime[i] = -1;
			}
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
						if (respawnTime[i] < 0)
						{
							lives[i]--;
							respawnTime[i] = MAX_RESPAWN_TIME;
						}

						if (respawnTime[i] == 0)
						{
							player[i].Destroy();
							if (lives[i] > 0)
								player[i] = player[i].Respawn();
							else
								player[i] = null;
							respawnTime[i]--;
						}
						else
							respawnTime[i]--;
					}
				}
			}

            // update ammo
            foreach (TrapAmmo t in ammo)
				if (t != null)
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
				if (t != null)
					t.Draw(sb);

            // draw players
			for (int i = 0; i < player.Length; i++)
				if (player[i] != null)
					player[i].Draw(sb);

            // draw walls
//			foreach (Wall w in walls)
//				w.Draw(sb);

            // draw player status
            for (int i = 0; i < player.Length; i++)
	            sb.DrawString(MainGame.fnt_basicFont, "Player" + (i + 1).ToString() + ": " + lives[i], new Vector2(20, 20 + i * 20), Color.Gold);
        }

        public GameScreen GoBack()
        {
            return null;
//			return new MainMenu();
        }
    }
}
