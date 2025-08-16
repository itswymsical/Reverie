using ReLogic.Graphics;
using Reverie.Common.Players;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.UI;

namespace Reverie.Content.Items.Accessories;

public class MicrolithItem : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.rare = ItemRarityID.Blue;
        Item.width = Item.height = 32;
        Item.value = Item.sellPrice(gold: 1, silver: 8);
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<ReveriePlayer>().microlithEquipped = true;
    }
    public override void AddRecipes()
    {
        CreateRecipe()
        .AddIngredient(ItemID.SiltBlock, 45)
        .AddIngredient(ItemID.StoneBlock, 30)
        .AddIngredient(ItemID.AncientChisel)
        .AddTile(TileID.Furnaces)
        .Register();
    }
}

public class MicrolithGlobalTile : GlobalTile
{
    private bool bonusApplied;

    public override void Drop(int i, int j, int type)
    {
        base.Drop(i, j, type);

        Player player = Main.LocalPlayer;
        if (!player.GetModPlayer<ReveriePlayer>().microlithEquipped)
        {
            bonusApplied = false;
            return;
        }

        bool isValidTile = Main.tileSolid[type] && (Main.tileSpelunker[type] || TileID.Sets.Ore[type]);

        if (isValidTile && Main.rand.NextFloat() < 0.15f)
        {
            int itemType = GetGemOrOreItem(type);
            int extraItems = Main.rand.Next(1, 3);

            bonusApplied = true;
            player.QuickSpawnItem(new EntitySource_TileBreak(i, j), itemType, extraItems);
            HarvestNotificationManager.ShowHarvestNotification(itemType, extraItems);
        }
        else
        {
            bonusApplied = false;
        }
    }

    private int GetGemOrOreItem(int tileType)
    {
        // gem blocks drop actual gems instead of blocks
        return tileType switch
        {
            63 => ItemID.Sapphire,
            64 => ItemID.Ruby,
            65 => ItemID.Emerald,
            66 => ItemID.Topaz,
            67 => ItemID.Amethyst,
            68 => ItemID.Diamond,
            _ => TileLoader.GetItemDropFromTypeAndStyle(tileType)
        };
    }

    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        base.KillTile(i, j, type, ref fail, ref effectOnly, ref noItem);

        if (!bonusApplied) return;

        bonusApplied = false;

        // sparkly harvest effects
        Vector2 tilePos = new Vector2(i * 16, j * 16);
        for (int dustCount = 0; dustCount < 30; dustCount++)
        {
            float velX = Main.rand.NextFloat(-2f, 2f);
            float velY = Main.rand.NextFloat(-4f, -1f);
            Dust.NewDust(tilePos, 16, 16, DustID.SpelunkerGlowstickSparkle, velX, velY, Scale: 1.2f);
        }

        SoundEngine.PlaySound(SoundID.CoinPickup, tilePos);
    }
}

public class HarvestNotification : IInGameNotification
{
    private const int DURATION = 180;
    private const float FADE_TIME = 30f;

    public int ItemType { get; private set; }
    public int Count { get; private set; }
    public string ItemName { get; private set; }

    private int timer;
    private float yOffset;
    private float scale = 1f;
    private Color color = Color.White;

    public bool ShouldBeRemoved => timer >= DURATION;

    public HarvestNotification(int itemType, int count)
    {
        ItemType = itemType;
        Count = count;

        Item item = new();
        item.SetDefaults(itemType);
        ItemName = item.Name;

        timer = 0;
        yOffset = -325f; // Start at anchor position
    }

    public void AddToStack(int additionalCount)
    {
        Count += additionalCount;
        timer = 0; // Reset timer

        // Small bounce effect when adding
        scale = 1.4f;
    }

    public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        // Calculate fade
        float alpha = 1f;
        if (timer > DURATION - FADE_TIME)
        {
            alpha = 1f - (timer - (DURATION - FADE_TIME)) / FADE_TIME;
        }

        Vector2 drawPos = bottomAnchorPosition + new Vector2(30, yOffset);

        // Draw item texture
        if (TextureAssets.Item[ItemType]?.Value is Texture2D itemTexture)
        {
            var itemColor = color * alpha;
            var itemOrigin = itemTexture.Size() * 0.5f;

            spriteBatch.Draw(itemTexture, drawPos - new Vector2(40, 0), null, itemColor, 0f,
                itemOrigin, scale * 0.8f, SpriteEffects.None, 0f);
        }

        // Draw count text
        string countText = $"x{Count}";
        var font = FontAssets.MouseText.Value;
        var textSize = font.MeasureString(countText);
        var textPos = drawPos + new Vector2(-32, -textSize.Y * 0.5f);
        var textColor = new Color(204, 181, 72) * alpha;

        // Text outline
        for (int i = -1; i <= 1; i += 2)
        {
            for (int j = -1; j <= 1; j += 2)
            {
                spriteBatch.DrawString(font, countText, textPos + new Vector2(i, j),
                    Color.Black * alpha, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
        }

        spriteBatch.DrawString(font, countText, textPos, textColor, 0f, Vector2.Zero,
            scale, SpriteEffects.None, 0f);
    }

    public void PushAnchor(ref Vector2 positionAnchorBottom)
    {
        // Push anchor up by consistent notification height for better stacking
        positionAnchorBottom.Y -= 30f; // Fixed height per notification
    }

    public void Update()
    {
        timer++;

        // Smooth scale back to normal
        scale = MathHelper.Lerp(scale, 1f, 0.1f);

        // Float upward only during fade period
        if (timer > DURATION - FADE_TIME)
        {
            yOffset -= 0.5f;
        }
    }
}

public static class HarvestNotificationManager
{
    private static readonly List<HarvestNotification> ActiveNotifications = [];

    public static void ShowHarvestNotification(int itemType, int count)
    {
        var existing = ActiveNotifications.FirstOrDefault(n => n.ItemType == itemType);

        if (existing != null)
        {
            existing.AddToStack(count);
        }
        else
        {
            var notification = new HarvestNotification(itemType, count);
            ActiveNotifications.Add(notification);
            InGameNotificationsTracker.AddNotification(notification);
        }
    }

    public static void UpdateNotifications()
    {
        for (int i = ActiveNotifications.Count - 1; i >= 0; i--)
        {
            if (ActiveNotifications[i].ShouldBeRemoved)
            {
                ActiveNotifications.RemoveAt(i);
            }
        }
    }
}