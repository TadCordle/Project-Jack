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
	class ForceWave : Attack
	{
		private const float DAMAGE = 0.2f;
		private int destroyTimer;

		public ForceWave(World w, Vector2 position, Vector2 power, Category collisionCat)
		{
			body = BodyFactory.CreateRectangle(w, 16f * MainGame.PIXEL_TO_METER, 16f * MainGame.PIXEL_TO_METER, 5000f);
			body.BodyType = BodyType.Dynamic;
			body.Position = position;
			body.LinearVelocity = power;
			this.collisionCat = collisionCat;
			body.CollisionCategories = collisionCat;
			body.UserData = this;
			this.damage = DAMAGE;
			destroyTimer = 0;
		}

		public override void Update()
		{
			if (body.UserData == null)
				return;

			body.ApplyForce(-Vector2.UnitY * body.Mass * 9.8f);
			destroyTimer++;
			if (destroyTimer >= 4)
				body.UserData = null;
		}

		public override void Draw(SpriteBatch sb, Color c)
		{
			sb.Draw(MainGame.tex_wave, body.Position * MainGame.METER_TO_PIXEL, null, c, (float)Math.Atan2(body.LinearVelocity.Y, body.LinearVelocity.X), new Vector2(MainGame.tex_wave.Width / 2, MainGame.tex_wave.Height / 2), 0.5f, SpriteEffects.None, 1f);
		}
	}
}
