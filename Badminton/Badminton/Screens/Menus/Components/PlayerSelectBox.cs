using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using FarseerPhysics.Dynamics;

using Badminton.Stick_Figures;

namespace Badminton.Screens.Menus.Components
{
	class PlayerSelectBox
	{
		private PlayerIndex index;
		private Vector2 position;

		private State state;

		private StickFigure player;

		private bool joinPressed, backPressed, leftPressed, rightPressed;
		private bool usesKeyboard;
		private int selectedColor;

		public PlayerSelectBox(Vector2 position, PlayerIndex index, int colorIndex)
		{
			this.position = position;
			this.index = index;

			joinPressed = true;
			backPressed = true;
			leftPressed = true;
			rightPressed = true;

			selectedColor = colorIndex;
			usesKeyboard = false;

			state = GetState();
		}

		private State GetState()
		{
			Array vals = Enum.GetValues(typeof(PlayerIndex));
			int i;
			for (i = 0; i < vals.Length; i++)
			{
				if ((PlayerIndex)vals.GetValue(i) == index)
					break;
			}
			
			if (GamePad.GetState(index).IsConnected)
				return State.Controller;
			else
			{
				if (i == 0 || GamePad.GetState((PlayerIndex)vals.GetValue(i - 1)).IsConnected)
					return State.Keyboard;
				else
					return State.Off;
			}
		}

		public void Update(World w)
		{
			if (state != State.SelectingPlayer && state != State.Ready)
				state = GetState();

			if (state == State.Keyboard)
			{
				if (Keyboard.GetState().IsKeyDown(Keys.Enter))
				{
					if (!joinPressed)
					{
						joinPressed = true;
						usesKeyboard = true;
						state = State.SelectingPlayer;
					}
				}
				else
					joinPressed = false;
			}
			else if (state == State.Controller)
			{
				if (GamePad.GetState(index).IsButtonDown(Buttons.A))
				{
					if (!joinPressed)
					{
						joinPressed = true;
						state = State.SelectingPlayer;
					}
				}
				else
					joinPressed = false;
			}
			else if (state == State.SelectingPlayer)
			{
				if (player == null)
				{
					player = new StickFigure(w, (position + Vector2.UnitY * 200 + Vector2.UnitX * 250) * MainGame.PIXEL_TO_METER, Category.None, 3f, LocalPlayer.Colors[selectedColor]);
					player.Stand();
				}
				else
				{
					player.Color = LocalPlayer.Colors[selectedColor];
					player.Update();
				}

				bool pressingLeft, pressingRight, pressingBack;
				if (usesKeyboard)
				{
					pressingLeft = Keyboard.GetState().IsKeyDown(Keys.Left);
					pressingRight = Keyboard.GetState().IsKeyDown(Keys.Right);
					pressingBack = Keyboard.GetState().IsKeyDown(Keys.Back) || Keyboard.GetState().IsKeyDown(Keys.Escape);
				}
				else
				{
					pressingLeft = GamePad.GetState(index).IsButtonDown(Buttons.DPadLeft) || GamePad.GetState(index).IsButtonDown(Buttons.LeftThumbstickLeft);
					pressingRight = GamePad.GetState(index).IsButtonDown(Buttons.DPadRight) || GamePad.GetState(index).IsButtonDown(Buttons.LeftThumbstickRight);
					pressingBack = GamePad.GetState(index).IsButtonDown(Buttons.Back) || GamePad.GetState(index).IsButtonDown(Buttons.B);
				}

				if (pressingLeft)
				{
					if (!leftPressed)
					{
						selectedColor = selectedColor == 0 ? LocalPlayer.Colors.Length - 1 : selectedColor - 1;
						leftPressed = true;
					}
				}
				else
					leftPressed = false;

				if (pressingRight)
				{
					if (!rightPressed)
					{
						selectedColor = selectedColor == LocalPlayer.Colors.Length - 1 ? 0 : selectedColor + 1;
						rightPressed = true;
					}
				}
				else
					rightPressed = false;

				if (pressingBack)
				{
					if (!backPressed)
					{
						backPressed = true;
						usesKeyboard = false;
						if (usesKeyboard)
							state = State.Keyboard;
						else
							state = State.Controller;
					}
				}
				else
					backPressed = false;

				if ((!usesKeyboard && GamePad.GetState(index).IsButtonDown(Buttons.A)) || (usesKeyboard && Keyboard.GetState().IsKeyDown(Keys.Enter)))
				{
					if (!joinPressed)
					{
						joinPressed = true;
						state = State.Ready;
					}
				}
				else
					joinPressed = false;
			}
			else if (state == State.Ready)
			{
				bool pressingBack;
				if (usesKeyboard)
					pressingBack = Keyboard.GetState().IsKeyDown(Keys.Escape) || Keyboard.GetState().IsKeyDown(Keys.Back);
				else
					pressingBack = GamePad.GetState(index).IsButtonDown(Buttons.Back) || GamePad.GetState(index).IsButtonDown(Buttons.B);

				if (pressingBack)
				{
					if (!backPressed)
					{
						state = State.SelectingPlayer;
						backPressed = true;
					}
				}
				else
					backPressed = false;
			}
		}

		public bool IsReady()
		{
			return this.state == State.Off || this.state == State.Ready;
		}

		public bool CanExit()
		{
			return state == State.Off || state == State.Controller || state == State.Keyboard;
		}

		public void Draw(SpriteBatch sb)
		{
			if (state == State.Off)
				sb.Draw(MainGame.tex_ps_off, position, Color.White);
			else if (state == State.Keyboard)
				sb.Draw(MainGame.tex_ps_keyboard, position, Color.White);
			else if (state == State.Controller)
				sb.Draw(MainGame.tex_ps_controller, position, Color.White);
			else
			{
				sb.Draw(MainGame.tex_ps_blank, position, Color.White);
				if (player != null)
					player.Draw(sb);
				if (state == State.Ready)
					sb.DrawString(MainGame.fnt_basicFont, "Ready!", this.position + Vector2.One * 10, Color.Green);
			}
		}

		private enum State
		{
			Controller,
			Keyboard,
			Off,
			SelectingPlayer,
			Ready
		}
	}
}
