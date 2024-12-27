using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Enums;
using Reverie.Content.Terraria.Tiles.Canopy;

namespace Reverie.Content.Tiles.Canopy
{
	public class AlderwoodTree : ModTree
	{
		// This is a blind copy-paste from Vanilla's PurityPalmTree settings.
		// TODO: This needs some explanations
		public override TreePaintingSettings TreeShaderSettings => new TreePaintingSettings {
			UseSpecialGroups = true,
			SpecialGroupMinimalHueValue = 11f / 72f,
			SpecialGroupMaximumHueValue = 0.25f,
			SpecialGroupMinimumSaturationValue = 0.88f,
			SpecialGroupMaximumSaturationValue = 1f
		};
		public override void SetStaticDefaults() => GrowsOnTileId = [ModContent.TileType<Woodgrass>()];

		// This is the primary texture for the trunk. Branches and foliage use different settings.
		public override Asset<Texture2D> GetTexture() => ModContent.Request<Texture2D>($"{Assets.Terraria.Tiles.Canopy}AlderwoodTree");
        public override TreeTypes CountsAsTreeType => TreeTypes.Ash;
        public override int SaplingGrowthType(ref int style) {
			style = 0;
			return ModContent.TileType<AlderwoodSapling>();
		}
        public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight) {
			topTextureFrameWidth = 124;
			topTextureFrameHeight = 98;

		}
		public override Asset<Texture2D> GetBranchTextures() => ModContent.Request<Texture2D>($"{Assets.Terraria.Tiles.Canopy}AlderwoodTree_Branches");
		public override Asset<Texture2D> GetTopTextures() => ModContent.Request<Texture2D>($"{Assets.Terraria.Tiles.Canopy}AlderwoodTree_Tops");	
		public override int DropWood() => ItemID.Wood;
		
	}
}