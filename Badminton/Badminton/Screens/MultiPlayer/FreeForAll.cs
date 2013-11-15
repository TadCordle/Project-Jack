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
	class FreeForAll : GameScreen
    {
        World world;
        List<Wall> walls;

        LocalPlayer testFigure1, testFigure2, testFigure3, testFigure4;

		List<LocalPlayer> players;

        public FreeForAll()
        {
            world = new World(new Vector2(0, 9.8f)); // That'd be cool to have gravity as a map property, so you could play 0G levels

			players = new List<LocalPlayer>();
            players.Add(new LocalPlayer(world, new Vector2(480 * MainGame.PIXEL_TO_METER, 480 * MainGame.PIXEL_TO_METER), Category.Cat1, 1.5f, Color.Red, PlayerIndex.One));
            players.Add(new LocalPlayer(world, new Vector2(1200 * MainGame.PIXEL_TO_METER, 480 * MainGame.PIXEL_TO_METER), Category.Cat2, 1.5f, Color.Green, PlayerIndex.Two));
            players.Add(new LocalPlayer(world, new Vector2(900 * MainGame.PIXEL_TO_METER, 480 * MainGame.PIXEL_TO_METER), Category.Cat3, 1.5f, Color.Blue, PlayerIndex.Three));
            players.Add(new LocalPlayer(world, new Vector2(1000 * MainGame.PIXEL_TO_METER, 480 * MainGame.PIXEL_TO_METER), Category.Cat4, 1.5f, Color.Cyan, PlayerIndex.Four));

            walls = new List<Wall>();
            walls.Add(new Wall(world, 960 * MainGame.PIXEL_TO_METER, 1040 * MainGame.PIXEL_TO_METER, 1200 * MainGame.PIXEL_TO_METER, 32 * MainGame.PIXEL_TO_METER, 0.0f));
        }

		public GameScreen Update(GameTime gameTime)
        {
			foreach (LocalPlayer p in players)
				p.Update();

            // These two lines stay here, even after we delete testing stuff
            world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
            return this;
        }

		public void Draw(SpriteBatch sb)
        {
            sb.Draw(MainGame.tex_bg_castle, new Rectangle(0, 0, 1920, 1080), Color.White);

			foreach (LocalPlayer p in players)
				p.Draw(sb);

			foreach (Wall w in walls)
                w.Draw(sb);
        }

		public GameScreen Exit()
        {
            return new MainMenu();
        }
    }
}
