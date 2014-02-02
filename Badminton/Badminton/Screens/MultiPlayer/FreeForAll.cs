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

		PlayerValues[] info;

		bool timed;
		int millisLeft;

		bool gameOver;

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

			player = new StickFigure[bots ? 4 : colors.Length];
			this.info = new PlayerValues[bots ? 4 : colors.Length];

			for (int i = 0; i < colors.Length; i++)
				player[i] = new LocalPlayer(world, spawnPoints[i] * MainGame.PIXEL_TO_METER, Categories[i], 1.5f, limbStrength, suddenDeath, colors[i], Players[i]);

			if (bots && colors.Length < 4)
			{
				for (int i = colors.Length; i < 4; i++)
					player[i] = new BotPlayer(world, spawnPoints[i] * MainGame.PIXEL_TO_METER, Categories[i], 1.5f, limbStrength, suddenDeath, new Color(i * 60, i * 60, i * 60), Players[i], player[0]);
			}

			for (int i = 0; i < info.Length; i++)
				info[i] = new PlayerValues(lives);

			timed = minutes > 0;
			millisLeft = minutes * 60000;
			gameOver = false;
		}

        public GameScreen Update(GameTime gameTime)
        {
			for (int i = 0; i < player.Length; i++)
			{
				if (player[i] != null && info[i].HasLives())
				{
					player[i].Update();
					if (player[i].IsDead || player[i].Position.Y * MainGame.METER_TO_PIXEL > 1080)
					{
						if (info[i].RespawnTimer < 0)
							info[i].Kill();

						if (info[i].ShouldRespawn())
						{
							player[i].Destroy();
							if (info[i].HasLives())
								player[i] = player[i].Respawn();
							else
								player[i] = null;
							info[i].RespawnTimer--;
						}
						else
							info[i].RespawnTimer--;
					}
				}
			}

            // Update ammo
            foreach (TrapAmmo t in ammo)
				if (t != null)
					t.Update();

			// Endgame
			gameOver = true;
			for (int i = 0, remaining = 0; i < info.Length; i++)
			{
				if (info[i].Lives > 0)
				{
					remaining++;
					if (remaining >= 2)
					{
						gameOver = false;
						break;
					}
				}
			}

			if (timed && !gameOver)
			{
				millisLeft -= gameTime.ElapsedGameTime.Milliseconds;
				if (millisLeft <= 0)
					gameOver = true;
			}

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

			// Todo: reposition, make font bigger
			MainGame.DrawOutlineText(sb, MainGame.fnt_basicFont, millisLeft / 60000 + ":" + (millisLeft % 60000 / 1000 < 10 ? "0" : "") + (millisLeft % 60000 / 1000 < 0 ? 0 : millisLeft % 60000 / 1000), Vector2.One, Color.White);
			for (int i = 0; i < info.Length; i++)
				info[i].Draw(sb, Vector2.UnitX * 450 + Vector2.UnitX * i * 300 + Vector2.UnitY * 940, player[i]);

			if (gameOver)
			{
				// draw game over results
			}
        }

		public GameScreen GoBack()
        {
            return null;
//			return new MainMenu();
        }
    }
}
