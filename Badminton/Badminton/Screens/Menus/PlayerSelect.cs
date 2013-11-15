using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Badminton.Screens.Menus
{
	class PlayerSelect : GameScreen
	{
		public int Mode { get; set; }

		private bool enterPressed;

		public PlayerSelect(int mode)
		{
			this.Mode = mode;
			enterPressed = true;
		}

		public GameScreen Update(GameTime time)
		{
			if (Keyboard.GetState().IsKeyDown(Keys.Enter))
			{
				if (!enterPressed)
					return new SettingsScreen(this);
			}
			else
				enterPressed = false;

			return this;
		}

		public GameScreen Exit()
		{
			return new MainMenu();
		}

		public void Draw(SpriteBatch sb)
		{
		}
	}
}
