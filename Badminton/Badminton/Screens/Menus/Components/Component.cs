using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Badminton.Screens.Menus.Components
{
	public abstract class Component
	{
		protected const float MAX_SCALE = 1.2f;
		protected const float MIN_SCALE = 1f;

		public bool Selected { get; set; }

		private string retString;
		public string ReturnString { get { return retString; } }

		protected float scale;
		protected Rectangle hitBox;
		protected Vector2 position;

		public Component(Vector2 position, string retString)
		{
			this.retString = retString;
			this.scale = MIN_SCALE;
			this.position = position;
		}

		public static void UpdateSelection(List<Component> comp)
		{
			foreach (Component c in comp)
			{
				c.Update();
//				if (c.IsMouseOver())
//				{
//					GetSelectedComponent(comp).Selected = false;
//					c.Selected = true;
//				}
			}
		}

		public static Component GetSelectedComponent(List<Component> comp)
		{
			foreach (Component c in comp)
				if (c.Selected)
					return c;

			comp[0].Selected = true;
			return comp[0];
		}

		public bool IsMouseOver()
		{
			return this.hitBox.Contains((int)(Mouse.GetState().X / MainGame.RESOLUTION_SCALE), (int)(Mouse.GetState().Y / MainGame.RESOLUTION_SCALE));
		}

		public void Update()
		{
			if (Selected)
			{
				if (scale < MAX_SCALE)
				{
					scale += 0.02f;
					hitBox = new Rectangle((int)(position.X - hitBox.Width / 2 * scale), (int)(position.Y - hitBox.Height / 2 * scale), (int)(hitBox.Width * scale), (int)(hitBox.Height * scale));
				}
			}
			else
			{
				if (scale > MIN_SCALE)
				{
					scale -= 0.02f;
					hitBox = new Rectangle((int)(position.X - hitBox.Width / 2 * scale), (int)(position.Y - hitBox.Height / 2 * scale), (int)(hitBox.Width * scale), (int)(hitBox.Height * scale));
				}
			}
		}

		public abstract void Draw(SpriteBatch sb);
	}
}
