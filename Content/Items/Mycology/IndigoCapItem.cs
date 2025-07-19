namespace Reverie.Content.Items.Mycology;

public class IndigoCapItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.width = Item.height = 24;
        Item.rare = ItemRarityID.Quest;
        Item.maxStack = Item.CommonMaxStack;
    }

    //public override void ModifyTooltips(List<TooltipLine> tooltips)
    //{
    //    var player = ModContent.GetInstance<MissionPlayer>();
    //    var mission = player.GetMission(MissionID.SporeSplinter);

    //    if (mission?.Progress == MissionProgress.Ongoing)
    //    {
    //        tooltips.Add(new TooltipLine(Mod, "MissionItem", $"[i:{Type}] Required for active Mission.")
    //        {
    //            OverrideColor = Color.DarkGray
    //        });
    //    }
    //}
}
public class IndigoCapSporesItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.width = Item.height = 24;
        Item.rare = ItemRarityID.Blue;
        Item.maxStack = Item.CommonMaxStack;
    }
}