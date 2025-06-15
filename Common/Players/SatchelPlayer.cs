using Reverie.Content.Items.Botany;
using Reverie.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;

namespace Reverie.Common.Players;

public class SatchelPlayer : ModPlayer
{
    public bool flowerSatchelVisible;
    public Item activeSatchel;

    public float moveSpeedBonus;

    public float pickSpeedBonus;
    public float buildSpeedBonus;
    public int defenseBonus;
    public float damageBonus;
    public float critBonus;
    public int lifeBonus;
    public int manaBonus;

    public float meleeDamageBonus;
    public float magicDamageBonus;

    public bool hasFlameGuard;
    public bool hasInfernoStrike;
    public bool hasBlazingAura;
    public bool daybloom_speedBoost;
    public bool blinkroot_OreChime;
    public bool daybloom_Ironclad;

    public List<string> activeEffects = [];

    private const int MAX_STACK_FOR_STATS = 30;

    public override void Initialize()
    {
        flowerSatchelVisible = false;
        activeSatchel = null;
        ResetBonuses();
    }

    public override void ResetEffects()
    {
        if (activeSatchel == null || activeSatchel.IsAir)
        {
            flowerSatchelVisible = false;
            activeSatchel = null;
        }

        ResetBonuses();
        CalculateFlowerEffects();
    }

    public override void PostUpdateEquips()
    {
        #region general stat buffs
        if (moveSpeedBonus > 0)
            Player.moveSpeed += moveSpeedBonus;

        if (pickSpeedBonus > 0)
            Player.pickSpeed -= pickSpeedBonus;

        if (buildSpeedBonus > 0)
        {
            Player.tileSpeed += buildSpeedBonus;
            Player.wallSpeed += buildSpeedBonus;
        }

        if (defenseBonus > 0)
            Player.statDefense += defenseBonus;

        if (damageBonus > 0)
            Player.GetDamage(DamageClass.Generic) += damageBonus;

        if (meleeDamageBonus > 0)
            Player.GetDamage(DamageClass.Melee) += meleeDamageBonus;

        if (magicDamageBonus > 0)
            Player.GetDamage(DamageClass.Magic) += magicDamageBonus;

        if (critBonus > 0)
            Player.GetCritChance(DamageClass.Generic) += critBonus;

        if (manaBonus > 0)
            Player.statManaMax2 += manaBonus;
        if (lifeBonus > 0)
            Player.statLifeMax2 += lifeBonus;
        #endregion

        if (daybloom_Ironclad)
        {
            int metalItems = CountMetalItems();
            if (metalItems > 0)
            {
                Player.statDefense += metalItems / 4; // +1 defense per each 4 item
            }
        }
        if (daybloom_speedBoost && Main.dayTime)
        {
            Player.moveSpeed += 0.04f;
        }
    }
    public bool HasMetalItem(Item item)
    {
        if (!item.IsMadeFromMetal([ItemID.IronBar, ItemID.LeadBar]))
            return false;

        for (int i = 0; i < 58; i++)
        {
            if (item.type == Player.inventory[i].type && Player.inventory[i].stack > 0)
                return true;
        }

        return false;
    }

    private int CountMetalItems()
    {
        int count = 0;
        for (int i = 0; i < 58; i++) // Main inventory + armor + accessories
        {
            Item item = Player.inventory[i];
            if (!item.IsAir && item.IsMadeFromMetal([ItemID.IronBar, ItemID.LeadBar]))
            {
                count++;
            }
        }
        // Check armor and accessories
        for (int i = 0; i < Player.armor.Length; i++)
        {
            Item item = Player.armor[i];
            if (!item.IsAir && item.IsMadeFromMetal([ItemID.IronBar, ItemID.LeadBar]))
            {
                count++;
            }
        }
        return count;
    }

    public int timer;
    public override void PostUpdate()
    {
        if (blinkroot_OreChime && Player.statLife > Player.statLifeMax2 * 0.9f)
        {
            Player.AddBuff(BuffID.Hunter, 60);
        }
    }

