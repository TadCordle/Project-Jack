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

		private Keys upKey, rightKey, leftKey, downKey, punchKey, kickKey;
		private Buttons jumpBtn, rightBtn, leftBtn, crouchBtn, punchBtn, kickBtn;
		private bool punchBtnPressed, punchKeyPressed, kickBtnPressed, kickKeyPressed;

		public LocalPlayer(World world, Vector2 position, Category collisionCat, float scale, Color color, PlayerIndex player)
			: base(world, position, collisionCat, scale, color)
		{
			this.player = player;
            //hascontroller = GamePad.GetState(player).IsConnected;
			punchBtnPressed = punchKeyPressed = true;
			kickBtnPressed = kickKeyPressed = true;
            LastFacedLeft = true;
			jumpBtn = Buttons.A;
			rightBtn = Buttons.LeftThumbstickRight;
			leftBtn = Buttons.LeftThumbstickLeft;
			crouchBtn = Buttons.LeftTrigger;
			punchBtn = Buttons.X;
			kickBtn = Buttons.B;

			if (player == PlayerIndex.One)
			{
				upKey = Keys.W;
				rightKey = Keys.D;
				leftKey = Keys.A;
				downKey = Keys.S;
				punchKey = Keys.G;
				kickKey = Keys.H;
			}
			else if (player == PlayerIndex.Two)
			{
				upKey = Keys.Up;
				rightKey = Keys.Right;
				leftKey = Keys.Left;
				downKey = Keys.Down;
				punchKey = Keys.RightControl;
				kickKey = Keys.Enter;
			}
		}

		public override void Update()
		{
			bool stand = true;

			// Jump
			if (Keyboard.GetState().IsKeyDown(upKey) || GamePad.GetState(player).IsButtonDown(jumpBtn))
			{
				Jump();
				stand = false;
			}

			// Walk
			if (Keyboard.GetState().IsKeyDown(rightKey) || GamePad.GetState(player).IsButtonDown(rightBtn))
			{
				WalkRight();
				stand = false;
                LastFacedLeft = false;
			}
			else if (Keyboard.GetState().IsKeyDown(leftKey) || GamePad.GetState(player).IsButtonDown(leftBtn))
			{
				WalkLeft();
				stand = false;
                LastFacedLeft = true;
			}

			// Crouch
			if (Keyboard.GetState().IsKeyDown(downKey) || GamePad.GetState(player).IsButtonDown(crouchBtn))
			{
				if (!Keyboard.GetState().IsKeyDown(upKey) && !GamePad.GetState(player).IsButtonDown(jumpBtn))
				{
					Crouching = true;
					stand = false;
				}
			}
			else
				Crouching = false;

			// Punch
			if (Keyboard.GetState().IsKeyDown(punchKey))
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

			// Kick
			if (Keyboard.GetState().IsKeyDown(kickKey))
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

			if (stand)
				Stand();

			base.Update();
		}

		public override void Draw(SpriteBatch sb)
		{
			sb.DrawString(MainGame.fnt_basicFont, Math.Atan2(GamePad.GetState(player).ThumbSticks.Left.Y, GamePad.GetState(player).ThumbSticks.Left.X).ToString(), Vector2.Zero, Color.White);

			base.Draw(sb);
		}
	}
}
