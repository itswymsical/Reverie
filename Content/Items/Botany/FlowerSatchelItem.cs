// Modified from Spirit Mod's Backpack Item class
// Credits: Spirit Mod, GabeHasWon
// https://github.com/GabeHasWon/SpiritReforged/blob/master/Common/UI/BackpackInterface/BackbackUISlot.cs#L1

using ReLogic.Graphics;
using System.IO;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader.IO;
using Reverie.Common.Players;

namespace Reverie.Content.Items.Botany;

public class FlowerSatchelItem : ModItem
{
    private const int SLOT_COUNT = 3;
    public Item[] items;

    public override void SetStaticDefaults() 
        => ItemID.Sets.OpenableBag[Type] = true;

    public override void SetDefaults()
    {
        Item.consumable = false;
        Item.rare = ItemRarityID.Green;
        Item.width = Item.height = 26;
        Item.maxStack = 1;
        Item.value = Item.sellPrice(gold: 1);

        if (items == null)
        {
            items = new Item[SLOT_COUNT];
            for (var i = 0; i < SLOT_COUNT; i++)
                items[i] = new Item();
        }
    }

    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        if (line.Name == "ItemName")
        {
            var pos = new Vector2(line.X, line.Y);
            var text = line.Text;
            var textWidth = FontAssets.MouseText.Value.MeasureString(text).X;

            Main.spriteBatch.DrawString(
                FontAssets.MouseText.Value,
                text,
                pos,
                Color.White
            );

            var time = Main.GlobalTimeWrappedHourly;
            var frameWidth = 10;
            var frameHeight = 14;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null, Main.UIScaleMatrix);

            for (var i = 0; i < 4; i++)
            {
                var individualTime = time + i * 1.5f;
                var swayAmount = (float)Math.Sin(individualTime * 1.5f) * 15f;
                var verticalDrift = individualTime * 8f % 40f;
                var horizontalDrift = individualTime * 15f % textWidth;

                var offset = new Vector2(
                    swayAmount + horizontalDrift,
                    verticalDrift
                );

                var rotationSpeed = 0.8f;
                var rotation = individualTime * rotationSpeed + i * MathHelper.PiOver2;

                var particleColor = Color.ForestGreen * (0.5f + (float)Math.Sin(time * 1.2f) * 0.3f);

                var frame = (int)(Main.GameUpdateCount / 6 + i) % 8;
                var sourceRect = new Rectangle(
                    0,
                    frameHeight * frame,
                    frameWidth,
                    frameHeight
                );

                Main.spriteBatch.Draw(
                    ModContent.Request<Texture2D>($"{VFX_DIRECTORY}MagnoliaLeaf").Value,
                    pos + offset,
                    sourceRect,
                    particleColor * 1.15f,
                    rotation,
                    new Vector2(frameWidth / 2f, frameHeight / 2f),
                    1f,
                    SpriteEffects.None,
                    0f
                );
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.UIScaleMatrix);

            return true;
        }

        return base.PreDrawTooltipLine(line, ref yOffset);
    }

    public override void AddRecipes()
    {
        CreateRecipe(4)
            .AddIngredient(ModContent.ItemType<MagnoliaItem>())
            .AddIngredient(ItemID.Daybloom)
            .AddIngredient(ItemID.Shiverthorn)
            .AddIngredient(ItemID.Blinkroot)
            .AddIngredient(ItemID.Moonglow)
            .AddIngredient(ItemID.Deathweed)
            .AddIngredient(ItemID.Waterleaf)
            .AddTile(TileID.WorkBenches)
            .Register();
    }


    //public override void UpdateInventory(Player player)
    //{
    //    bool foundFirst = false;
    //    int lastSatchelSlot = -1;

    //    // Find duplicate satchels
    //    for (int i = 0; i < player.inventory.Length; i++)
    //    {
    //        if (player.inventory[i].type == ModContent.ItemType<FlowerSatchel>())
    //        {
    //            if (!foundFirst)
    //                foundFirst = true;
    //            else
    //                lastSatchelSlot = i;
    //        }
    //    }

    //    // Remove duplicate if found
    //    if (lastSatchelSlot != -1)
    //    {
    //        player.inventory[lastSatchelSlot].TurnToAir();
    //    }
    //}

    #region Checks
    public override bool ConsumeItem(Player player) => false;

    public override void RightClick(Player player)
    {
        var modPlayer = player.GetModPlayer<SatchelPlayer>();

        // Toggle satchel visibility and store reference
        modPlayer.flowerSatchelVisible = !modPlayer.flowerSatchelVisible;
        modPlayer.activeSatchel = modPlayer.flowerSatchelVisible ? Item : null;

        SoundEngine.PlaySound(SoundID.MenuOpen);
    }

    public override bool CanPickup(Player player) => !player.HasItemInAnyInventory(ModContent.ItemType<FlowerSatchelItem>());

    public bool IsValidFlowerItem(Item item)
    {
        return item.IsAir || item.type == ModContent.ItemType<MagnoliaItem>() || item.type == ItemID.Daybloom ||
               item.type == ItemID.Fireblossom || item.type == ItemID.Shiverthorn || item.type == ItemID.Blinkroot ||
               item.type == ItemID.Moonglow || item.type == ItemID.Deathweed || item.type == ItemID.SkyBlueFlower
               || item.type == ItemID.Waterleaf || item.type == ItemID.Sunflower;
    }
    #endregion

    #region Serialization & Network
    public override void SaveData(TagCompound tag)
    {
        for (var i = 0; i < SLOT_COUNT; i++)
        {
            if (items[i] is not null && !items[i].IsAir)
            {
                tag[$"flower{i}"] = ItemIO.Save(items[i]);
            }
        }
    }

    public override void LoadData(TagCompound tag)
    {
        items = new Item[SLOT_COUNT];

        for (var i = 0; i < SLOT_COUNT; i++)
        {
            if (tag.TryGet($"flower{i}", out TagCompound itemTag))
            {
                items[i] = ItemIO.Load(itemTag);
            }
            else
            {
                items[i] = new Item();
            }
        }
    }

    public override void NetSend(BinaryWriter writer)
    {
        for (var i = 0; i < SLOT_COUNT; i++)
        {
            ItemIO.Send(items[i], writer, true);
        }
    }

    public override void NetReceive(BinaryReader reader)
    {
        for (var i = 0; i < SLOT_COUNT; i++)
        {
            ItemIO.Receive(items[i], reader, true);
        }
    }
    #endregion
}
