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
    public float enduranceBonus;
    public float damageBonus;
    public float critBonus;
    public int lifeBonus;
    public int manaBonus;

    public float meleeDamageBonus;
    public float magicDamageBonus;

    public bool shiverthorn_FrozenCrit;
    public bool shiverthorn_Frostburn;
    public bool shiverthorn_Dangersense;
    public bool daybloom_speedBoost;
    public bool blinkroot_OreChime;
    public bool daybloom_Ironclad;

    // Separate effect lists
    public List<string> individualFlowerEffects = [];
    public List<string> comboEffects = [];

    // Combined list for compatibility (if needed elsewhere)
    public List<string> activeEffects => GetCombinedEffects();

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

        if (enduranceBonus > 0)
            Player.endurance += enduranceBonus;

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
                Player.statDefense += metalItems / 4; // +1 defense per each 4 item         
        }

        if (daybloom_speedBoost && Main.dayTime)
            Player.moveSpeed += 0.04f;

        if (shiverthorn_FrozenCrit && Player.ZoneSnow)
            Player.GetCritChance(DamageClass.Generic) += 0.06f;
    }

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
    {
        base.OnHitByNPC(npc, hurtInfo);
        if (shiverthorn_Dangersense)
        {
            Player.AddBuff(BuffID.Dangersense, 90);
        }
    }

    public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
    {
        base.OnHitByProjectile(proj, hurtInfo);
        if (shiverthorn_Dangersense)
        {
            Player.AddBuff(BuffID.Dangersense, 90);
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
        enduranceBonus = 0f;
        damageBonus = 0f;
        critBonus = 0f;
        manaBonus = 0;
        lifeBonus = 0;
        meleeDamageBonus = 0f;
        magicDamageBonus = 0f;

        shiverthorn_FrozenCrit = false;
        shiverthorn_Frostburn = false;
        shiverthorn_Dangersense = false;
        daybloom_speedBoost = false;
        blinkroot_OreChime = false;
        daybloom_Ironclad = false;

        individualFlowerEffects.Clear();
        comboEffects.Clear();
    }

    private void CalculateFlowerEffects()
    {
        if (activeSatchel?.ModItem is not FlowerSatchelItem satchel)
            return;

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

        foreach (var kvp in flowerCounts)
        {
            ApplyFlowerEffect(kvp.Key, kvp.Value);
        }

        ApplyComboEffects(flowerCounts);
    }

    private void ApplyFlowerEffect(int itemType, int count)
    {
        if (!FlowerEffectConfig.FlowerEffects.TryGetValue(itemType, out var effect))
            return;

        effect.ApplyEffect(this, count);

        var applicableThresholds = effect.ThresholdEffects
            .Where(t => count >= t.Key)
            .OrderBy(t => t.Key);

        foreach (var threshold in applicableThresholds)
        {
            threshold.Value.ApplyEffect(this);
        }
    }

    private void ApplyComboEffects(Dictionary<int, int> flowerCounts)
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

    // Method to get effects for display based on alt key state
    public List<string> GetDisplayEffects()
    {
        var displayEffects = new List<string>();

        // Always show combo effects
        displayEffects.AddRange(comboEffects);

        // Only show individual flower effects when holding left-alt
        if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftAlt))
        {
            displayEffects.AddRange(individualFlowerEffects);
        }
        else if (individualFlowerEffects.Count > 0)
        {
            // Show hint when there are hidden effects
            displayEffects.Add("Hold [Left Alt] to show flower effects");
        }

        return displayEffects;
    }

    // Legacy method for compatibility
    private List<string> GetCombinedEffects()
    {
        var combined = new List<string>();
        combined.AddRange(comboEffects);
        combined.AddRange(individualFlowerEffects);
        return combined;
    }

    public string GetSummary()
    {
        var totalEffects = comboEffects.Count + individualFlowerEffects.Count;
        if (totalEffects == 0)
            return "No active effects";

        return $"{totalEffects} active effect{(totalEffects > 1 ? "s" : "")}";
    }
}

/// <summary>
/// Configuration system for flower effects in the Flower Satchel.
/// </summary>
public static class FlowerEffectConfig
{
    public class FlowerEffect_Special
    {
        public Action<SatchelPlayer> ApplyEffect { get; set; }
    }

    public class FlowerEffect
    {
        public int ItemType { get; set; }
        public string Name { get; set; }
        public Action<SatchelPlayer, int> ApplyEffect { get; set; }
        public Dictionary<int, FlowerEffect_Special> ThresholdEffects { get; set; } = [];
    }

    public class ComboEffect
    {
        public string Name { get; set; }
        public short[] RequiredFlowers { get; set; }
        public Action<SatchelPlayer, int> ApplyEffect { get; set; }
    }

