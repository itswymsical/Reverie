using Reverie.Common.Configs;
using Reverie.Common.Players;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Reverie.Common.UI.ExperienceBar;

internal class ExperienceMeter : UIState
{
    private UIText text;
    private UIText capacity;
    private UIElement area;
    private UIImage barFrame;

    public override void OnInitialize()
    {
        area = new UIElement();
        area.Top.Set(16, 0f);
        area.Width.Set(154, 0f);
        area.Height.Set(74, 0f);
        area.Left.Set(-area.Width.Pixels * 3.15f, 1f);

        barFrame = new UIImage(ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Experience/ExperienceBar"));
        barFrame.Left.Set(16, 0f);
        barFrame.Top.Set(0, 0f);
        barFrame.Width.Set(154, 0f);
        barFrame.Height.Set(74, 0f);

        text = new UIText("0/0", 0.92f);
        text.Width.Set(154, 0f);
        text.Height.Set(74, 0f);
        text.Top.Set(44, 0f);
        text.Left.Set(-8f, 0f);

        capacity = new UIText("0", 0.75f);
        capacity.Width.Set(154, 0f);
        capacity.Height.Set(73.25f, 0f);
        capacity.Top.Set(4f, 0f);
        capacity.Left.Set(78f, 0f);

        area.Append(barFrame);
        area.Append(capacity);
        area.Append(text);
        Append(area);
    }

    public override void Draw(SpriteBatch spriteBatch) => base.Draw(spriteBatch);

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);
        var modPlayer = Main.LocalPlayer.GetModPlayer<ExperiencePlayer>();
        var config = ModContent.GetInstance<ExperienceMeterConfig>();

        // Calculate fill percentage based on current exp vs max capacity
        var xpPercentage = modPlayer.MaxExperience > 0 ? (float)modPlayer.curExp / modPlayer.MaxExperience : 0f;
        xpPercentage = MathHelper.Clamp(xpPercentage, 0f, 1f);

        var hitbox = barFrame.GetInnerDimensions().ToRectangle();

        capacity.Width.Set(154, 0f);
        capacity.Height.Set(74f, 0f);
        capacity.Top.Set(11.5f, 0f);
        capacity.Left.Set(72f, 0f);

        spriteBatch.Draw(
            ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Experience/ExperienceBar_Empty").Value,
            hitbox,
            config.BarColor
        );

        var fillableWidth = hitbox.Width / 2 + 20;
        var fillableHeight = 12;
        var fillWidth = (int)(fillableWidth * xpPercentage);

        var xOffset = 6;
        var yOffset = hitbox.Height - fillableHeight - 18;

        var fillRect = new Rectangle(hitbox.X + xOffset, hitbox.Y + yOffset, fillWidth, fillableHeight);

        // Draw the experience fill bar
        for (var i = 0; i < fillWidth; i += 12)
        {
            var segmentWidth = Math.Min(12, fillWidth - i);
            var sourceRect = new Rectangle(0, 0, segmentWidth, 12);
            var destRect = new Rectangle(hitbox.X + xOffset + i, hitbox.Y + yOffset, segmentWidth, 12);
            spriteBatch.Draw(
                ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Experience/ExperienceBar_Fill").Value,
                destRect,
                sourceRect,
                config.BarColor
            );
        }

        spriteBatch.Draw(
            ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Experience/ExperienceBar_Moon").Value,
            hitbox,
            config.BarColor
        );
    }

    public override void Update(GameTime gameTime)
    {
        var modPlayer = Main.LocalPlayer.GetModPlayer<ExperiencePlayer>();

        // Display current exp / max exp
        text.SetText($"{modPlayer.curExp} / {modPlayer.MaxExperience}", 0.6f, false);

        // Display capacity level instead of character level
        capacity.SetText($"{modPlayer.expCapacity}", 1f, false);

        base.Update(gameTime);
    }
}

[Autoload(Side = ModSide.Client)]
internal class ExperienceUISystem : ModSystem
{
    private UserInterface ExperienceMeterUserInterface;
    private ExperienceMeter ExperienceMeter;

    public override void Load()
    {
        ExperienceMeter = new();
        ExperienceMeterUserInterface = new();
        ExperienceMeterUserInterface.SetState(ExperienceMeter);
    }

    public override void UpdateUI(GameTime gameTime) => ExperienceMeterUserInterface?.Update(gameTime);

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        var resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
        if (resourceBarIndex != -1)
        {
            layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                "Reverie: Experience Meter",
                delegate
                {
                    ExperienceMeterUserInterface.Draw(Main.spriteBatch, new GameTime());
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }
}