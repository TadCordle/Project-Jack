using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerPhysics.Dynamics;

namespace Badminton.Attacks
{
	interface Attack
	{
		Body PhysicsBody { get; }

		/// <summary>
		/// The damage dealt by the attack
		/// </summary>
		float Damage { get; }

		/// <summary>
		/// Called once every frame
		/// </summary>
		void Update();

		/// <summary>
		/// Draws the attack
		/// </summary>
		void Draw(SpriteBatch sb, Color c);
	}
}
