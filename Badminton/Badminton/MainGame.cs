using System;
using System.Collections.Generic;
using System.Linq;

using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using Badminton.Screens;
using Badminton.Screens.Menus;
using Badminton.Screens.MultiPlayer;

namespace Badminton
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class MainGame : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;

		public const float METER_TO_PIXEL = 60f;
        public const float PIXEL_TO_METER = 1f / METER_TO_PIXEL;
	    public static float RESOLUTION_SCALE;
		GameScreen currentScreen;

		public static SpriteFont fnt_basicFont, fnt_bigFont, fnt_midFont;
		public static Texture2D tex_box, tex_heart, tex_clock, tex_wave, tex_longRange, tex_trapClosed, tex_trapOpen, tex_explosionParticle;
		public static Texture2D tex_head, tex_torso, tex_armUpper, tex_armLower, tex_legUpper, tex_legLower, tex_debugHead, tex_debugTorso, tex_debugLimb;
		public static Texture2D tex_evil_head, tex_evil_torso, tex_evil_armUpper, tex_evil_armLower, tex_evil_legUpper, tex_evil_legLower;
		public static Texture2D tex_bg_castle, tex_bg_pillar, tex_bg_octopus, tex_bg_graveyard, tex_bg_clocktower, tex_bg_circus, tex_fg_circus;
		public static Texture2D tex_blank;
		public static Texture2D tex_endGame;

		public static Texture2D tex_logo, tex_mm_coop, tex_mm_comp, tex_mm_cust, tex_mm_exit;
		public static Texture2D tex_ps_blank, tex_ps_controller, tex_ps_keyboard, tex_ps_off, tex_ps_next;
		public static Texture2D tex_btnUp, tex_btnDown;
		public static Texture2D tex_cbChecked, tex_cbUnchecked;

		public static SoundEffect[] sfx_punches;
		public static SoundEffect sfx_whoosh, sfx_shoot, sfx_explode, sfx_hit;
		public static Song mus_menu, mus_castle, mus_pillar, mus_octopus, mus_graveyard, mus_clocktower, mus_circus;

		public static Game mainGame;

		private bool escapePressed;

		public MainGame()
		{
			graphics = new GraphicsDeviceManager(this);
#if DEBUG
			RESOLUTION_SCALE = 0.667f;
#else
			RESOLUTION_SCALE = 1f; //0.9185185185f; //1f;
#endif
			graphics.PreferredBackBufferWidth = Math.Min(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, (int)(1920 * RESOLUTION_SCALE));
			graphics.PreferredBackBufferHeight = Math.Min(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height, (int)(1080 * RESOLUTION_SCALE));
#if DEBUG
			graphics.IsFullScreen = false;
#else
			graphics.IsFullScreen = true;
#endif
			IsMouseVisible = true;
			graphics.ApplyChanges();
			Content.RootDirectory = "Content";

			Map.MapKeys = new Dictionary<Texture2D, string>();

			mainGame = this;
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			escapePressed = true;

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			fnt_basicFont = Content.Load<SpriteFont>("fonts/basicFont");
			fnt_bigFont = Content.Load<SpriteFont>("fonts/bigFont");
			fnt_midFont = Content.Load<SpriteFont>("fonts/midFont");

			tex_bg_castle = Content.Load<Texture2D>("textures/castle");
			tex_bg_pillar = Content.Load<Texture2D>("textures/pillar");
			tex_bg_octopus = Content.Load<Texture2D>("textures/octopus");
			tex_bg_graveyard = Content.Load<Texture2D>("textures/graveyard");
			tex_bg_clocktower = Content.Load<Texture2D>("textures/clocktower");
			tex_bg_circus = Content.Load<Texture2D>("textures/circus");
			tex_fg_circus = Content.Load<Texture2D>("textures/circus foreground");

			tex_box = Content.Load<Texture2D>("textures/box");
			tex_heart = Content.Load<Texture2D>("textures/heart");
			tex_clock = Content.Load<Texture2D>("textures/clock");
			tex_head = Content.Load<Texture2D>("textures/stick figure/head");
			tex_armUpper = Content.Load<Texture2D>("textures/stick figure/armupper");
			tex_armLower = Content.Load<Texture2D>("textures/stick figure/armlower");
			tex_legUpper = Content.Load<Texture2D>("textures/stick figure/legupper");
			tex_legLower = Content.Load<Texture2D>("textures/stick figure/leglower");
			tex_torso = Content.Load<Texture2D>("textures/stick figure/torso");
			tex_evil_head = Content.Load<Texture2D>("textures/evil stick figure/head");
			tex_evil_armUpper = Content.Load<Texture2D>("textures/evil stick figure/armupper");
			tex_evil_armLower = Content.Load<Texture2D>("textures/evil stick figure/armlower");
			tex_evil_legUpper = Content.Load<Texture2D>("textures/evil stick figure/legupper");
			tex_evil_legLower = Content.Load<Texture2D>("textures/evil stick figure/leglower");
			tex_evil_torso = Content.Load<Texture2D>("textures/evil stick figure/torso");
			tex_debugHead = Content.Load<Texture2D>("textures/stick figure/debug head");
			tex_debugTorso = Content.Load<Texture2D>("textures/stick figure/debug torso");
			tex_debugLimb = Content.Load<Texture2D>("textures/stick figure/limb");
			tex_wave = Content.Load<Texture2D>("textures/force wave");
			tex_longRange = Content.Load<Texture2D>("textures/long range");
			tex_trapClosed = Content.Load<Texture2D>("textures/trap_closed");
			tex_trapOpen = Content.Load<Texture2D>("textures/trap_open");
			tex_explosionParticle = Content.Load<Texture2D>("textures/explosion particle");
			tex_endGame = Content.Load<Texture2D>("textures/end game");

			tex_cbChecked = Content.Load<Texture2D>("textures/menu assets/checkbox_checked");
			tex_cbUnchecked = Content.Load<Texture2D>("textures/menu assets/checkbox_unchecked");
			tex_logo = Content.Load<Texture2D>("textures/menu assets/logo");
			tex_mm_comp = Content.Load<Texture2D>("textures/menu assets/mm_competitive");
			tex_mm_coop = Content.Load<Texture2D>("textures/menu assets/mm_cooperative");
			tex_mm_cust = Content.Load<Texture2D>("textures/menu assets/mm_custom");
			tex_mm_exit = Content.Load<Texture2D>("textures/menu assets/mm_exit");

			tex_ps_blank = Content.Load<Texture2D>("textures/menu assets/ps_blank");
			tex_ps_controller = Content.Load<Texture2D>("textures/menu assets/ps_controller");
			tex_ps_keyboard = Content.Load<Texture2D>("textures/menu assets/ps_keyboard");
			tex_ps_off = Content.Load<Texture2D>("textures/menu assets/ps_off");
			tex_ps_next = Content.Load<Texture2D>("textures/menu assets/ps_next");

			tex_btnUp = Content.Load<Texture2D>("textures/menu assets/up");
			tex_btnDown = Content.Load<Texture2D>("textures/menu assets/down");

			sfx_punches = new SoundEffect[] {
				Content.Load<SoundEffect>("sfx/Punch1"),
				Content.Load<SoundEffect>("sfx/Punch2"),
				Content.Load<SoundEffect>("sfx/Thwap") };
			sfx_whoosh = Content.Load<SoundEffect>("sfx/Whoosh");
			sfx_hit = Content.Load<SoundEffect>("sfx/hit");
			sfx_shoot = Content.Load<SoundEffect>("sfx/shoot");
			sfx_explode = Content.Load<SoundEffect>("sfx/boom");
			mus_menu = Content.Load<Song>("menu music");
			mus_castle = Content.Load<Song>("castle music");
			mus_pillar = Content.Load<Song>("pillar music");
			mus_octopus = Content.Load<Song>("octopus music");
			mus_graveyard = Content.Load<Song>("castle music");
			mus_clocktower = Content.Load<Song>("octopus music");
			mus_circus = Content.Load<Song>("castle music");
			MediaPlayer.IsRepeating = true;
			MediaPlayer.Volume = 1f;
//			MediaPlayer.Volume = 0f;

			tex_blank = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
			tex_blank.SetData(new[] { Color.White });

			Map.MapKeys.Add(MainGame.tex_bg_castle, "castle");
			Map.MapKeys.Add(MainGame.tex_bg_pillar, "pillar");
			Map.MapKeys.Add(MainGame.tex_bg_octopus, "octopus");
			Map.MapKeys.Add(MainGame.tex_bg_graveyard, "graveyard");
			Map.MapKeys.Add(MainGame.tex_bg_clocktower, "clocktower");
			Map.MapKeys.Add(MainGame.tex_bg_circus, "circus");

//			currentScreen = new SingleMap();
//			currentScreen = new FreeForAll();
			currentScreen = new MainMenu();
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
//			if (Keyboard.GetState().IsKeyDown(Keys.Z))
//				this.TargetElapsedTime = TimeSpan.FromSeconds(0.3f);
//			else
//				this.TargetElapsedTime = TimeSpan.FromSeconds(0.016f);

			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
			{
				if (!escapePressed)
				{
					escapePressed = true;
					currentScreen = currentScreen == null ? null : currentScreen.GoBack();
				}
			}
			else
				escapePressed = false;

			currentScreen = currentScreen == null ? null : currentScreen.Update(gameTime);
			if (currentScreen == null)
				this.Exit();

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Multiply(Color.DarkGray, 0.7f));

			Matrix transformMatrix = Matrix.CreateScale(RESOLUTION_SCALE);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, null, null, null, null, transformMatrix);
			currentScreen.Draw(spriteBatch);
			spriteBatch.End();

			base.Draw(gameTime);
		}

		public static void DrawOutlineText(SpriteBatch sb, SpriteFont font, string text, Vector2 position, Color c)
		{
			sb.DrawString(font, text, position + Vector2.UnitX + Vector2.UnitY, Color.Black);
			sb.DrawString(font, text, position + Vector2.UnitX - Vector2.UnitY, Color.Black);
			sb.DrawString(font, text, position - Vector2.UnitX - Vector2.UnitY, Color.Black);
			sb.DrawString(font, text, position - Vector2.UnitX + Vector2.UnitY, Color.Black);
			sb.DrawString(font, text, position, c);
		}
	}
}
