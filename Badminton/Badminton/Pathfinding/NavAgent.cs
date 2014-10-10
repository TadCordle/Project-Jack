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
using Badminton.Stick_Figures;

namespace Badminton.Pathfinding
{
    public class NavAgent
    {
        AiPlayer bot;
        NavMesh mesh;

        public NavAgent(AiPlayer bot, NavMesh mesh)
        {
            this.bot = bot;
            this.mesh = mesh;
        }

        List<NavNode> closedset = new List<NavNode>();
        //List<NavNode> path = new List<NavNode>();
        List<Vector2> destinations = new List<Vector2>();
        Dictionary<NavNode, float> opensetCosts = new Dictionary<NavNode, float>();
        NavNode ncurrent, ngoal;

        //enum state { idle, busy, solved };
        //state currentstate = state.idle;
        //float idletimer = 0f;
        //float idletimermax = 1.5f;
        //float dt = 0.01f;

        bool busy = false;

        public void GetDestinations(StickFigure target)
        {
            if (!busy)
            {
                busy = true;
                foreach (NavNode p in closedset) p.SetPrevious(this, null);
                foreach (NavNode p in opensetCosts.Keys) p.SetPrevious(this, null);
                ncurrent = mesh.GetNearestNode(bot.Position);
                ngoal = mesh.GetNearestNode(target.Position);
                opensetCosts[ncurrent] = 0;

                while (opensetCosts.Count > 0)
                {
                    // next node has the minimum cost in the open set
                    float minTotalCost = float.MaxValue;
                    foreach (NavNode n in opensetCosts.Keys)
                    {
                        if (opensetCosts[n] < minTotalCost)
                        {
                            ncurrent = n;
                            minTotalCost = opensetCosts[n];
                        }
                    }
                    if (ncurrent == ngoal)
                    {
                        // Chain the nodes together into a path, starting at the end
                        while (ncurrent.GetPrevious(this) != null)
                        {
                            destinations.Insert(0, ncurrent.Position);
                            ncurrent = ncurrent.GetPrevious(this);
                        }
                        bot.SetDestinations(destinations);
                        busy = false;
                        return;
                    }
                    else
                    {
                        closedset.Add(ncurrent);
                        for (int j = 0; j < ncurrent.Neighbors.Count; j++)
                        {
                            NavNode n = ncurrent.Neighbors[j];
                            if (!closedset.Contains(n) && !opensetCosts.ContainsKey(n))
                            {
                                // Add to open set and calculate costs
                                opensetCosts[n] = opensetCosts[ncurrent] + ncurrent.Costs[j];
                                n.SetPrevious(this, ncurrent);
                            }
                        }
                        opensetCosts.Remove(ncurrent);
                    }
                }
                busy = false;
            }
            
        }




        /*
        public void Navigate(StickFigure target)
        {
            //if (currentstate == state.idle)
            //{
            idletimer += dt;
            if (idletimer >= idletimermax)
            {
                idletimer = 0;
                currentstate = state.busy;
                foreach (NavNode p in closedset) p.SetPrevious(bot, null);
                foreach (NavNode p in opensetCosts.Keys) p.SetPrevious(bot, null);
                ncurrent = mesh.GetNearestNode(bot.Position);
                ngoal = mesh.GetNearestNode(target.Position);
                opensetCosts[ncurrent] = 0;
            }
            //}
            else if (currentstate == state.busy)
            {
                if (opensetCosts.Count > 0)
                {
                    float minTotalCost = float.MaxValue;
                    foreach (NavNode n in opensetCosts.Keys)
                    {
                        if (opensetCosts[n] < minTotalCost)
                        {
                            ncurrent = n;
                            minTotalCost = opensetCosts[n];
                        }
                    }
                    if (ncurrent == ngoal)
                    {
                        path.Add(ncurrent);
                        currentstate = state.solved;
                    }
                    else
                    {
                        closedset.Add(ncurrent);
                        for (int j = 0; j < ncurrent.Neighbors.Count; j++)
                        {
                            NavNode n = ncurrent.Neighbors[j];
                            if (!closedset.Contains(n) && !opensetCosts.ContainsKey(n))
                            {
                                // Add to open set and calculate costs
                                opensetCosts[n] = opensetCosts[ncurrent] + ncurrent.Costs[j];
                                n.SetPrevious(bot, ncurrent);
                            }
                        }
                        opensetCosts.Remove(ncurrent);
                    }
                }
                else
                {
                    // Failed search
                    currentstate = state.idle;
                    idletimer = 0f;
                }
            }
            else if (currentstate == state.solved)
            {
                while (ncurrent.GetPrevious(bot) != null)
                {
                    path.Insert(0, ncurrent); // reading it backwards
                    ncurrent = ncurrent.GetPrevious(bot);
                }
                bot.SetDestinationsFromNodes(path);
                currentstate = state.idle;
                idletimer = 0f;
            }

            
        }
        */
    }
}