    private void ResetBonuses()
    {
        moveSpeedBonus = 0f;
        pickSpeedBonus = 0f;
        buildSpeedBonus = 0f;
        defenseBonus = 0;
        damageBonus = 0f;
        critBonus = 0f;
        manaBonus = 0;
        lifeBonus = 0;
        meleeDamageBonus = 0f;
        magicDamageBonus = 0f;

        hasFlameGuard = false;
        hasInfernoStrike = false;
        hasBlazingAura = false;
        daybloom_speedBoost = false;
        blinkroot_OreChime = false;
        daybloom_Ironclad = false;

        activeEffects.Clear();
    }

    private void CalculateFlowerEffects()
    {
        if (activeSatchel?.ModItem is not FlowerSatchelItem satchel)
            return;

        // Count flower types and stacks (capped at MAX_STACK_FOR_STATS)
        var flowerCounts = new Dictionary<int, int>();
        var totalFlowers = 0;

        foreach (var item in satchel.items)
        {
            if (!item.IsAir)
            {
                var effectiveStack = Math.Min(item.stack, MAX_STACK_FOR_STATS);
                flowerCounts[item.type] = flowerCounts.GetValueOrDefault(item.type) + effectiveStack;
                totalFlowers += effectiveStack;
            }
        }

        // Apply individual flower effects using the configuration system
        foreach (var kvp in flowerCounts)
        {
            ApplyFlowerEffect(kvp.Key, kvp.Value);
        }

        // Apply combination effects
        ApplyCombinationEffects(flowerCounts);
    }

    private void ApplyFlowerEffect(int itemType, int count)
    {
        if (!FlowerEffectConfig.FlowerEffects.TryGetValue(itemType, out var effect))
            return;

        // Apply the base flower effect
        effect.ApplyEffect(this, count);

        // Apply threshold effects - find the highest threshold we meet
        var applicableThresholds = effect.ThresholdEffects
            .Where(t => count >= t.Key)
            .OrderBy(t => t.Key);

        foreach (var threshold in applicableThresholds)
        {
            threshold.Value.ApplyEffect(this);
        }
    }

    private void ApplyCombinationEffects(Dictionary<int, int> flowerCounts)
    {
        foreach (var combo in FlowerEffectConfig.ComboEffects)
        {
            // Check if all required flowers are present
            var hasAllFlowers = combo.RequiredFlowers.All(f => flowerCounts.ContainsKey(f));
            if (!hasAllFlowers)
                continue;

            // Find the minimum count among required flowers
            var minCount = combo.RequiredFlowers.Select(f => flowerCounts[f]).Min();
            if (minCount > 0)
            {
                combo.ApplyEffect(this, minCount);
            }
        }
    }

    public string GetEffectsSummary()
    {
        if (activeEffects.Count == 0)
            return "No active effects";

        return $"{activeEffects.Count} active effect{(activeEffects.Count > 1 ? "s" : "")}";
    }
}

/// <summary>
/// Configuration system for flower effects in the Flower Satchel.
/// </summary>
public static class FlowerEffectConfig
{
    public class ThresholdEffect
    {
        public Action<SatchelPlayer> ApplyEffect { get; set; }
    }

    public class FlowerEffect
    {
        public int ItemType { get; set; }
        public string Name { get; set; }
        public Action<SatchelPlayer, int> ApplyEffect { get; set; }
        public Dictionary<int, ThresholdEffect> ThresholdEffects { get; set; } = [];
    }

    public class ComboEffect
    {
        public string Name { get; set; }
        public short[] RequiredFlowers { get; set; }
        public Action<SatchelPlayer, int> ApplyEffect { get; set; }
    }

