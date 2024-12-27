using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using Reverie.Common.Systems.WorldGeneration;
using ReverieMusic;
using static Reverie.Common.Systems.WorldGeneration.WoodlandCanopy.CanopyGeneration;
using Reverie.Common.Systems.WorldGeneration.WoodlandCanopy;

namespace Reverie.Content.Biomes
{
    public class WoodlandCanopyBiome : ModBiome
	{
        public override int Music => MusicLoader.GetMusicSlot(ReverieMusic.ReverieMusic.Instance, $"{Assets.Music}Woodhaven");
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
        public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.GetInstance<CanopyBackgroundStyle>();
        public override string BestiaryIcon => base.BestiaryIcon;
		public override string BackgroundPath => base.BackgroundPath;
        public override Color? BackgroundColor => new Color(95, 143, 65);

        public override bool IsBiomeActive(Player player)
        {
            int canopyTop = canopyY - canopyRadiusV;
            int canopyBottom = canopyY + canopyRadiusV;

            int canopyMiddle = (canopyTop + canopyBottom) / 2;

            Point playerTile = player.position.ToTileCoordinates();

            bool inCanopyX = Math.Abs(playerTile.X - canopyX) <= canopyRadiusH;

            bool inUpperHalfY = playerTile.Y >= canopyTop && playerTile.Y < canopyMiddle;

            bool belowDirtLayer = playerTile.Y > Main.worldSurface + 20;

            bool enoughCanopyBlocks = ModContent.GetInstance<TileCountSystem>().canopyBlockCount >= 100;

            return inCanopyX && inUpperHalfY && belowDirtLayer && enoughCanopyBlocks;
        }
    }
    public class LowerCanopyBiome : ModBiome
    {
        public override int Music => MusicLoader.GetMusicSlot(ReverieMusic.ReverieMusic.Instance, $"{Assets.Music}LowerCanopy");
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
        public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.GetInstance<CanopyBackgroundStyle>();
        public override string BestiaryIcon => base.BestiaryIcon;
        public override string BackgroundPath => base.BackgroundPath;
        public override Color? BackgroundColor => new Color(95, 143, 65);
        public override bool IsBiomeActive(Player player)
        {
            int canopyTop = canopyY - canopyRadiusV;
            int canopyBottom = canopyY + canopyRadiusV;

            int canopyMiddle = (canopyTop + canopyBottom) / 2;

            Point playerTile = player.position.ToTileCoordinates();

            bool inCanopyX = Math.Abs(playerTile.X - canopyX) <= canopyRadiusH;

            bool inLowerHalfY = playerTile.Y >= canopyMiddle && playerTile.Y <= canopyBottom;

            bool enoughCanopyBlocks = ModContent.GetInstance<TileCountSystem>().canopyBlockCount >= 100;

            return inCanopyX && inLowerHalfY && enoughCanopyBlocks;
        }
    }
}
