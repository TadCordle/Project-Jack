using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Badminton.Screens.MultiPlayer;

namespace Badminton.Screens
{
    class MultiMapSelect : GameScreen
    {
        private bool enterPressed;
        private int mode; // get the selected mode from MuliModeSelect

        public MultiMapSelect(int mode)
        {
            enterPressed = true;
            this.mode = mode;
        }

        public GameScreen Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                if (!enterPressed)
                {
                    enterPressed = true;
                    if (mode == 0)
                    {
                        return new Match_TDM();
                    }

                    if (mode == 1)
                    {
                        return new FreeForAll();
                    }

                    if (mode == 2)
                    {
                        return new OneVsAll(); 
                    }
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

