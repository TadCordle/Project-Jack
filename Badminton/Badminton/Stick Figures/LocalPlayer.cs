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
		private bool punchPressed, kickPressed, lastFacedLeft;

		public LocalPlayer(World world, Vector2 position, Category collisionCat, Color color, PlayerIndex player)
			: base(world, position, collisionCat, color)
		{
			this.player = player;
            //hascontroller = GamePad.GetState(player).IsConnected;
			punchPressed = true;
			kickPressed = true;
            lastFacedLeft = true;
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

			if (Keyboard.GetState().IsKeyDown(upKey) || GamePad.GetState(player).IsButtonDown(jumpBtn))
			{
				Jump();
				stand = false;
			}

			if (Keyboard.GetState().IsKeyDown(rightKey) || GamePad.GetState(player).IsButtonDown(rightBtn))
			{
				WalkRight();
				stand = false;
                lastFacedLeft = false;
			}


			else if (Keyboard.GetState().IsKeyDown(leftKey) || GamePad.GetState(player).IsButtonDown(leftBtn))
			{
				WalkLeft();
				stand = false;
                lastFacedLeft = true;
			}

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

			if (Keyboard.GetState().IsKeyDown(punchKey))
			{
				if (!punchPressed)
				{
					Vector2 direction = Vector2.Zero;

					if (Keyboard.GetState().IsKeyDown(upKey))
						direction += Vector2.UnitY;
					
					if (Keyboard.GetState().IsKeyDown(downKey))
						direction -= Vector2.UnitY;

					if (Keyboard.GetState().IsKeyDown(leftKey))
					{
						punchPressed = true;
						direction -= Vector2.UnitX;
					}
					else if (Keyboard.GetState().IsKeyDown(rightKey))
					{
						punchPressed = true;
						direction += Vector2.UnitX;
					}

					if (direction.Length() == 0)
					{
						if (lastFacedLeft)
							direction = -Vector2.UnitX;
						else
							direction = Vector2.UnitX;
					}

					Punch((float)Math.Atan2(direction.Y, direction.X));
				}
			}
			else
				punchPressed = false;

			if (GamePad.GetState(player).IsButtonDown(punchBtn))
			{
				if (!punchPressed)
				{
					punchPressed = true;
					Punch((float)Math.Atan2(GamePad.GetState(player).ThumbSticks.Left.Y, GamePad.GetState(player).ThumbSticks.Left.X));
				}
			}
			else
				punchPressed = false;

			if (GamePad.GetState(player).IsButtonDown(kickBtn))
			{
				if (!kickPressed)
				{
					kickPressed = true;
					Kick((float)Math.Atan2(GamePad.GetState(player).ThumbSticks.Left.Y, GamePad.GetState(player).ThumbSticks.Left.X));
				}
			}
			else
				kickPressed = false;

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
