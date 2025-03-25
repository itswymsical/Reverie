using Reverie.Core.Dialogue;
using Terraria.GameContent;

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

        public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
        {
            if (line.Name == "ItemName" && line.Mod == "Terraria")
            {
                Color rarityColor = line.OverrideColor.GetValueOrDefault(line.Color);
                Vector2 basePosition = new Vector2(line.X, line.Y);

                float time = Main.GlobalTimeWrappedHourly * 3f;
                float amplitude = 2.73f;

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null, Main.UIScaleMatrix);

                string text = line.Text;
                float posX = basePosition.X;

                for (int i = 0; i < text.Length; i++)
                {
                    float sineOffset = (float)Math.Sin(time + i * 0.5f) * amplitude;
                    Vector2 charPos = new Vector2(posX, basePosition.Y + sineOffset);

                    string charStr = text[i].ToString();
                    Vector2 textSize = FontAssets.MouseText.Value.MeasureString(charStr);
                    float charWidth = textSize.X * line.BaseScale.X;

                    Color glowColor = Color.Lerp(rarityColor, Color.Gray, (float)Math.Sin(time + i * 0.2f) * 0.5f + 0.5f);

                    Utils.DrawBorderString(
                        Main.spriteBatch,
                        charStr,
                        charPos,
                        glowColor,
                        line.BaseScale.X * 1.1f);

                    posX += charWidth;
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.UIScaleMatrix);

                posX = basePosition.X;
                for (int i = 0; i < text.Length; i++)
                {
                    float sineOffset = (float)Math.Sin(time + i * 0.5f) * amplitude;
                    Vector2 charPos = new Vector2(posX, basePosition.Y + sineOffset);

                    string charStr = text[i].ToString();
                    Vector2 textSize = FontAssets.MouseText.Value.MeasureString(charStr);
                    float charWidth = textSize.X * line.BaseScale.X;

                    Utils.DrawBorderString(
                        Main.spriteBatch,
                        charStr,
                        charPos,
                        rarityColor,
                        line.BaseScale.X);

                    posX += charWidth;
                }

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
