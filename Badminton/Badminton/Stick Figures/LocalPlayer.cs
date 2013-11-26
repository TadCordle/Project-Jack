using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;

namespace Badminton.Stick_Figures
{
	class LocalPlayer : StickFigure
	{
		private PlayerIndex player;

		private Keys upKey, rightKey, leftKey, downKey, trapKey;
		private Buttons jumpBtn, rightBtn, leftBtn, crouchBtn, punchBtn, kickBtn, shootBtn, trapBtn;
		private bool punchBtnPressed, punchKeyPressed, kickBtnPressed, kickKeyPressed, shootBtnPressed, shootKeyPressed, trapKeyPressed, trapBtnPressed;
		private bool usesKeyboard;
		private float lastShootAngle;

		private static Color[] colors = new Color[] { Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Cyan, Color.Magenta, Color.White };
		public static Color[] Colors { get { return colors; } }

		public LocalPlayer(World world, Vector2 position, Category collisionCat, float scale, Color color, PlayerIndex player)
			: base(world, position, collisionCat, scale, color)
		{
			this.player = player;
			punchBtnPressed = punchKeyPressed = false;
			kickBtnPressed = kickKeyPressed = false;
			shootBtnPressed = shootKeyPressed = false;
			trapBtnPressed = trapKeyPressed = false;
			usesKeyboard = !GamePad.GetState(player).IsConnected;
			lastShootAngle = 0f;

			jumpBtn = Buttons.A;
			rightBtn = Buttons.LeftThumbstickRight;
			leftBtn = Buttons.LeftThumbstickLeft;
			crouchBtn = Buttons.LeftTrigger;
			punchBtn = Buttons.X;
			kickBtn = Buttons.B;
			shootBtn = Buttons.RightTrigger;
			trapBtn = Buttons.Y;

			upKey = Keys.W;
			rightKey = Keys.D;
			leftKey = Keys.A;
			downKey = Keys.S;
			trapKey = Keys.T;
		}

