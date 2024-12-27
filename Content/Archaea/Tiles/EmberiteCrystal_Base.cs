using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.Audio;
using System;

namespace Reverie.Content.Archaea.Tiles
{
	public abstract class EmberiteCrystal_Base : ModTile
	{
        public sealed override void SetStaticDefaults()
        {
			Main.tileLighted[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileFrameImportant[Type] = true;
            MineResist = 1.35f;
            MinPick = 60;
            HitSound = SoundID.DD2_CrystalCartImpact;
            TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
			TileID.Sets.IgnoresNearbyHalfbricksWhenDrawn[Type] = true;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
            SafeSetDefaults();
			Main.tileLavaDeath[Type] = false;
			AddMapEntry(new Color(230, 200, 50));

            DustType = DustID.Lava;
		}

		public virtual void SafeSetDefaults() { }

		public override void NumDust(int i, int j, bool fail, ref int num)
            => num = fail ? 3 : 6;

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.351f;
			g = 0.3f;
			b = 0.078f;
		}
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Vector2 center = new Vector2(i * 16 + 16, j * 16 + 16); // Center of the tile
            for (int num = 0; num < 16; num++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(1.5f, 1.5f);
                Dust dust = Dust.NewDustPerfect(center, DustID.SolarFlare, speed * Main.rand.NextFloat(0.8f, 1.2f), Scale: Main.rand.NextFloat(.2f, .3f));
                dust.noGravity = false;
                dust.fadeIn = 1f;
            }
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (!fail)
            {
                Vector2 center = new Vector2(i * 16 + 8, j * 16 + 8);
                for (int num = 0; num < 12; num++)
                {
                    Vector2 speed = Main.rand.NextVector2CircularEdge(1.5f, 1.5f);
                    Dust dust = Dust.NewDustPerfect(center, DustID.SolarFlare, speed * Main.rand.NextFloat(0.8f, 1.2f), Scale: Main.rand.NextFloat(.2f, .3f));
                    dust.noGravity = false;
                    dust.fadeIn = 1f;
                }
            }
        }
        public override bool KillSound(int i, int j, bool fail)
        {
            SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, new Vector2(i * 16, j * 16));
            return base.KillSound(i, j, fail);
        }
        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (Main.rand.NextBool(450))
            {
                Dust dust = Main.dust[Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, DustID.FlameBurst)];
                dust.fadeIn = 0.2f;
                dust.scale = 0.07f;
                dust.position += dust.velocity;
                dust.rotation += dust.velocity.X * 0.1f;
                dust.velocity.X += (float)Math.Sin(Main.GameUpdateCount / 20) * 0.01f;
                dust.velocity.X = MathHelper.Clamp(dust.velocity.X, -0.5f, 0.5f);

                dust.velocity.Y -= 0.08f;
                dust.velocity.Y = Math.Max(dust.velocity.Y, -1f);
                dust.scale -= 0.001f;
                if (dust.scale < 0.04f)
                {
                    dust.active = false;
                }
            }
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			var shineShader = Filters.Scene["DustifiedCrystalShine"];
			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, shineShader.GetShader().Shader);


			if (shineShader != null)
			{
				Effect effect = shineShader.GetShader().Shader;
				if (effect != null)
				{
					effect.Parameters["uTime"]?.SetValue((float)Main.GameUpdateCount * 0.02f);
					effect.Parameters["uOpacity"]?.SetValue(0.8f);
				}
			}
			return true;
		}
		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default);
		}
	}
	public sealed class EmberiteCrystal_Large1 : EmberiteCrystal_Base
    {
        public override string Texture => Assets.Archaea.Tiles.Emberite + Name;
        public override void SafeSetDefaults()
		{
            Main.tileLavaDeath[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
			TileObjectData.newTile.Height = 5;
			TileObjectData.newTile.Origin = new Point16(0, 0);
			TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 18];
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 2, 1);

			TileObjectData.newTile.StyleWrapLimit = 2;
			TileObjectData.newTile.StyleMultiplier = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addAlternate(1);

            TileObjectData.addTile(Type);
		}
	}

	public sealed class EmberiteCrystal_Large2 : EmberiteCrystal_Base
    {
        public override string Texture => Assets.Archaea.Tiles.Emberite + Name;
        public override void SafeSetDefaults()
		{
            Main.tileLavaDeath[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style5x4);
			TileObjectData.newTile.Origin = new Point16(0, 0);
			TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 18];
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 2, 1);

			TileObjectData.newTile.StyleWrapLimit = 2;
			TileObjectData.newTile.StyleMultiplier = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 2, 2);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addAlternate(1);

            TileObjectData.addTile(Type);
		}
	}

	public sealed class EmberiteCrystal_Large3 : EmberiteCrystal_Base
    {
        public override string Texture => Assets.Archaea.Tiles.Emberite + Name;
        public override void SafeSetDefaults()
		{
            Main.tileLavaDeath[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style4x2);
			TileObjectData.newTile.Height = 4;
			TileObjectData.newTile.Origin = new Point16(0, 0);
			TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 18];
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 2, 0);

			TileObjectData.newTile.StyleWrapLimit = 2;
			TileObjectData.newTile.StyleMultiplier = 2;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;

			TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
			TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
			TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile, 2, 2);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addAlternate(1);
            TileObjectData.addTile(Type);
		}
	}
    public sealed class EmberiteCrystal_Medium1 : EmberiteCrystal_Base
    {
        public override string Texture => Assets.Archaea.Tiles.Emberite + Name;
        public override void SafeSetDefaults()
        {
            Main.tileLavaDeath[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 18 };
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 3, 0);

            TileObjectData.newTile.StyleWrapLimit = 2;
            TileObjectData.newTile.StyleMultiplier = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;

            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addAlternate(1);

            TileObjectData.addTile(Type);
        }
    }
    public sealed class EmberiteCrystal_Medium2 : EmberiteCrystal_Base
    {
        public override string Texture => Assets.Archaea.Tiles.Emberite + Name;
        public override void SafeSetDefaults()
        {
            Main.tileLavaDeath[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 18 };
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 3, 0);

            TileObjectData.newTile.StyleWrapLimit = 2;
            TileObjectData.newTile.StyleMultiplier = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;

            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addAlternate(1);

            TileObjectData.addTile(Type);
        }
    }
    public sealed class EmberiteCrystal_Medium3 : EmberiteCrystal_Base
    {
        public override string Texture => Assets.Archaea.Tiles.Emberite + Name;
        public override void SafeSetDefaults()
        {
            Main.tileLavaDeath[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 18 };
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 3, 0);

            TileObjectData.newTile.StyleWrapLimit = 2;
            TileObjectData.newTile.StyleMultiplier = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.Direction = TileObjectDirection.PlaceRight;

            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addAlternate(1);

            TileObjectData.addTile(Type);
        }
    }
}
