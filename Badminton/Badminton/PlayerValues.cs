using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Badminton.Stick_Figures;

namespace Badminton
{
	public class PlayerValues
	{
		public int Lives { get; set; }
		public int RespawnTimer { get; set; }

		private const int MAX_RESPAWN_TIME = 15;

		public PlayerValues(int lives)
		{
			Lives = lives;
            RespawnTimer = MAX_RESPAWN_TIME;
		}

        public void ResetTimer()
        {
            RespawnTimer = MAX_RESPAWN_TIME;
        }

		public void Kill()
		{
            Lives--;
            RespawnTimer = MAX_RESPAWN_TIME;
		}

		public bool ShouldRespawn()
		{
			return RespawnTimer <= 0;
		}

		public bool HasLives()
		{
			return Lives > 0;
		}

		// TODO: Replace font with something a little bigger
		public void Draw(SpriteBatch sb, Vector2 position, StickFigure figure)
		{
			sb.Draw(MainGame.tex_blank, position, null, Color.Black, 0f, Vector2.Zero, new Vector2(172, 32), SpriteEffects.None, 0f);
			float health = figure != null ? figure.ScalarHealth : 0f;
			if (health > 1f || health < 0f)
				health = 0f;
			sb.Draw(MainGame.tex_blank, position + Vector2.UnitY + Vector2.UnitX, null, figure.Color, 0f, Vector2.Zero, new Vector2(health * 170, 30), SpriteEffects.None, 0f);
			
			sb.Draw(figure.EvilSkin ? MainGame.tex_evil_head : MainGame.tex_head, position - Vector2.UnitX * 40 - Vector2.UnitY * 10, null, figure.Color, 0f, Vector2.Zero, Vector2.One * 0.55f, SpriteEffects.None, 0f);
			
			sb.Draw(MainGame.tex_heart, position + Vector2.UnitX * 20 + Vector2.UnitY * 35, null, Color.White, 0f, Vector2.Zero, Vector2.One * 0.33f, SpriteEffects.None, 0f);
			MainGame.DrawOutlineText(sb, MainGame.fnt_basicFont, "x" + Lives, position + Vector2.UnitX * 57 + Vector2.UnitY * 40, Color.White);
			sb.Draw(MainGame.tex_trapClosed, position + Vector2.UnitX * 96 + Vector2.UnitY * 37, null, Color.White, 0f, Vector2.Zero, Vector2.One * 1f, SpriteEffects.None, 0f);
			MainGame.DrawOutlineText(sb, MainGame.fnt_basicFont, "x" + figure.TrapAmmo, position + Vector2.UnitX * 128 + Vector2.UnitY * 40, Color.White);
		}
	}
}
