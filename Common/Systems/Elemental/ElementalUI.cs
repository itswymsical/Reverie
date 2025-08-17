using Reverie.Common.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;

namespace Reverie.Common.Systems.Elemental;
public class ElementalKeybind : ModPlayer
{
    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (ReverieSystem.OpenElementalUI.JustPressed)
        {
            ModContent.GetInstance<ElementalUISystem>().ToggleUI();
        }
    }
}

public class ElementalBindingUI : UIState
{
    private UIPanel mainPanel;
    private UIItemSlot itemSlot;
    private CustomExpSlider expSlider;
    private UIText expText;
    private UIPanel bindButton;
    private UIText bindButtonText;

    private Item[] itemArray;
    private int selectedExp;

    public override void OnInitialize()
    {
        itemArray = new Item[1];
        itemArray[0] = new Item();

        mainPanel = new UIPanel();
        mainPanel.Width.Set(300, 0f);
        mainPanel.Height.Set(200, 0f);
        mainPanel.HAlign = 0.5f;
        mainPanel.VAlign = 0.5f;
        mainPanel.BackgroundColor = Color.CornflowerBlue * 0.8f;
        mainPanel.BorderColor = Color.Black;
        Append(mainPanel);

        itemSlot = new UIItemSlot(itemArray, 0, ItemSlot.Context.ChestItem);
        itemSlot.Left.Set(20, 0f);
        itemSlot.Top.Set(20, 0f);
        mainPanel.Append(itemSlot);

        expSlider = new CustomExpSlider();
        expSlider.Left.Set(20, 0f);
        expSlider.Top.Set(80, 0f);
        expSlider.Width.Set(260, 0f);
        expSlider.Height.Set(20, 0f);
        mainPanel.Append(expSlider);

        expText = new UIText("0 / 0 EXP");
        expText.Left.Set(20, 0f);
        expText.Top.Set(110, 0f);
        mainPanel.Append(expText);

        bindButton = new UIPanel();
        bindButton.Left.Set(20, 0f);
        bindButton.Top.Set(140, 0f);
        bindButton.Width.Set(120, 0f);
        bindButton.Height.Set(30, 0f);
        bindButton.BackgroundColor = Color.DarkBlue * 0.8f;
        bindButton.BorderColor = Color.White;
        bindButton.OnLeftClick += BindElement;
        mainPanel.Append(bindButton);

        bindButtonText = new UIText("Bind Experience");
        bindButtonText.HAlign = 0.5f;
        bindButtonText.VAlign = 0.5f;
        bindButton.Append(bindButtonText);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        var player = Main.LocalPlayer;
        var expPlayer = player.GetModPlayer<ExperiencePlayer>();

        expSlider.MaxValue = expPlayer.curExp;
        selectedExp = expSlider.Value;

        Item currentItem = itemArray[0];
        int minRequired = 0;
        if (currentItem != null && !currentItem.IsAir && currentItem.TryGetGlobalItem<ElementalGlobalItem>(out var globalItem))
        {
            minRequired = globalItem.GetMinimumBinding(currentItem);

            if (selectedExp > 0 && selectedExp < minRequired)
            {
                expSlider.Value = minRequired;
                selectedExp = minRequired;
            }

            expSlider.MinValue = minRequired;
        }

        if (minRequired > 0)
        {
            expText.SetText($"{selectedExp} / {expPlayer.curExp} EXP (Min: {minRequired})");
        }
        else
        {
            expText.SetText($"{selectedExp} / {expPlayer.curExp} EXP");
        }

        bool canBind = currentItem != null && !currentItem.IsAir && selectedExp >= minRequired &&
                      expPlayer.CanAfford(selectedExp) && IsValidWeapon(currentItem) &&
                      currentItem.TryGetGlobalItem<ElementalGlobalItem>(out _);

        bindButton.BackgroundColor = canBind ? Color.DarkBlue * 0.8f : Color.Gray * 0.6f;
        bindButtonText.TextColor = canBind ? Color.White : Color.DarkGray;

        if (bindButton.IsMouseHovering && canBind)
        {
            bindButton.BackgroundColor = Color.Blue;
        }
    }

