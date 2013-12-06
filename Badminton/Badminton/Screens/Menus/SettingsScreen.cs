using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Badminton.Screens.MultiPlayer;
using Badminton.Screens.Menus.Components;

namespace Badminton.Screens.Menus
{
	class SettingsScreen : GameScreen
	{
		private PlayerSelect prevScreen;
		bool backPressed, confirmPressed, mouseClicked, upPressed, downPressed;

		float gravity;
		float limbStrength;
		float timeLimit;
		int lives;

		List<Component> components;
		List<Texture2D> maps;
		int selectedMap;

		public SettingsScreen(PlayerSelect prevScreen)
		{
			this.prevScreen = prevScreen;
			backPressed = true;
			confirmPressed = true;
			mouseClicked = true;
			upPressed = true;
			downPressed = true;

			gravity = 9.8f;
			limbStrength = 1f;
			timeLimit = 0;
			lives = 5;

			selectedMap = 0;

			maps = new List<Texture2D>();
			maps.Add(MainGame.tex_bg_castle);

			components = new List<Component>();
			components.Add(new Button(new Vector2(800, 750), MainGame.tex_btnUp, "grav_up"));
			components.Add(new Button(new Vector2(800, 782), MainGame.tex_btnDown, "grav_down"));
			components.Add(new Button(new Vector2(800, 830), MainGame.tex_btnUp, "limb_up"));
			components.Add(new Button(new Vector2(800, 862), MainGame.tex_btnDown, "limb_down"));
			components.Add(new Button(new Vector2(800, 910), MainGame.tex_btnUp, "time_up"));
			components.Add(new Button(new Vector2(800, 942), MainGame.tex_btnDown, "time_down"));
			components.Add(new Button(new Vector2(800, 990), MainGame.tex_btnUp, "lives_up"));
			components.Add(new Button(new Vector2(800, 1022), MainGame.tex_btnDown, "lives_down"));
			components.Add(new CheckBox(new Vector2(1050, 765), "Sudden death mode", "death"));
			components.Add(new CheckBox(new Vector2(1050, 845), "Allow ranged attacks", "ranged"));
			components.Add(new CheckBox(new Vector2(1050, 925), "Allow mines", "traps"));
			components.Add(new CheckBox(new Vector2(1050, 1005), "Fill empty slots with bots", "bots"));
			components[0].Selected = true;
		}

		public GameScreen Update(GameTime time)
		{
			Component.UpdateSelection(components);

			if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start))
			{
				switch (prevScreen.Mode)
				{
					case -1:
						return new SingleMap();
					case 0:
						return new Match_TDM();
					case 1:
						return new FreeForAll();
					case 2:
						return new OneVsAll();
				}
			}
			return this;
		}

		public GameScreen GoBack()
		{
			return prevScreen;
		}

		public void Draw(SpriteBatch sb)
		{
			sb.Draw(maps[selectedMap], new Rectangle(420, 100, 1080, 600), Color.White);

			sb.DrawString(MainGame.fnt_basicFont, "Gravity:", new Vector2(540, 750), Color.Black);
			sb.DrawString(MainGame.fnt_basicFont, gravity.ToString(), new Vector2(730, 750), Color.Black);
			sb.DrawString(MainGame.fnt_basicFont, "Limb strength:", new Vector2(540, 830), Color.Black);
			sb.DrawString(MainGame.fnt_basicFont, (limbStrength * 100).ToString() + "%", new Vector2(730, 830), Color.Black);
			sb.DrawString(MainGame.fnt_basicFont, "Time limit:", new Vector2(540, 910), Color.Black);
			sb.DrawString(MainGame.fnt_basicFont, timeLimit.ToString(), new Vector2(730, 910), Color.Black);
			sb.DrawString(MainGame.fnt_basicFont, "Lives:", new Vector2(540, 990), Color.Black);
			sb.DrawString(MainGame.fnt_basicFont, lives.ToString(), new Vector2(730, 990), Color.Black);
			foreach (Component c in components)
				c.Draw(sb);
		}
	}
}
