using System;
using Microsoft.Xna.Framework;
using Reverie.Content.Archaea.Tiles.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace Reverie.Content.Archaea.Tiles.RedDesert
{
    public class RedSand : ModTile
    {
        public override string Texture => Assets.Archaea.Tiles.RedDesert + Name;
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBrick[Type] = true;
            Main.tileSand[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;

            TileID.Sets.isDesertBiomeSand[Type] =true;
            TileID.Sets.Conversion.Sand[Type] = true;
            TileID.Sets.ForAdvancedCollision.ForSandshark[Type] = true;
            TileID.Sets.Falling[Type] = true;
			TileID.Sets.CanBeDugByShovel[Type] = true;

            DustType = DustID.CrimsonPlants;
            HitSound = SoundID.Dig;
			MineResist = 0.2f;
            AddMapEntry(new Color(199, 90, 44));
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
            int projectileType = ProjectileType<RedSandBall>();
            if (WorldGen.noTileActions)
				return true;

			Tile above = Main.tile[i, j - 1];
			Tile below = Main.tile[i, j + 1];
			bool canFall = true;

			if (below == null || below.HasTile)
				canFall = false;

			if (above.HasTile && (TileID.Sets.BasicChest[above.TileType] 
				|| TileID.Sets.BasicChestFake[above.TileType] 
				|| above.TileType == TileID.PalmTree))
				canFall = false;

			if (canFall)
			{				
				float x = i * 16 + 8;
				float y = j * 16 + 8;

				if (Main.netMode == NetmodeID.SinglePlayer)
				{
					Main.tile[i, j].ClearTile();
					int proj = Projectile.NewProjectile(default, x, y, 0f, 0.41f, projectileType, 10, 0f, Main.myPlayer);
					Main.projectile[proj].ai[0] = 1f;
					WorldGen.SquareTileFrame(i, j);
				}
				else if (Main.netMode == NetmodeID.Server)
				{
					WorldGen.TileEmpty(i, j);
					bool spawnProj = true;

					for (int k = 0; k < 1000; k++)
					{
						Projectile otherProj = Main.projectile[k];

						if (otherProj.active && otherProj.owner == Main.myPlayer && otherProj.type == projectileType && Math.Abs(otherProj.timeLeft - 3600) < 60 && otherProj.Distance(new Vector2(x, y)) < 4f)
						{
							spawnProj = false;
							break;
						}
					}

					if (spawnProj)
					{
						int proj = Projectile.NewProjectile(default, x, y, 0f, 2.5f, projectileType, 10, 0f, Main.myPlayer);
						Main.projectile[proj].velocity.Y = 0.5f;
						Main.projectile[proj].position.Y += 2f;
						Main.projectile[proj].netUpdate = true;
					}

					NetMessage.SendTileSquare(-1, i, j, 1);
					WorldGen.SquareTileFrame(i, j);
				}
				return false;
			}
			return true;
		}
		public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 2 : 6;
        
    }
}