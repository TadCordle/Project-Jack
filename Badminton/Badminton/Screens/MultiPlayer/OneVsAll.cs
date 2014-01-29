using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerPhysics.Dynamics;

using Badminton.Stick_Figures;
using Badminton.Screens.Menus;

namespace Badminton.Screens.MultiPlayer
{
    class OneVsAll : GameScreen
    {
        World world;
        List<Wall> walls;
        LocalPlayer testFigure1, testFigure2, testFigure3, testFigure4;

        public OneVsAll()
        {
            world = new World(new Vector2(0, 9.8f)); // That'd be cool to have gravity as a map property, so you could play 0G levels

 //           testFigure1 = new LocalPlayer(world, new Vector2(480 * MainGame.PIXEL_TO_METER, 480 * MainGame.PIXEL_TO_METER), Category.Cat1, 1.5f, Color.Red, PlayerIndex.One);
 //           testFigure2 = new LocalPlayer(world, new Vector2(1200 * MainGame.PIXEL_TO_METER, 480 * MainGame.PIXEL_TO_METER), Category.Cat2, 1.5f, Color.LightBlue, PlayerIndex.Two);
 //           testFigure3 = new LocalPlayer(world, new Vector2(900 * MainGame.PIXEL_TO_METER, 480 * MainGame.PIXEL_TO_METER), Category.Cat2, 1.5f, Color.Blue, PlayerIndex.Three);
 //           testFigure4 = new LocalPlayer(world, new Vector2(1000 * MainGame.PIXEL_TO_METER, 480 * MainGame.PIXEL_TO_METER), Category.Cat2, 1.5f, Color.Cyan, PlayerIndex.Four);

            walls = new List<Wall>();
            walls.Add(new Wall(world, 960 * MainGame.PIXEL_TO_METER, 1040 * MainGame.PIXEL_TO_METER, 1200 * MainGame.PIXEL_TO_METER, 32 * MainGame.PIXEL_TO_METER, 0.0f));
        }

        public GameScreen Update(GameTime gameTime)
        {
            testFigure1.Update();
            testFigure2.Update();
            testFigure3.Update();
            testFigure4.Update();

            // These two lines stay here, even after we delete testing stuff
            world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
            return this;
        }

		public void Draw(SpriteBatch sb)
        {
            sb.Draw(MainGame.tex_bg_castle, new Rectangle(0, 0, 1920, 1080), Color.White);

            testFigure1.Draw(sb);
            testFigure2.Draw(sb);
            testFigure3.Draw(sb);
            testFigure4.Draw(sb);

            foreach (Wall w in walls)
                w.Draw(sb);
        }

		public GameScreen GoBack()
        {
            return new MainMenu();
        }
    }
}
