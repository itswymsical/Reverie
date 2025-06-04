// Modified from Spirit Mod's Backpack slot
// Credits: Spirit Mod, GabeHasWon
// https://github.com/GabeHasWon/SpiritReforged/blob/master/Common/UI/Misc/BasicItemSlot.cs#L8

using Terraria.Audio;
using Terraria.UI;

namespace Reverie.Common.UI.FlowerSatchel;

internal class BasicItemSlot : UIElement
{
    public float Scale { get; private set; }

    protected readonly Item[] _items;
    protected readonly int _index;
    protected readonly int _context;

    public BasicItemSlot(Item item, int context = ItemSlot.Context.ChestItem, float scale = .85f)
    {
        _items = [item];
        _index = 0;
        _context = context;
        Scale = scale;

        Width = Height = new StyleDimension(52 * Scale, 0f);
    }

    public BasicItemSlot(Item[] items, int index, int context = ItemSlot.Context.ChestItem, float scale = .85f)
    {
        _items = items;
        _index = index;
        _context = context;
        Scale = scale;

        Width = Height = new StyleDimension(52 * Scale, 0f);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        var oldScale = Main.inventoryScale;
        Main.inventoryScale = Scale;

        ItemSlot.Draw(spriteBatch, ref _items[_index], _context, GetDimensions().ToRectangle().TopLeft());

        Main.inventoryScale = oldScale;

        if (IsMouseHovering)
            SlotLogic(ref _items[_index]);
    }

    protected virtual void SlotLogic(ref Item item)
    {
        Main.LocalPlayer.mouseInterface = true;

        // Check if the item is valid for this slot before handling
        if (Main.mouseItem.IsAir || ValidItemForSlot(Main.mouseItem))
        {
            ItemSlot.Handle(ref item, ItemSlot.Context.InventoryItem);
        }
        else if (Main.mouseLeft && Main.mouseLeftRelease)
        {
            // Play error sound for invalid items
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
    }

    /// <summary>
    /// Determines if the given item can be placed in this slot.
    /// </summary>
    /// <param name="item">The item to validate</param>
    /// <returns>True if the item can be placed in this slot, false otherwise</returns>
    public virtual bool ValidItemForSlot(Item item)
    {
        return true; // Base implementation allows all items
    }
}