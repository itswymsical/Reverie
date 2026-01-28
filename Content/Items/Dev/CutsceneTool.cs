using Reverie.Core.Cinematics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.GameContent;
using Terraria.UI;
using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;

namespace Reverie.Content.Items.Dev;

public class CutsceneTool : ModItem
{
    public override string Texture => PLACEHOLDER;

    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 28;
        Item.useTime = Item.useAnimation = 20;
        Item.value = Item.buyPrice(0);
        Item.rare = ItemRarityID.Quest;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            // Toggle the UI
            CutsceneToolUI.Toggle();
            return true;
        }
        return null;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        tooltips.Add(new TooltipLine(Mod, "CutscenePlayer", "Opens cutscene selector"));
    }
}

public class CutsceneToolUI : IInGameNotification
{
    private static CutsceneToolUI instance;
    private static bool isActive = false;

    private readonly List<CutsceneEntry> _cutscenes = [];
    private int _selectedIndex = 0;
    private int _scrollOffset = 0;
    private const int MAX_VISIBLE_ITEMS = 10;
    private const int ITEM_HEIGHT = 30;
    private const int UI_WIDTH = 400;
    private const int UI_HEIGHT = 350;

    private Rectangle _uiBounds;
    private bool _isMouseOverUI = false;

    public bool ShouldBeRemoved => !isActive;

    private class CutsceneEntry
    {
        public string Name { get; set; }
        public Type Type { get; set; }
    }

