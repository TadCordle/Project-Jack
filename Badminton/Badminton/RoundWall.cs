using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

namespace Badminton
{
	class RoundWall : Wall
	{
		private Body body;

		private float radius;
		public float Radius { get { return radius; } }

		public RoundWall(World w, float x, float y, float radius)
		{
			this.radius = radius * MainGame.PIXEL_TO_METER;
			body = BodyFactory.CreateCircle(w, this.radius, 1f);
			body.Friction = 100f;
			body.Position = new Vector2(x, y) * MainGame.PIXEL_TO_METER;
			body.BodyType = BodyType.Static;
			body.CollisionCategories = Category.Cat31; // change when more players are added
			body.UserData = this;
		}

		public void Draw(SpriteBatch sb)
		{
			// don't worry about it
		}
	}
}
