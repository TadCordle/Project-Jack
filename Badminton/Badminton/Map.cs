using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

using FarseerPhysics.Dynamics;
using Badminton.Pathfinding;

namespace Badminton
{
    public class MapData
    {
        public List<Wall> walls;
        public Vector2[] spawnPoints;
        public Vector3[] ammoPoints;
        public PathMesh pathMesh = null;
        public Texture2D background = null;
        public Texture2D foreground = null;
        public Song music = null;
        public bool HasForeground
        {
            get { return (foreground != null); }
        }
    }

	public class Map
	{
		public static Dictionary<Texture2D, string> MapKeys;

		public static MapData LoadMap(World w, string name)
		{
			if (name == "castle")
				return LoadCastle(w);
			else if (name == "pillar")
				return LoadPillar(w);
			else if (name == "octopus")
				return LoadOctopus(w);
			else if (name == "graveyard")
				return LoadGraveyard(w);
			else if (name == "clocktower")
				return LoadClocktower(w);
			else if (name == "circus")
				return LoadCircus(w);
			else
				return LoadCastle(w);
		}

		public static MapData LoadCastle(World w)
		{
            MapData data = new MapData();
            data.walls = new List<Wall>();
            data.walls.Add(new Wall(w, 980, 880, 1404, 128, 0));
            data.walls.Add(new Wall(w, 319, 368, 152, 976, 0));
            data.walls.Add(new Wall(w, 1564, 359, 108, 915, 0));
            data.walls.Add(new Wall(w, 980, -200, 1404, 128, 0));
            data.walls.Add(new Wall(w, 450, 518, 209, 47, 0));
            data.walls.Add(new Wall(w, 1402, 518, 216, 55, 0));
            data.walls.Add(new Wall(w, 963, 256, 172, 44, 0));

            data.spawnPoints = new Vector2[4];
            data.spawnPoints[0] = new Vector2(468, 366);
            data.spawnPoints[1] = new Vector2(1383, 375);
            data.spawnPoints[2] = new Vector2(486, 699);
            data.spawnPoints[3] = new Vector2(1380, 704);

            data.ammoPoints = new Vector3[1];
            data.ammoPoints[0] = new Vector3(960, 170, 1800); // (x, y, respawn time)

            data.pathMesh = new PathMesh();
            PathNode node01 = new PathNode(0, 0);
            PathNode node02 = new PathNode(1, 1);
            node01.AddNeighbor(node02);
            node02.AddNeighbor(node01);

			data.background = MainGame.tex_bg_castle;
            data.music = MainGame.mus_castle;

			return data;
		}

		public static MapData LoadPillar(World w)
		{
            MapData data = new MapData();
            data.walls = new List<Wall>();
            data.walls.Add(new Wall(w, 960, 850, 1530, 130, 0));
            data.walls.Add(new Wall(w, 960, 969, 1406, 222, 0));
            data.walls.Add(new Wall(w, 980, -200, 1404, 128, 0));
            data.walls.Add(new Wall(w, 980, -300, 1404, 128, 0));

            data.spawnPoints = new Vector2[4];
            data.spawnPoints[0] = new Vector2(500, 640);
            data.spawnPoints[1] = new Vector2(820, 640);
            data.spawnPoints[2] = new Vector2(1120, 640);
            data.spawnPoints[3] = new Vector2(1420, 640);

            data.ammoPoints = new Vector3[1];
            data.ammoPoints[0] = new Vector3(960, 440, 1800);

            data.pathMesh = new PathMesh();
            PathNode node01 = new PathNode(0, 0);
            PathNode node02 = new PathNode(1, 1);
            node01.AddNeighbor(node02);
            node02.AddNeighbor(node01);

			data.background = MainGame.tex_bg_pillar;
			data.music = MainGame.mus_pillar;
            
            return data;
		}

		public static MapData LoadOctopus(World w)
		{
            MapData data = new MapData();
			data.walls = new List<Wall>();
            data.walls.Add(new Wall(w, 1695, 453, 244, 1562, 0));
            data.walls.Add(new Wall(w, 326, 453, 200, 1458, 0));
            data.walls.Add(new Wall(w, 1008, 914, 1296, 240, 0));
            data.walls.Add(new Wall(w, 485, 537, 141, 75, 0));
            data.walls.Add(new Wall(w, 1443, 520, 342, 88, 0));
            data.walls.Add(new Wall(w, 980, -200, 1404, 128, 0));

            data.spawnPoints = new Vector2[4];
            data.spawnPoints[0] = new Vector2(682, 692);
            data.spawnPoints[1] = new Vector2(1216, 692);
            data.spawnPoints[2] = new Vector2(1496, 319);
            data.spawnPoints[3] = new Vector2(508, 389);

            data.ammoPoints = new Vector3[1];
            data.ammoPoints[0] = new Vector3(1000, 300, 1800);

            data.pathMesh = new PathMesh();
            PathNode node01 = new PathNode(0, 0);
            PathNode node02 = new PathNode(1, 1);
            node01.AddNeighbor(node02);
            node02.AddNeighbor(node01);

			data.background = MainGame.tex_bg_octopus;
			data.music = MainGame.mus_octopus;

			return data;
		}

