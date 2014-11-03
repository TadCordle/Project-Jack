using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using FarseerPhysics.Dynamics;

using Badminton.Stick_Figures;
using Badminton.Screens.Menus;
using Badminton.Attacks;

namespace Badminton.Screens.MultiPlayer
{
	class SingleMap : GameScreen
	{
		World world;
		List<Wall> walls;
		List<StickFigure> enemies;
		List<StickFigure> temp;
		Dictionary<StickFigure, int> toRemove;
		StickFigure winStick;
		StickFigure[] player;
		Vector2[] spawnPoints;
		TrapAmmo[] ammo;
		Texture2D background, foreground;
		Song music;

		PlayerValues[] info;

		int millis;
		int startPause;
		bool enterPressed;
		float limbStrength;
		bool suddenDeath;

		bool gameOver;
		int kills;
		float maxEnemies;

		private static Category[] Categories = new Category[] { Category.Cat1, Category.Cat2, Category.Cat3, Category.Cat4 };
		private static PlayerIndex[] Players = new PlayerIndex[] { PlayerIndex.One, PlayerIndex.Two, PlayerIndex.Three, PlayerIndex.Four };

		public SingleMap(Color[] colors, string mapString, float gravity, int lives, float limbStrength, bool suddenDeath, bool traps, bool longRange, bool bots)
		{
			world = new World(new Vector2(0, gravity));
			this.limbStrength = limbStrength;
			this.suddenDeath = suddenDeath;

			MapData data = Map.LoadMap(world, mapString);
            background = data.background;
            walls = data.walls;
            spawnPoints = data.spawnPoints;
            Vector3[] ammoPoints = data.ammoPoints;
			ammo = new TrapAmmo[ammoPoints.Length];
			if (traps)
				for (int i = 0; i < ammoPoints.Length; i++)
					ammo[i] = new TrapAmmo(world, new Vector2(ammoPoints[i].X, ammoPoints[i].Y) * MainGame.PIXEL_TO_METER, (int)ammoPoints[i].Z);
            music = data.music;
			MediaPlayer.Play(music);
            foreground = data.foreground;

			StickFigure.AllowTraps = traps;
			StickFigure.AllowLongRange = longRange;

			player = new StickFigure[bots ? 4 : colors.Length];
			this.info = new PlayerValues[bots ? 4 : colors.Length];
			enemies = new List<StickFigure>();
			toRemove = new Dictionary<StickFigure, int>();
			temp = new List<StickFigure>();
			winStick = null;

			for (int i = 0; i < colors.Length; i++)
				if (colors[i] != null)
				{
					player[i] = new LocalPlayer(world, spawnPoints[i] * MainGame.PIXEL_TO_METER, Category.Cat1, 1.5f, limbStrength, suddenDeath ? 0.001f : 1f, false, colors[i], Players[i]);
					player[i].LockControl = true;
				}

			for (int i = 0; i < info.Length; i++)
				info[i] = new PlayerValues(lives);

			maxEnemies = player.Length;
			kills = 0;
			millis = 0;
			startPause = 180;
			gameOver = false;
			enterPressed = true;
		}

		public GameScreen Update(GameTime gameTime)
		{
			startPause--;
			if (startPause == 0)
			{
				foreach (StickFigure s in player)
					if (s != null)
						s.LockControl = false;
			}

			for (int i = 0; i < player.Length; i++)
			{
				if (player[i] != null && info[i].HasLives())
				{
					player[i].Update();
					if (player[i].IsDead)
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

			foreach (StickFigure s in enemies)
			{
				s.Update();
				if (s.IsDead && !toRemove.ContainsKey(s))
				{
					if (!gameOver)
						kills++;
					toRemove.Add(s, 255);
				}
			}
			temp.Clear();
			foreach (StickFigure s in from StickFigure s in toRemove.Keys select s)
				if (!temp.Contains(s))
					temp.Add(s);
			foreach (StickFigure s in temp)
			{
				toRemove[s]--;
				if (toRemove[s] <= 0)
				{
					enemies.Remove(s);
					toRemove.Remove(s);
					s.Destroy();
				}
			}

			if (enemies.Count - toRemove.Count < (int)maxEnemies && startPause < 0)
			{
				enemies.Add(new AiPlayer(world, new Vector2(new Random().Next(1000) + 460, 0) * MainGame.PIXEL_TO_METER, Category.Cat2, 1.5f, this.limbStrength, suddenDeath ? 0.001f : 0.4f, true, Color.White, PlayerIndex.Four, player));
				enemies[enemies.Count - 1].Invulnerability = 0;
			}

			// Update ammo
			foreach (TrapAmmo t in ammo)
				if (t != null)
					t.Update();

			// Endgame
			if (!gameOver)
			{
				if (maxEnemies < 4)
					maxEnemies += 0.0002f;
				if (startPause < 0)
					millis += gameTime.ElapsedGameTime.Milliseconds;
				gameOver = GameIsOver();

				if (Keyboard.GetState().IsKeyDown(Keys.Enter) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start))
					enterPressed = true;
			}
			else
			{
				if (winStick == null)
				{
					winStick = new StickFigure(world, new Vector2(960, 440) * MainGame.PIXEL_TO_METER, Category.None, 3f, 1, 1, true, Color.White);
					winStick.Invulnerability = 0;
					winStick.Stand();
				}
				
				if (winStick != null)
				{
					winStick.Update();
					winStick.ApplyForce(world.Gravity * -1);
				}

				if (Keyboard.GetState().IsKeyDown(Keys.Enter) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start))
				{
					if (!enterPressed)
						return GoBack();
				}
				else
					enterPressed = false;
			}

			world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
			return this;
		}

