using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Common;

namespace Badminton.Pathfinding
{
    // Each level has a tree made of these
    public class NavNode
    {
        public Vector2 Position;
        public List<NavNode> Neighbors;
        public List<float> Costs;

        // Index by which bot is searching (prevents conflicts)
        Dictionary<NavAgent, NavNode> Previous;

        public NavNode(NavMesh mesh, float x, float y)
        {
            Position = new Vector2(x,y) * MainGame.PIXEL_TO_METER;
            Neighbors = new List<NavNode>();
            Costs = new List<float>();
            Previous = new Dictionary<NavAgent, NavNode>();
            mesh.AddNode(this);
        }

        // Index by which bot is searching (prevents conflicts)
        public void SetPrevious(NavAgent agent, NavNode node)
        {
            Previous[agent] = node;
        }

        // Index by which bot is searching (prevents conflicts)
        public NavNode GetPrevious(NavAgent agent)
        {
            if (Previous.ContainsKey(agent))
            {
                return Previous[agent];
            }
            else return null;
        }

        public void AddNeighbor(NavNode n)
        {
            if (!Neighbors.Contains(n))
            {
                Neighbors.Add(n);
                Costs.Add(EdgeCost(n));
            }
        }

        public void AddNeighbors(params NavNode[] list)
        {
            foreach (NavNode n in list)
            {
                if (this != n)
                {
                    this.AddNeighbor(n);
                }
            }
        }

        private float EdgeCost(NavNode n)
        {
            float x, y;
            x = Math.Abs(n.Position.X - this.Position.X);
            y = this.Position.Y - n.Position.Y; // negative if the neighbor is above
            // We will probably adjust this:
            return 1;// x + y * 1.2f;
        }

        public void RecalculateEdgeCosts()
        {
            int count = Neighbors.Count;
            Costs.Clear();
            for (int i = 0; i < count; i++)
            {
                Costs.Add(EdgeCost(Neighbors[i]));
            }
        }
    }
}
