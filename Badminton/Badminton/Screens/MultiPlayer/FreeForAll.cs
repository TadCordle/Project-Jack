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
    class FreeForAll : GameMode, GameScreen
    {
       public FreeForAll(Color[] colors, string mapString, float gravity, int minutes, int lives, float limbStrength, bool suddenDeath, bool traps, bool longRange, bool bots)
            : base(colors, mapString, gravity, lives, limbStrength, suddenDeath, traps, longRange, bots)
        {

            if (bots && colors.Length < 4)
			{
				for (int i = colors.Length; i < 4; i++)
				{
					player[i] = new AiPlayer(world, spawnPoints[i] * MainGame.PIXEL_TO_METER, Categories[i], 1.5f, limbStrength, suddenDeath ? 0.001f : 1f, false, new Color(i * 60, i * 60, i * 60), Players[i], player);
					player[i].LockControl = true;
				}
			}

			timed = minutes > 0;
			timeRemaining = (minutes == 0 ? -1 : minutes * 60000);
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
                        bool s = info[i].ShouldRespawn();
                        bool s2 = (info[i].RespawnTimer < 0);
                        if (s != s2)
                        {
                            Console.Write("wtf");
                        }
                        if (s2)
                            info[i].Kill();
                        
                        if (s)
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

            // Update ammo
            foreach (TrapAmmo t in ammo)
				if (t != null)
					t.Update();

			// Endgame
			if (!gameOver)
			{
				if (timed && startPause < 0)
                    timeRemaining -= deltatime;
				gameOver = GameIsOver(winners);

				if (Keyboard.GetState().IsKeyDown(Keys.Enter) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start))
					enterPressed = true;
			}
			else
			{
				if (winSticks.Count == 0)
				{
					for (int i = 0; i < winners.Count; i++)
					{
						winSticks.Add(new StickFigure(world, new Vector2(960 + 160 * i - 80 * (winners.Count - 1), 440) * MainGame.PIXEL_TO_METER, Category.None, 3f, 1, 1, false, player[winners[i]].Color));
						winSticks[i].Invulnerability = 0;
						winSticks[i].Stand();
					}
				}

				foreach (StickFigure s in winSticks)
				{
                    s.Update(deltatime);
					s.ApplyForce(world.Gravity * -1);
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

		protected virtual bool GameIsOver(List<int> winners)
		{
			bool gameOver = true;
			winners.Clear();
			for (int i = 0; i < info.Length; i++)
			{
				if (info[i].Lives > 0)
				{
					winners.Add(i);
					if (winners.Count >= 2)
					{
						gameOver = false;
						winners.Clear();
						break;
					}
				}
			}

			if (gameOver)
				return true;
			else if (timed && timeRemaining <= 0)
			{
				gameOver = true;
				winners.Add(0);
				for (int i = 1, max = info[0].Lives; i < info.Length; i++)
				{
					if (info[i].Lives > max)
					{
						winners.Clear();
						winners.Add(i);
						max = info[i].Lives;
					}
					else if (info[i].Lives == max)
						winners.Add(i);
				}
			}

			return gameOver;
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

            // draw walls
//			foreach (Wall w in walls)
//				w.Draw(sb);

			if (foreground != null)
				sb.Draw(foreground, new Rectangle(0, 0, 1920, 1080), Color.White);

			// Draw HUD
			if (timeRemaining >= 0)
			{
				Color c = new Color(255, 255, 255, 150);
				sb.Draw(MainGame.tex_blank, new Rectangle(0, 0, 200, 63), c);
				sb.Draw(MainGame.tex_clock, Vector2.One, null, Color.White, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 1f);
				sb.DrawString(MainGame.fnt_midFont, timeRemaining / 60000 + ":" + (timeRemaining % 60000 / 1000 < 10 ? "0" : "") + (timeRemaining % 60000 / 1000 < 0 ? 0 : timeRemaining % 60000 / 1000), Vector2.UnitX * 70 + Vector2.UnitY * 5, Color.Black);
//				MainGame.DrawOutlineText(sb, MainGame.fnt_midFont, timeRemaining / 60000 + ":" + (timeRemaining % 60000 / 1000 < 10 ? "0" : "") + (timeRemaining % 60000 / 1000 < 0 ? 0 : timeRemaining % 60000 / 1000), Vector2.UnitX * 70 + Vector2.UnitY * 5, Color.White);
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

			if (winners.Count == 1)
			{
				string winString = "Winner: Player " + (winners[0] + 1) + "!";
				sb.DrawString(MainGame.fnt_midFont, winString, new Vector2(960 - MainGame.fnt_midFont.MeasureString(winString).X / 2, 700), Color.Black);
				if (winSticks.Count > 0)
					winSticks[0].Draw(sb);
			}
			else
			{
				sb.DrawString(MainGame.fnt_midFont, "Tie reached!", new Vector2(960 - MainGame.fnt_midFont.MeasureString("Tie reached!").X / 2, 700), Color.Black);
				string tiedPlayers = "Tied players: ";
				if (winners.Count == 0)
					tiedPlayers += "Nobody";
				else
					foreach (int i in winners)
						tiedPlayers += (tiedPlayers.Length == 14 ? "" : ", ") + (i + 1).ToString();
				sb.DrawString(MainGame.fnt_midFont, tiedPlayers, new Vector2(960 - MainGame.fnt_midFont.MeasureString(tiedPlayers).X / 2, 775), Color.Black);
				foreach (StickFigure s in winSticks)
					s.Draw(sb);
			}
		}

		public GameScreen GoBack()
        {
			MediaPlayer.Stop();
			return new MainMenu();
        }
    }
}