    public static readonly Dictionary<int, FlowerEffect> FlowerEffects = new()
    {
        [ItemID.Daybloom] = new FlowerEffect
        {
            ItemType = ItemID.Daybloom,
            Name = $"[i:{ItemID.Daybloom}]",
            ApplyEffect = (player, count) =>
            {
                var hpBonus = count;
                player.lifeBonus += hpBonus;
                player.activeEffects.Add($"[i:{ItemID.Daybloom}]+{hpBonus} maximum life");

                var defBonus = count / 5;
                if (defBonus > 0)
                {
                    player.defenseBonus += defBonus;
                    player.activeEffects.Add($"[i:{ItemID.Daybloom}]+{defBonus} defense");
                }
            },
            ThresholdEffects = new()
            {
                [10] = new ThresholdEffect
                {
                    ApplyEffect = (player) =>
                    {
                        player.daybloom_speedBoost = true;
                        player.activeEffects.Add($"[i:{ItemID.Daybloom}]gain +4% speed boost during the day");
                    }
                },
                [30] = new ThresholdEffect
                {
                    ApplyEffect = (player) =>
                    {
                        player.daybloom_Ironclad = true;
                        player.activeEffects.Add($"[i:{ItemID.Daybloom}][MAX] +1 defense per each 4 items made of iron on your person");
                    }
                },
            }
        },

        [ItemID.Blinkroot] = new FlowerEffect
        {
            ItemType = ItemID.Blinkroot,
            Name = $"[i:{ItemID.Blinkroot}]",
            ApplyEffect = (player, count) =>
            {
                // Mining speed: 1% per flower
                var pickBonus = count * 0.0084f;
                player.pickSpeedBonus += pickBonus;
                player.activeEffects.Add($"[i:{ItemID.Blinkroot}]+{pickBonus:P0} mining speed");

                // Damage: 1% per 3 flowers
                var damageBonus = count * (0.01f / 3f);
                if (damageBonus > 0)
                {
                    player.damageBonus += damageBonus;
                    player.activeEffects.Add($"[i:{ItemID.Blinkroot}]+{damageBonus:P1} damage");
                }
            },
            ThresholdEffects = new()
            {
                [15] = new ThresholdEffect
                {
                    ApplyEffect = (player) =>
                    {
                        player.Player.nightVision = true;
                        player.activeEffects.Add($"[i:{ItemID.Blinkroot}][grants[i:{ItemID.NightOwlPotion}]effects]");
                    }
                },
                [30] = new ThresholdEffect
                {
                    ApplyEffect = (player) =>
                    {
                        player.blinkroot_OreChime = true;
                        player.activeEffects.Add($"[i:{ItemID.Blinkroot}][gain[i:{ItemID.HunterPotion}]effects when above 90% HP]");
                    }
                }
            }
        },

        [ItemID.Fireblossom] = new FlowerEffect
        {
            ItemType = ItemID.Fireblossom,
            Name = $"[i:{ItemID.Fireblossom}]",
            ApplyEffect = (player, count) =>
            {
                // Melee damage: 1.5% per flower
                var meleeBonus = count * 0.015f;
                player.meleeDamageBonus += meleeBonus;
                player.activeEffects.Add($"[i:{ItemID.Fireblossom}]+{meleeBonus:P0} melee damage");

                // Defense: 1 per 10 flowers
                var defBonus = count / 10;
                if (defBonus > 0)
                {
                    player.defenseBonus += defBonus;
                    player.activeEffects.Add($"[i:{ItemID.Fireblossom}]+{defBonus} defense");
                }
            },
            ThresholdEffects = new()
            {
                [5] = new ThresholdEffect
                {
                    ApplyEffect = (player) =>
                    {
                        // You might need to implement custom damage reduction logic
                        player.activeEffects.Add("[Flame Guard: 50% reduced fire damage]");
                    }
                },
                [15] = new ThresholdEffect
                {
                    ApplyEffect = (player) =>
                    {
                        // Would need to be handled in OnHitNPC or similar
                        player.activeEffects.Add("[Inferno Strike: Melee attacks inflict On Fire!]");
                    }
                },
                [25] = new ThresholdEffect
                {
                    ApplyEffect = (player) =>
                    {
                        // Would need to be handled in PostUpdate or similar
                        player.activeEffects.Add("[Blazing Aura: Enemies take damage when near]");
                    }
                },
                [30] = new ThresholdEffect
                {
                    ApplyEffect = (player) =>
                    {
                        player.activeEffects.Add("[Pyroclasm: Max effect reached]");
                    }
                }
            }
        },

        [ItemID.Shiverthorn] = new FlowerEffect
        {
            ItemType = ItemID.Shiverthorn,
            Name = $"[i:{ItemID.Shiverthorn}]",
            ApplyEffect = (player, count) =>
            {
                // Critical strike: 0.5% per flower
                var critBonus = count * 0.5f;
                player.critBonus += critBonus;
                player.activeEffects.Add($"[i:{ItemID.Shiverthorn}]+{critBonus:F1}% critical strike chance");

                // Build speed: 1.5% per flower
                var buildBonus = count * 0.015f;
                player.buildSpeedBonus += buildBonus;
                player.activeEffects.Add($"[i:{ItemID.Shiverthorn}]+{buildBonus:P0} placement speed");
            },
            ThresholdEffects = new()
            {
                [8] = new ThresholdEffect
                {
                    ApplyEffect = (player) =>
                    {
                        player.Player.buffImmune[BuffID.Chilled] = true;
                        player.Player.buffImmune[BuffID.Frozen] = true;
                        player.activeEffects.Add("[Frost Shield: Immunity to Chilled and Frozen]");
                    }
                },
                [16] = new ThresholdEffect
                {
                    ApplyEffect = (player) =>
                    {
                        // Would need custom crit damage implementation
                        player.activeEffects.Add("[Icy Precision: +5% crit damage]");
                    }
                },
                [24] = new ThresholdEffect
                {
                    ApplyEffect = (player) =>
                    {
                        // Would need to be handled in OnHitNPC with crit check
                        player.activeEffects.Add("[Permafrost: Crits slow enemies]");
                    }
                },
                [30] = new ThresholdEffect
                {
                    ApplyEffect = (player) =>
                    {
                        player.activeEffects.Add("[Absolute Zero: Max effect reached]");
                    }
                }
            }
        }
    };

