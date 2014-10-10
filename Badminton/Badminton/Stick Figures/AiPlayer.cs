using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Badminton.Pathfinding;

namespace Badminton.Stick_Figures
{
    public class AiPlayer : StickFigure
    {
        /* Bot is capable of pathfinding
         * by using the node graphs that are on a map.
         */

        private PlayerIndex player;
        private StickFigure target;
        private Random random;
        private List<Vector2> destinations;

        public StickFigure[] ListStickFigures;

        public NavAgent nav;
        private NavMesh navmesh;

        private float distanceSelfDestruct = 1.5f;
        private float percentSelfDestruct = 40;
        private float attackCooldown, attackCooldownMax, attackCooldownMin;
        private float missileCooldown, missileCooldownMax, missileCooldownMin;
        protected float bombCooldown, bombCooldownMax, bombCooldownMin;
        private float pathcheckCooldown, pathcheckCooldownMax = 1.5f;
        protected float tick = 0.006f;

        public AiPlayer(World world, Vector2 position, Category collisionCat, float scale, float limbStrength, float limbDefense, bool evilSkin, Color color, PlayerIndex player, StickFigure[] dudes, NavMesh mesh)
            : base(world, position, collisionCat, scale, limbStrength, limbDefense, evilSkin, color)
        {
            destinations = new List<Vector2>();
            this.player = player;
            this.target = null;
            this.ListStickFigures = dudes;
            this.random = new Random();
            nav = new NavAgent(this, mesh);
            SetAttackTimeRange(0.1f, 0.3f);
            SetMissileTimeRange(0.25f, 0.75f);
            SetBombTimeRange(2f, 4f);
        }

        #region CustomizeAI
        public void SetAttackTimeRange(float TimeMin, float TimeMax)
        {
            attackCooldownMax = TimeMax;
            attackCooldownMin = TimeMin;
            attackCooldown = attackCooldownMin + (float)random.NextDouble() * (attackCooldownMax - attackCooldownMin);
        }

        public void SetBombTimeRange(float TimeMin, float TimeMax)
        {
            bombCooldownMax = TimeMax;
            bombCooldownMin = TimeMin;
            bombCooldown = bombCooldownMin + (float)random.NextDouble() * (bombCooldownMax - bombCooldownMin);
        }

        public void SetMissileTimeRange(float TimeMin, float TimeMax)
        {
            missileCooldownMax = TimeMax;
            missileCooldownMin = TimeMin;
            missileCooldown = missileCooldownMin + (float)random.NextDouble() * (missileCooldownMax - missileCooldownMin);
        }

        #endregion

        public override StickFigure Respawn()
        {
            return new AiPlayer(world, startPosition, collisionCat, scale, limbStrength, limbDefense, EvilSkin, color, player, this.ListStickFigures, this.navmesh);
        }

        protected void ThrowBomb()
        {
            // TODO: Custom aiming patterns
            ThrowTrap((float)(random.NextDouble() * Math.PI * 2));
            bombCooldown = bombCooldownMin + (float)random.NextDouble() * (bombCooldownMax - bombCooldownMin);
        }

        private void GetNewTarget()
        {
            float mindist = float.MaxValue;
            float dist;
            this.target = ListStickFigures[0];
            //foreach (StickFigure s in ListStickFigures)
            //{
            //    if (s != this && !s.IsDead && (s.CollisionCategory != this.collisionCat))
            //    {
            //        dist = 1; // estimate path distance
            //        if (dist < mindist)
            //        {
            //            mindist = dist;
            //            this.target = s;
            //        }
            //    }
            //}
        }

        private void UpdatePath()
        {
            destinations.Clear();
            Vector2 v = this.target.Position - this.Position;
            destinations.Add(this.Position + 0.33f * v + (new Vector2((float)random.NextDouble(), (float)random.NextDouble())));
            destinations.Add(this.Position + 0.66f * v + (new Vector2((float)random.NextDouble(), (float)random.NextDouble())));
            destinations.Add(this.target.Position);
            //destinations = navmesh.GetPathPositions(this.Position, target.Position);
        }

