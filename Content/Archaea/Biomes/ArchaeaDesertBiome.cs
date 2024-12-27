using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using Reverie.Common.Systems.WorldGeneration;
using ReverieMusic;
using Terraria.ID;
using Reverie.Common.Systems.Subworlds.Archaea;
using SubworldLibrary;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.GameContent.Shaders;

namespace Reverie.Content.Archaea.Biomes
{
    public class ArchaeaDesertBiome : ModBiome
	{
        public override int Music => MusicID.OtherworldlyDesert;
        public override SceneEffectPriority Priority => SceneEffectPriority.BiomeLow;
        public override string BestiaryIcon => base.BestiaryIcon;
		public override string BackgroundPath => base.BackgroundPath;
        public override Color? BackgroundColor => Color.SandyBrown;

        public override bool IsBiomeActive(Player player)
            => player.ZoneDesert && SubworldSystem.IsActive<ArchaeaSubworld>();
	}
}
