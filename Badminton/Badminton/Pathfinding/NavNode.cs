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

        public NavNode Previous = null; // for pathfinding

        public NavNode(float x, float y)
        {
            Position = new Vector2(x, y);
            Neighbors = new List<NavNode>();
            Costs = new List<float>();
        }

        public void AddNeighbor(NavNode n)
        {
            if (!Neighbors.Contains(n))
            {
                Neighbors.Add(n);
                Costs.Add(EdgeCost(n));
            }
        }

        private float EdgeCost(NavNode n)
        {
            float x, y;
            x = Math.Abs(n.Position.X - this.Position.X);
            y = this.Position.Y - n.Position.Y; // negative if the neighbor is above
            // We will probably adjust this:
            return x + y;
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
