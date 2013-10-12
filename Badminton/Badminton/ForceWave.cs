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

namespace Badminton
{
	class ForceWave
	{
		public Body body;
		private Category collisionCat;
		private World world;
		private int destroyTimer;

		public float Damage { get; set; }

		public ForceWave(World w, Vector2 position, Vector2 power, Category collisionCat)
		{
			this.world = w;
			body = BodyFactory.CreateRectangle(w, 8 * MainGame.PIXEL_TO_METER, 8 * MainGame.PIXEL_TO_METER, 10000f);
			body.Position = position;
			body.BodyType = BodyType.Dynamic;
			body.LinearVelocity = power;
			this.collisionCat = collisionCat;
			body.CollisionCategories = collisionCat;
			body.UserData = this;
			this.Damage = 0.2f;

			destroyTimer = 0;
		}

		public void Update()
		{
			if (body.UserData == null)
				return;

			body.ApplyForce(-Vector2.UnitY * body.Mass * 9.8f);
			destroyTimer++;
			if (destroyTimer >= 3)
				body.UserData = null;
		}

		public void Draw(SpriteBatch sb, Color c)
		{
			sb.Draw(MainGame.tex_wave, body.Position * MainGame.METER_TO_PIXEL, null, c, (float)Math.Atan2(body.LinearVelocity.Y, body.LinearVelocity.X), new Vector2(MainGame.tex_wave.Width / 2, MainGame.tex_wave.Height / 2), 0.5f, SpriteEffects.None, 1f);
		}
	}
}
