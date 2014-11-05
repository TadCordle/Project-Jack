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
	class SingleMap : GameMode, GameScreen
	{
		List<StickFigure> enemies;
		List<StickFigure> temp;
		Dictionary<StickFigure, int> toRemove;
		StickFigure winStick;
		int millis;
		float limbStrength;
		bool suddenDeath;

		int kills;
		float maxEnemies;
        
        public SingleMap(Color[] colors, string mapString, float gravity, int lives, float limbStrength, bool suddenDeath, bool traps, bool longRange, bool bots)
            : base(colors, mapString, gravity, lives, limbStrength, suddenDeath, traps, longRange, bots)
		{
			enemies = new List<StickFigure>();
			toRemove = new Dictionary<StickFigure, int>();
			temp = new List<StickFigure>();
			winStick = null;
            this.limbStrength = limbStrength;
            this.suddenDeath = suddenDeath;
			maxEnemies = player.Length;
			kills = 0;
			millis = 0;
		}

		public GameScreen Update(GameTime gameTime)
		{
            int deltatime = gameTime.ElapsedGameTime.Milliseconds;
            startPause -= deltatime;
			if (startPause <= 0)
			{
				foreach (StickFigure s in player)
					if (s != null)
						s.LockControl = false;
			}

			for (int i = 0; i < player.Length; i++)
			{
				if (player[i] != null && info[i].HasLives())
				{
                    player[i].Update(deltatime);
					if (player[i].IsDead)
					{
                        //if (info[i].RespawnTimer < 0)
                        //    info[i].Kill();

                        if (info[i].ShouldRespawn())
                        {
                            player[i].Destroy();
                            if (info[i].HasLives())
                                player[i] = player[i].Respawn();
                            else
                                player[i] = null;
                            //info[i].RespawnTimer -= deltatime;
                        }
                        else
                            info[i].RespawnTimer -= deltatime;
					}
				}
			}

			foreach (StickFigure s in enemies)
			{
                s.Update(deltatime);
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
                    millis += deltatime;
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
                    winStick.Update(deltatime);
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
