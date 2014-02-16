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
using Badminton.Attacks;

namespace Badminton.Screens
{
	class SingleMap : GameScreen
	{
		World world;
		List<Wall> walls;
		Vector2[] spawnPoints;
		TrapAmmo[] ammo;
		Texture2D background;

		StickFigure testFigure1;

		public SingleMap()
		{
			world = new World(new Vector2(0, 9.8f)); // That'd be cool to have gravity as a map property, so you could play 0G levels

			object[] map = Map.LoadCastle(world);
			background = (Texture2D)map[0];
			walls = (List<Wall>)map[1];
			spawnPoints = (Vector2[])map[2];
			Vector3[] ammoPoints = (Vector3[])map[3];
			ammo = new TrapAmmo[ammoPoints.Length];
			for (int i = 0; i < ammoPoints.Length; i++)
				ammo[i] = new TrapAmmo(world, new Vector2(ammoPoints[i].X, ammoPoints[i].Y) * MainGame.PIXEL_TO_METER, (int)ammoPoints[i].Z);

			testFigure1 = new LocalPlayer(world, spawnPoints[0] * MainGame.PIXEL_TO_METER, Category.Cat1, 1.5f, 1.0f, 1.0f, true, Color.Red, PlayerIndex.One);
		}

		public virtual GameScreen Update(GameTime gameTime)
		{
			testFigure1.Update();

			foreach (TrapAmmo t in ammo)
				t.Update();

			// These two lines stay here, even after we delete testing stuff
			world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);
			return this;
		}

		public virtual GameScreen GoBack()
		{
			return null; //new MainMenu(); // Change this to show confirmation dialog later
		}

		public virtual void Draw(SpriteBatch sb)
		{
			sb.Draw(background, new Rectangle(0, 0, 1920, 1080), Color.White);

			testFigure1.Draw(sb);

			foreach (TrapAmmo t in ammo)
				t.Draw(sb);

			foreach (Wall w in walls)
				w.Draw(sb);
		}
	}
}
