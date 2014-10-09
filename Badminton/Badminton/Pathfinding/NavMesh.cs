using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Common;

namespace Badminton.Pathfinding
{
    // Each level gets one of these
    public class NavMesh
    {
        List<NavNode> nodes;

        public List<Vector2> GetPathPositions(Vector2 vstart, Vector2 vgoal)
        {
            List<NavNode> path = GetPathFromNodes(GetNearestNode(vstart), GetNearestNode(vgoal));
            return NodesToPositions(path);
        }

        public List<NavNode> GetPathFromNodes(NavNode nstart, NavNode ngoal)
        {
            List<NavNode> closedset = new List<NavNode>();
            List<NavNode> openset = new List<NavNode>();
            List<NavNode> path = new List<NavNode>();
            NavNode ncurrent;
            openset.Add(nstart);
            while (openset.Count > 0)
            {
                // TODO: sort openset
                ncurrent = openset[0];
                openset.Remove(ncurrent);
                if (ncurrent == ngoal)
                {
                    path.Add(ncurrent);
                    goto Succeeded;
                }
                else
                {
                    closedset.Add(ncurrent);
                    foreach (NavNode p in ncurrent.Neighbors)
                    {
                        if (!closedset.Contains(p) && !openset.Contains(p))
                        {
                            openset.Add(p);
                            p.Previous = ncurrent;
                        }
                    }
                }
            }
            goto Cleanup;
            Succeeded:
            // Chain the nodes together into a path, starting at the end
            while (ncurrent.Previous != null)
            {
                path.Add(ncurrent);
                ncurrent = ncurrent.Previous;
            }
            path.Reverse();
            Cleanup: // reset nodes
            foreach (NavNode p in closedset)   p.Previous = null;
            foreach (NavNode p in openset)     p.Previous = null;
            return path;
        }

        // Get the result from a path search and get their positions
        public List<Vector2> NodesToPositions(List<NavNode> nlist)
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
