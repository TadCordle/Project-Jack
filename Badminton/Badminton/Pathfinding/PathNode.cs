using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Common;

namespace Badminton.Pathfinding
{
    // Each level has a tree made of these
    public class PathNode
    {
        public Vector2 Position;
        public List<PathNode> Neighbors;
        public List<float> Costs;

        public PathNode(float x, float y)
        {
            Position = new Vector2(x, y);
            Neighbors = new List<PathNode>();
            Costs = new List<float>();
        }

        public void AddNeighbor(PathNode n)
        {
            if (!Neighbors.Contains(n))
            {
                Neighbors.Add(n);
                Costs.Add(EdgeCost(n));
            }
        }

        private float EdgeCost(PathNode n)
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
