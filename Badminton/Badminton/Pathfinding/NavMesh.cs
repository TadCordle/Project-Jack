using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FarseerPhysics;
using FarseerPhysics.Common;

namespace Badminton.Pathfinding
{
    // Each level gets one of these
    public class NavMesh
    {
        List<NavNode> nodes;

        public NavMesh()
        {
            nodes = new List<NavNode>();
        }
/*
        public List<Vector2> GetPathPositions(Vector2 vstart, Vector2 vgoal, Stick_Figures.BotPlayer bot)
        {
            List<NavNode> path = GetPathFromNodes(GetNearestNode(vstart), GetNearestNode(vgoal), bot);
            return NodesToPositions(path);
        }

        public List<NavNode> GetPathFromNodes(NavNode nstart, NavNode ngoal, Stick_Figures.BotPlayer bot)
        {
            List<NavNode> closedset = new List<NavNode>();
            //List<NavNode> openset = new List<NavNode>();
            List<NavNode> path = new List<NavNode>();
            Dictionary<NavNode, float> opensetCosts = new Dictionary<NavNode, float>();

            NavNode ncurrent = nstart; // initial value, is meaningless

            opensetCosts[nstart] = 0;
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

                opensetCosts.Remove(ncurrent);
                if (ncurrent == ngoal)
                {
                    path.Add(ncurrent);
                    goto Succeeded;
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
                }
            }
            goto Cleanup;
            Succeeded:
            // Chain the nodes together into a path, starting at the end
            while (ncurrent.GetPrevious(bot) != null)
            {
                path.Insert(0, ncurrent);
                ncurrent = ncurrent.GetPrevious(bot);
            }
        Cleanup: // reset nodes
            foreach (NavNode p in closedset) p.SetPrevious(bot, null);
            foreach (NavNode p in opensetCosts.Keys) p.SetPrevious(bot, null);
            return path;
        }
        */
        // Get the result from a path search and get their positions
        public static List<Vector2> NodesToPositions(List<NavNode> nlist)
        {
            List<Vector2> v = new List<Vector2>();
            foreach (NavNode n in nlist)
            {
                v.Add(n.Position);
            }
            return v;
        }

        public void AddNode(NavNode pnode)
        {
            nodes.Add(pnode);
        }

        // Given a point in space, get the closest node
        public NavNode GetNearestNode(Vector2 location)
        {
            float dmin = float.MaxValue;
            float d;
            NavNode n = null;
            for (int i = 0; i < nodes.Count; i++)
            {
                d = (location - nodes[i].Position).LengthSquared();
                if (d < dmin)
                {
                    dmin = d;
                    n = nodes[i];
                }
            }
            Console.WriteLine("Nearest node to " + location + " is at " + n.Position);
            return n;
        }

        // The above, but return its position
        public Vector2 GetNearestNodePosition(Vector2 location)
        {
            return GetNearestNode(location).Position;
        }

        public void DrawNodes(SpriteBatch sb, Stick_Figures.StickFigure sf)
        {
            foreach (NavNode node in nodes)
            {
                sf.DrawLine(sb, MainGame.tex_blank, 2, Color.Green, sf.Position * MainGame.METER_TO_PIXEL, node.Position * MainGame.METER_TO_PIXEL);
                foreach (NavNode neighbor in node.Neighbors)
                {
                    sf.DrawLine(sb, MainGame.tex_blank, 2, Color.Red, node.Position * MainGame.METER_TO_PIXEL, neighbor.Position * MainGame.METER_TO_PIXEL);
                    
                }
            }
        }
    }
}
