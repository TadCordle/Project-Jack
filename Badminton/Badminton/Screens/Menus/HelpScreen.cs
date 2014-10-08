using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Badminton.Screens.Menus;
using Badminton.Screens.Menus.Components;
using Badminton.Stick_Figures;
using FarseerPhysics.Dynamics;

namespace Badminton.Screens.Menus
{
    class HelpScreen:GameScreen
    {
        private LocalPlayer player;
        private World world;

        private List<Wall> walls;


        public HelpScreen()
        {
            world = new World(new Vector2(0, 9.8f));

			MapData data = Map.LoadCastle(world);
            walls = data.walls;
			walls.Add(new Wall(world, 970, 540, 20, 1500, 0));
			LocalPlayer.AllowLongRange = true;
			LocalPlayer.AllowTraps = true;
            player = new LocalPlayer(world, new Vector2(700, 500) * MainGame.PIXEL_TO_METER, Category.Cat1, 1.5f, 1f, 1f, false, LocalPlayer.Colors[1], PlayerIndex.One);
        }

        public GameScreen GoBack() 
        {
            return new MainMenu();
        }

        public GameScreen Update(GameTime gameTime)
        {
            world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
            player.Update();
       
            return this;
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(MainGame.tex_bg_castle, new Rectangle(0, 0, 1920, 1080), Color.White);
            sb.Draw(MainGame.tex_blank, new Rectangle(960, 50, 900, 980), new Color(255, 255, 255, 200));

			sb.DrawString(MainGame.fnt_bigFont, "Help", Vector2.UnitX * 1315 + Vector2.UnitY * 102, Color.Black);
            sb.DrawString(MainGame.fnt_midFont, "MoveRight:  Left Stick / D Key", new Vector2(1000, 200), Color.Black);
			sb.DrawString(MainGame.fnt_midFont, "MoveLeft:  Left Stick / A Key", new Vector2(1000, 300), Color.Black);
			sb.DrawString(MainGame.fnt_midFont, "Crouch:  Left Trigger / S Key", new Vector2(1000, 400), Color.Black);
			sb.DrawString(MainGame.fnt_midFont, "Jump:  Button-A / W Key", new Vector2(1000, 500), Color.Black);
			sb.DrawString(MainGame.fnt_midFont, "Punch:  Button-X / LeftClick", new Vector2(1000, 600), Color.Black);
			sb.DrawString(MainGame.fnt_midFont, "Kick:  Button-B / RightClick", new Vector2(1000, 700), Color.Black);
			sb.DrawString(MainGame.fnt_midFont, "Shoot:  Right Trigger / MiddleClick", new Vector2(1000, 800), Color.Black);
			sb.DrawString(MainGame.fnt_midFont, "Trap:  Button-Y / T Key", new Vector2(1000, 900), Color.Black);
			sb.DrawString(MainGame.fnt_basicFont, "(Select/Esc to go back)", new Vector2(1600, 992), Color.Black);

            player.Draw(sb);


            /*/ 
            jumpBtn = Buttons.A;
			rightBtn = Buttons.LeftThumbstickRight;
			leftBtn = Buttons.LeftThumbstickLeft;
			crouchBtn = Buttons.LeftTrigger;
			punchBtn = Buttons.X;
			kickBtn = Buttons.B;
			shootBtn = Buttons.RightTrigger;
			trapBtn = Buttons.Y;
			upKey = Keys.W;
			rightKey = Keys.D;
			leftKey = Keys.A;
			downKey = Keys.S;
			trapKey = Keys.T;
            /*/


//          foreach (Wall w in walls)
//              w.Draw(sb);
        }
    }
}
