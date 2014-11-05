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
    public class BotPlayer : StickFigure
    {
        private PlayerIndex player;
        private StickFigure target;
        private int newtarget_countdown = 100, move_countdown, melee_countdown, shoot_countdown;
        private bool should_walk_left = false, should_walk_right = false, should_jump = false;
        private float angle;

        private Random random;
        public StickFigure[] ListStickFigures;

        private Pathfinding.NavAgent nav;
        public StickFigure Target { get { return this.target; } }

        public BotPlayer(World world, Vector2 position, Category collisionCat, float scale, float limbStrength, float limbDefense, bool evilSkin, Color color, PlayerIndex player, StickFigure[] dudes)
            : base(world, position, collisionCat, scale, limbStrength, limbDefense, evilSkin, color)
        {
            this.player = player;
            this.target = null;
            //attention_radius = 100;
            this.ListStickFigures = dudes;
            random = new Random();
            //if (nav == null) nav = new Pathfinding.NavAgent(this, mesh);
            move_countdown = 50 + 150 * random.Next(); // initial delay 0.05-0.25s
            melee_countdown = 100 + 500 * random.Next(); // initial delay 0.1-0.4s
            shoot_countdown = 200 + 500 * random.Next(); // initial delay 0.2-0.7s
        }

        public override StickFigure Respawn()
        {
            return new BotPlayer(world, startPosition, collisionCat, scale, limbStrength, limbDefense, EvilSkin, color, player, this.ListStickFigures);
        }

        private void ResetTarget()
        {
            float closestDist = 999f, dist;
            foreach (StickFigure s in ListStickFigures)
            {
                if (s != this && !s.IsDead && (s.CollisionCategory != this.collisionCat) && !IsRaycast(s.Position))
                {
                    if ((this.target != null) && (s is BotPlayer) && random.Next(100) < 25)
                    {
                        continue; // sometimes prefer humans
                    }
                    dist = (s.Position - this.Position).Length();
                    if ((random.Next(100) < 60) && (dist < closestDist))
                    {
                        closestDist = dist; // sometimes not pick the closest guy
                        this.target = s;
                    }
                }
            }
        }

        private bool IsRaycast(Vector2 v)
        {
            bool b = false;
            world.RayCast((f, p, n, fr) =>
            {
                if (f != null && f.Body.UserData is Wall)
                {
                    b = true;
                    return 1; // raycast is dumb
                }
                else
                {
                    b = false;
                    return 0;
                }
            }, Position, v);
            return b;
        }

        public override void Update(int milliseconds)
        {
            if (!IsDead)
            {
                bool stand = true;

                // Before determining movement or anything, determine whether to blow up
                if (melee_countdown <= 0 || shoot_countdown <= 0)
                {
                    if (ScalarHealth < 0.1f)
                    {
                        bool b = false;
                        foreach (StickFigure s in ListStickFigures)
                        {
                            if (s != this && !s.IsDead && (s.CollisionCategory != this.collisionCat) && (s.Position - this.Position).Length() < 1.5f)
                                b = true;
                        }
                        if (b)
                        {
                            if (random.Next(100) <= 1)
                            {
                                Explode();
                                base.Update(milliseconds);
                                return;
                            }
                        }
                    }
                }

                // first, check the target
                if ((this.target == null) || this.target.IsDead || (newtarget_countdown <= 0))
                {
                    ResetTarget();
                    newtarget_countdown = 4000;
                }

                // check motion
                if (move_countdown > 0) // continue the current motion
                {
                    if (should_walk_left)
                    {
                        WalkLeft();
                        stand = false;
                    }
                    else if (should_walk_right)
                    {
                        WalkRight();
                        stand = false;
                    }
                    if (should_jump)
                    {
                        Jump();
                        stand = false;
                        should_jump = false; // only jump once
                    }
                }
                else if (target != null)
                {
                    // choose new direction every 0.1-0.5 sec
                    move_countdown = random.Next(100,500);
                    should_walk_left = false;
                    should_walk_right = false;
                    should_jump = false;
                    if (target.Position.X - this.Position.X > 0.1f)
                    {
                        should_walk_right = true;
                    }
                    else if (target.Position.X - this.Position.X < -0.1f)
                    {
                        should_walk_left = true;
                    }
                    if ((target.Position.Y - this.Position.Y < -4f))
                    {
                        should_jump = true;
                    }
                }

                if (target != null) // determine attack
                {
                    if (melee_countdown <= 0 && (target.Position - this.Position).Length() < 3f)
                    {
                        if (Math.Abs(target.Position.Y - this.Position.Y) < 0.1f) // approximate left/right
                        {
                            if (target.Position.X > this.Position.X)
                                Punch(0f); //  right
                            else
                                Punch(MathHelper.Pi); // left
                        }
                        else if (Math.Abs(target.Position.X - this.Position.X) < 0.1f) // approximate up/down
                        {
                            if (target.Position.Y < this.Position.Y) // above
                                Punch(MathHelper.PiOver2);
                            else // below
                                Kick(-MathHelper.PiOver2);
                        }
                        else // need to calculate angle
                        {
                            angle = (float)Math.Atan2((-target.Position.Y + this.Position.Y), (target.Position.X - this.Position.X));
                            if (target.Position.Y < this.Position.Y)
                                Punch(angle);
                            else
                                Kick(angle);
                        }
                        melee_countdown = random.Next(100,200); // delay 0.1-0.2s
                    }
                    else if (shoot_countdown <= 0)// && (target.Position - this.Position).Length() > 5f)
                    { // too far -> shoot projectile
                        angle = (float)Math.Atan2((-target.Position.Y + this.Position.Y), (target.Position.X - this.Position.X));
                        Aim(angle);
                        LongRangeAttack();
                        shoot_countdown = random.Next(500, 1500); // delay 0.5-1.5 sec
                    }
                }
                if (stand) Stand();
                // Decrement countdowns
                if (newtarget_countdown > 0)
                    newtarget_countdown -= milliseconds;
                if (move_countdown > 0)
                    move_countdown -= milliseconds;
                if (melee_countdown > 0)
                    melee_countdown -= milliseconds;
                if (shoot_countdown > 0)
                    shoot_countdown -= milliseconds;
            }
            base.Update(milliseconds);
        }

    }
}