    public static void Toggle()
    {
        if (isActive)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    public static void Show()
    {
        if (instance == null)
        {
            instance = new CutsceneToolUI();
            instance.DiscoverCutscenes();
        }

        isActive = true;
        InGameNotificationsTracker.AddNotification(instance);
    }

    public static void Hide()
    {
        isActive = false;
    }

    private void DiscoverCutscenes()
    {
        _cutscenes.Clear();

        // Get all types that inherit from Cutscene
        var cutsceneTypes = ModContent.GetInstance<Reverie>()
            .Code.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Cutscene)) && !t.IsAbstract)
            .OrderBy(t => t.Name);

        foreach (var type in cutsceneTypes)
        {
            _cutscenes.Add(new CutsceneEntry
            {
                Name = type.Name,
                Type = type
            });
        }
    }

    public void Update()
    {
        if (!isActive) return;

        if (Main.keyState.IsKeyDown(Keys.Escape) && !Main.oldKeyState.IsKeyDown(Keys.Escape))
        {
            Hide();
            return;
        }
    }

    private void OnItemClick(int listIndex)
    {
        if (PlayerInput.IgnoreMouseInterface) return;

        Main.LocalPlayer.mouseInterface = true;

        if (!Main.mouseLeft || !Main.mouseLeftRelease) return;

        Main.mouseLeftRelease = false;

        if (_selectedIndex == listIndex)
        {
            // Second click on same item - play cutscene
            PlayCutscene(_cutscenes[listIndex]);
        }
        else
        {
            _selectedIndex = listIndex;
        }
    }

    private void EnsureSelectedVisible()
    {
        if (_selectedIndex < _scrollOffset)
        {
            _scrollOffset = _selectedIndex;
        }
        else if (_selectedIndex >= _scrollOffset + MAX_VISIBLE_ITEMS)
        {
            _scrollOffset = _selectedIndex - MAX_VISIBLE_ITEMS + 1;
        }
    }

    private Rectangle GetItemBounds(int visualIndex)
    {
        var x = _uiBounds.X + 10;
        var y = _uiBounds.Y + 50 + visualIndex * ITEM_HEIGHT;
        return new Rectangle(x, y, UI_WIDTH - 20, ITEM_HEIGHT - 2);
    }

    private void PlayCutscene(CutsceneEntry entry)
    {
        try
        {
            // Use reflection to call the generic PlayCutscene method
            var method = typeof(CutsceneSystem).GetMethod(nameof(CutsceneSystem.PlayCutscene),
                BindingFlags.Public | BindingFlags.Static,
                null,
                Type.EmptyTypes,
                null);

            if (method != null)
            {
                var genericMethod = method.MakeGenericMethod(entry.Type);
                genericMethod.Invoke(null, null);
                Hide();
            }
        }
        catch (Exception ex)
        {
            ModContent.GetInstance<Reverie>().Logger.Error($"Failed to play cutscene {entry.Name}: {ex.Message}");
        }
    }

    public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        if (!isActive) return;

        // Center the UI on screen
        var x = (Main.screenWidth - UI_WIDTH) / 2;
        var y = (Main.screenHeight - UI_HEIGHT) / 2;
        _uiBounds = new Rectangle(x, y, UI_WIDTH, UI_HEIGHT);

        // Check if mouse is over UI
        var mousePos = new Vector2(Main.mouseX, Main.mouseY);
        _isMouseOverUI = _uiBounds.Contains(mousePos.ToPoint());

        // Tell Terraria the mouse is over UI to prevent clicks from passing through
        if (_isMouseOverUI && !PlayerInput.IgnoreMouseInterface)
        {
            Main.LocalPlayer.mouseInterface = true;
        }

        // Draw background panel
        DrawPanel(spriteBatch, _uiBounds, new Color(23, 25, 35) * 0.95f);

        // Draw title
        var title = "Cutscene Selector";
        var titleFont = FontAssets.DeathText.Value;
        var titleSize = titleFont.MeasureString(title);
        var titlePos = new Vector2(x + (UI_WIDTH - titleSize.X) / 2, y + 10);
        Utils.DrawBorderString(spriteBatch, title, titlePos, Color.White);

        // Draw cutscene list
        var visibleCount = Math.Min(MAX_VISIBLE_ITEMS, _cutscenes.Count - _scrollOffset);
        for (var i = 0; i < visibleCount; i++)
        {
            var listIndex = i + _scrollOffset;
            DrawCutsceneEntry(spriteBatch, _cutscenes[listIndex], i, listIndex == _selectedIndex);
        }

        // Draw scroll indicator
        if (_cutscenes.Count > MAX_VISIBLE_ITEMS)
        {
            DrawScrollbar(spriteBatch);
        }

        // Draw controls hint
        var controls = _cutscenes.Count > 0
            ? "Click: Play | Esc: Close"
            : "No cutscenes found";
        var font = FontAssets.MouseText.Value;
        var controlsSize = font.MeasureString(controls);
        var controlsPos = new Vector2(x + (UI_WIDTH - controlsSize.X) / 2, y + UI_HEIGHT - 25);
        Utils.DrawBorderString(spriteBatch, controls, controlsPos, Color.Gray);
    }

    private void DrawCutsceneEntry(SpriteBatch spriteBatch, CutsceneEntry entry, int visualIndex, bool isSelected)
    {
        var bounds = GetItemBounds(visualIndex);

        // Check if mouse is hovering
        var isHovering = bounds.Contains(Main.MouseScreen.ToPoint());

        // Draw selection highlight
        if (isSelected)
        {
            DrawPanel(spriteBatch, bounds, new Color(70, 130, 180) * 0.5f);
        }

        // Draw hover highlight
        if (isHovering)
        {
            DrawPanel(spriteBatch, bounds, Color.White * 0.1f);
        }

        // Draw cutscene name
        var font = FontAssets.MouseText.Value;
        var textPos = new Vector2(bounds.X + 5, bounds.Y + (bounds.Height - font.LineSpacing) / 2);
        Utils.DrawBorderString(spriteBatch, entry.Name, textPos, isSelected ? Color.Yellow : Color.White);

        // Handle click if hovering
        if (isHovering)
        {
            var listIndex = visualIndex + _scrollOffset;
            OnItemClick(listIndex);
        }
    }

    private void DrawScrollbar(SpriteBatch spriteBatch)
    {
        var scrollbarX = _uiBounds.X + UI_WIDTH - 15;
        var scrollbarY = _uiBounds.Y + 50;
        var scrollbarHeight = MAX_VISIBLE_ITEMS * ITEM_HEIGHT;

        // Draw scrollbar track
        var track = new Rectangle(scrollbarX, scrollbarY, 8, scrollbarHeight);
        DrawPanel(spriteBatch, track, Color.Black * 0.3f);

        // Draw scrollbar thumb
        var thumbHeight = (float)MAX_VISIBLE_ITEMS / _cutscenes.Count * scrollbarHeight;
        var thumbY = scrollbarY + _scrollOffset / (float)(_cutscenes.Count - MAX_VISIBLE_ITEMS) * (scrollbarHeight - thumbHeight);
        var thumb = new Rectangle(scrollbarX, (int)thumbY, 8, (int)thumbHeight);
        DrawPanel(spriteBatch, thumb, Color.Gray * 0.8f);
    }

    private void DrawPanel(SpriteBatch spriteBatch, Rectangle bounds, Color color)
    {
        // Draw filled rectangle
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, bounds, color);

        // Draw border
        var borderThickness = 2;
        spriteBatch.Draw(TextureAssets.MagicPixel.Value,
            new Rectangle(bounds.X, bounds.Y, bounds.Width, borderThickness),
            Color.Black * 0.5f);
        spriteBatch.Draw(TextureAssets.MagicPixel.Value,
            new Rectangle(bounds.X, bounds.Bottom - borderThickness, bounds.Width, borderThickness),
            Color.Black * 0.5f);
        spriteBatch.Draw(TextureAssets.MagicPixel.Value,
            new Rectangle(bounds.X, bounds.Y, borderThickness, bounds.Height),
            Color.Black * 0.5f);
        spriteBatch.Draw(TextureAssets.MagicPixel.Value,
            new Rectangle(bounds.Right - borderThickness, bounds.Y, borderThickness, bounds.Height),
            Color.Black * 0.5f);
    }

    public void PushAnchor(ref Vector2 positionAnchorBottom)
    {
        // We're using centered positioning, so we don't need to adjust the anchor
    }
}