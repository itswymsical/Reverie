using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using Reverie.Common.Systems.WorldGeneration;
using ReverieMusic;
using Terraria.ID;
using Reverie.Common.Systems.Subworlds.Archaea;
using SubworldLibrary;
using Terraria.GameContent;

namespace Reverie.Content.Archaea.Biomes
{
    public class EmberiteCavernsBiome : ModBiome
	{
        public override int Music => MusicLoader.GetMusicSlot(ReverieMusic.ReverieMusic.Instance, $"{Assets.Music}EmberiteCaverns");
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeMedium;
        public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.GetInstance<EmberiteUGBackgroundStyle>();
        public override string BestiaryIcon => base.BestiaryIcon;
		public override string BackgroundPath => base.BackgroundPath;
        public override Color? BackgroundColor => Color.OrangeRed;

		public override bool IsBiomeActive(Player player)
            => TileCountSystem.Instance.emberiteCavernsBlockCount > 200 && SubworldSystem.IsActive<ArchaeaSubworld>();
	}
}
