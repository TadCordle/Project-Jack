using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Badminton.Screens
{
	/// <summary>
	/// A map select screen for single player games
	/// </summary>
	class SingleMapSelect : GameScreen
	{
		private bool enterPressed;

		public SingleMapSelect()
		{
			enterPressed = true;
		}

		public GameScreen Update(GameTime gameTime)
		{
			if (Keyboard.GetState().IsKeyDown(Keys.Enter))
			{
				if (!enterPressed)
				{
					enterPressed = true;
					return new SingleMap(); // Change this when there are actual choices
				}
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
			sb.DrawString(MainGame.fnt_basicFont, "This'll be a map select eventually. Press enter to go to test map.", Vector2.Zero, Color.Black);
		}
	}
}
