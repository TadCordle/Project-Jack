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

        private float distanceSelfDestruct = 1.5f;
        private float percentSelfDestruct = 40;
        private int attackCooldown, attackCooldownMax, attackCooldownMin;
        private int missileCooldown, missileCooldownMax, missileCooldownMin;
        protected int bombCooldown, bombCooldownMax, bombCooldownMin;

        public StickFigure Target { get { return this.target; } }

        public AiPlayer(World world, Vector2 position, Category collisionCat, float scale, float limbStrength, float limbDefense, bool evilSkin, Color color, PlayerIndex player, StickFigure[] dudes)
            : base(world, position, collisionCat, scale, limbStrength, limbDefense, evilSkin, color)
        {
            destinations = new List<Vector2>();
            this.player = player;
            this.target = null;
            this.ListStickFigures = dudes;
            random = new Random();
            if (nav == null) nav = new NavAgent(this);
            
            SetAttackTimeRange(100, 300); // milliseconds
            SetMissileTimeRange(250, 750);
            SetBombTimeRange(2000, 4000);
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            if (nav != null) nav.Draw(sb);
        }

        #region CooldownParameters
        public void SetAttackTimeRange(int MillisecondsMin, int MillisecondsMax)
        {
            attackCooldownMax = MillisecondsMax;
            attackCooldownMin = MillisecondsMin;
            attackCooldown = random.Next(MillisecondsMin, MillisecondsMax);
        }

        public void SetBombTimeRange(int MillisecondsMin, int MillisecondsMax)
        {
            bombCooldownMax = MillisecondsMax;
            bombCooldownMin = MillisecondsMin;
            bombCooldown = random.Next(MillisecondsMin, MillisecondsMax);
        }

        public void SetMissileTimeRange(int MillisecondsMin, int MillisecondsMax)
        {
            missileCooldownMax = MillisecondsMax;
            missileCooldownMin = MillisecondsMin;
            missileCooldown = random.Next(MillisecondsMin, MillisecondsMax);
        }

        #endregion

        public override StickFigure Respawn()
        {
            return new AiPlayer(world, startPosition, collisionCat, scale, limbStrength, limbDefense, EvilSkin, color, player, this.ListStickFigures);
        }

        protected void ThrowBomb()
        {
            // TODO: Custom aiming patterns
            ThrowTrap((float)(random.NextDouble() * Math.PI * 2));
            bombCooldown = random.Next(bombCooldownMin, bombCooldownMax);
        }

        private void GetNewTarget()
        {
            float mindist = float.MaxValue;
            float dist;
            this.target = ListStickFigures[0];
            foreach (StickFigure s in ListStickFigures)
            {
                if (s != this && !s.IsDead && (s.CollisionCategory != this.collisionCat))
                {
                    dist = 1; // estimate path distance
                    if (dist < mindist)
                    {
                        mindist = dist;
                        this.target = s;
                    }
                }
            }
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
                        Vector2.Distance(s.Position, this.Position) < distanceSelfDestruct)
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
            int tick = 1;// gameTime.ElapsedGameTime.Milliseconds;
            attackCooldown -= tick;
            missileCooldown -= tick;
            bombCooldown -= tick;
            // update behavior
            if (ShouldSelfDestruct())
            {
                Explode();
                if (nav != null) nav.Delete();
            }
            else if (IsDead)
            {
                if (nav!=null) nav.Delete();
            }
            else
            {
                // Verify that current target is still valid

                if ((this.target == null) || this.target.IsDead)
                {
                    GetNewTarget();
                    //  destinations = nav.GetDestinations(target);
                }
                else if (destinations.Count == 0)
                {
                    // Also periodically update path
                    //nav.GetDestinations(target);
                    //pathcheckCooldown = pathcheckCooldownMax;
                }
                else
                {
                    // Move along path
                    if (Vector2.Distance(destinations[0], this.Position) < 1f)
                    {

                        destinations.RemoveAt(0);
                        if (destinations.Count == 0)
                        {
                            //Console.WriteLine("Reached destination, updating path");
                            // Get a new list of destinations
                            //nav.GetDestinations(target);
                        }
                        //else
                        //{
                        //    Console.WriteLine("Reached destination");
                        //}
                    }
                    if (attackCooldown <= 0 || missileCooldown <= 0) // ready to attack
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
            if (attackCooldown <= 0 && Vector2.Distance(target.Position, this.Position) < 3f)
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
                attackCooldown = random.Next(attackCooldownMin, attackCooldownMax);
            }
            else if (missileCooldown <= 0 && HasLineOfSight(target.Position))
            { // too far -> shoot projectile
                float angle = (float)Math.Atan2((-target.Position.Y + this.Position.Y), (target.Position.X - this.Position.X));
                Aim(angle);
                LongRangeAttack();
                missileCooldown = random.Next(missileCooldownMin, missileCooldownMax);
            }
        }

    }
}
