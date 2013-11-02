using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;

namespace Badminton.Attacks
{
	class ExplosionParticle : Attack
	{
		private int destroyTimer;

		public ExplosionParticle(World w, Vector2 position, Vector2 velocity, Category collisionCat)
			: base(w, collisionCat, 0.2f)
		{
			body = BodyFactory.CreateCircle(w, 5f * MainGame.PIXEL_TO_METER, 1.0f);
			body.BodyType = BodyType.Dynamic;
			body.CollisionCategories = collisionCat;
			body.CollidesWith = Category.All & ~collisionCat;
			body.Position = position;
			body.LinearVelocity = velocity;
			body.UserData = this;
			body.OnCollision += new OnCollisionEventHandler(HitWall);

			destroyTimer = 0;
		}

		private bool HitWall(Fixture fixA, Fixture fixB, Contact contact)
		{
			if (fixB.Body.UserData is Wall)
				body.UserData = null;
			return contact.IsTouching;
		}

		public override void Update()
		{
			if (body.UserData == null)
				return;

			destroyTimer++;
			if (destroyTimer >= 20 && world.BodyList.Contains(body))
				body.UserData = null;
		}

		public override void Draw(SpriteBatch sb, Color c)
		{
			if (body.UserData != null)
				sb.Draw(MainGame.tex_explosionParticle, body.Position * MainGame.METER_TO_PIXEL, null, c, 0f, Vector2.One * 5f, 1f, SpriteEffects.None, 1f);
		}
	}
}
