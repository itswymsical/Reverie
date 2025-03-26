using Reverie.Utilities;
using Terraria.DataStructures;

namespace Reverie.Common.Players.Animation;

public class WeaponFrontDrawLayer : PlayerDrawLayer
{
    public AnimationPlayer animPlayer = ModContent.GetInstance<AnimationPlayer>();

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        var player = drawInfo.drawPlayer;

        return !player.dead && player.itemAnimation <= 0
            && !player.HeldItem.IsAir && player.HeldItem.DealsDamage();
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        var player = drawInfo.drawPlayer;
        var animPlayer = player.GetModPlayer<AnimationPlayer>();
        animPlayer.DrawWeaponInFront(ref drawInfo);
    }
}
