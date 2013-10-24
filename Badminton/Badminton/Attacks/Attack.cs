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
	public class Attack
	{
		public Body PhysicsBody { get { return body; } }
		public float Damage { get { return damage; } }

		protected float damage;
		protected Body body;
		protected Category collisionCat;

		/// <summary>
		/// Called once every frame
		/// </summary>
		public virtual void Update() { }

		/// <summary>
		/// Draws the attack
		/// </summary>
		public virtual void Draw(SpriteBatch sb, Color c) { }
	}
}
