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
		bool upPressed, downPressed, leftPressed, rightPressed;

		float gravity;
		float limbStrength;
		int timeLimit;
		int lives;

		List<Component> components;
		List<Texture2D> maps;
		Dictionary<string, bool> checkValues;
		int selectedComponent, selectedMap;

		public SettingsScreen(PlayerSelect prevScreen)
		{
			this.prevScreen = prevScreen;
			upPressed = true;
			downPressed = true;
			leftPressed = true;
			rightPressed = true;

			gravity = 9.8f;
			limbStrength = 1f;
			timeLimit = 0;
			lives = 5;

			selectedComponent = 0;
			selectedMap = 0;

			maps = new List<Texture2D>();
			maps.Add(MainGame.tex_bg_castle);

			checkValues = new Dictionary<string, bool>();
			components = new List<Component>();
			components.Add(new Button(new Vector2(740, 350), maps[selectedMap], "map"));
			components.Add(new Button(new Vector2(800, 765), MainGame.tex_btnUp, "grav"));
			components.Add(new Button(new Vector2(800, 845), MainGame.tex_btnUp, "limb"));
			components.Add(new Button(new Vector2(800, 925), MainGame.tex_btnUp, "time"));
			components.Add(new Button(new Vector2(800, 1005), MainGame.tex_btnUp, "lives"));
			components.Add(new CheckBox(new Vector2(1050, 765), "Sudden death mode", "death"));
			checkValues.Add("death", false);
			components.Add(new CheckBox(new Vector2(1050, 845), "Allow ranged attacks", "ranged"));
			checkValues.Add("ranged", false);
			components.Add(new CheckBox(new Vector2(1050, 925), "Allow mines", "traps"));
			checkValues.Add("traps", false);
			components.Add(new CheckBox(new Vector2(1050, 1005), "Fill empty slots with bots", "bots"));
			checkValues.Add("bots", false);
			components[0].Selected = true;

			((CheckBox)components[6]).Checked = true;
			checkValues["ranged"] = true;
			((CheckBox)components[7]).Checked = true;
			checkValues["traps"] = true;
			if (prevScreen.Colors.Length == 1)
			{
				checkValues["bots"] = true;
				((CheckBox)components[8]).Checked = true;
			}
		}

		public GameScreen Update(GameTime time)
		{
			Component.UpdateSelection(components);
			if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.B))
				return GoBack();

			if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftThumbstickLeft) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadLeft) || Keyboard.GetState().IsKeyDown(Keys.Left))
				UpdateSelection(false, ref leftPressed);
			else
				leftPressed = false;

			if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftThumbstickRight) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadRight) || Keyboard.GetState().IsKeyDown(Keys.Right))
				UpdateSelection(true, ref rightPressed);
			else
				rightPressed = false;

			if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftThumbstickUp) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadUp) || Keyboard.GetState().IsKeyDown(Keys.Up))
			{
				if (!upPressed)
				{
					upPressed = true;
					selectedComponent = selectedComponent == 0 ? components.Count - 1 : selectedComponent - 1;
					Component.GetSelectedComponent(components).Selected = false;
					components[selectedComponent].Selected = true;
				}
			}
			else
				upPressed = false;

			if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftThumbstickDown) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadDown) || Keyboard.GetState().IsKeyDown(Keys.Down))
			{
				if (!downPressed)
				{
					downPressed = true;
					selectedComponent = selectedComponent == components.Count - 1 ? 0 : selectedComponent + 1;
					Component.GetSelectedComponent(components).Selected = false;
					components[selectedComponent].Selected = true;
				}
			}
			else
				downPressed = false;

			if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Start) || Keyboard.GetState().IsKeyDown(Keys.Enter))
			{

				switch (prevScreen.Mode)
				{
					// Pass parameters eventually
					case -1:
						return new FreeForAll(prevScreen.Colors, Map.MapKeys[maps[selectedMap]], gravity, timeLimit, lives, limbStrength, checkValues["death"], checkValues["traps"], checkValues["ranged"], checkValues["bots"]);
					case 0:
						return new FreeForAll(prevScreen.Colors, Map.MapKeys[maps[selectedMap]], gravity, timeLimit, lives, limbStrength, checkValues["death"], checkValues["traps"], checkValues["ranged"], checkValues["bots"]);
					case 1:
						return new Match_TDM();
					case 2:
						return new OneVsAll();
				}
			}

			if (prevScreen.Colors.Length == 1)
			{
				((CheckBox)components[8]).Checked = true;
				checkValues["bots"] = true;
			}

			return this;
		}

		private void UpdateSelection(bool positive, ref bool confirmPressed)
		{
			int delta = positive ? 1 : -1;
			string s = Component.GetSelectedComponent(components).ReturnString;

			if (!confirmPressed)
			{
				confirmPressed = true;

				if (s == "map")
				{
					selectedMap = selectedMap + delta;
					if (selectedMap < 0)
						selectedMap = maps.Count - 1;
					else if (selectedMap >= maps.Count)
						selectedMap = 0;
				}
				else if (s == "time")
					timeLimit = timeLimit + delta > 10 ? 10 : (timeLimit + delta < 0 ? 0 : timeLimit + delta);
				else if (s == "lives")
					lives = lives + delta > 10 ? 10 : (lives + delta < 1 ? 1 : lives + delta);
				else if (s == "death" || s == "ranged" || s == "traps" || s == "bots")
				{
					((CheckBox)Component.GetSelectedComponent(components)).Checked = !((CheckBox)Component.GetSelectedComponent(components)).Checked;
					checkValues[s] = ((CheckBox)Component.GetSelectedComponent(components)).Checked;
				}
			}

			if (s == "grav")
				gravity = gravity + 0.04f * delta > 12 ? 12 : (gravity + 0.04f * delta < 0 ? 0 : gravity + 0.04f * delta);
			else if (s == "limb")
				limbStrength = limbStrength + 0.01f * delta > 1 ? 1 : (limbStrength + 0.01f * delta < 0 ? 0 : limbStrength + 0.01f * delta);
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
			sb.DrawString(MainGame.fnt_basicFont, ((int)(limbStrength * 100)).ToString() + "%", new Vector2(730, 830), Color.Black);
			sb.DrawString(MainGame.fnt_basicFont, "Time limit:", new Vector2(540, 910), Color.Black);
			sb.DrawString(MainGame.fnt_basicFont, timeLimit.ToString(), new Vector2(730, 910), Color.Black);
			sb.DrawString(MainGame.fnt_basicFont, "Lives:", new Vector2(540, 990), Color.Black);
			sb.DrawString(MainGame.fnt_basicFont, lives.ToString(), new Vector2(730, 990), Color.Black);
			foreach (Component c in components)
				if (c.ReturnString != "map")
					c.Draw(sb);
		}
	}
}
