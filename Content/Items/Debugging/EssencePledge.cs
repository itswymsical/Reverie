using Terraria.GameContent.Creative;
using Terraria.Audio;
using Reverie.Common.Players;

namespace Reverie.Content.Items.Debugging;

public class EssencePledge : ModItem
{
    public override string Texture => "Terraria/Images/UI/Bestiary/Icon_Locked";
    public override void SetStaticDefaults()
    {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 50, 0);
        Item.useAnimation = 15;
        Item.useTime = 15;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.consumable = false;
    }

    public override bool AltFunctionUse(Player player)
    {
        return true;
    }

    public override bool? UseItem(Player player)
    {
        if (player.altFunctionUse == 2)
        {
            var classPlayer = player.GetModPlayer<ClassStatPlayer>();

            if (classPlayer.CurrentClass == ClassType.None)
            {
                classPlayer.SetClass(ClassType.Vanguard);
                Main.NewText("You have pledged your soul to the path of the Vanguard!", Color.Orange);
                SoundEngine.PlaySound(SoundID.Item4);
            }
            else if (classPlayer.CurrentClass == ClassType.Vanguard)
            {
                Main.NewText("You have already pledged to the Vanguard path.", Color.Orange);
            }
            else
            {
                Main.NewText("You must abandon your current path before pledging to the Vanguard.", Color.Red);
            }

            return true;
        }
        else
        {
            var classPlayer = player.GetModPlayer<ClassStatPlayer>();

            if (classPlayer.CurrentClass == ClassType.None)
            {
                Main.NewText("You have not yet pledged to any path. Right-click to become a Vanguard.", Color.Gray);
                return true;
            }

            var message = "Current Title: Vanguard";

            var expPlayer = player.GetModPlayer<ExperiencePlayer>();
            message += $"\nLevel: {expPlayer.playerLevel}";

            message += "\n[c/AAAAFF:Stat Bonuses:]";
            message += $"\n- Mana: +{classPlayer.levelManaBonus} MP";
            message += $"\n- Health: +{classPlayer.levelHealthBonus} HP";
            message += $"\n- Minions: +{classPlayer.levelMinionBonus}";

            Main.NewText(message, Color.Orange);

            SoundEngine.PlaySound(SoundID.Item4);
            return true;
        }
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddRecipeGroup(RecipeGroupID.IronBar, 8)
            .AddIngredient(ItemID.FallenStar, 1)
            .AddTile(TileID.Anvils)
            .Register();
    }
}