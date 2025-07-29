using ReLogic.Graphics;
using Reverie.Common.Items.Types;
using Reverie.Common.Players;
using Reverie.Common.UI.RelicUI;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.UI;

namespace Reverie.Content.Items.Accessories;

public class MicrolithItem : RelicItem
{
    public override int MaxLevel => 5;
    public override int XPPerLevel => 60;
    public override float XPAbsorptionRate => 0.55f;

    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.Blue;
        Item.width = Item.height = 32;
        Item.value = Item.sellPrice(gold: 1, silver: 8);
    }

    public override void UpdateRelicEffects(Player player)
    {
        player.GetModPlayer<ReveriePlayer>().microlithEquipped = true;
    }

    public override void AddTooltips(List<TooltipLine> tooltips)
    {
        float dropChance = 0.10f + (RelicLevel - 1) * 0.08f;
        int minExtra = Math.Max(1, RelicLevel - 3);
        int maxExtra = RelicLevel + 1;

        tooltips.Add(new TooltipLine(Mod, "DropChance", $"{dropChance:P0} chance[i:{ItemID.AmethystStoneBlock}]"));

        tooltips.Add(new TooltipLine(Mod, "DropAmount", $"+{minExtra}-{maxExtra} extra ore [i:{ItemID.CopperPickaxe}]"));
    }
}

public class MicrolithGlobalTile : GlobalTile
{
    public bool harvestApplied;

    public override void Drop(int i, int j, int type)
    {
        base.Drop(i, j, type);

        Player player = Main.player[Main.myPlayer];
        int itemType = TileLoader.GetItemDropFromTypeAndStyle(type);
        bool shinyOre = Main.tileSpelunker[type] && Main.tileSolid[type] && (type != TileID.Pots || type != TileID.Containers
            || type != TileID.Heart || type != TileID.LifeCrystalBoulder || type != TileID.LifeFruit || !(type >= 63 && type <= 68));

        if (player.GetModPlayer<ReveriePlayer>().microlithEquipped)
        {
            if (TileID.Sets.Ore[type] || shinyOre)
            {
                var relicPlayer = player.GetModPlayer<RelicPlayer>();
                if (relicPlayer.CurrentRelic is MicrolithItem microlith)
                {
                    // Scale drop chance: 10% base + 8% per level (10%, 18%, 26%, 34%, 42% at max)
                    float dropChance = 0.10f + (microlith.RelicLevel - 1) * 0.08f;

                    if (Main.rand.NextFloat() < dropChance)
                    {
                        // Scale extra items: level 1 = 1-2, level 5 = 2-4 items
                        int minExtra = Math.Max(1, microlith.RelicLevel - 3);
                        int maxExtra = microlith.RelicLevel;
                        int extraItems = Main.rand.Next(minExtra, maxExtra + 1);

                        harvestApplied = true;

                        player.QuickSpawnItem(new EntitySource_TileBreak(i, j), itemType, extraItems);
                        HarvestNotificationManager.ShowHarvestNotification(itemType, extraItems);
                    }
                }
            }
            else
            {
                harvestApplied = false;
            }
        }
    }

    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        base.KillTile(i, j, type, ref fail, ref effectOnly, ref noItem);

        Player player = Main.LocalPlayer;

        if (harvestApplied && TileID.Sets.Ore[type])
        {
            harvestApplied = false;
            for (int num = 0; num < 60; num += 2)
            {
                Dust.NewDust(new Vector2(i * 16, j * 16), 8, 8, DustID.SpelunkerGlowstickSparkle, Main.rand.NextFloat((float)num * 0.05f), -4f, Scale: 1f);
            }
            SoundEngine.PlaySound(SoundID.CoinPickup, new Vector2(i * 16, j * 16));
        }
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


// Helper class to manage and prevent duplicates
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