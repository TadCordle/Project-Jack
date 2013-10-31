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

			object[] map = new object[3];
			map[0] = MainGame.tex_bg_castle;
			map[1] = walls;
			map[2] = spawnPoints;

			return map;
		}
	}
}