        // DEBUG PURPOSES - DRAW PATH
        public override void Draw(SpriteBatch sb)
        {
            if (destinations.Count > 0)
            {
                DrawLine(sb, MainGame.tex_blank, 2, Color.Red, Position * MainGame.METER_TO_PIXEL, destinations[0] * MainGame.METER_TO_PIXEL);
                for (int i = 0; i < destinations.Count - 1; i++)
                {
                    DrawLine(sb, MainGame.tex_blank, 2, Color.Red, destinations[i] * MainGame.METER_TO_PIXEL, destinations[i + 1] * MainGame.METER_TO_PIXEL);
                }
            }
            base.Draw(sb);
        }

        private bool ShouldSelfDestruct()
        {
            // Blow up if about to die
            if (ScalarHealth < 0.1f && (random.Next(100) < percentSelfDestruct))
            {
                foreach (StickFigure s in ListStickFigures)
                {
                    if (s != this &&
                        !s.IsDead &&
                        (s.CollisionCategory != this.collisionCat) &&
                        (s.Position - this.Position).Length() < distanceSelfDestruct)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void Update()
        {
            // update timers
            bool stand = true;
            attackCooldown -= tick;
            missileCooldown -= tick;
            bombCooldown -= tick;
            pathcheckCooldown -= tick;
            // update behavior
            if (ShouldSelfDestruct())
            {
                Explode();
            }
            else if (!IsDead)
            {
                // Verify that current target is still valid

                if ((this.target == null) || this.target.IsDead)
                {
                    GetNewTarget();
                    //  destinations = nav.GetDestinations(target);
                }
                else if (pathcheckCooldown <= 0 || destinations.Count == 0)
                {
                    // Also periodically update path
                    nav.GetDestinations(target);
                    pathcheckCooldown = pathcheckCooldownMax;
                }
                else
                {
                    // Move along path
                    if ((destinations[0] - this.Position).Length() < 1f)
                    {

                        destinations.RemoveAt(0);
                        if (destinations.Count == 0)
                        {
                            //Console.WriteLine("Reached destination, updating path");
                            // Get a new list of destinations
                            nav.GetDestinations(target);
                        }
                        //else
                        //{
                        //    Console.WriteLine("Reached destination");
                        //}
                    }
                    else if (attackCooldown <= 0 || missileCooldown <= 0) // ready to attack
                    {
                        Attack();
                    }
                    else if (bombCooldown <= 0 && TrapAmmo > 0)
                    {
                        ThrowBomb();
                    }
                    // Determine movement
                    if (destinations.Count > 0)
                    {
                        float dx = destinations[0].X - this.Position.X;
                        if (dx > 0.1f)
                        {
                            WalkRight();
                            stand = false;
                        }
                        else if (dx < -0.1f)
                        {
                            WalkLeft();
                            stand = false;
                        }
                        if (destinations[0].Y - this.Position.Y < -1f)
                        {
                            Jump();
                            stand = false;
                        }
                    }

                }

            }
            if (stand) Stand();
            base.Update();
        }

        public void SetDestinations(List<Vector2> d)
        {
            destinations = d;
        }


        private bool HasLineOfSight(Vector2 location)
        {
            bool b = false;
            world.RayCast((f, p, n, fr) =>
            {
                if (f != null && f.Body.UserData is Wall)
                {
                    b = false;
                    return 1;
                }
                else
                {
                    b = true;
                    return 0;
                }
            }, Position, location);
            return b;
        }

        private void Attack() // returns true if has attacked
        {
            // TODO (eventually): exact angles
            if (attackCooldown <= 0 && (target.Position - this.Position).Length() < 3f)
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
                    float angle = (float)Math.Atan2((-target.Position.Y + this.Position.Y), (target.Position.X - this.Position.X));
                    if (target.Position.Y < this.Position.Y)
                        Punch(angle);
                    else
                        Kick(angle);
                }
                attackCooldown = attackCooldownMin + (float)random.NextDouble() * (attackCooldownMax - attackCooldownMin);
            }
            else if (missileCooldown <= 0 && HasLineOfSight(target.Position))
            { // too far -> shoot projectile
                float angle = (float)Math.Atan2((-target.Position.Y + this.Position.Y), (target.Position.X - this.Position.X));
                Aim(angle);
                LongRangeAttack();
                missileCooldown = missileCooldownMin + (float)random.NextDouble() * (missileCooldownMax - missileCooldownMin);
            }
        }

    }
}
