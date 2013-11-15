using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Badminton.Screens.Menus
{
	class MainMenu : GameScreen
	{
		private List<string> choices; // change to list of buttons later
		private int currentChoice;
		private bool upPressed, downPressed, enterPressed;

		public MainMenu()
		{
			choices = new List<string>();
			choices.Add("Competitive");
			choices.Add("Cooperative");
			choices.Add("Character Customization");
			choices.Add("Exit");

			currentChoice = 0;
			enterPressed = true;
			upPressed = true;
			downPressed = true;
		}

		public GameScreen Update(GameTime gameTime)
		{
			// Choose selection
			if (Keyboard.GetState().IsKeyDown(Keys.Enter))
			{
				if (!enterPressed)
				{
					enterPressed = true;
					if (currentChoice == 0)
						return new PlayerSelect(0);
					else if (currentChoice == 1)
						return new PlayerSelect(1); // Change this when we make multiplayer lobby
					else if (currentChoice == 2)
						return this; // Change this when we make character customization
					else
						return null;
				}
			}
			else
				enterPressed = false;

			// Move cursor with arrow keys
			if (Keyboard.GetState().IsKeyDown(Keys.Up))
			{
				if (!upPressed)
				{
					upPressed = true;
					currentChoice = Math.Max(0, currentChoice - 1);
				}
			}
			else
				upPressed = false;

			if (Keyboard.GetState().IsKeyDown(Keys.Down))
			{
				if (!downPressed)
				{
					downPressed = true;
					currentChoice = Math.Min(currentChoice + 1, choices.Count - 1);
				}
			}
			else
				downPressed = false;

			// Stay on this screen if no selections have been made
			return this;
		}

		public GameScreen Exit()
		{
			return null;
		}

		public void Draw(SpriteBatch sb)
		{
			// Draw buttons and title and stuff here when we have graphics, for now drawing text
			for (int y = 0; y < 4; y++)
				sb.DrawString(MainGame.fnt_basicFont, choices[y], new Vector2(480 - MainGame.fnt_basicFont.MeasureString(choices[y]).X / 2, 300 + y * 32), y == currentChoice ? Color.Yellow : Color.Black);
		}
	}
}
