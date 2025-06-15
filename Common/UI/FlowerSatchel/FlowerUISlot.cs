// Modified from Spirit Mod's Backpack slot
// Credits: Spirit Mod, GabeHasWon
// https://github.com/GabeHasWon/SpiritReforged/blob/master/Common/UI/BackpackInterface/BackbackUISlot.cs#L1

using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Reverie.Common.Players;
using Reverie.Content.Items.Botany;

namespace Reverie.Common.UI.FlowerSatchel;

public class FlowerSatchelUISlot : UIElement
{
    public const int CONTEXT = ItemSlot.Context.ChestItem;
    public const float SCALE = 1f;
    private readonly int _slotIndex;

    public FlowerSatchelUISlot(int index)
    {
        _slotIndex = index;
        Width = Height = new StyleDimension(52 * SCALE, 0f);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        var player = Main.LocalPlayer;
        var modPlayer = player.GetModPlayer<SatchelPlayer>();

        if (!modPlayer.flowerSatchelVisible || modPlayer.activeSatchel?.ModItem is not FlowerSatchelItem satchel)
            return;

        base.DrawSelf(spriteBatch);

        var oldScale = Main.inventoryScale;
        Main.inventoryScale = SCALE;

        ref var item = ref satchel.items[_slotIndex];

        // Draw the slot
        ItemSlot.Draw(spriteBatch, ref item, CONTEXT, GetDimensions().ToRectangle().TopLeft());

        if (item.IsAir)
        {
            var texture = TextureAssets.MagicPixel.Value;
            var source = texture.Frame();
            spriteBatch.Draw(
                texture,
                GetDimensions().Center(),
                source,
                Color.White * .35f,
                0f,
                source.Size() / 2f,
                Main.inventoryScale,
                SpriteEffects.None,
                0f
            );
        }

        HandleItemSlotLogic(ref item, satchel);
        Main.inventoryScale = oldScale;
    }

    private void HandleItemSlotLogic(ref Item item, FlowerSatchelItem satchel)
    {
        if (!IsMouseHovering)
            return;

        Main.LocalPlayer.mouseInterface = true;

        // Show item hover text
        if (item.IsAir)
        {
            Main.hoverItemName = "Flower Slot";
            Main.mouseText = true;
        }
        else
        {
            ItemSlot.MouseHover(ref item, CONTEXT);
        }

        // Handle clicking
        if (Main.mouseLeft && Main.mouseLeftRelease)
        {
            var heldItem = Main.mouseItem;

            // Only allow flower items or emptying the slot
            if (heldItem.IsAir || satchel.IsValidFlowerItem(heldItem))
            {
                ItemSlot.LeftClick(ref item, CONTEXT);
                SoundEngine.PlaySound(SoundID.Grab);
            }
            else
            {
                // Optional: Play error sound when trying to insert invalid item
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
        }
    }
}

internal class FlowerSatchelUIState : UIState
{
    private const float SLOT_SPACING = 44f;
    private const int SLOTS_PER_ROW = 3;
    private Item _lastSatchel;
    private UIText _effectsTitle;
    private UIText _effectsList;

    public override void OnInitialize()
    {
        Width = Height = StyleDimension.Fill;
    }

    public override void Update(GameTime gameTime)
    {
        if (!Main.playerInventory)
        {
            _lastSatchel = null;
            ClearSlots();
            return;
        }

        var modPlayer = Main.LocalPlayer.GetModPlayer<SatchelPlayer>();

        if (modPlayer.activeSatchel?.ModItem is FlowerSatchelItem satchel)
        {
            if (_lastSatchel != satchel.Item)
                RefreshSlots(satchel);

            _lastSatchel = satchel.Item;

            // Update effects text
            UpdateEffectsDisplay(modPlayer);
        }
        else
        {
            if (_lastSatchel != null)
                ClearSlots();

            _lastSatchel = null;
        }

        base.Update(gameTime);
    }

    private void UpdateEffectsDisplay(SatchelPlayer modPlayer)
    {
        if (_effectsTitle != null)
        {
            _effectsTitle.SetText(modPlayer.GetEffectsSummary());
        }

        if (_effectsList != null && modPlayer.activeEffects.Count > 0)
        {
            var effectsText = string.Join("\n", modPlayer.activeEffects);
            _effectsList.SetText(effectsText);
        }
        else if (_effectsList != null)
        {
            _effectsList.SetText("");
        }
    }

    private void RefreshSlots(FlowerSatchelItem satchel)
    {
        ClearSlots();

        // Add title text
        _effectsTitle = new UIText("No active effects", 0.925f, false)
        {
            Left = new StyleDimension(Main.screenWidth / 48, 0),
            Top = new StyleDimension(Main.screenHeight / 2.7f, 0),
            TextColor = Color.LightGreen * 0.95f,
            ShadowColor = Color.Black
        };
        Append(_effectsTitle);

        // Add effects list below title
        _effectsList = new UIText("", 0.8f, false)
        {
            Left = new StyleDimension(Main.screenWidth / 48, 0),
            Top = new StyleDimension(Main.screenHeight / 2.7f + 25, 0),
            TextColor = Color.White * 0.8f,
            ShadowColor = Color.Black
        };
        Append(_effectsList);

        // Add flower slots in a single row
        float baseX = Main.screenWidth / 20 - SLOTS_PER_ROW * 47 / 2;
        var baseY = Main.screenHeight / 3.23f;

        for (var i = 0; i < satchel.items.Length; i++)
        {
            var slot = new FlowerInventorySlot(satchel.items, i, satchel)
            {
                Left = new StyleDimension(baseX + i * 47, 0),
                Top = new StyleDimension(baseY, 0),
                Width = StyleDimension.FromPixels(32),
                Height = StyleDimension.FromPixels(32)
            };

            Append(slot);
        }
    }

    private void ClearSlots()
    {
        RemoveAllChildren();
        _effectsTitle = null;
        _effectsList = null;
    }
}

internal class FlowerInventorySlot : BasicItemSlot
{
    private readonly FlowerSatchelItem _satchel;

    public FlowerInventorySlot(Item[] items, int index, FlowerSatchelItem satchel)
        : base(items, index, ItemSlot.Context.GuideItem, 0.85f)
    {
        _satchel = satchel;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);
    }

    private Color GetFlowerGlowColor(int itemType)
    {
        return itemType switch
        {
            ItemID.Daybloom => Color.Yellow,
            ItemID.Blinkroot => Color.LightBlue,
            ItemID.Fireblossom => Color.OrangeRed,
            ItemID.Shiverthorn => Color.Cyan,
            _ => Color.Transparent
        };
    }

    public override bool ValidItemForSlot(Item item)
    {
        return _satchel.IsValidFlowerItem(item);
    }
}