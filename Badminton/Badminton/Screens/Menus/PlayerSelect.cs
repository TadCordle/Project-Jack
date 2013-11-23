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

		bool backPressed;

		public PlayerSelect(int mode)
		{
			world = new World(Vector2.Zero);
			backPressed = true;
			
			this.Mode = mode;
			boxes = new List<PlayerSelectBox>();
			boxes.Add(new PlayerSelectBox(new Vector2(270, 145), PlayerIndex.One, 0));
			boxes.Add(new PlayerSelectBox(new Vector2(770, 145), PlayerIndex.Two, 1));
			boxes.Add(new PlayerSelectBox(new Vector2(270, 550), PlayerIndex.Three, 2));
			boxes.Add(new PlayerSelectBox(new Vector2(770, 550), PlayerIndex.Four, 3));
		}

		public GameScreen Update(GameTime time)
		{
			foreach (PlayerSelectBox box in boxes)
				box.Update(world);

			if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.B))
			{
				if (!backPressed)
				{
					backPressed = true;
					return this.GoBack();
				}
			}
			else
				backPressed = false;

			world.Step((float)time.ElapsedGameTime.TotalSeconds);
			return this;
		}

		public GameScreen GoBack()
		{
			foreach (PlayerSelectBox p in boxes)
				if (!p.CanExit())
					return this;

			return new MainMenu();
		}

		public void Draw(SpriteBatch sb)
		{
			foreach (PlayerSelectBox box in boxes)
				box.Draw(sb);
		}
	}
}
