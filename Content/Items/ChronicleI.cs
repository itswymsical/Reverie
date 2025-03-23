using Reverie.Core.Dialogue;
using Terraria.UI.Chat;

namespace Reverie.Content.Items
{
    public class ChronicleI : ModItem
    {
        public override string Texture => "Reverie/Assets/Textures/Items/ArchiverChronicle";
        public override void SetDefaults()
        {
            Item.useTime = Item.useAnimation = 20;
            Item.value = Item.buyPrice(0);
            Item.rare = ItemRarityID.White;
            Item.useStyle = ItemUseStyleID.HoldUp;
        }

        // Calamity Mod, currently placeholder
        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Name == "ItemName" && line.Mod == "Terraria")
            {
                Color rarityColor = line.OverrideColor ?? line.Color;
                Vector2 basePosition = new Vector2(line.X, line.Y);

                float backInterpolant = (float)Math.Pow(Main.GlobalTimeWrappedHourly * 0.81f % 1f, 1.5f);
                Vector2 backScale = line.BaseScale * MathHelper.Lerp(1f, 1.2f, backInterpolant);
                Color backColor = Color.Lerp(rarityColor, Color.Gold, backInterpolant) * (float)Math.Pow(1f - backInterpolant, 0.46f);
                Vector2 backPosition = basePosition - new Vector2(1f, 0.1f) * backInterpolant * 10f;

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null, Main.UIScaleMatrix);

                for (int i = 0; i < 2; i++)
                    ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, line.Font, line.Text, backPosition, backColor, line.Rotation, line.Origin, backScale, line.MaxWidth, line.Spread);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.UIScaleMatrix);

                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, line.Font, line.Text, basePosition, rarityColor, line.Rotation, line.Origin, line.BaseScale, line.MaxWidth, line.Spread);

                return false;
            }
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (DialogueManager.Instance.StartDialogue(NPCDataManager.Default, DialogueID.ChronicleI_Chapter1, true))
                return false;

            return base.CanUseItem(player);
        }
        public override bool? UseItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
                DialogueManager.Instance.StartDialogue(NPCDataManager.Default, DialogueID.ChronicleI_Chapter1, true);

            return true;
        }
    }
}
