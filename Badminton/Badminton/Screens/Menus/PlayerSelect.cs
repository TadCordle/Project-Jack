using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Badminton.Screens.Menus.Components;

namespace Badminton.Screens.Menus
{
	class PlayerSelect : GameScreen
	{
		public int Mode { get; set; }

		List<PlayerSelectBox> boxes;

		public PlayerSelect(int mode)
		{
			this.Mode = mode;
			boxes = new List<PlayerSelectBox>();
			boxes.Add(new PlayerSelectBox(new Vector2(470, 145), PlayerIndex.One));
			boxes.Add(new PlayerSelectBox(new Vector2(970, 145), PlayerIndex.Two));
			boxes.Add(new PlayerSelectBox(new Vector2(470, 550), PlayerIndex.Three));
			boxes.Add(new PlayerSelectBox(new Vector2(970, 550), PlayerIndex.Four));
		}

		public GameScreen Update(GameTime time)
		{
			foreach (PlayerSelectBox box in boxes)
				box.Update();

			return this;
		}

		public GameScreen Exit()
		{
			return new MainMenu();
		}

		public void Draw(SpriteBatch sb)
		{
			foreach (PlayerSelectBox box in boxes)
				box.Draw(sb);
		}
	}
}
