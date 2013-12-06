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

		List<Button> upDowns;
		List<CheckBox> checkBoxes;
		List<Texture2D> maps;

		public SettingsScreen(PlayerSelect prevScreen)
		{
			this.prevScreen = prevScreen;
			backPressed = true;
			confirmPressed = true;
			mouseClicked = true;
			upPressed = true;
			downPressed = true;

			maps = new List<Texture2D>();
			maps.Add(MainGame.tex_bg_castle);

			upDowns = new List<Button>();
			checkBoxes = new List<CheckBox>();
		}

		public GameScreen Update(GameTime time)
		{

			return this;
		}

		public GameScreen GoBack()
		{
			return prevScreen;
		}

		public void Draw(SpriteBatch sb)
		{
		}
	}
}
