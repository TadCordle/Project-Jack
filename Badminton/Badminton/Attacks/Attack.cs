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
		protected World world;
		protected Body body;
		protected Category collisionCat;

		public Attack(World w, Category collisionCat, float damage)
		{
			this.world = w;
			this.collisionCat = collisionCat;
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
