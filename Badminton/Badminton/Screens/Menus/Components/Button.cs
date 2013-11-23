using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Badminton.Screens.Menus.Components
{
	class Button
	{
		private const float MAX_SCALE = 1.2f;
		private const float MIN_SCALE = 1f;

		public bool Selected { get; set; }

		private string retString;
		public string ReturnString { get { return retString; } }

		private float scale;
		private Vector2 position;
		private Texture2D texture;
		private Rectangle hitBox;

		public Button(Vector2 position, Texture2D texture, string retString)
		{
			this.retString = retString;
			this.scale = MIN_SCALE;
			this.position = position;
			this.texture = texture;

			hitBox = new Rectangle((int)position.X - texture.Width / 2, (int)position.Y - texture.Height / 2, texture.Width, texture.Height);
		}

		public void Update()
		{
			if (Selected)
			{
				if (scale < MAX_SCALE)
				{
					scale += 0.02f;
					hitBox = new Rectangle((int)(position.X - texture.Width / 2 * scale), (int)(position.Y - texture.Height / 2 * scale), (int)(texture.Width * scale), (int)(texture.Height * scale));
				}
			}
			else
			{
				if (scale > MIN_SCALE)
				{
					scale -= 0.02f;
					hitBox = new Rectangle((int)(position.X - texture.Width / 2 * scale), (int)(position.Y - texture.Height / 2 * scale), (int)(texture.Width * scale), (int)(texture.Height * scale));
				}
			}
		}

		public bool IsMouseOver()
		{
			return this.hitBox.Contains((int)(Mouse.GetState().X / MainGame.RESOLUTION_SCALE), (int)(Mouse.GetState().Y / MainGame.RESOLUTION_SCALE));
		}

		public void Draw(SpriteBatch sb)
		{
			sb.Draw(texture, position, null, Color.White, 0f, new Vector2(texture.Width / 2, texture.Height / 2), scale, SpriteEffects.None, 1f);
		}

		public static void UpdateButtons(List<Button> buttons)
		{
			foreach (Button b in buttons)
			{
				b.Update();
				if (b.IsMouseOver())
				{
					GetSelectedButton(buttons).Selected = false;
					b.Selected = true;
				}
			}
		}

		public static Button GetSelectedButton(List<Button> buttons)
		{
			foreach (Button b in buttons)
				if (b.Selected)
					return b;

			// If no buttons are selected, select the first one
			buttons[0].Selected = true;
			return buttons[0];
		}
	}
}
