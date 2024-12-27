using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace Reverie.Content.Biomes
{
	public class CanopyBackgroundStyle : ModUndergroundBackgroundStyle
	{
		public override void FillTextureArray(int[] textureSlots) {
			textureSlots[0] = BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/bg0");
			textureSlots[1] = BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/bg1");
            textureSlots[2] = BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/bg2");
			textureSlots[3] = BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/bg3");
		}
	}
}