		protected virtual bool GameIsOver()
		{
			for (int i = 0; i < info.Length; i++)
				if (info[i].Lives > 0)
					return false;
			return true;
		}

		public void Draw(SpriteBatch sb)
		{
			// draw background
			sb.Draw(background, new Rectangle(0, 0, 1920, 1080), Color.White);

			// draw ammo
			foreach (TrapAmmo t in ammo)
				if (t != null)
					t.Draw(sb);

			// draw players
			for (int i = 0; i < player.Length; i++)
				if (player[i] != null)
					player[i].Draw(sb);

			// draw enemies
			foreach (StickFigure s in enemies)
				if (s != null)
					s.Draw(sb, toRemove.ContainsKey(s) ? (byte)toRemove[s] : (byte)255);

			// draw walls
			//			foreach (Wall w in walls)
			//				w.Draw(sb);

			if (foreground != null)
				sb.Draw(foreground, new Rectangle(0, 0, 1920, 1080), Color.White);

			// Draw HUD
			if (millis >= 0)
			{
				Color c = new Color(255, 255, 255, 150);
				sb.Draw(MainGame.tex_blank, new Rectangle(0, 0, 200, 63), c);
				sb.Draw(MainGame.tex_evil_head, Vector2.One, null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 1f);
				sb.DrawString(MainGame.fnt_midFont, kills.ToString(), Vector2.UnitX * 70 + Vector2.UnitY * 5, Color.Black);
				//				MainGame.DrawOutlineText(sb, MainGame.fnt_midFont, millisLeft / 60000 + ":" + (millisLeft % 60000 / 1000 < 10 ? "0" : "") + (millisLeft % 60000 / 1000 < 0 ? 0 : millisLeft % 60000 / 1000), Vector2.UnitX * 70 + Vector2.UnitY * 5, Color.White);
			}

			if (gameOver) // Exactly what it sounds like
				DrawGameOver(sb);
			else
				for (int i = 0; i < info.Length; i++)
					info[i].Draw(sb, Vector2.UnitX * 450 + Vector2.UnitX * i * 300 + Vector2.UnitY * 970, player[i]);

			if (startPause > 0)
				MainGame.DrawOutlineText(sb, MainGame.fnt_bigFont, "Ready...", new Vector2(900, 500), Color.Red);
			else if (startPause > -120)
				MainGame.DrawOutlineText(sb, MainGame.fnt_bigFont, "GO!", new Vector2(930, 500), Color.Green);
		}

		protected virtual void DrawGameOver(SpriteBatch sb)
		{
			Color c = new Color(255, 255, 255, 220);
			sb.Draw(MainGame.tex_blank, new Rectangle(550, 100, 820, 880), c);
			sb.Draw(MainGame.tex_endGame, new Rectangle(550, 100, 820, 880), Color.White);
			if (winStick != null)
				winStick.Draw(sb);
			string winString = "Kills: " + kills.ToString();
			sb.DrawString(MainGame.fnt_bigFont, winString, new Vector2(960 - MainGame.fnt_bigFont.MeasureString(winString).X / 2, 700), Color.Black);
		}

		public GameScreen GoBack()
		{
			MediaPlayer.Stop();
			return new MainMenu();
		}
	}
}
