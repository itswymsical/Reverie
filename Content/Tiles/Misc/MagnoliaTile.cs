using System.Collections.Generic;
using Terraria.GameContent.Metadata;
using Terraria.Localization;
using Terraria.ObjectData;
using Reverie.Content.Items.Botany;

namespace Reverie.Content.Tiles.Misc
{
	public enum PlantStage : byte
	{
		Planted,
		Growing,
		Grown
	}

	public class MagnoliaTile : ModTile
	{
		private const int FrameWidth = 18;

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] =
				Main.tileObsidianKill[Type] =
				Main.tileCut[Type] =
				Main.tileNoFail[Type] = true;

			TileID.Sets.ReplaceTileBreakUp[Type] =
				TileID.Sets.IgnoredInHouseScore[Type] =
				TileID.Sets.IgnoredByGrowingSaplings[Type] = true;

			TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

			var name = CreateMapEntryName();
			AddMapEntry(new Color(128, 128, 128), name);

			TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
			TileObjectData.newTile.AnchorValidTiles = [
				TileID.Grass,
				TileID.LivingWood,
			];

			TileObjectData.newTile.AnchorAlternateTiles = [
				TileID.ClayPot,
				TileID.PlanterBox
			];

			TileObjectData.addTile(Type);

			HitSound = SoundID.Grass;
			DustType = DustID.BubbleBurst_White;
		}

		public override bool CanPlace(int i, int j)
		{
			var tile = Framing.GetTileSafely(i, j);

			if (tile.HasTile)
			{
				int tileType = tile.TileType;
				if (tileType == Type)
				{
					var stage = GetStage(i, j);
					return stage == PlantStage.Grown;
				}

				else
				{
					if (Main.tileCut[tileType] || TileID.Sets.BreakableWhenPlacing[tileType] || tileType == TileID.WaterDrip || tileType == TileID.LavaDrip || tileType == TileID.HoneyDrip || tileType == TileID.SandDrip)
					{
						var foliageGrass = tileType == TileID.Plants || tileType == TileID.Plants2;
						var moddedFoliage = tileType >= TileID.Count && (Main.tileCut[tileType] || TileID.Sets.BreakableWhenPlacing[tileType]);
						var harvestableVanillaHerb = Main.tileAlch[tileType] && WorldGen.IsHarvestableHerbWithSeed(tileType, tile.TileFrameX / 18);

						if (foliageGrass || moddedFoliage || harvestableVanillaHerb)
						{
							WorldGen.KillTile(i, j);
							if (!tile.HasTile && Main.netMode == NetmodeID.MultiplayerClient)
							{
								NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j);
							}

							return true;
						}
					}

					return false;
				}
			}

			return true;
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects)
		{
			if (i % 2 == 0)
			{
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
			=> offsetY = -2;

		public override bool CanDrop(int i, int j)
		{
			var stage = GetStage(i, j);

			if (stage == PlantStage.Planted)
				return false;

			return true;
		}

		public override IEnumerable<Item> GetItemDrops(int i, int j)
		{
			var stage = GetStage(i, j);

			var worldPosition = new Vector2(i, j).ToWorldCoordinates();
			var nearestPlayer = Main.player[Player.FindClosest(worldPosition, 16, 16)];

			var herbItemType = ModContent.ItemType<MagnoliaItem>();
			var herbItemStack = 1;

			if (nearestPlayer.active && (nearestPlayer.HeldItem.type == ItemID.StaffofRegrowth || nearestPlayer.HeldItem.type == ItemID.AcornAxe))
			{
				herbItemStack = Main.rand.Next(1, 3);
			}
			else if (stage == PlantStage.Grown)
			{
				herbItemStack = 1;
			}

			if (herbItemType > 0 && herbItemStack > 0)
			{
				yield return new Item(herbItemType, herbItemStack);
			}
		}

		public override bool IsTileSpelunkable(int i, int j)
		{
			var stage = GetStage(i, j);

			return stage == PlantStage.Grown;
		}

		public override void RandomUpdate(int i, int j)
		{
			var tile = Framing.GetTileSafely(i, j);
			var stage = GetStage(i, j);

			if (stage != PlantStage.Grown)
			{
				tile.TileFrameX += FrameWidth;


				if (Main.netMode != NetmodeID.SinglePlayer)
				{
					NetMessage.SendTileSquare(-1, i, j, 1);
				}
			}
		}

		private static PlantStage GetStage(int i, int j)
		{
			var tile = Framing.GetTileSafely(i, j);
			return (PlantStage)(tile.TileFrameX / FrameWidth);
		}
	}
}