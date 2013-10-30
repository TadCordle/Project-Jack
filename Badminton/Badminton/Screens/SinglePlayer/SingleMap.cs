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
		Vector2[] spawnPoints;
		Texture2D background;

		LocalPlayer testFigure1, testFigure2;

		public SingleMap()
		{
			world = new World(new Vector2(0, 9.8f)); // That'd be cool to have gravity as a map property, so you could play 0G levels

			object[] map = Map.LoadCastle(world);
			background = (Texture2D)map[0];
			walls = (List<Wall>)map[1];
			spawnPoints = (Vector2[])map[2];

			testFigure1 = new LocalPlayer(world, spawnPoints[0] * MainGame.PIXEL_TO_METER, Category.Cat1, 1.5f, Color.Red, PlayerIndex.One);
			testFigure2 = new LocalPlayer(world, spawnPoints[1] * MainGame.PIXEL_TO_METER, Category.Cat2, 1.5f, Color.Green, PlayerIndex.Two);
		}

		public virtual GameScreen Update(GameTime gameTime)
		{
			testFigure1.Update();
			testFigure2.Update();

			if (testFigure1.IsDead || testFigure1.Position.Y * MainGame.METER_TO_PIXEL > 1080)
			{
				testFigure1.Destroy();
				testFigure1 = new LocalPlayer(world, spawnPoints[0] * MainGame.PIXEL_TO_METER, Category.Cat1, 1.5f, Color.Red, PlayerIndex.One);
			}
			if (testFigure2.IsDead || testFigure2.Position.Y * MainGame.METER_TO_PIXEL > 1080)
			{
				testFigure2.Destroy();
				testFigure2 = new LocalPlayer(world, spawnPoints[1] * MainGame.PIXEL_TO_METER, Category.Cat2, 1.5f, Color.Green, PlayerIndex.Two);
			}

			// These two lines stay here, even after we delete testing stuff
			world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
			return this;
		}

		public virtual GameScreen Exit()
		{
			return null; //new MainMenu(); // Change this to show confirmation dialog later
		}

		public virtual void Draw(SpriteBatch sb)
		{
			sb.Draw(background, new Rectangle(0, 0, 1920, 1080), Color.White);

			testFigure1.Draw(sb);
			testFigure2.Draw(sb);

//			foreach (Wall w in walls)
//				w.Draw(sb);
		}
	}
}
