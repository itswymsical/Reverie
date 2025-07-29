using Reverie.Common.Items.Types;
using System.Collections.Generic;

namespace Reverie.Content.Items.Relics;

public class VitalityRelic : RelicItem
{
    public override int MaxLevel => 15;
    public override int XPPerLevel => 120;
    public override float XPAbsorptionRate => 0.8f; // Absorbs less XP than other relics
    public override string Texture => PLACEHOLDER;


    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.rare = ItemRarityID.Pink;
    }

    public override void UpdateRelicEffects(Player player)
    {
        // Base health boost scales with level
        var healthBoost = GetLevelScaling(20f, 15f); // 20 + (level-1) * 15
        player.statLifeMax2 += (int)healthBoost;

        // Life regen scales more slowly
        var regenBoost = GetLevelScaling(1f, 0.5f);
        player.lifeRegen += (int)regenBoost;

        // At higher levels, gain damage reduction
        if (RelicLevel >= 5)
        {
            var damageReduction = GetLevelScaling(0f, 0.02f); // 2% per level after 5
            player.endurance += damageReduction;
        }

        // At max level, gain special effect
        if (RelicLevel >= MaxLevel)
        {
            player.buffImmune[BuffID.Bleeding] = true;
            player.buffImmune[BuffID.Poisoned] = true;
        }
    }

    public override void AddTooltips(List<TooltipLine> tooltips)
    {
        var healthBoost = (int)GetLevelScaling(20f, 15f);
        var regenBoost = (int)GetLevelScaling(1f, 0.5f);

        tooltips.Add(new TooltipLine(Mod, "HealthBonus", $"+{healthBoost} max life")
        {
            OverrideColor = Color.Pink
        });

        tooltips.Add(new TooltipLine(Mod, "RegenBonus", $"+{regenBoost} life regeneration")
        {
            OverrideColor = Color.Pink
        });

        if (RelicLevel >= 5)
        {
            var damageReduction = (int)(GetLevelScaling(0f, 0.02f) * 100);
            tooltips.Add(new TooltipLine(Mod, "DamageReduction", $"{damageReduction}% damage reduction")
            {
                OverrideColor = Color.Orange
            });
        }

        if (RelicLevel >= MaxLevel)
        {
            tooltips.Add(new TooltipLine(Mod, "Immunity", "Immune to bleeding and poison")
            {
                OverrideColor = Color.LimeGreen
            });
        }
    }

    public override void OnLevelUp()
    {
        base.OnLevelUp();

        // Custom level up effects for this relic
        for (int i = 0; i < 12; i++)
        {
            var dust = Dust.NewDustDirect(
                Main.LocalPlayer.position - new Vector2(20),
                Main.LocalPlayer.width + 40,
                Main.LocalPlayer.height + 40,
                DustID.Blood,
                Main.rand.NextFloat(-2f, 2f),
                Main.rand.NextFloat(-3f, 1f),
                100, default, 1.5f
            );
            dust.noGravity = true;
        }
    }
}