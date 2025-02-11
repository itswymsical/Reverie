using Terraria.GameContent;

namespace Reverie.Common.Systems;

public class AssetModifierSystem : ModSystem
{
    public override void OnModLoad()
    {
        Main.QueueMainThreadAction(() =>
        {
            var kingSlimeAsset = ModContent.Request<Texture2D>("Reverie/Assets/Textures/NPCs/KingSlime/KingSlime");
            TextureAssets.Npc[NPCID.KingSlime] = kingSlimeAsset;
        });
    }

    public override void Unload()
    {
        TextureAssets.Npc[NPCID.KingSlime] = null;
    }
}