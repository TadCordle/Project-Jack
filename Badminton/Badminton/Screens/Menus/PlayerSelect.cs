using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using FarseerPhysics.Dynamics;

using Badminton.Screens.Menus.Components;

namespace Badminton.Screens.Menus
{
	class PlayerSelect : GameScreen
	{
		public int Mode { get; set; }

		List<PlayerSelectBox> boxes;
		List<CheckBox> gameModes;
		Button allReady;
		World world;

		bool backPressed, confirmPressed, mouseClicked, upPressed, downPressed;
		bool showButton;

		public PlayerSelect(int mode)
		{
			world = new World(Vector2.Zero);
			backPressed = true;
			confirmPressed = true;
			mouseClicked = true;
			upPressed = true;
			downPressed = true;
			showButton = false;
			
			this.Mode = mode;
			boxes = new List<PlayerSelectBox>();
			boxes.Add(new PlayerSelectBox(new Vector2(270, 145), PlayerIndex.One, 0));
			boxes.Add(new PlayerSelectBox(new Vector2(770, 145), PlayerIndex.Two, 1));
			boxes.Add(new PlayerSelectBox(new Vector2(270, 550), PlayerIndex.Three, 2));
			boxes.Add(new PlayerSelectBox(new Vector2(770, 550), PlayerIndex.Four, 3));

			gameModes = new List<CheckBox>();
			if (mode == 0)
			{
				gameModes.Add(new CheckBox(new Vector2(1400, 300), "Free For All", "ffa"));
				gameModes.Add(new CheckBox(new Vector2(1400, 360), "Team Deathmatch", "tdm"));
				gameModes.Add(new CheckBox(new Vector2(1400, 420), "1 Vs All", "1va"));
				gameModes[0].Checked = true;
			}

			allReady = new Button(new Vector2(1500, 600), MainGame.tex_ps_next, "next");
		}

		public GameScreen Update(GameTime time)
		{
			foreach (PlayerSelectBox box in boxes)
				box.Update(world);

			CheckBox.UpdateCheckboxes(gameModes);
			

			if (Mouse.GetState().LeftButton == ButtonState.Pressed)
			{
				if (!mouseClicked)
				{
					mouseClicked = true;
					foreach (CheckBox cb in gameModes)
					{
						if (cb.IsMouseOver())
						{
							CheckBox.UncheckAll(gameModes);
							cb.Checked = true;
						}
					}
					if (showButton && allReady.IsMouseOver())
						return new SettingsScreen(this);
				}
			}
			else
				mouseClicked = false;

			if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.B))
			{
				if (!backPressed)
				{
					backPressed = true;
					return this.GoBack();
				}
			}
			else
				backPressed = false;

			bool usingKeyboard = !GamePad.GetState(PlayerIndex.One).IsConnected;
			if (boxes[0].IsReady())
			{
				if (!showButton)
				{
					bool selectedBoxes = false;
					foreach (CheckBox cb in gameModes)
						if (cb.Selected)
							selectedBoxes = true;

					if (!selectedBoxes)
						gameModes[0].Selected = true;
				}

				if (usingKeyboard && Keyboard.GetState().IsKeyDown(Keys.Down) || !usingKeyboard && GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadDown) || !usingKeyboard && GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftThumbstickDown))
				{
					if (!downPressed)
					{
						downPressed = true;
						if (showButton && allReady.Selected)
						{
							allReady.Selected = false;
							gameModes[0].Selected = true;
						}
						else
						{
							for (int i = 0; i < gameModes.Count; i++)
							{
								if (gameModes[i].Selected)
								{
									gameModes[i].Selected = false;
									i++;
									if (i < gameModes.Count)
										gameModes[i].Selected = true;
									else
									{
										if (showButton)
											allReady.Selected = true;
										else
											gameModes[0].Selected = true;
									}
								}
							}
						}
					}
				}
				else
					downPressed = false;

				if (usingKeyboard && Keyboard.GetState().IsKeyDown(Keys.Up) || !usingKeyboard && GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadUp) || !usingKeyboard && GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftThumbstickUp))
				{
					if (!upPressed)
					{
						upPressed = true;
						if (showButton && allReady.Selected)
						{
							allReady.Selected = false;
							gameModes[gameModes.Count - 1].Selected = true;
						}
						else
						{
							for (int i = gameModes.Count - 1; i >= 0; i--)
							{
								if (gameModes[i].Selected)
								{
									gameModes[i].Selected = false;
									i--;
									if (i >= 0)
										gameModes[i].Selected = true;
									else
									{
										if (showButton)
											allReady.Selected = true;
										else
											gameModes[gameModes.Count - 1].Selected = true;
									}
								}
							}
						}
					}
				}
				else
					upPressed = false;

				if (usingKeyboard && Keyboard.GetState().IsKeyDown(Keys.Enter) || !usingKeyboard && GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A))
				{
					if (!confirmPressed)
					{
						confirmPressed = true;
						foreach (CheckBox cb in gameModes)
						{
							if (cb.Selected)
							{
								CheckBox.UncheckAll(gameModes);
								cb.Checked = true;
							}
						}

						if (showButton && allReady.Selected)
							return new SettingsScreen(this);
					}
				}
				else
					confirmPressed = false;
			}

			bool ready = true;
			bool playerSelected = false;
			foreach (PlayerSelectBox p in boxes)
			{
				if (!p.IsReady())
					ready = false;
				if (p.PlayerSelected())
					playerSelected = true;
			}
			if (ready && playerSelected)
				showButton = true;
			else
			{
				showButton = false;
				allReady.Selected = false;
			}

			if (showButton)
				allReady.Update();

			world.Step((float)time.ElapsedGameTime.TotalSeconds);
			return this;
		}

		public GameScreen GoBack()
		{
			foreach (PlayerSelectBox p in boxes)
				if (!p.CanExit())
					return this;

			return new MainMenu();
		}

		public void Draw(SpriteBatch sb)
		{
			foreach (PlayerSelectBox box in boxes)
				box.Draw(sb);
			foreach (CheckBox cb in gameModes)
				cb.Draw(sb);
			if (showButton)
				allReady.Draw(sb);
		}
	}
}
