using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;

namespace Badminton.Stick_Figures
{
    class BotPlayer : StickFigure
    {
        private PlayerIndex player;
        private StickFigure target;
        private float attention_radius, attention_countdown, attention_countdown_max, attention_countdown_delta;
        private bool hasTarget;
        private float melee_radius;

        public StickFigure[] ListStickFigures;

        public BotPlayer(World world, Vector2 position, Category collisionCat, float scale, float limbStrength, float limbDefense, bool evilSkin, Color color, PlayerIndex player, StickFigure[] dudes)
            : base(world, position, collisionCat, scale, limbStrength, limbDefense, evilSkin, color)
        {
            this.player = player;
            this.hasTarget = false;
            attention_radius = 100;
            this.ListStickFigures = dudes;
            attention_countdown_max = 5f;
            attention_countdown = attention_countdown_max;
            attention_countdown_delta = 0.003f;
        }

		public override StickFigure Respawn()
		{
            return new BotPlayer(world, startPosition, collisionCat, scale, limbStrength, limbDefense, evilSkin, color, player, this.ListStickFigures);
		}

        public override void Update()
        {
            bool stand = true;
            // get target
            if (attention_countdown <= 0)
            {
                this.hasTarget = false;
                attention_countdown = attention_countdown_max;
            }
            else
            {
                attention_countdown -= attention_countdown_delta;
            }

            if (!this.hasTarget || (this.target is BotPlayer))
            {
                foreach (StickFigure s in ListStickFigures)
                {
                    if ((s.CollisionCategory!=this.collisionCat)) {
                    // check collisioncat
                        if ((s.Position - this.Position).Length() <= attention_radius)
                        {
                            this.hasTarget = true;
                            this.target = s;
                            break;
                        }
                    }
                }
            }
            else
            {
                

                if (target.Position.X > this.Position.X)
                {
                    WalkRight();
                    stand = false;
                }
                else if (target.Position.X <= this.Position.X)
                {
                    WalkLeft();
                    stand = false;
                }
                if (target.Position.Y < this.Position.Y - 4)
                {
                    Jump();
                    stand = false;
                }


                if ((target.Position - this.Position).Length() < 1)
                {
                    if (target.Position.X > this.Position.X)
                    {
                        Punch(0);
                    }
                    else
                    {
                        Punch(MathHelper.Pi);
                    }
                }
                else if (((target.Position - this.Position).Length() < 2.5) &&
                    (target.Position.Y > this.Position.Y))
                {
                    if (target.Position.X > this.Position.X)
                    {
                        Kick(0);
                    }
                    else
                    {
                        Kick(MathHelper.Pi);
                    }
                }
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
