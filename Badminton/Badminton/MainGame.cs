﻿using System;
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
		public static Vector2 RESOLUTION_SCALE = new Vector2(0.5f, 0.5f);

		Screens.GameScreen currentScreen;

		public static SpriteFont fnt_basicFont;
		public static Texture2D tex_box, tex_wave, tex_longRange;
		public static Texture2D tex_head, tex_torso, tex_limb;
		public static Texture2D tex_bg;

		public static SoundEffect[] sfx_punches;
		public static SoundEffect sfx_whoosh;
		public static Song music;

		private bool escapePressed;

		public MainGame()
		{
			graphics = new GraphicsDeviceManager(this);
            // Not all monitors can support 1920x1080 resolution
            graphics.PreferredBackBufferWidth = Math.Min(1920, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width);
            graphics.PreferredBackBufferHeight = Math.Min(1080, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
            RESOLUTION_SCALE.X = graphics.PreferredBackBufferWidth / 1920f;
			RESOLUTION_SCALE.Y = graphics.PreferredBackBufferHeight / 1080f;
			graphics.IsFullScreen = true;
			IsMouseVisible = true;
			graphics.ApplyChanges();
			Content.RootDirectory = "Content";
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

			tex_bg = Content.Load<Texture2D>("textures/background");
			tex_box = Content.Load<Texture2D>("textures/box");
			tex_head = Content.Load<Texture2D>("textures/stick figure/head");
			tex_limb = Content.Load<Texture2D>("textures/stick figure/limb");
			tex_torso = Content.Load<Texture2D>("textures/stick figure/torso");
			tex_wave = Content.Load<Texture2D>("textures/force wave");
			tex_longRange = Content.Load<Texture2D>("textures/long range");

			sfx_punches = new SoundEffect[] {
				Content.Load<SoundEffect>("sfx/Punch1"),
				Content.Load<SoundEffect>("sfx/Punch2"),
				Content.Load<SoundEffect>("sfx/Thwap") };
			sfx_whoosh = Content.Load<SoundEffect>("sfx/Whoosh");
			music = Content.Load<Song>("music");
			MediaPlayer.IsRepeating = true;
//			MediaPlayer.Play(music);

			currentScreen = new Screens.SingleMap();
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
					currentScreen = currentScreen == null ? null : currentScreen.Exit();
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

			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
			currentScreen.Draw(spriteBatch);
			spriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
