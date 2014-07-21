using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using FarseerPhysics.Dynamics;

namespace Badminton
{
	public class Map
	{
		public static Dictionary<Texture2D, string> MapKeys;

		public static object[] LoadMap(World w, string name)
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

		public static object[] LoadCastle(World w)
		{
			List<Wall> walls = new List<Wall>();
			walls.Add(new Wall(w, 980, 880, 1404, 128, 0));
			walls.Add(new Wall(w, 319, 368, 152, 976, 0));
			walls.Add(new Wall(w, 1564, 359, 108, 915, 0));
			walls.Add(new Wall(w, 980, -200, 1404, 128, 0));
			walls.Add(new Wall(w, 450, 518, 209, 47, 0));
			walls.Add(new Wall(w, 1402, 518, 216, 55, 0));
			walls.Add(new Wall(w, 963, 256, 172, 44, 0));
			
			Vector2[] spawnPoints = new Vector2[4];
			spawnPoints[0] = new Vector2(468, 366);
			spawnPoints[1] = new Vector2(1383, 375);
			spawnPoints[2] = new Vector2(486, 699);
			spawnPoints[3] = new Vector2(1380, 704);

			Vector3[] ammoPoints = new Vector3[1];
			ammoPoints[0] = new Vector3(960, 170, 1800); // (x, y, respawn time)

			object[] map = new object[5];
			map[0] = MainGame.tex_bg_castle;
			map[1] = walls;
			map[2] = spawnPoints;
			map[3] = ammoPoints;
			map[4] = MainGame.mus_castle;

			return map;
		}

		public static object[] LoadPillar(World w)
		{
			List<Wall> walls = new List<Wall>();
			walls.Add(new Wall(w, 960, 850, 1530, 130, 0));
			walls.Add(new Wall(w, 960, 969, 1406, 222, 0));
			walls.Add(new Wall(w, 980, -200, 1404, 128, 0));

			Vector2[] spawnPoints = new Vector2[4];
			spawnPoints[0] = new Vector2(500, 640);
			spawnPoints[1] = new Vector2(820, 640);
			spawnPoints[2] = new Vector2(1120, 640);
			spawnPoints[3] = new Vector2(1420, 640);
						walls.Add(new Wall(w, 980, -300, 1404, 128, 0));

			Vector3[] ammoPoints = new Vector3[1];
			ammoPoints[0] = new Vector3(960, 440, 1800);

			object[] map = new object[5];
			map[0] = MainGame.tex_bg_pillar;
			map[1] = walls;
			map[2] = spawnPoints;
			map[3] = ammoPoints;
			map[4] = MainGame.mus_pillar;

			return map;
		}

		public static object[] LoadOctopus(World w)
		{
			List<Wall> walls = new List<Wall>();
			walls.Add(new Wall(w, 1695, 453, 244, 1562, 0));
			walls.Add(new Wall(w, 326, 453, 200, 1458, 0));
			walls.Add(new Wall(w, 1008, 914, 1296, 240, 0));
			walls.Add(new Wall(w, 485, 537, 141, 75, 0));
			walls.Add(new Wall(w, 1443, 520, 342, 88, 0));
			walls.Add(new Wall(w, 980, -200, 1404, 128, 0));

			Vector2[] spawnPoints = new Vector2[4];
			spawnPoints[0] = new Vector2(682, 692);
			spawnPoints[1] = new Vector2(1216, 692);
			spawnPoints[2] = new Vector2(1496, 319);
			spawnPoints[3] = new Vector2(508, 389);

			Vector3[] ammoPoints = new Vector3[1];
			ammoPoints[0] = new Vector3(1000, 300, 1800);

			object[] map = new object[5];
			map[0] = MainGame.tex_bg_octopus;
			map[1] = walls;
			map[2] = spawnPoints;
			map[3] = ammoPoints;
			map[4] = MainGame.mus_octopus;

			return map;
		}

