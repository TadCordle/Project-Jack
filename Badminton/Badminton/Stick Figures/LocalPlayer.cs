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

		private Keys upKey, rightKey, leftKey, downKey;
		private Buttons jumpBtn, rightBtn, leftBtn, crouchBtn, punchBtn, kickBtn, shootBtn;
		private bool punchBtnPressed, punchKeyPressed, kickBtnPressed, kickKeyPressed, shootBtnPressed, shootKeyPressed;
		private bool usesKeyboard;

		public LocalPlayer(World world, Vector2 position, Category collisionCat, float scale, Color color, PlayerIndex player)
			: base(world, position, collisionCat, scale, color)
		{
			this.player = player;
			punchBtnPressed = punchKeyPressed = false;
			kickBtnPressed = kickKeyPressed = false;
			shootBtnPressed = shootKeyPressed = false;
			usesKeyboard = !GamePad.GetState(player).IsConnected;

			jumpBtn = Buttons.A;
			rightBtn = Buttons.LeftThumbstickRight;
			leftBtn = Buttons.LeftThumbstickLeft;
			crouchBtn = Buttons.LeftTrigger;
			punchBtn = Buttons.X;
			kickBtn = Buttons.B;
			shootBtn = Buttons.RightTrigger;

			upKey = Keys.W;
			rightKey = Keys.D;
			leftKey = Keys.A;
			downKey = Keys.S;
		}

		public override void Update()
		{
			bool stand = true;

			// Jump
			if (usesKeyboard ? Keyboard.GetState().IsKeyDown(upKey) : GamePad.GetState(player).IsButtonDown(jumpBtn))
			{
				Jump();
				stand = false;
			}

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
					Aim((float)Math.Atan2(GamePad.GetState(player).ThumbSticks.Right.Y, GamePad.GetState(player).ThumbSticks.Right.X));
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
				}

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
