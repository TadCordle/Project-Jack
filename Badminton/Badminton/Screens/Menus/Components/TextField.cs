using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Badminton.Screens.Menus.Components
{
	class TextField
	{
		private string text;
		public string Text { get { return text; } }

		private Vector2 position;

		public TextField(string defaultText, Vector2 position)
		{
			this.text = "";
			this.position = position;
		}

		public void Update()
		{

		}

		public bool IsMouseOver()
		{
			return false;
		}

		public void Draw(SpriteBatch sb)
		{

		}
	}
}
