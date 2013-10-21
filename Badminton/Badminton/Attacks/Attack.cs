using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

namespace Badminton.Attacks
{
	public abstract class Attack
	{
		public Body PhysicsBody { get { return body; } }
		public float Damage { get { return damage; } }

		protected float damage;
		protected Body body;
		protected Category collisionCat;

		public Attack(World w, Vector2 position, Vector2 power, float damage, Category collisionCat)
		{
			body = BodyFactory.CreateRectangle(w, 100f * MainGame.PIXEL_TO_METER, 16f * MainGame.PIXEL_TO_METER, 5000f);
			body.BodyType = BodyType.Dynamic;
			body.Position = position;
			body.LinearVelocity = power;
			this.collisionCat = collisionCat;
			body.CollisionCategories = collisionCat;
			body.UserData = this;
			this.damage = damage;
		}

		/// <summary>
		/// Called once every frame
		/// </summary>
		public abstract void Update();

		/// <summary>
		/// Draws the attack
		/// </summary>
		public abstract void Draw(SpriteBatch sb, Color c);
	}
}
