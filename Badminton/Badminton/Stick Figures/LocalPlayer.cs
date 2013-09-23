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

		private Keys jumpKey, rightKey, leftKey, crouchKey, punchKey;
		private Buttons jumpBtn, rightBtn, leftBtn, crouchBtn, punchBtn;
		private bool punchPressed, lastFacedLeft;

		public LocalPlayer(World world, Vector2 position, Category collisionCat, Color color, PlayerIndex player)
			: base(world, position, collisionCat, color)
		{
			this.player = player;
            //hascontroller = GamePad.GetState(player).IsConnected;
			punchPressed = true;
            lastFacedLeft = true;
			jumpBtn = Buttons.A;
			rightBtn = Buttons.LeftThumbstickRight;
			leftBtn = Buttons.LeftThumbstickLeft;
			crouchBtn = Buttons.LeftTrigger;
			punchBtn = Buttons.X;

			if (player == PlayerIndex.One)
			{
				jumpKey = Keys.W;
				rightKey = Keys.D;
				leftKey = Keys.A;
				crouchKey = Keys.LeftControl;
				punchKey = Keys.G;
			}
			else if (player == PlayerIndex.Two)
			{
				jumpKey = Keys.Up;
				rightKey = Keys.Right;
				leftKey = Keys.Left;
				crouchKey = Keys.Down;
				punchKey = Keys.RightControl;
			}
		}

		public override void Update()
		{
			bool stand = true;

			if (Keyboard.GetState().IsKeyDown(jumpKey) || GamePad.GetState(player).IsButtonDown(jumpBtn))
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

			if (Keyboard.GetState().IsKeyDown(crouchKey) || GamePad.GetState(player).IsButtonDown(crouchBtn))
			{
				if (!Keyboard.GetState().IsKeyDown(jumpKey) && !GamePad.GetState(player).IsButtonDown(jumpBtn))
				{
					Crouching = true;
					stand = false;
				}
			}
			else
				Crouching = false;

			if (Keyboard.GetState().IsKeyDown(punchKey) || GamePad.GetState(player).IsButtonDown(punchBtn))
			{
                if (!punchPressed && (lastFacedLeft || Keyboard.GetState().IsKeyDown(leftKey)))
                {
                    punchPressed = true;
                    Punch((float)Math.PI);
                }

                else if (!punchPressed && (!lastFacedLeft || Keyboard.GetState().IsKeyDown(rightKey)))
                {
                    punchPressed = true;
                    Punch((float)0);
                }

                else if (!punchPressed)
                {
                    punchPressed = true;
                    Punch((float)Math.Atan2(GamePad.GetState(player).ThumbSticks.Left.Y, GamePad.GetState(player).ThumbSticks.Left.X));
                }
			}
			else
				punchPressed = false;

			if (stand)
				Stand();

			base.Update();
		}

		public override void Draw(SpriteBatch sb)
		{
			sb.DrawString(MainGame.fnt_basicFont, (Math.Atan2(GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.Y, GamePad.GetState(PlayerIndex.One).ThumbSticks.Right.X) - MathHelper.PiOver2).ToString(), Vector2.One * 64, Color.Black);

			base.Draw(sb);
		}
	}
}
