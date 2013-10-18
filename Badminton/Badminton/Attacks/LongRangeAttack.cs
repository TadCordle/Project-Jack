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
	class LongRangeAttack : Attack
	{
		private float damage;
		public Body body;
		private Category collisionCat;

		public float Damage { get { return damage; } }
		public Body PhysicsBody { get { return body; } }
	
		public LongRangeAttack(World w, Vector2 position, Vector2 power, float damage, Category collisionCat)
		{
			body = BodyFactory.CreateRectangle(w, 100f * MainGame.PIXEL_TO_METER, 16f * MainGame.PIXEL_TO_METER, 5000f);
			body.BodyType = BodyType.Dynamic;
			body.Position = position;
			body.LinearVelocity = power;
			this.collisionCat = collisionCat;
			body.CollisionCategories = collisionCat;
			body.UserData = this;
			this.damage = damage;

			body.OnCollision += new OnCollisionEventHandler(HitWall);
		}

		private bool HitWall(Fixture f1, Fixture f2, Contact c)
		{
			if (f2.Body.UserData is Wall)
				body.UserData = null;
			return true;
		}

		public void Update()
		{
			if (body.UserData == null)
				return;

			body.ApplyForce(-Vector2.UnitY * body.Mass * 9.8f);
		}

		public void Draw(SpriteBatch sb, Color c)
		{
			sb.Draw(MainGame.tex_longRange, body.Position * MainGame.METER_TO_PIXEL, null, c, (float)Math.Atan2(body.LinearVelocity.Y, body.LinearVelocity.X), new Vector2(MainGame.tex_wave.Width / 2, MainGame.tex_wave.Height / 2), 1f, SpriteEffects.None, 1f);
		}
	}
}
