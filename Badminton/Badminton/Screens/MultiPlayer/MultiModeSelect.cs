using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Badminton.Screens.MultiPlayer;

namespace Badminton.Screens
{
    class MultiModeSelect: GameScreen
    {

        private List<string> modes;
        private int currentChoice;
        private bool upPressed, downPressed, enterPressed;
        private int mode; // pass to mapselect
      
        public MultiModeSelect()
        {
            modes = new List<string>();
            modes.Add("TeamDeathMode");
            modes.Add("FreeForAll");
            modes.Add("One vs All");   // currently 3 modes 

            currentChoice = 0;
            enterPressed = true;
            upPressed = true;
            downPressed = true;
        }

        public GameScreen Update(GameTime gametime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                if (!enterPressed)
                {
                    enterPressed = true;
                    if (currentChoice == 0)
                        return new MultiMapSelect(0); // pass the mode to Mapselect
                    else if (currentChoice == 1)
                        return new MultiMapSelect(1);
                    else if (currentChoice == 2)
                        return new MultiMapSelect(2); 
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
					currentChoice = Math.Min(currentChoice + 1, modes.Count - 1);
				}
			}
			else
				downPressed = false;

			// Stay on this screen if no selections have been made
			return this;
        }

        public GameScreen Exit()
        {
            return new MainMenu();   // back to mainmenu
        }

        public void Draw(SpriteBatch sb)
        {

            for (int y = 0; y < 3; y++)
                sb.DrawString(MainGame.fnt_basicFont, modes[y], new Vector2(480 - MainGame.fnt_basicFont.MeasureString(modes[y]).X / 2, 300 + y * 32), y == currentChoice ? Color.Yellow : Color.Black);
        }
        
    }
}
