using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using FarseerPhysics.Dynamics;

using Badminton.Screens.Menus.Components;

namespace Badminton.Screens.Menus
{
	class PlayerSelect : GameScreen
	{
		public int Mode { get; set; }

		List<PlayerSelectBox> boxes;
		World world;

		public PlayerSelect(int mode)
		{
			world = new World(Vector2.Zero);
			
			this.Mode = mode;
			boxes = new List<PlayerSelectBox>();
			boxes.Add(new PlayerSelectBox(new Vector2(470, 145), PlayerIndex.One, Color.Red));
			boxes.Add(new PlayerSelectBox(new Vector2(970, 145), PlayerIndex.Two, Color.Green));
			boxes.Add(new PlayerSelectBox(new Vector2(470, 550), PlayerIndex.Three, Color.Cyan));
			boxes.Add(new PlayerSelectBox(new Vector2(970, 550), PlayerIndex.Four, Color.Yellow));
		}

		public GameScreen Update(GameTime time)
		{
			foreach (PlayerSelectBox box in boxes)
				box.Update(world);

			world.Step((float)time.ElapsedGameTime.TotalSeconds);
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