    public static readonly List<ComboEffect> ComboEffects =
    [
        new ComboEffect
        {
            Name = "Solar Swiftness",
            RequiredFlowers = [ItemID.Daybloom, ItemID.Blinkroot],
            ApplyEffect = (player, minCount) =>
            {
                var bonus = minCount * 0.015f;
                player.moveSpeedBonus += bonus;
                player.damageBonus += bonus;
                player.activeEffects.Add($"Solar Swiftness: +{bonus:P0} speed & damage");
            }
        },

        new ComboEffect
        {
            Name = "Thermal Shock",
            RequiredFlowers = new[] { ItemID.Fireblossom, ItemID.Shiverthorn },
            ApplyEffect = (player, minCount) =>
            {
                var critBonus = minCount * 0.3f;
                var damageBonus = minCount * 0.01f;
                player.critBonus += critBonus;
                player.damageBonus += damageBonus;
                player.activeEffects.Add($"Thermal Shock: +{critBonus:F1}% crit, +{damageBonus:P0} damage");
            }
        },

        new ComboEffect
        {
            Name = "Nature's Vigor",
            RequiredFlowers = [ItemID.Daybloom, ItemID.Fireblossom],
            ApplyEffect = (player, minCount) =>
            {
                var defBonus = minCount / 3;
                var speedBonus = minCount * 0.01f;
                if (defBonus > 0)
                {
                    player.defenseBonus += defBonus;
                }
                player.moveSpeedBonus += speedBonus;
                player.activeEffects.Add($"Nature's Vigor: +{defBonus} defense, +{speedBonus:P0} speed");
            }
        }
    ];
}