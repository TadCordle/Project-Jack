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
	class Trap : Attack
	{
		public bool Open { get; set; }

		private List<ExplosionParticle> particles;

		public Trap(World world, Vector2 position, Vector2 velocity, Category collisionCat)
			: base(world, collisionCat, 0f)
		{
			body = BodyFactory.CreateCircle(world, 16 * MainGame.PIXEL_TO_METER, 1f);
			body.BodyType = BodyType.Dynamic;
			body.Position = position;
			body.LinearVelocity = velocity;
			body.CollisionCategories = collisionCat;
			body.UserData = this;
			body.OnCollision += new OnCollisionEventHandler(OpenTrap);

			Open = false;
			particles = new List<ExplosionParticle>();
		}

		private bool OpenTrap(Fixture a, Fixture b, Contact c)
		{
			if (b.Body.UserData is Wall)
			{
				if (!Open)
				{
					Vector2 normal;
					FarseerPhysics.Common.FixedArray2<Vector2> pointlist;

					c.GetWorldManifold(out normal, out pointlist);
					Vector2 position = pointlist[0];

					Open = true;
					if (world.BodyList.Contains(body))
						world.RemoveBody(body);
					body = BodyFactory.CreateRectangle(world, 100 * MainGame.PIXEL_TO_METER, 16 * MainGame.PIXEL_TO_METER, 1f);
					body.BodyType = BodyType.Static;
					body.Position = position + normal / normal.Length() * 10 * MainGame.PIXEL_TO_METER;
					body.Rotation = (float)Math.Atan2(normal.Y, normal.X) + MathHelper.PiOver2;
					body.CollisionCategories = collisionCat;
					body.UserData = this;
				}
			}
			else if (b.Body.UserData is Trap)
			{
				if (!Open && ((Trap)b.Body.UserData).Open)
				{
					body.UserData = null;
					((Trap)b.Body.UserData).Explode();
				}
			}
			else if (b.Body.UserData is Stick_Figures.StickFigure)
			{
				if (Open)
					Explode();
			}

			return c.IsTouching;
		}

		public void Explode()
		{
			Random r = new Random();
			for (int i = 0; i < 50; i++)
			{
				float angle = (float)r.NextDouble() * MathHelper.TwoPi;
				particles.Add(new ExplosionParticle(world, body.Position, new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * r.Next(8, 11), collisionCat));
			}

			// Assign user data to lone particle. This will keep the explosion updated/drawn even after the trap is destroyed.
			body.CollisionCategories = Category.None;
			body.UserData = new ExplosionParticle(world, Vector2.One * -10000, Vector2.Zero, Category.None);
			particles.Add((ExplosionParticle)body.UserData);

			// TODO: Play sound
		}

		public override void Update()
		{
			List<ExplosionParticle> toRemove = new List<ExplosionParticle>();
			foreach (ExplosionParticle p in particles)
			{
				p.Update();
				if (p.PhysicsBody.UserData == null)
				{
					world.RemoveBody(p.PhysicsBody);
					toRemove.Add(p);
				}
			}
			foreach (ExplosionParticle p in toRemove)
				particles.Remove(p);

			if (body.UserData is ExplosionParticle && particles.Count == 0)
				body.UserData = null;
		}

		public override void Draw(SpriteBatch sb, Color c)
		{
			if (body.UserData is Trap)
			{
				if (Open)
					sb.Draw(MainGame.tex_trapOpen, body.Position * MainGame.METER_TO_PIXEL, null, c, body.Rotation, new Vector2(MainGame.tex_trapOpen.Width / 2, MainGame.tex_trapOpen.Height / 2), 1f, SpriteEffects.None, 1f);
				else
					sb.Draw(MainGame.tex_trapClosed, body.Position * MainGame.METER_TO_PIXEL, null, c, body.Rotation, new Vector2(MainGame.tex_trapClosed.Width / 2, MainGame.tex_trapClosed.Height / 2), 1f, SpriteEffects.None, 1f);
			}

			foreach (ExplosionParticle p in particles)
				p.Draw(sb, c);
		}
	}
}
