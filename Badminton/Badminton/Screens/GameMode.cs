using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using FarseerPhysics.Dynamics;
using Badminton.Stick_Figures;
using Badminton.Attacks;


namespace Badminton.Screens
{
    public abstract class GameMode
    {
        protected World world;
        protected List<Wall> walls;

        protected StickFigure[] player;
        protected Vector2[] spawnPoints;
        protected TrapAmmo[] ammo;
        protected Texture2D background, foreground;
        protected Song music;
        protected PlayerValues[] info;
        protected bool timed;
        protected int timeRemaining; // milliseconds

        // Initial values
        protected bool enterPressed = true;
        protected bool gameOver = false;
        protected List<int> winners = new List<int>();
        protected List<StickFigure> winSticks = new List<StickFigure>();
        protected int startPause = 2500; // milliseconds
        protected static Category[] Categories = new Category[] { Category.Cat1, Category.Cat2, Category.Cat3, Category.Cat4 };
        protected static PlayerIndex[] Players = new PlayerIndex[] { PlayerIndex.One, PlayerIndex.Two, PlayerIndex.Three, PlayerIndex.Four };

        public GameMode(Color[] colors, string mapString, float gravity, int lives, float limbStrength, bool suddenDeath, bool traps, bool longRange, bool bots)
        {
            world = new World(new Vector2(0, gravity));

            MapData data = Map.LoadMap(world, mapString);
            background = data.background;
            walls = data.walls;
            spawnPoints = data.spawnPoints;
            Vector3[] ammoPoints = data.ammoPoints;
            ammo = new TrapAmmo[ammoPoints.Length];
            if (traps)
                for (int i = 0; i < ammoPoints.Length; i++)
                    ammo[i] = new TrapAmmo(world, new Vector2(ammoPoints[i].X, ammoPoints[i].Y) * MainGame.PIXEL_TO_METER, (int)ammoPoints[i].Z);
            music = data.music;
            MediaPlayer.Play(music);
            foreground = data.foreground;

            StickFigure.AllowTraps = traps;
            StickFigure.AllowLongRange = longRange;

            player = new StickFigure[bots ? 4 : colors.Length];
            info = new PlayerValues[bots ? 4 : colors.Length];

            for (int i = 0; i < colors.Length; i++)
                if (colors[i] != null)
                {
                    player[i] = new LocalPlayer(world, spawnPoints[i] * MainGame.PIXEL_TO_METER, Category.Cat1, 1.5f, limbStrength, suddenDeath ? 0.001f : 1f, false, colors[i], Players[i]);
                    player[i].LockControl = true;
                }

            for (int i = 0; i < info.Length; i++)
                info[i] = new PlayerValues(lives);
        }
    }

}