		public override void Update()
		{
			bool stand = true;

			// Walk
			if (usesKeyboard ? Keyboard.GetState().IsKeyDown(rightKey) : GamePad.GetState(player).IsButtonDown(rightBtn))
			{
				WalkRight();
				stand = false;
			}
			else if (usesKeyboard ? Keyboard.GetState().IsKeyDown(leftKey) : GamePad.GetState(player).IsButtonDown(leftBtn))
			{
				WalkLeft();
				stand = false;
			}

			// Jump
			if (usesKeyboard ? Keyboard.GetState().IsKeyDown(upKey) : GamePad.GetState(player).IsButtonDown(jumpBtn))
			{
				Jumping = true;
				stand = false;
			}
			else
				Jumping = false;

			// Crouch
			if (usesKeyboard ? Keyboard.GetState().IsKeyDown(downKey) : GamePad.GetState(player).IsButtonDown(crouchBtn))
			{
				if (usesKeyboard ? !Keyboard.GetState().IsKeyDown(upKey) : !GamePad.GetState(player).IsButtonDown(jumpBtn))
				{
					Crouching = true;
					stand = false;
				}
			}
			else
				Crouching = false;

			if (usesKeyboard)
			{
				// Shoot
				if (Mouse.GetState().MiddleButton == ButtonState.Pressed)
				{
					shootKeyPressed = true;
					Aim((float)-Math.Atan2(Mouse.GetState().Y / MainGame.RESOLUTION_SCALE - torso.Position.Y * MainGame.METER_TO_PIXEL, 
										  Mouse.GetState().X / MainGame.RESOLUTION_SCALE - torso.Position.X * MainGame.METER_TO_PIXEL));
				}
				else
				{
					if (shootKeyPressed)
						LongRangeAttack();
					shootKeyPressed = false;
					
					// Punch
					if (Mouse.GetState().LeftButton == ButtonState.Pressed)
					{
						if (!punchKeyPressed)
						{
							Vector2 direction = Vector2.Zero;

							if (Keyboard.GetState().IsKeyDown(upKey))
								direction += Vector2.UnitY;
							if (Keyboard.GetState().IsKeyDown(downKey))
								direction -= Vector2.UnitY;
							if (Keyboard.GetState().IsKeyDown(leftKey))
								direction -= Vector2.UnitX;
							if (Keyboard.GetState().IsKeyDown(rightKey))
								direction += Vector2.UnitX;

							if (direction.Length() == 0)
							{
								if (LastFacedLeft)
									direction = -Vector2.UnitX;
								else
									direction = Vector2.UnitX;
							}

							punchKeyPressed = true;
							Punch((float)Math.Atan2(direction.Y, direction.X));
						}
					}
					else
						punchKeyPressed = false;

					if (Keyboard.GetState().IsKeyDown(trapKey))
					{
						if (!trapKeyPressed)
						{
							trapKeyPressed = true;
							float angle = (float)-Math.Atan2(Mouse.GetState().Y / MainGame.RESOLUTION_SCALE - torso.Position.Y * MainGame.METER_TO_PIXEL,
										  Mouse.GetState().X / MainGame.RESOLUTION_SCALE - torso.Position.X * MainGame.METER_TO_PIXEL);
							ThrowTrap(angle);
						}
					}
					else
						trapKeyPressed = false;
				}

				// Kick
				if (Mouse.GetState().RightButton == ButtonState.Pressed)
				{
					if (!kickKeyPressed)
					{
						Vector2 direction = Vector2.Zero;

						if (Keyboard.GetState().IsKeyDown(upKey))
							direction += Vector2.UnitY;
						if (Keyboard.GetState().IsKeyDown(downKey))
							direction -= Vector2.UnitY;
						if (Keyboard.GetState().IsKeyDown(leftKey))
							direction -= Vector2.UnitX;
						if (Keyboard.GetState().IsKeyDown(rightKey))
							direction += Vector2.UnitX;

						if (direction.Length() == 0)
						{
							if (LastFacedLeft)
								direction = -Vector2.UnitX;
							else
								direction = Vector2.UnitX;
						}

						kickKeyPressed = true;
						Kick((float)Math.Atan2(direction.Y, direction.X));
					}
				}
				else
					kickKeyPressed = false;
			}
			else
			{
				// Shoot
				if (GamePad.GetState(player).IsButtonDown(shootBtn))
				{
					shootBtnPressed = true;
					if (GamePad.GetState(player).ThumbSticks.Right.Length() > 0)
						lastShootAngle = (float)Math.Atan2(GamePad.GetState(player).ThumbSticks.Right.Y, GamePad.GetState(player).ThumbSticks.Right.X);
					else if (GamePad.GetState(player).ThumbSticks.Left.Length() > 0)
						lastShootAngle = (float)Math.Atan2(GamePad.GetState(player).ThumbSticks.Left.Y, GamePad.GetState(player).ThumbSticks.Left.X);
					Aim(lastShootAngle);
				}
				else
				{
					if (shootBtnPressed)
						LongRangeAttack();
					shootBtnPressed = false;

					// Punch
					if (GamePad.GetState(player).IsButtonDown(punchBtn))
					{
						if (!punchBtnPressed)
						{
							punchBtnPressed = true;
							float angle = (float)Math.Atan2(GamePad.GetState(player).ThumbSticks.Left.Y, GamePad.GetState(player).ThumbSticks.Left.X);
							if (angle == 0)
							{
								if (LastFacedLeft)
									Punch(MathHelper.Pi);
								else
									Punch(0);
							}
							else
								Punch(angle);
						}
					}
					else
						punchBtnPressed = false;

					if (GamePad.GetState(player).IsButtonDown(trapBtn))
					{
						if (!trapBtnPressed)
						{
							trapBtnPressed = true;
							float angle = (float)Math.Atan2(GamePad.GetState(player).ThumbSticks.Left.Y, GamePad.GetState(player).ThumbSticks.Left.X);
							if (angle == 0)
							{
								if (LastFacedLeft)
									ThrowTrap(MathHelper.Pi);
								else
									ThrowTrap(0);
							}
							else
								ThrowTrap(angle);
						}
					}
					else
						trapBtnPressed = false;
				}

				// Kick
				if (GamePad.GetState(player).IsButtonDown(kickBtn))
				{
					if (!kickBtnPressed)
					{
						kickBtnPressed = true;
						float angle = (float)Math.Atan2(GamePad.GetState(player).ThumbSticks.Left.Y, GamePad.GetState(player).ThumbSticks.Left.X);
						if (angle == 0)
						{
							if (LastFacedLeft)
								Kick(MathHelper.Pi);
							else
								Kick(0);
						}
						else
							Kick(angle);
					}
				}
				else
					kickBtnPressed = false;
			}

			if (stand)
				Stand();

			base.Update();
		}

		public override void Draw(SpriteBatch sb)
		{
			base.Draw(sb);
		}
	}
}
