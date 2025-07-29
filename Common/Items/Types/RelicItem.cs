using System.Collections.Generic;
using System.IO;
using Terraria.GameContent;
using Terraria.ModLoader.IO;

namespace Reverie.Common.Items.Types;

public abstract class RelicItem : ModItem
{
    public int RelicLevel { get; set; } = 1;
    public int StoredXP { get; set; } = 0;
    public virtual int MaxLevel => 10;
    public virtual int XPPerLevel => 100;

    public virtual float XPAbsorptionRate => 1.0f;

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (XPAbsorptionRate != 1.0f)
        {
            var percentage = (int)(XPAbsorptionRate * 100);
            tooltips.Add(new TooltipLine(Mod, "RelicAbsorption", $"xp absorption: {percentage}%")
            {
                OverrideColor = Color.White
            });
        }

        AddTooltips(tooltips);

        tooltips.Add(new TooltipLine(Mod, "RelicItem", $"Relic Item")
        {
            OverrideColor = Color.Orange
        });
    }

    public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
    {
        if (line.Name == "RelicItem" && line.Mod == Mod.Name)
        {

            var basePosition = new Vector2(line.X, line.Y);

            var time = Main.GlobalTimeWrappedHourly * 3f;
            var amplitude = 2.03f;
            var rarityColor = line.OverrideColor.GetValueOrDefault(line.Color);

            var text = line.Text;
            var fullTextSize = FontAssets.MouseText.Value.MeasureString(text);
            var totalTextWidth = fullTextSize.X * line.BaseScale.X;
            var textHeight = fullTextSize.Y * line.BaseScale.Y;

            var posX = basePosition.X;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.UIScaleMatrix);

            posX = basePosition.X;
            for (var i = 0; i < text.Length; i++)
            {
                var sineOffset = (float)Math.Sin(time + i * 0.5f) * amplitude;
                var charPos = new Vector2(posX, basePosition.Y + sineOffset);
                var charStr = text[i].ToString();
                var textSize = FontAssets.MouseText.Value.MeasureString(charStr);
                var charWidth = textSize.X * line.BaseScale.X;

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

    public virtual void AddTooltips(List<TooltipLine> tooltips) { }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        UpdateRelicEffects(player);
    }

    public virtual void UpdateRelicEffects(Player player)
    {
    }

    public bool CanGainXP()
    {
        return RelicLevel < MaxLevel;
    }

    public void AddXP(int amount)
    {
        if (!CanGainXP()) return;

        amount = (int)(amount * XPAbsorptionRate);
        StoredXP += amount;

        while (StoredXP >= XPPerLevel * RelicLevel && RelicLevel < MaxLevel)
        {
            StoredXP -= XPPerLevel * RelicLevel;
            RelicLevel++;
            OnLevelUp();
        }
    }

    public virtual void OnLevelUp()
    {
        for (int i = 0; i < 8; i++)
        {
            var dust = Dust.NewDustDirect(
                Main.LocalPlayer.position - new Vector2(16),
                Main.LocalPlayer.width + 32,
                Main.LocalPlayer.height + 32,
                DustID.GoldFlame,
                0f, 0f, 100, default, 1.2f
            );
            dust.velocity *= 0.5f;
            dust.noGravity = true;
        }
    }

    public int ExtractAllXP()
    {
        var totalXP = StoredXP;

        for (var i = 1; i < RelicLevel; i++)
        {
            totalXP += XPPerLevel * i;
        }

        RelicLevel = 1;
        StoredXP = 0;

        return totalXP;
    }

    protected float GetLevelScaling(float baseValue, float scalingPerLevel = 0.1f)
    {
        return baseValue + (RelicLevel - 1) * scalingPerLevel;
    }

    public override void SaveData(TagCompound tag)
    {
        tag["RelicLevel"] = RelicLevel;
        tag["StoredXP"] = StoredXP;
    }

    public override void LoadData(TagCompound tag)
    {
        RelicLevel = tag.GetInt("RelicLevel");
        StoredXP = tag.GetInt("StoredXP");

        if (RelicLevel < 1) RelicLevel = 1;
        if (RelicLevel > MaxLevel) RelicLevel = MaxLevel;
        if (StoredXP < 0) StoredXP = 0;
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write(RelicLevel);
        writer.Write(StoredXP);
    }

    public override void NetReceive(BinaryReader reader)
    {
        RelicLevel = reader.ReadInt32();
        StoredXP = reader.ReadInt32();
    }

    public override ModItem Clone(Item newEntity)
    {
        var clone = (RelicItem)base.Clone(newEntity);
        clone.RelicLevel = RelicLevel;
        clone.StoredXP = StoredXP;
        return clone;
    }
}