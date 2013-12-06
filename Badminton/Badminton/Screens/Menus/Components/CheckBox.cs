using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Badminton.Screens.Menus.Components
{
	public class CheckBox : Component
	{
		private string retString;
		public string ReturnString { get { return retString; } }

		private string text;

		public bool Checked { get; set; }

		public CheckBox(Vector2 position, string text, string retString)
			: base(position)
		{
			this.retString = retString;
			this.text = text;

			this.hitBox = new Rectangle((int)(position.X - 24), (int)(position.Y - 24), 48, 48);
		}

		public override void Draw(SpriteBatch sb)
		{
			sb.Draw(Checked ? MainGame.tex_cbChecked : MainGame.tex_cbUnchecked, position, null, Color.White, 0f, new Vector2(24, 24), scale, SpriteEffects.None, 1f);
			sb.DrawString(MainGame.fnt_basicFont, text, position + Vector2.UnitX * 40 - Vector2.UnitY * 15, Color.Black);
		}

		public static void UncheckAll(List<Component> comp)
		{
			var cbs = from Component c in comp
					  where c is CheckBox
					  select c as CheckBox;

			foreach (CheckBox cb in cbs)
				cb.Checked = false;
		}
	}
}
