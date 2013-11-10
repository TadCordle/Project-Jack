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
		public LongRangeAttack(World w, Vector2 position, Vector2 power, float damage, Category collisionCat)
			: base(w, collisionCat, damage)
		{
			body = BodyFactory.CreateRectangle(w, 100f * MainGame.PIXEL_TO_METER, 16f * MainGame.PIXEL_TO_METER, 50f);
			body.BodyType = BodyType.Dynamic;
			body.Position = position;
			body.LinearVelocity = power;
			body.CollisionCategories = collisionCat;
			body.UserData = this;
			body.OnCollision += new OnCollisionEventHandler(HitWall);
		}

		private bool HitWall(Fixture f1, Fixture f2, Contact c)
		{
			if (f2.Body.UserData is Wall || f2.Body.UserData is Attack)
				body.UserData = null;
			return true;
		}

		public override void Update()
		{
			if (body.UserData == null)
				return;

			body.ApplyForce(-Vector2.UnitY * body.Mass * 9.8f);
		}

		public override void Draw(SpriteBatch sb, Color c)
		{
			sb.Draw(MainGame.tex_box, body.Position * MainGame.METER_TO_PIXEL, null, c, (float)Math.Atan2(body.LinearVelocity.Y, body.LinearVelocity.X), new Vector2(MainGame.tex_box.Width / 2, MainGame.tex_box.Height / 2), new Vector2(100f / MainGame.tex_box.Width, 16f / MainGame.tex_box.Height), SpriteEffects.None, 1f);
		}
	}
}
