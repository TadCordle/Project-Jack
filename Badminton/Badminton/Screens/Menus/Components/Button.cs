using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Badminton.Screens.Menus.Components
{
	public class Button : Component
	{
		private Texture2D texture;

		public Button(Vector2 position, Texture2D texture, string retString)
			: base(position, retString)
		{
			this.texture = texture;

			hitBox = new Rectangle((int)position.X - texture.Width / 2, (int)position.Y - texture.Height / 2, texture.Width, texture.Height);
		}

		public override void Draw(SpriteBatch sb)
		{
			sb.Draw(texture, position, null, Color.White, 0f, new Vector2(texture.Width / 2, texture.Height / 2), scale, SpriteEffects.None, 1f);
		}
	}
}
