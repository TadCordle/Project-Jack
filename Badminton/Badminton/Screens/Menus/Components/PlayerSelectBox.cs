using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Badminton.Screens.Menus.Components
{
	class PlayerSelectBox
	{
		private PlayerIndex index;
		private Vector2 position;

		private State state;

		private bool joinPressed, backPressed;

		public PlayerSelectBox(Vector2 position, PlayerIndex index)
		{
			this.position = position;
			this.index = index;

			joinPressed = true;
			backPressed = true;

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

		public void Update()
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
			}
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
