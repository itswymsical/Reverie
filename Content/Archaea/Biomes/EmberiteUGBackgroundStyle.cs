using Terraria.ModLoader;

namespace Reverie.Content.Archaea.Biomes
{
    public class EmberiteUGBackgroundStyle : ModUndergroundBackgroundStyle
    {
        public override void FillTextureArray(int[] textureSlots)
        {
            textureSlots[0] = BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/EmberiteBG1");
            textureSlots[1] = BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/EmberiteBG1");
            textureSlots[2] = BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/EmberiteBG1");
            textureSlots[3] = BackgroundTextureLoader.GetBackgroundSlot(Mod, "Assets/Backgrounds/EmberiteBG1");
        }
    }
}
