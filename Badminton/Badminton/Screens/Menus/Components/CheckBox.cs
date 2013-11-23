using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Badminton.Screens.Menus.Components
{
	class CheckBox
	{
		private const float MAX_SCALE = 1.2f;
		private const float MIN_SCALE = 1f;

		public bool Selected { get; set; }

		private string retString;
		public string ReturnString { get { return retString; } }

		private float scale;
		private Vector2 position;
		private string text;
		private Rectangle hitBox;

		public bool Checked { get; set; }

		public CheckBox(Vector2 position, string text, string retString)
		{
			this.retString = retString;
			this.scale = MIN_SCALE;
			this.position = position;
			this.text = text;

			this.hitBox = new Rectangle((int)(position.X - 24), (int)(position.Y - 24), 48, 48);
		}

		public void Update()
		{
			if (Selected)
			{
				if (scale < MAX_SCALE)
				{
					scale += 0.02f;
					hitBox = new Rectangle((int)(position.X - 24 * scale), (int)(position.Y - 24 * scale), (int)(48 * scale), (int)(48 * scale));
				}
			}
			else
			{
				if (scale > MIN_SCALE)
				{
					scale -= 0.02f;
					hitBox = new Rectangle((int)(position.X - 24 * scale), (int)(position.Y - 24 * scale), (int)(48 * scale), (int)(48 * scale));
				}
			}
		}

		public bool IsMouseOver()
		{
			return this.hitBox.Contains((int)(Mouse.GetState().X / MainGame.RESOLUTION_SCALE), (int)(Mouse.GetState().Y / MainGame.RESOLUTION_SCALE));
		}

		public void Draw(SpriteBatch sb)
		{
			sb.Draw(Checked ? MainGame.tex_cbChecked : MainGame.tex_cbUnchecked, position, null, Color.White, 0f, new Vector2(24, 24), scale, SpriteEffects.None, 1f);
			sb.DrawString(MainGame.fnt_basicFont, text, position + Vector2.UnitX * 40 - Vector2.UnitY * 15, Color.Black);
		}

		public static void UpdateCheckboxes(List<CheckBox> cbs)
		{
			foreach (CheckBox cb in cbs)
			{
				cb.Update();
				if (cb.IsMouseOver())
				{
					if (GetSelectedCheckBox(cbs) != null)
						GetSelectedCheckBox(cbs).Selected = false;
					cb.Selected = true;
				}
			}
		}

		public static CheckBox GetSelectedCheckBox(List<CheckBox> cbs)
		{
			foreach (CheckBox cb in cbs)
				if (cb.Selected)
					return cb;
			return null;
		}

		public static void UncheckAll(List<CheckBox> cbs)
		{
			foreach (CheckBox cb in cbs)
				cb.Checked = false;
		}
	}
}
