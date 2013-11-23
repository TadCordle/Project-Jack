using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Badminton.Screens.MultiPlayer;

namespace Badminton.Screens.Menus
{
	class SettingsScreen : GameScreen
	{
		private PlayerSelect prevScreen;

		private bool enterPressed;

		public SettingsScreen(PlayerSelect prevScreen)
		{
			this.prevScreen = prevScreen;
			enterPressed = true;
		}

		public GameScreen Update(GameTime time)
		{
			if (Keyboard.GetState().IsKeyDown(Keys.Enter))
			{
				if (!enterPressed)
				{
					if (prevScreen.Mode == 0)
						return new SingleMap();
					else
						return new SingleMap();
				}
			}
			else
				enterPressed = false;

			return this;
		}

		public GameScreen GoBack()
		{
			return prevScreen;
		}

		public void Draw(SpriteBatch sb)
		{
		}
	}
}