    public static readonly Dictionary<int, FlowerEffect> FlowerEffects = new()
    {
        [ItemID.Blinkroot] = new FlowerEffect
        {
            ItemType = ItemID.Blinkroot,
            Name = $"[i:{ItemID.Blinkroot}]",
            ApplyEffect = (player, count) =>
            {
                // Mining speed: 1% per flower
                var pickBonus = count * 0.0084f;
                player.pickSpeedBonus += pickBonus;
                player.individualFlowerEffects.Add($"[i:{ItemID.Blinkroot}]+{pickBonus:P0} mining speed");

                // Damage: 1% per 3 flowers
                var damageBonus = count * (0.01f / 3f);
                if (damageBonus > 0)
                {
                    player.damageBonus += damageBonus;
                    player.individualFlowerEffects.Add($"[i:{ItemID.Blinkroot}]+{damageBonus:P1} damage");
                }
            },
            ThresholdEffects = new()
            {
                [15] = new FlowerEffect_Special
                {
                    ApplyEffect = (player) =>
                    {
                        player.Player.nightVision = true;
                        player.individualFlowerEffects.Add($"[i:{ItemID.Blinkroot}][grants[i:{ItemID.NightOwlPotion}]effects]");
                    }
                },
                [30] = new FlowerEffect_Special
                {
                    ApplyEffect = (player) =>
                    {
                        player.blinkroot_OreChime = true;
                        player.individualFlowerEffects.Add($"[i:{ItemID.Blinkroot}][gain[i:{ItemID.HunterPotion}]effects when above 90% HP]");
                    }
                }
            }
        },

        [ItemID.Daybloom] = new FlowerEffect
        {
            ItemType = ItemID.Daybloom,
            Name = $"[i:{ItemID.Daybloom}]",
            ApplyEffect = (player, count) =>
            {
                var hpBonus = count;
                player.lifeBonus += hpBonus;
                player.individualFlowerEffects.Add($"[i:{ItemID.Daybloom}]+{hpBonus} maximum life");

                var defBonus = count / 6;
                if (defBonus > 0)
                {
                    player.defenseBonus += defBonus;
                    player.individualFlowerEffects.Add($"[i:{ItemID.Daybloom}]+{defBonus} defense");
                }
            },
            ThresholdEffects = new()
            {
                [10] = new FlowerEffect_Special
                {
                    ApplyEffect = (player) =>
                    {
                        player.daybloom_speedBoost = true;
                        player.individualFlowerEffects.Add($"[i:{ItemID.Daybloom}]gain +4% speed boost during the day");
                    }
                },
                [30] = new FlowerEffect_Special
                {
                    ApplyEffect = (player) =>
                    {
                        player.daybloom_Ironclad = true;
                        player.individualFlowerEffects.Add($"[i:{ItemID.Daybloom}][MAX] +1 defense per each 4 items made of iron on your person");
                    }
                },
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
                player.individualFlowerEffects.Add($"[i:{ItemID.Fireblossom}]+{meleeBonus:P0} melee damage");

                var critBonus = count / 6;
                if (critBonus > 0)
                {
                    player.critBonus += critBonus;
                    player.individualFlowerEffects.Add($"[i:{ItemID.Fireblossom}]+{critBonus} critical strike chance");
                }
            },
            ThresholdEffects = new()
            {
                [15] = new FlowerEffect_Special
                {
                    ApplyEffect = (player) =>
                    {
                        // Would need to be handled in OnHitNPC or similar
                        player.individualFlowerEffects.Add($"[i:{ItemID.Fireblossom}]Melee attacks inflict On Fire!");
                    }
                },
                [30] = new FlowerEffect_Special
                {
                    ApplyEffect = (player) =>
                    {
                        player.individualFlowerEffects.Add($"[i:{ItemID.Fireblossom}]Max effect reached");
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
                var critBonus = count * 0.3f;
                player.critBonus += critBonus;
                player.individualFlowerEffects.Add($"[i:{ItemID.Shiverthorn}]+{critBonus:F1}% critical strike chance");

                var buildBonus = count * 0.00675f;
                player.buildSpeedBonus += buildBonus;
                player.individualFlowerEffects.Add($"[i:{ItemID.Shiverthorn}]+{buildBonus:P0} placement speed");
            },
            ThresholdEffects = new()
            {
                [15] = new FlowerEffect_Special
                {
                    ApplyEffect = (player) =>
                    {
                        player.shiverthorn_Dangersense = true;
                        player.individualFlowerEffects.Add($"[i:{ItemID.Shiverthorn}] grants[i:{ItemID.TrapsightPotion}]shortly when hit");
                    }
                },
                [30] = new FlowerEffect_Special
                {
                    ApplyEffect = (player) =>
                    {
                        player.shiverthorn_FrozenCrit = true;
                        player.individualFlowerEffects.Add($"[i:{ItemID.Shiverthorn}] +6% crit damage while in any frozen biome");
                    }
                },
            }
        }
    };

    public static readonly List<ComboEffect> ComboEffects =
    [
        new ComboEffect
        {
            Name = "Dayblink",
            RequiredFlowers = [ItemID.Daybloom, ItemID.Blinkroot],
            ApplyEffect = (player, minCount) =>
            {
                var bonus = minCount * 0.0032f;
                player.enduranceBonus += bonus;
                player.comboEffects.Add($"([i:{ItemID.Daybloom}]+[i:{ItemID.Blinkroot}]) +{bonus:P0} damage reduction");
            }
        },

        new ComboEffect
        {
            Name = "Obsidian Boquet",
            RequiredFlowers = [ItemID.Fireblossom, ItemID.Waterleaf],
            ApplyEffect = (player, minCount) =>
            {
                var hpBonus = minCount + 2;
                player.lifeBonus += hpBonus;
                player.comboEffects.Add($"([i:{ItemID.Fireblossom}]+[i:{ItemID.Shiverthorn}]) +{hpBonus} maximum life");
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
                player.comboEffects.Add($"Nature's Vigor: +{defBonus} defense, +{speedBonus:P0} speed");
            }
        }
    ];
}