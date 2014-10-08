using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Common;

namespace Badminton.Pathfinding
{
    // Each level gets one of these
    public class PathMesh
    {
        List<PathNode> nodes;

        public List<PathNode> GetPathFromPositions(Vector2 start, Vector2 end)
        {
            PathNode nstart = GetNearestNode(start);
            PathNode nend = GetNearestNode(end);
            List<PathNode> neighbors = nstart.Neighbors;
            List<PathNode> visited = new List<PathNode>();
            List<PathNode> tosearch = new List<PathNode>(neighbors);
            List<PathNode> path = new List<PathNode>();
            // TO DO
            return path;
        }

        // Get the result from a path search and get their positions
        public List<Vector2> NodesToPositions(List<PathNode> nlist)
        {
            List<Vector2> v = new List<Vector2>();
            foreach (PathNode n in nlist)
            {
                v.Add(n.Position);
            }
            return v;
        }

        public void AddNode(PathNode pnode)
        {
            nodes.Add(pnode);
        }

        // Given a point in space, get the closest node
        public PathNode GetNearestNode(Vector2 location)
        {
            float dmin = float.MaxValue;
            float d;
            PathNode n = null;
            for (int i = 0; i < nodes.Count; i++)
            {
                d = (location - nodes[i].Position).LengthSquared();
                if (d < dmin)
                {
                    n = nodes[i];
                }
            }
            return n;
        }

        // The above, but return its position
        public Vector2 GetNearestNodePosition(Vector2 location)
        {
            return GetNearestNode(location).Position;
        }
    }
}
