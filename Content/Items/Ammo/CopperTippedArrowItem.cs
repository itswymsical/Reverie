using Reverie.Content.Projectiles.Ammo;
using Reverie.Core.Missions;
using Reverie.Core.Missions.Core;

namespace Reverie.Content.Items.Ammo;

public class CopperTippedArrowItem : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 14;
        Item.height = 36;

        Item.damage = 4;
        Item.DamageType = DamageClass.Ranged;

        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.knockBack = 4.4f;
        Item.value = Item.sellPrice(copper: 22);
        Item.shoot = ModContent.ProjectileType<CopperTippedArrowProj>();
        Item.shootSpeed = 3.2f;
        Item.ammo = AmmoID.Arrow;
    }

    public override void AddRecipes()
    {
        CreateRecipe(25)
        .AddIngredient(ItemID.WoodenArrow, 25)
        .AddIngredient(ItemID.CopperBar, 2)
        .AddTile(TileID.Anvils)
        .AddCondition(new Condition(Instance.GetLocalization("Conditions.CopperStandardComplete"),
        () => Main.LocalPlayer.GetModPlayer<MissionPlayer>()
        .GetMission(MissionID.CopperStandard)?.Progress == MissionProgress.Completed))
        .Register();

        CreateRecipe(25)
        .AddIngredient(ItemID.WoodenArrow, 25)
        .AddIngredient(ItemID.CopperBar, 2)
        .AddTile(TileID.Anvils)
        .AddCondition(new Condition(Instance.GetLocalization("Conditions.CopperStandardActive"),
        () => Main.LocalPlayer.GetModPlayer<MissionPlayer>()
        .GetMission(MissionID.CopperStandard)?.Progress == MissionProgress.Active))
        .Register();
    }
}