using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Badminton.Stick_Figures;

namespace Badminton.Pathfinding
{
    public class NavAgent : DrawableGameComponent
    {
        AiPlayer bot;
        NavMesh mesh;
        Random random;

        List<NavNode> closedSet = new List<NavNode>();
        List<NavNode> openSet = new List<NavNode>();
        Dictionary<NavNode, float> costs = new Dictionary<NavNode, float>();

        List<Vector2> path = new List<Vector2>();
        NavNode ncurrent, ngoal;

        int idlecooldown, idlecooldownMin = 25, idlecooldownMax = 100; //milliseconds

        enum States : int { idle, start, getnext, expand, done };
        States state = States.idle;

        #region Drawing
        // DEBUG PURPOSES - DRAW PATH
        public void DrawLine(SpriteBatch batch, Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);

            batch.Draw(MainGame.tex_blank, point1, null, Color.Yellow,
                       angle, Vector2.Zero, new Vector2(length, 1f),
                       SpriteEffects.None, 0);
        }


        public void Draw(SpriteBatch sb)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                DrawLine(sb, path[i] * MainGame.METER_TO_PIXEL, path[i + 1] * MainGame.METER_TO_PIXEL);
            }
            Vector2 pos = bot.Position * MainGame.METER_TO_PIXEL;
            //Console.WriteLine(bot.Position.ToString() + "->" + pos.ToString());

            if (mesh == null) sb.DrawString(MainGame.fnt_midFont, "no mesh", pos, Color.Red);
            else if (bot.Target == null) sb.DrawString(MainGame.fnt_midFont, "no target", pos, Color.Red);
            else if (state == States.idle) sb.DrawString(MainGame.fnt_midFont, "idle " + idlecooldown.ToString(), pos, Color.Cyan);
            else if (state == States.done) sb.DrawString(MainGame.fnt_midFont, "done", pos, Color.Cyan);
            else if (state == States.start) sb.DrawString(MainGame.fnt_midFont, "start", pos, Color.Cyan);
            else if (state == States.getnext) sb.DrawString(MainGame.fnt_midFont, "getnext", pos, Color.Cyan);
            else if (state == States.expand) sb.DrawString(MainGame.fnt_midFont, "expand", pos, Color.Cyan);
        }
        #endregion

        public NavAgent(AiPlayer bot)
            : base(MainGame.mainGame)
        {
            this.bot = bot;
            mesh = Map.navMesh;
            if (mesh == null) Console.WriteLine("New NavAgent NO MESH");
            else Console.WriteLine("New NavAgent OKAY");
            if (random == null) random = new Random();
            Game.Components.Add(this);
        }

        public void Delete()
        {
            Game.Components.Remove(this);
            this.Dispose();
        }

        void Reset()
        {
            idlecooldown = random.Next(idlecooldownMin, idlecooldownMax);
            state = States.idle;
            Clear();
        }

        void Clear()
        {
            foreach (NavNode p in openSet) p.SetPrevious(this, null);
            foreach (NavNode p in closedSet) p.SetPrevious(this, null);
            openSet.Clear();
            closedSet.Clear();
            costs.Clear();
        }

        public override void Update(GameTime gameTime)
        {
            if (bot == null || mesh == null) this.Delete();
            else
            {
                int tick = 1;// gameTime.ElapsedGameTime.Milliseconds;
                idlecooldown -= tick;
                if (bot.Target == null)
                {
                    // stall
                    //    Console.WriteLine("stalling...");
                }
                else if (state == States.idle)
                {
                    //  Console.WriteLine("idle...");
                    if (idlecooldown >= 0) idlecooldown -= tick;
                    else state = States.start;
                }
                else if (state == States.start)
                {
                    //Console.WriteLine("starting...");
                    Clear();
                    path.Add(bot.Position);
                    path.Add(bot.Target.Position);
                    ncurrent = mesh.GetNearestNode(bot.Position);
                    ngoal = mesh.GetNearestNode(bot.Target.Position);
                    openSet.Add(ncurrent);
                    costs[ncurrent] = 0;
                    state = States.getnext; // initialize
                }
                else if (state == States.getnext)
                {
                    //Console.WriteLine("get next");
                    if (openSet.Count == 0) Reset();
                    else
                    {
                        // next node has the minimum cost in the open set
                        float minTotalCost = float.MaxValue;
                        foreach (NavNode n in openSet)
                        {
                            if (costs[n] < minTotalCost)
                            {
                                ncurrent = n;
                                minTotalCost = costs[n];
                            }
                        }
                        if (ncurrent == ngoal)
                        {
                            state = States.done;
                        }
                        else
                        {
                            closedSet.Add(ncurrent);
                            openSet.Remove(ncurrent);
                            // state = States.expand;
                        }
                    }
                    //}
                    //else if (state == States.expand)
                    //{
                    //    //Console.WriteLine("Expand...");
                    for (int j = 0; j < ncurrent.Neighbors.Count; j++)
                    {
                        NavNode neighbor = ncurrent.Neighbors[j];
                        if (!closedSet.Contains(neighbor) && !openSet.Contains(neighbor))
                        {
                            // Add to open set and calculate costs
                            openSet.Add(neighbor);
                            costs[neighbor] = costs[ncurrent] + ncurrent.Costs[j];
                            neighbor.SetPrevious(this, ncurrent);
                        }
                    }
                    //state = States.getnext;
                }
                else if (state == States.done)
                {
                    //Console.WriteLine("done");
                    path.Clear();
                    while (ncurrent.GetPrevious(this) != null)
                    {
                        path.Insert(0, ncurrent.Position);
                        ncurrent = ncurrent.GetPrevious(this);
                    }
                    bot.SetDestinations(path);
                    Reset();
                }
            }
        }

    }
}
