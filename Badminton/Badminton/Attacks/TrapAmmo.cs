using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;

namespace Badminton.Attacks
{
	class TrapAmmo
	{
		private World world;
		private Body body;

		private int maxRespawnTime;
		private int respawnTime;
		private Vector2 respawnPosition;

		private float yVal;

		public TrapAmmo(World w, Vector2 position, int respawnTime)
		{
			this.world = w;
			body = BodyFactory.CreateCircle(world, 16 * MainGame.PIXEL_TO_METER, 1f);
			body.BodyType = BodyType.Static;
			body.CollisionCategories = Category.All;
			body.Position = Vector2.UnitX * -10000 + Vector2.UnitY * position.Y;
			this.respawnPosition = position;
			this.maxRespawnTime = respawnTime;
			this.respawnTime = maxRespawnTime;
			body.UserData = this;
			yVal = 0;
		}

		public void PickUp()
		{
			body.Position = Vector2.UnitX * -10000 + Vector2.UnitY * body.Position.Y;
			respawnTime = maxRespawnTime;
		}

		public void Update()
		{
			respawnTime--;
			if (respawnTime == 0)
				body.Position = this.respawnPosition;

			// Do floaty thing
			yVal += 0.1f;
			body.Position = Vector2.UnitX * body.Position.X + Vector2.UnitY * ((float)Math.Sin(yVal) * 0.05f + respawnPosition.Y);
		}

		public void Draw(SpriteBatch sb)
		{
			sb.Draw(MainGame.tex_trapClosed, body.Position * MainGame.METER_TO_PIXEL, null, Color.White, body.Rotation, new Vector2(MainGame.tex_trapClosed.Width / 2, MainGame.tex_trapClosed.Height / 2), 1f, SpriteEffects.None, 1f);
		}
	}
}
