using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using FarseerPhysics;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;

using Badminton.Stick_Figures;

namespace Badminton.Screens
{
	class SingleMap : GameScreen
	{
		World world;
		List<Wall> walls;

		LocalPlayer testFigure1, testFigure2;

		public SingleMap()
		{
			world = new World(new Vector2(0, 9.8f)); // That'd be cool to have gravity as a map property, so you could play 0G levels

			testFigure1 = new LocalPlayer(world, new Vector2(480 * MainGame.PIXEL_TO_METER, 480 * MainGame.PIXEL_TO_METER), Category.Cat1, 1.5f, Color.Red, PlayerIndex.One);
			testFigure2 = new LocalPlayer(world, new Vector2(1200 * MainGame.PIXEL_TO_METER, 480 * MainGame.PIXEL_TO_METER), Category.Cat2, 1.5f, Color.Green, PlayerIndex.Two);
            
			walls = new List<Wall>();
			walls.Add(new Wall(world, 960 * MainGame.PIXEL_TO_METER, 1040 * MainGame.PIXEL_TO_METER, 1200 * MainGame.PIXEL_TO_METER, 32 * MainGame.PIXEL_TO_METER, 0.0f));
//			walls.Add(new Wall(world, 360 * MainGame.PIXEL_TO_METER, 540 * MainGame.PIXEL_TO_METER, 32 * MainGame.PIXEL_TO_METER, 1080 * MainGame.PIXEL_TO_METER, 0.0f));
//			walls.Add(new Wall(world, 1560 * MainGame.PIXEL_TO_METER, 540 * MainGame.PIXEL_TO_METER, 32 * MainGame.PIXEL_TO_METER, 1080 * MainGame.PIXEL_TO_METER, 0.0f));
		}

		public GameScreen Update(GameTime gameTime)
		{
			testFigure1.Update();
			testFigure2.Update();

			// These two lines stay here, even after we delete testing stuff
			world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
			return this;
		}

		public GameScreen Exit()
		{
			return null; //new MainMenu(); // Change this to show confirmation dialog later
		}

		public void Draw(SpriteBatch sb)
		{
			sb.Draw(MainGame.tex_bg, new Rectangle(0, 0, 1920, 1080), Color.White);

			testFigure1.Draw(sb);
			testFigure2.Draw(sb);

			foreach (Wall w in walls)
				w.Draw(sb);
		}
	}
}
