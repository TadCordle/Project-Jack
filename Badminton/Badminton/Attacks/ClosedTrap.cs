using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;

namespace Badminton.Attacks
{
	class ClosedTrap : Attack
	{
		public ClosedTrap(World world, Vector2 position, Vector2 velocity, Category collisionCat)
			: base(world, collisionCat, 0f)
		{
			body = BodyFactory.CreateCircle(world, 16 * MainGame.PIXEL_TO_METER, 1f);
			body.BodyType = BodyType.Dynamic;
			body.Position = position;
			body.LinearVelocity = velocity;
			body.CollisionCategories = collisionCat;
			body.UserData = this;
			body.OnCollision += new OnCollisionEventHandler(OpenTrap);
		}

		private bool OpenTrap(Fixture a, Fixture b, Contact c)
		{
			if (b.Body.UserData is Wall)
			{
				Vector2 normal;
				FarseerPhysics.Common.FixedArray2<Vector2> pointlist;

				c.GetWorldManifold(out normal, out pointlist);
				Vector2 position = pointlist[0];

				// Create open trap

				body.UserData = null;
			}

			return c.IsTouching;
		}

		public override void Update()
		{
			if (body.UserData == null)
				return;
		}

		public override void Draw(SpriteBatch sb, Color c)
		{
			sb.Draw(MainGame.tex_trapClosed, body.Position * MainGame.METER_TO_PIXEL, null, c, body.Rotation, new Vector2(MainGame.tex_trapClosed.Width / 2, MainGame.tex_trapClosed.Height / 2), 1f, SpriteEffects.None, 1f);
		}
	}
}
