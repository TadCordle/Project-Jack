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
		public Color[] Colors;

		List<PlayerSelectBox> boxes;
		List<Component> components;
		World world;

		bool backPressed, confirmPressed, upPressed, downPressed;
		bool showButton;

		public PlayerSelect(int mode)
		{
			world = new World(Vector2.Zero);
			backPressed = true;
			confirmPressed = true;
			upPressed = true;
			downPressed = true;
			showButton = false;
			
			this.Mode = mode;
			boxes = new List<PlayerSelectBox>();
			boxes.Add(new PlayerSelectBox(new Vector2(270, 145), PlayerIndex.One, 0));
			boxes.Add(new PlayerSelectBox(new Vector2(770, 145), PlayerIndex.Two, 1));
			boxes.Add(new PlayerSelectBox(new Vector2(270, 550), PlayerIndex.Three, 2));
			boxes.Add(new PlayerSelectBox(new Vector2(770, 550), PlayerIndex.Four, 3));

			components = new List<Component>();
			if (mode == 0)
			{
				components.Add(new CheckBox(new Vector2(1400, 300), "Free For All", "0"));
				components.Add(new CheckBox(new Vector2(1400, 360), "Team Deathmatch", "1"));
				components.Add(new CheckBox(new Vector2(1400, 420), "1 Vs All", "2"));
				((CheckBox)components[0]).Checked = true;
			}

			components.Add(new Button(new Vector2(1500, 600), MainGame.tex_ps_next, "next"));
		}

		public GameScreen Update(GameTime time)
		{
			foreach (PlayerSelectBox box in boxes)
				box.Update(world);

			Component.UpdateSelection(components);
			

//			if (Mouse.GetState().LeftButton == ButtonState.Pressed)
//			{
//				if (!mouseClicked)
//				{
//					mouseClicked = true;
//					foreach (Component c in components)
//					{
//						if (c.IsMouseOver())
//						{
//							if (c is CheckBox)
//							{
//								CheckBox.UncheckAll(components);
//								((CheckBox)c).Checked = true;
//							}
//							else
//							{
//								if (showButton && components[components.Count - 1].IsMouseOver())
//									return new SettingsScreen(this);
//							}
//						}
//					}
//				}
//			}
//			else
//				mouseClicked = false;

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
				if (showButton)
				{
					bool selectedBoxes = false;
					foreach (Component cb in components)
						if (cb.Selected)
							selectedBoxes = true;

					if (!selectedBoxes)
						components[0].Selected = true;
				}

				if (usingKeyboard && Keyboard.GetState().IsKeyDown(Keys.Down) || !usingKeyboard && GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadDown) || !usingKeyboard && GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftThumbstickDown))
				{
					if (!downPressed)
					{
						downPressed = true;
						for (int i = 0; i < components.Count; i++)
						{
							if (components[i].Selected)
							{
								components[i].Selected = false;
								i++;
								if (i < components.Count)
									components[i].Selected = true;
								else
									components[0].Selected = true;
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
						for (int i = components.Count - 1; i >= 0; i--)
						{
							if (components[i].Selected)
							{
								components[i].Selected = false;
								i--;
								if (i >= 0)
									components[i].Selected = true;
								else
									components[components.Count - 1].Selected = true;
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
						foreach (Component c in components)
						{
							if (c.Selected)
							{
								if (c is CheckBox)
								{
									CheckBox.UncheckAll(components);
									((CheckBox)c).Checked = true;
									this.Mode = int.Parse(((CheckBox)c).ReturnString);
								}
								else
								{
									if (showButton && components[components.Count - 1].Selected)
									{
										int numColors = 0;
										foreach (PlayerSelectBox p in boxes)
										{
											if (p.PlayerSelected())
												numColors++;
										}
										Colors = new Color[numColors];
										int i = 0;
										foreach (PlayerSelectBox p in boxes)
										{
											if (p.PlayerSelected())
											{
												Colors[i] = p.Color;
												i++;
											}
										}
										backPressed = true; // Prevent from going back again if returning from settings screen
										return new SettingsScreen(this);
									}
								}
							}
						}
					}
				}
				else
					confirmPressed = false;
			}
			else
				foreach (Component c in components)
					c.Selected = false;

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
				components[components.Count - 1].Selected = false;
			}

			if (components.Count > 1)
			{
				if (((CheckBox)components[0]).Checked)
				{
					boxes[0].SpecialSkin = false;
					boxes[1].SpecialSkin = false;
					boxes[2].SpecialSkin = false;
					boxes[3].SpecialSkin = false;
				}
				else if (((CheckBox)components[1]).Checked)
				{
					boxes[0].SpecialSkin = false;
					boxes[1].SpecialSkin = true;
					boxes[2].SpecialSkin = false;
					boxes[3].SpecialSkin = true;
				}
				else if (((CheckBox)components[2]).Checked)
				{
					boxes[0].SpecialSkin = false;
					boxes[1].SpecialSkin = true;
					boxes[2].SpecialSkin = true;
					boxes[3].SpecialSkin = true;
				}
			}

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
			sb.Draw(MainGame.tex_bg_castle, new Rectangle(0, 0, 1920, 1080), Color.White);
			Color c = new Color(255, 255, 255, 150);
			sb.Draw(MainGame.tex_blank, new Rectangle(100, 50, 1720, 980), c);

			foreach (PlayerSelectBox box in boxes)
				box.Draw(sb);
			foreach (Component comp in components)
				if (comp is CheckBox)
					comp.Draw(sb);

			if (showButton)
				components[components.Count - 1].Draw(sb);
			if (boxes[0].IsReady() && boxes[0].PlayerSelected())
				sb.DrawString(MainGame.fnt_midFont, "Player 1: Select game mode!", new Vector2(1283, 200), Color.Black);
		}
	}
}
