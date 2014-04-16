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

namespace Badminton.Screens
{
    class HelpScreen:GameScreen
    {
        private LocalPlayer player;
        private World world;

        private List<Wall> walls;


        public HelpScreen()
        {
            world = new World(new Vector2(0, 9.8f));

            
            walls = new List<Wall>();
            walls.Add(new Wall(world, 300, 100, 400,20,0));
            walls.Add(new Wall(world, 300, 700, 400, 20, 0));
            walls.Add(new Wall(world, 100, 400, 20, 600, 0));
            walls.Add(new Wall(world, 500, 400, 20, 600, 0));

            player = new LocalPlayer(world, new Vector2(400, 400) * MainGame.PIXEL_TO_METER, Category.Cat1, 3f, 1f, 1f, false, LocalPlayer.Colors[1], PlayerIndex.One);
        
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
            sb.Draw(MainGame.tex_blank, new Rectangle(100, 50, 1720, 980), new Color(255, 255, 255, 150));

            sb.DrawString(MainGame.fnt_midFont, "MoveRight  LeftThumbstickRight / D", new Vector2(800, 200), Color.Brown);
            sb.DrawString(MainGame.fnt_midFont, "MoveLeft:  LeftThumbstickLeft / A", new Vector2(800, 300), Color.Brown);
            sb.DrawString(MainGame.fnt_midFont, "Crouch:  LeftThumbstickDown / S", new Vector2(800, 400), Color.Brown);
            sb.DrawString(MainGame.fnt_midFont, "Jump:  Button-A / SpaceBar", new Vector2(800, 500), Color.Brown);
            sb.DrawString(MainGame.fnt_midFont, "Punch:  Button-X / LeftClick", new Vector2(800, 600), Color.Brown);
            sb.DrawString(MainGame.fnt_midFont, "Kick:  Button-B / RightClick", new Vector2(800, 700), Color.Brown);
            sb.DrawString(MainGame.fnt_midFont, "Shoot:  RightTrigger / ScrollWheel", new Vector2(800, 800), Color.Brown);
            sb.DrawString(MainGame.fnt_midFont, "Trap:  Button-Y / T", new Vector2(800, 900), Color.Brown);

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


            foreach (Wall w in walls)
                w.Draw(sb);
        }
    }
}
