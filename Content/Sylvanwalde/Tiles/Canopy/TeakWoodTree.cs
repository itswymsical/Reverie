using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Enums;

namespace Reverie.Content.Sylvanwalde.Tiles.Canopy
{
	public class TeakWoodTree : ModTree
	{
		public override TreePaintingSettings TreeShaderSettings => new TreePaintingSettings {
			UseSpecialGroups = true,
			SpecialGroupMinimalHueValue = 11f / 72f,
			SpecialGroupMaximumHueValue = 0.25f,
			SpecialGroupMinimumSaturationValue = 0.88f,
			SpecialGroupMaximumSaturationValue = 1f
		};
		public override void SetStaticDefaults() => GrowsOnTileId = [ModContent.TileType<LoamGrassTile>()];

		public override Asset<Texture2D> GetTexture() => ModContent.Request<Texture2D>($"{Assets.Sylvanwalde.Tiles.Canopy}TeakWoodTree");
        public override TreeTypes CountsAsTreeType => TreeTypes.Forest;
        public override int SaplingGrowthType(ref int style) {
			style = 0;
			return ModContent.TileType<TeakWoodSapling>();
		}
        public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight) {
			topTextureFrameWidth = 124;
			topTextureFrameHeight = 98;

		}
		public override Asset<Texture2D> GetBranchTextures() => ModContent.Request<Texture2D>($"{Assets.Sylvanwalde.Tiles.Canopy}TeakWoodTree_Branches");
		public override Asset<Texture2D> GetTopTextures() => ModContent.Request<Texture2D>($"{Assets.Sylvanwalde.Tiles.Canopy}TeakWoodTree_Tops");	
		public override int DropWood() => ItemID.Wood;
		
	}
}