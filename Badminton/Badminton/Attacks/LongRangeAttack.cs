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
		List<Particle> particles;
		const int maxParticleTime = 15;
		Random r;

		public LongRangeAttack(World w, Vector2 position, Vector2 power, float damage, Category collisionCat)
			: base(w, collisionCat, damage)
		{
			body = BodyFactory.CreateRectangle(w, 24f * MainGame.PIXEL_TO_METER, 24f * MainGame.PIXEL_TO_METER, 50f);
			body.BodyType = BodyType.Dynamic;
			body.Position = position;
			body.LinearVelocity = power;
			body.Rotation = (float)Math.Atan2(body.LinearVelocity.Y, body.LinearVelocity.X);
			body.CollisionCategories = collisionCat;
			body.UserData = this;
			body.OnCollision += new OnCollisionEventHandler(HitWall);
			particles = new List<Particle>();
			r = new Random();
		}

		private bool HitWall(Fixture f1, Fixture f2, Contact c)
		{
			if (f2.Body.UserData is Wall || f2.Body.UserData is Attack)
			{
				if (f2.Body.UserData is Trap)
					((Trap)f2.Body.UserData).Explode();
				body.UserData = null;
			}
			if (f2.Body.UserData is TrapAmmo)
				return false;

			return true;
		}

		public override void Update()
		{
			if (body.UserData == null)
				return;

			for (int i = 0; i < particles.Count; i++)
			{
				particles[i].time--;
				if (particles[i].time <= 0)
				{
					particles.RemoveAt(i);
					i--;
				}
			}

			if (r.Next(3) == 1)
				particles.Add(new Particle(this.PhysicsBody.Position * MainGame.METER_TO_PIXEL + Vector2.UnitX * (r.Next(10) - 5) + Vector2.UnitY * (r.Next(10) - 5), maxParticleTime));
			
			body.ApplyForce(-Vector2.UnitY * body.Mass * world.Gravity.Y);
		}

		public override void Draw(SpriteBatch sb, Color c)
		{
			sb.Draw(MainGame.tex_longRange, body.Position * MainGame.METER_TO_PIXEL, null, c, body.Rotation, new Vector2(MainGame.tex_box.Width / 2, MainGame.tex_box.Height / 2), new Vector2(24f / MainGame.tex_box.Width, 24f / MainGame.tex_box.Height), SpriteEffects.None, 1f);
			foreach (Particle p in particles)
				sb.Draw(MainGame.tex_explosionParticle, p.position, c);
		}

		private class Particle
		{
			public int time;
			public Vector2 position;

			public Particle(Vector2 position, int time)
			{
				this.position = position;
				this.time = time;
			}
		}
	}
}