		public static MapData LoadGraveyard(World w)
		{
            MapData data = new MapData();
			data.walls = new List<Wall>();
            data.walls.Add(new Wall(w, 965, 974, 1958, 223, 0));
            data.walls.Add(new Wall(w, -12, 476, 24, 1000, 0));
            data.walls.Add(new Wall(w, 1932, 476, 24, 1000, 0));
            data.walls.Add(new Wall(w, 494, 614, 278, 26, 0));
            data.walls.Add(new Wall(w, 1569, 812, 241, 281, -MathHelper.Pi / 6));
            data.walls.Add(new Wall(w, 1497, 684, 229, 86, -MathHelper.Pi / 180 * 11.8f));
            data.walls.Add(new Wall(w, 839, 368, 114, 21, 0));
            data.walls.Add(new Wall(w, 980, -200, 1404, 128, 0));

            data.spawnPoints = new Vector2[4];
            data.spawnPoints[0] = new Vector2(501, 741);
            data.spawnPoints[1] = new Vector2(1468, 554);
            data.spawnPoints[2] = new Vector2(501, 455);
            data.spawnPoints[3] = new Vector2(973, 735);

            data.ammoPoints = new Vector3[1];
            data.ammoPoints[0] = new Vector3(837, 292, 1800);

            data.pathMesh = new PathMesh();
            PathNode node01 = new PathNode(0, 0);
            PathNode node02 = new PathNode(1, 1);
            node01.AddNeighbor(node02);
            node02.AddNeighbor(node01);

			data.background = MainGame.tex_bg_graveyard;
			data.music = MainGame.mus_graveyard;
            
			return data;
		}

		public static MapData LoadClocktower(World w)
		{
			/////
			// These are all wrong
			/////
            MapData data = new MapData();
			data.walls = new List<Wall>();
            data.walls.Add(new Wall(w, -12, 476, 24, 1000, 0));
            data.walls.Add(new Wall(w, 1932, 476, 24, 1000, 0));
            data.walls.Add(new Wall(w, 751, 443, 312, 46, 0));
            data.walls.Add(new RoundWall(w, 758, 999, 114));
            data.walls.Add(new RoundWall(w, -30, 1230, 633));
            data.walls.Add(new RoundWall(w, 573, 1044, 88));
            data.walls.Add(new RoundWall(w, 991, 914, 152));
            data.walls.Add(new RoundWall(w, 1487, 1047, 227));
            data.walls.Add(new RoundWall(w, 1873, 787, 257));
            data.walls.Add(new RoundWall(w, 1196, 1051, 93));
            data.walls.Add(new Wall(w, 980, -200, 1404, 128, 0));

            data.spawnPoints = new Vector2[4];
            data.spawnPoints[0] = new Vector2(586, 654);
            data.spawnPoints[1] = new Vector2(1516, 589);
            data.spawnPoints[2] = new Vector2(981, 580);
            data.spawnPoints[3] = new Vector2(758, 327);

            data.ammoPoints = new Vector3[1];
            data.ammoPoints[0] = new Vector3(1285, 427, 1800);

            data.pathMesh = new PathMesh();
            PathNode node01 = new PathNode(0, 0);
            PathNode node02 = new PathNode(1, 1);
            node01.AddNeighbor(node02);
            node02.AddNeighbor(node01);

			data.background = MainGame.tex_bg_clocktower;
			data.music = MainGame.mus_clocktower;

			return data;
		}

		public static MapData LoadCircus(World w)
		{
            MapData data = new MapData();
			data.walls = new List<Wall>();
            data.walls.Add(new Wall(w, -12, 476, 24, 1000, 0));
            data.walls.Add(new Wall(w, 1932, 476, 24, 1000, 0));
            data.walls.Add(new Wall(w, 1038, 960, 2174, 282, 0));
            data.walls.Add(new Wall(w, 933, 729, 525, 164, 0));
            data.walls.Add(new Wall(w, 1390, 481, 139, 31, 0));
            data.walls.Add(new Wall(w, 980, -200, 1404, 128, 0));

            data.spawnPoints = new Vector2[4];
            data.spawnPoints[0] = new Vector2(250, 690);
            data.spawnPoints[1] = new Vector2(1422, 348);
            data.spawnPoints[2] = new Vector2(550, 690);
            data.spawnPoints[3] = new Vector2(1370, 690);

            data.ammoPoints = new Vector3[1];
            data.ammoPoints[0] = new Vector3(950, 405, 1800);

            data.pathMesh = new PathMesh();
            PathNode node01 = new PathNode(0, 0);
            PathNode node02 = new PathNode(1, 1);
            node01.AddNeighbor(node02);
            node02.AddNeighbor(node01);

            data.background = MainGame.tex_bg_circus;
            data.music = MainGame.mus_circus;
            data.foreground = MainGame.tex_fg_circus;

			return data;
		}
	}
}
