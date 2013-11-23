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

		public static SpriteFont fnt_basicFont;
		public static Texture2D tex_box, tex_wave, tex_longRange, tex_trapClosed, tex_trapOpen, tex_explosionParticle;
		public static Texture2D tex_head, tex_torso, tex_armUpper, tex_armLower, tex_legUpper, tex_legLower, tex_debugHead, tex_debugTorso, tex_debugLimb;
		public static Texture2D tex_bg_castle;
		public static Texture2D tex_blank;

		public static Texture2D tex_logo, tex_mm_coop, tex_mm_comp, tex_mm_cust, tex_mm_exit;
		public static Texture2D tex_ps_blank, tex_ps_controller, tex_ps_keyboard, tex_ps_off, tex_ps_next;
		public static Texture2D tex_cbChecked, tex_cbUnchecked;

		public static SoundEffect[] sfx_punches;
		public static SoundEffect sfx_whoosh;
		public static Song music;

		public static Game mainGame;

		private bool escapePressed;

		public MainGame()
		{
			graphics = new GraphicsDeviceManager(this);
			
			RESOLUTION_SCALE = 0.667f;
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

			tex_bg_castle = Content.Load<Texture2D>("textures/background");
			tex_box = Content.Load<Texture2D>("textures/box");
			tex_head = Content.Load<Texture2D>("textures/stick figure/head");
			tex_armUpper = Content.Load<Texture2D>("textures/stick figure/armupper");
			tex_armLower = Content.Load<Texture2D>("textures/stick figure/armlower");
			tex_legUpper = Content.Load<Texture2D>("textures/stick figure/legupper");
			tex_legLower = Content.Load<Texture2D>("textures/stick figure/leglower");
			tex_torso = Content.Load<Texture2D>("textures/stick figure/torso");
			tex_debugHead = Content.Load<Texture2D>("textures/stick figure/debug head");
			tex_debugTorso = Content.Load<Texture2D>("textures/stick figure/debug torso");
			tex_debugLimb = Content.Load<Texture2D>("textures/stick figure/limb");
			tex_wave = Content.Load<Texture2D>("textures/force wave");
			tex_longRange = Content.Load<Texture2D>("textures/long range");
			tex_trapClosed = Content.Load<Texture2D>("textures/trap_closed");
			tex_trapOpen = Content.Load<Texture2D>("textures/trap_open");
			tex_explosionParticle = Content.Load<Texture2D>("textures/explosion particle");

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

			sfx_punches = new SoundEffect[] {
				Content.Load<SoundEffect>("sfx/Punch1"),
				Content.Load<SoundEffect>("sfx/Punch2"),
				Content.Load<SoundEffect>("sfx/Thwap") };
			sfx_whoosh = Content.Load<SoundEffect>("sfx/Whoosh");
			music = Content.Load<Song>("music");
			MediaPlayer.IsRepeating = true;
//			MediaPlayer.Play(music);

			tex_blank = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
			tex_blank.SetData(new[] { Color.White });

//			currentScreen = new SingleMap();
//          currentScreen = new FreeForAll();
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
	}
}
