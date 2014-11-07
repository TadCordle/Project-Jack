using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using FarseerPhysics;
using FarseerPhysics.Common;

namespace Badminton.Pathfinding
{
    // Each level gets one of these
    public class NavMesh : DrawableGameComponent
    {
        List<NavNode> nodes;

        public NavMesh(): base(MainGame.mainGame)
        {
            nodes = new List<NavNode>();
            Console.WriteLine("New NavMesh");
            if (!Game.Components.Contains(this)) Game.Components.Add(this);
        }

        ~NavMesh()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i] = null;
            }
            GC.Collect();
        }

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
                d = Vector2.Distance(location, nodes[i].Position);
                if (d < dmin)
                {
                    dmin = d;
                    n = nodes[i];
                }
            }
            //Console.WriteLine("Nearest node to " + location + " is at " + n.Position);
            return n;
        }

        // The above, but return its position
        public Vector2 GetNearestNodePosition(Vector2 location)
        {
            return GetNearestNode(location).Position;
        }

        float linewidth = 2f;
        public void DrawLine(SpriteBatch batch, Vector2 point1, Vector2 point2)
        {
            float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
            float length = Vector2.Distance(point1, point2);
            if (batch.IsDisposed) batch.Begin();
            batch.Draw(MainGame.tex_blank, point1, null, Color.Red,
                       angle, Vector2.Zero, new Vector2(length, linewidth),
                       SpriteEffects.None, 0);
        }

        public void Draw(SpriteBatch sb)
        {
            //int i = 1;
            //foreach (NavNode node in nodes)
            //{
            //    sb.DrawString(MainGame.fnt_midFont, i.ToString(), node.Position * MainGame.METER_TO_PIXEL, Color.Red);
            //    foreach (NavNode neighbor in node.Neighbors)
            //    {
            //        DrawLine(sb, node.Position * MainGame.METER_TO_PIXEL, neighbor.Position * MainGame.METER_TO_PIXEL);
            //    }
            //    i++;
            //}
           
        }
    }
}