		public static object[] LoadGraveyard(World w)
		{
			List<Wall> walls = new List<Wall>();
			walls.Add(new Wall(w, 965, 974, 1958, 223, 0));
			walls.Add(new Wall(w, -12, 476, 24, 1000, 0));
			walls.Add(new Wall(w, 1932, 476, 24, 1000, 0));
			walls.Add(new Wall(w, 494, 614, 278, 26, 0));
			walls.Add(new Wall(w, 1569, 812, 241, 281, -MathHelper.Pi / 6));
			walls.Add(new Wall(w, 1497, 684, 229, 86, -MathHelper.Pi / 180 * 11.8f));
			walls.Add(new Wall(w, 839, 368, 114, 21, 0));
			walls.Add(new Wall(w, 980, -200, 1404, 128, 0));

			Vector2[] spawnPoints = new Vector2[4];
			spawnPoints[0] = new Vector2(501, 741);
			spawnPoints[1] = new Vector2(1468, 554);
			spawnPoints[2] = new Vector2(501, 455);
			spawnPoints[3] = new Vector2(973, 735);

			Vector3[] ammoPoints = new Vector3[1];
			ammoPoints[0] = new Vector3(837, 292, 1800);

			object[] map = new object[5];
			map[0] = MainGame.tex_bg_graveyard;
			map[1] = walls;
			map[2] = spawnPoints;
			map[3] = ammoPoints;
			map[4] = MainGame.mus_graveyard;

			return map;
		}

		public static object[] LoadClocktower(World w)
		{
			/////
			// These are all wrong
			/////
			List<Wall> walls = new List<Wall>();
			walls.Add(new Wall(w, -12, 476, 24, 1000, 0));
			walls.Add(new Wall(w, 1932, 476, 24, 1000, 0));
			walls.Add(new Wall(w, 751, 443, 312, 46, 0));
			walls.Add(new RoundWall(w, 758, 999, 114));
			walls.Add(new RoundWall(w, -30, 1230, 633));
			walls.Add(new RoundWall(w, 573, 1044, 88));
			walls.Add(new RoundWall(w, 991, 914, 152));
			walls.Add(new RoundWall(w, 1487, 1047, 227));
			walls.Add(new RoundWall(w, 1873, 787, 257));
			walls.Add(new RoundWall(w, 1196, 1051, 93));
			walls.Add(new Wall(w, 980, -200, 1404, 128, 0));

			Vector2[] spawnPoints = new Vector2[4];
			spawnPoints[0] = new Vector2(586, 654);
			spawnPoints[1] = new Vector2(1516, 589);
			spawnPoints[2] = new Vector2(981, 580);
			spawnPoints[3] = new Vector2(758, 327);

			Vector3[] ammoPoints = new Vector3[1];
			ammoPoints[0] = new Vector3(1285, 427, 1800);

			object[] map = new object[5];
			map[0] = MainGame.tex_bg_clocktower;
			map[1] = walls;
			map[2] = spawnPoints;
			map[3] = ammoPoints;
			map[4] = MainGame.mus_clocktower;

			return map;
		}

		public static object[] LoadCircus(World w)
		{
			List<Wall> walls = new List<Wall>();
			walls.Add(new Wall(w, -12, 476, 24, 1000, 0));
			walls.Add(new Wall(w, 1932, 476, 24, 1000, 0));
			walls.Add(new Wall(w, 1038, 960, 2174, 282, 0));
			walls.Add(new Wall(w, 933, 729, 525, 164, 0));
			walls.Add(new Wall(w, 1390, 481, 139, 31, 0));
			walls.Add(new Wall(w, 980, -200, 1404, 128, 0));

			Vector2[] spawnPoints = new Vector2[4];
			spawnPoints[0] = new Vector2(250, 690);
			spawnPoints[1] = new Vector2(1422, 348);
			spawnPoints[2] = new Vector2(550, 690);
			spawnPoints[3] = new Vector2(1370, 690);

			Vector3[] ammoPoints = new Vector3[1];
			ammoPoints[0] = new Vector3(950, 405, 1800);

			object[] map = new object[6];
			map[0] = MainGame.tex_bg_circus;
			map[1] = walls;
			map[2] = spawnPoints;
			map[3] = ammoPoints;
			map[4] = MainGame.mus_circus;
			map[5] = MainGame.tex_fg_circus;

			return map;
		}
	}
}
