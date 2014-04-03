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
       // private bool hasTarget;
        private float melee_radius;

        public StickFigure[] ListStickFigures;

        public BotPlayer(World world, Vector2 position, Category collisionCat, float scale, float limbStrength, float limbDefense, bool evilSkin, Color color, PlayerIndex player, StickFigure[] dudes)
            : base(world, position, collisionCat, scale, limbStrength, limbDefense, evilSkin, color)
        {
            this.player = player;
            this.target = null;
            attention_radius = 100;
            this.ListStickFigures = dudes;
            attention_countdown_max = 5f;
            attention_countdown = attention_countdown_max;
            attention_countdown_delta = 0.0075f;
        }

		public override StickFigure Respawn()
		{
            return new BotPlayer(world, startPosition, collisionCat, scale, limbStrength, limbDefense, evilSkin, color, player, this.ListStickFigures);
		}

        public override void Update()
        {
            if (!IsDead)
            {
                bool stand = true;
                // get target

                if (this.target == null)// || (this.target is BotPlayer))
                {
                    float closestDist = 999f, dist;
                    Random r = new Random();
                    // May pick nearest stick figure, or nearest player-controlled stick figure.
                    // Makes the game more challenging.
                    if (r.Next(10) > 4)
                    {
                        foreach (StickFigure s in ListStickFigures)
                        {
                            if (s != this && !s.IsDead && (s.CollisionCategory != this.collisionCat))
                            {
                                dist = (s.Position - this.Position).Length() + (float)r.NextDouble() * 1.5f;
                                if (dist < closestDist)
                                {
                                    closestDist = dist;
                                    this.target = s;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (StickFigure s in ListStickFigures)
                        {
                            if (!(s is BotPlayer) && !s.IsDead && (s.CollisionCategory != this.collisionCat))
                            {
                                dist = (s.Position - this.Position).Length() + (float)r.NextDouble() * 1.5f;
                                if (dist < closestDist)
                                {
                                    closestDist = dist;
                                    this.target = s;
                                }
                                // check collisioncat
                                /*  if ((s.Position - this.Position).Length() <= attention_radius)
                                  {
                                      this.hasTarget = true;
                                      this.target = s;
                                      break;
                                  }*/
                            }
                        }
                    }

                }
                else if (this.target.IsDead)
                {
                    this.target = null;
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

                    if ((target.Position - this.Position).Length() < 1) // ADJUST AS NEEDED
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
                    else if (((target.Position - this.Position).Length() < 2.5) && // ADJUST AS NEEDED
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

                // After doing stuff, determine whether should check for better target
                if (attention_countdown <= 0)
                {
                    this.target = null;
                    attention_countdown = attention_countdown_max;
                }
                else
                {
                    attention_countdown -= attention_countdown_delta; // how to normalize this to actual game time?
                }
            }
            base.Update();
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }
    }
}
