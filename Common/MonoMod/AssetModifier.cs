using Terraria.GameContent;

namespace Reverie.Common.MonoMod;

public class AssetModifier : ModSystem
{
    public override void OnModLoad()
    {
        Main.QueueMainThreadAction(() =>
        {
            var kingSlimeAsset = ModContent.Request<Texture2D>($"{TEXTURE_DIRECTORY}NPCs/Bosses/KingSlime/KingSlime");
            TextureAssets.Npc[NPCID.KingSlime] = kingSlimeAsset;
        });
    }

    public override void Unload()
    {
        TextureAssets.Npc[NPCID.KingSlime] = null;
    }
}