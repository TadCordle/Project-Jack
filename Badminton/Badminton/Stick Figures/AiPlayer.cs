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
        private float distanceMeleeAttack = 5f;
        private float percentSelfDestruct = 40;
        private int attackCooldown, attackCooldownMax, attackCooldownMin;
        private int missileCooldown, missileCooldownMax, missileCooldownMin;
        protected int bombCooldown, bombCooldownMax, bombCooldownMin;
        private int attentionCooldown, attentionCooldownMax, attentionCooldownMin;

        public StickFigure Target { get { return this.target; } }

        public static int MAX_DIFFICULTY = 10;
        public static int MIN_DIFFICULTY = 0;

        public AiPlayer(World world, Vector2 position, Category collisionCat, float scale, float limbStrength, float limbDefense, bool evilSkin, Color color, PlayerIndex player, StickFigure[] dudes, int difficulty = 3)
            : base(world, position, collisionCat, scale, limbStrength, limbDefense, evilSkin, color)
        {
            destinations = new List<Vector2>();
            this.player = player;
            this.target = null;
            this.ListStickFigures = dudes;
            random = new Random();
            if (nav == null) nav = new NavAgent(this);

            // higher difficulty = lower difficulty factor, approaches 1/2
            // difficulty = 0 : 2x slower
            // difficulty = 1 : 1x slower
            // difficulty = 3 : default
            // difficulty = 10 : 2x faster
            double difficultyFactor = 0.5 + 3/( 1 + Math.Pow(4, difficulty/2.5));

            SetAttackTimeRange((int)(350 * difficultyFactor), (int)(1000 * difficultyFactor)); // milliseconds
            SetMissileTimeRange((int)(650 * difficultyFactor), (int)(1500 * difficultyFactor));
            SetBombTimeRange((int)(4000 * difficultyFactor), (int)(8000 * difficultyFactor));
            SetAttentionTimeRange(5000, 7500);
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            if (nav != null) nav.Draw(sb);
        }

        #region CooldownParameters
        private void SetAttentionTimeRange(int MillisecondsMin, int MillisecondsMax)
        {
            attentionCooldownMin = MillisecondsMin;
            attentionCooldownMax = MillisecondsMax;
            attentionCooldown = random.Next(MillisecondsMin, MillisecondsMax);
        }

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
            //this.target = ListStickFigures[0];
            foreach (StickFigure s in ListStickFigures)
            {
                if (s != this && !s.IsDead && (s.CollisionCategory != this.collisionCat))
                {
                    dist = 1; // estimate path distance
                    dist = (s.Position - this.Position).Length();
                    if (((float)random.NextDouble() < 0.6f) && (dist < mindist))
                    {
                        mindist = dist; // sometimes not pick the closest guy
                        this.target = s;
                    }
                }
            }
            attentionCooldown = random.Next(attentionCooldownMin, attentionCooldownMax);
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

        public override void Update(int milliseconds)
        {
            // update timers
            bool stand = true;
            attackCooldown -= milliseconds;
            missileCooldown -= milliseconds;
            bombCooldown -= milliseconds;
            attentionCooldown -= milliseconds;
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

                if (attentionCooldown<=0 || (this.target == null) || this.target.IsDead)
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
            base.Update(milliseconds);
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

        private void Attack()
        {
            // TODO (eventually): exact angles and values
            if (attackCooldown <= 0 && Vector2.Distance(target.Position, this.Position) < distanceMeleeAttack)
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
