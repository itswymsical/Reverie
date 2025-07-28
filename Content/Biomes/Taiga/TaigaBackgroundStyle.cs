using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Reverie.Content.Biomes.Taiga;

public class TaigaBackgroundStyle : ModSurfaceBackgroundStyle
{
    public override void ModifyFarFades(float[] fades, float transitionSpeed)
    {
        for (var i = 0; i < fades.Length; i++)
        {
            if (i == Slot)
            {
                fades[i] += transitionSpeed;
                if (fades[i] > 1f)
                {
                    fades[i] = 1f;
                }
            }
            else
            {
                fades[i] -= transitionSpeed;
                if (fades[i] < 0f)
                {
                    fades[i] = 0f;
                }
            }
        }
    }

    public override int ChooseFarTexture()
    {
        return 277;
    }

    public override int ChooseMiddleTexture()
    {
        return 281;
    }

    public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b)
    {
        return 278;
    }
}