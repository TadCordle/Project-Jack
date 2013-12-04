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
        private float attention_radius;
        private bool hasTarget;
        private float melee_radius;

        public List<StickFigure> ListStickFigures;

        public BotPlayer(World world, Vector2 position, Category collisionCat, float scale, Color color, PlayerIndex player, StickFigure target)
            : base(world, position, collisionCat, scale, color)
        {
            this.player = player;
            this.hasTarget = false;
            attention_radius = 100;
            //this.ListStickFigures = list;
            this.target = target;
        }

        public override void Update()
        {
            bool stand = true;
            // get target
            //foreach (StickFigure s in ListStickFigures)
            //{
                    // check collisioncat
                if ((target.Position - this.Position).Length() <= attention_radius)
                {
                    this.hasTarget = true;
                    //this.target = target;
                }
            //}
            if (this.hasTarget == false)
            {/*
                foreach (StickFigure s in ListStickFigures)
                {
                    // check collisioncat
                    if (true)
                    {
                        this.hasTarget = true;
                        this.target = s;
                    }
                }*/
                ;;
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