    private void BindElement(UIMouseEvent evt, UIElement listeningElement)
    {
        Item currentItem = itemArray[0];
        if (currentItem == null || currentItem.IsAir || selectedExp <= 0)
            return;

        var player = Main.LocalPlayer;
        var expPlayer = player.GetModPlayer<ExperiencePlayer>();

        if (!expPlayer.CanAfford(selectedExp) || !IsValidWeapon(currentItem))
            return;

        if (currentItem.TryGetGlobalItem<ElementalGlobalItem>(out var globalItem))
        {
            if (globalItem.TryBindExp(player, ElementType.Fire, selectedExp, currentItem))
            {
                expSlider.Value = 0;
                SoundEngine.PlaySound(SoundID.Item4);
            }
        }
    }

    private bool IsValidWeapon(Item item)
    {
        return item.damage > 0 && (item.DamageType == DamageClass.Melee ||
                                  item.DamageType == DamageClass.Ranged ||
                                  item.DamageType == DamageClass.Magic ||
                                  item.DamageType == DamageClass.Summon || 
                                  item.DamageType == DamageClass.MeleeNoSpeed || item.DamageType == DamageClass.SummonMeleeSpeed
                                  || item.DamageType == DamageClass.MagicSummonHybrid || item.DamageType == DamageClass.Default);
    }
}

public class CustomExpSlider : UIElement
{
    public int Value { get; set; }
    public int MaxValue { get; set; }
    public int MinValue { get; set; }

    private bool isDragging;
    private Rectangle sliderBar;
    private Rectangle sliderHandle;

    public override void OnInitialize()
    {
        Width.Set(260, 0f);
        Height.Set(20, 0f);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle dimensions = GetDimensions();

        sliderBar = new Rectangle((int)dimensions.X, (int)dimensions.Y + 8, (int)dimensions.Width, 4);
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, sliderBar, Color.Gray);

        if (MinValue > 0 && MaxValue > 0)
        {
            float minRatio = (float)MinValue / MaxValue;
            int minX = (int)(dimensions.X + minRatio * dimensions.Width);
            Rectangle minIndicator = new Rectangle(minX, (int)dimensions.Y + 6, 2, 8);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, minIndicator, Color.Red);
        }

        float ratio = MaxValue > 0 ? (float)Value / MaxValue : 0f;
        int handleX = (int)(dimensions.X + ratio * (dimensions.Width - 12));
        sliderHandle = new Rectangle(handleX, (int)dimensions.Y, 12, 20);

        Color handleColor = Value >= MinValue ? Color.White : Color.Pink;
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, sliderHandle, handleColor);
    }

    public override void LeftMouseDown(UIMouseEvent evt)
    {
        isDragging = true;
        UpdateValue(evt);
    }

    public override void LeftMouseUp(UIMouseEvent evt)
    {
        isDragging = false;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (isDragging && Main.mouseLeft)
        {
            UpdateValue(new UIMouseEvent(this, Main.MouseScreen));
        }
    }

    private void UpdateValue(UIMouseEvent evt)
    {
        CalculatedStyle dimensions = GetDimensions();
        float ratio = Math.Max(0f, Math.Min(1f, (evt.MousePosition.X - dimensions.X) / dimensions.Width));
        int newValue = (int)(ratio * MaxValue);

        Value = Math.Max(MinValue, newValue);
    }
}

public class ElementalUISystem : ModSystem
{
    private UserInterface elementalInterface;
    internal ElementalBindingUI elementalUI;

    public override void PostSetupContent()
    {
        elementalInterface = new UserInterface();
        elementalUI = new ElementalBindingUI();
        elementalUI.Activate();
    }

    public override void UpdateUI(GameTime gameTime)
    {
        if (IsUIOpen)
            elementalInterface?.Update(gameTime);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "Reverie: Elemental Binding UI",
                delegate
                {
                    if (IsUIOpen)
                        elementalInterface.Draw(Main.spriteBatch, new GameTime());
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }

    public bool IsUIOpen { get; private set; }

    public void ToggleUI()
    {
        if (IsUIOpen)
        {
            CloseUI();
        }
        else
        {
            OpenUI();
        }
    }

    public void OpenUI()
    {
        if (!IsUIOpen)
        {
            elementalInterface?.SetState(elementalUI);
            IsUIOpen = true;
        }
    }

    public void CloseUI()
    {
        if (IsUIOpen)
        {
            elementalInterface?.SetState(null);
            IsUIOpen = false;
        }
    }
}