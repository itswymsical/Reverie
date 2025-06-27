using Microsoft.Xna.Framework.Input;
using Reverie.Content.Items.Botany;
using Reverie.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace Reverie.Common.Players;

public class SatchelPlayer : ModPlayer
{
    #region Properties / Fields / Whatever
    public bool flowerSatchelVisible;
    public Item activeSatchel;

    public float moveSpeedBonus;
    public float pickSpeedBonus;
    public float buildSpeedBonus;

    public int defenseBonus;
    public float enduranceBonus;
    public float damageBonus;
    public float critBonus;
    public float kbBonus;
    public float thornsBonus;

    public int lifeBonus;
    public int lifeRegenBonus;
    public int manaRegenBonus;
    public int manaBonus;

    public float meleeDamageBonus;
    public float magicDamageBonus;

    public bool shiverthorn_FrozenCrit;
    public bool shiverthorn_Frostburn;
    public bool Combo_ExplorersCorsage;
    public bool Combo_SunriseSpray;
    public bool Combo_DawnsGrace;
    public bool daybloom_Ironclad;

    public List<string> individualFlowerEffects = [];
    public List<string> comboEffects = [];

    public List<string> activeEffects => GetCombinedEffects();

    private const int MAX_STACK_FOR_STATS = 30;

    public int timer;
    #endregion

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

    #region Stat Checks / Update Logic
    public override void PostUpdateEquips()
    {
        #region General Stat Buffs
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

        if (thornsBonus > 0)
            Player.thorns += thornsBonus;

        if (manaBonus > 0)
            Player.statManaMax2 += manaBonus;

        if (lifeBonus > 0)
            Player.statLifeMax2 += lifeBonus;

        if (lifeRegenBonus > 0)
            Player.lifeRegen += lifeRegenBonus;

        if (manaRegenBonus > 0)
            Player.manaRegen += manaRegenBonus;

        if (kbBonus > 0)
            Player.GetKnockback(DamageClass.Generic) += kbBonus;
        #endregion

        if (daybloom_Ironclad)
        {
            int metalItems = CountMetalItems();
            if (metalItems > 0)
                Player.statDefense += metalItems / 4;       
        }

        if (Combo_SunriseSpray && Main.dayTime)
            Player.moveSpeed += 0.04f;

        if (shiverthorn_FrozenCrit && Player.ZoneSnow)
            Player.GetCritChance(DamageClass.Generic) += 0.06f;
    }

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
    {
        base.OnHitByNPC(npc, hurtInfo);
        if (Combo_ExplorersCorsage)
        {
            Player.AddBuff(BuffID.Dangersense, 90);
        }
    }

    public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
    {
        base.OnHitByProjectile(proj, hurtInfo);
        if (Combo_ExplorersCorsage)
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
        for (int i = 0; i < 58; i++)
        {
            Item item = Player.inventory[i];
            if (!item.IsAir && item.IsMadeFromMetal([ItemID.IronBar, ItemID.LeadBar]))
            {
                count++;
            }
        }
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

    public override void PostUpdate()
    {
        if (Combo_DawnsGrace && Player.statLife > Player.statLifeMax2 * 0.9f)
        {
            Player.dangerSense = true;
            Player.detectCreature = true;
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
        kbBonus = 0f;
        thornsBonus = 0f;

        lifeBonus = 0;
        lifeRegenBonus = 0;
        manaRegenBonus = 0;
        manaBonus = 0;

        meleeDamageBonus = 0f;
        magicDamageBonus = 0f;

        shiverthorn_FrozenCrit = false;
        shiverthorn_Frostburn = false;
        Combo_ExplorersCorsage = false;
        Combo_SunriseSpray = false;
        Combo_DawnsGrace = false;
        daybloom_Ironclad = false;

        individualFlowerEffects.Clear();
        comboEffects.Clear();
    }
    #endregion

    #region Main Logic
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

        var flowersConsumedByCombos = new HashSet<int>();

        ApplyComboEffects(flowerCounts, flowersConsumedByCombos);

        foreach (var kvp in flowerCounts)
        {
            if (!flowersConsumedByCombos.Contains(kvp.Key))
            {
                ApplyFlowerEffect(kvp.Key, kvp.Value);
            }
        }
    }

    private void ApplyFlowerEffect(int itemType, int count)
    {
        if (!FlowerEffectConfig.FlowerEffects.TryGetValue(itemType, out var effect))
            return;

        effect.ApplyEffect(this, count);
    }

    private void ApplyComboEffects(Dictionary<int, int> flowerCounts, HashSet<int> consumedFlowers)
    {
        foreach (var combo in FlowerEffectConfig.ComboEffects)
        {
            var hasAllFlowers = combo.RequiredFlowers.All(f => flowerCounts.ContainsKey(f));
            if (!hasAllFlowers)
                continue;

            var minCount = combo.RequiredFlowers.Select(f => flowerCounts[f]).Min();
            if (minCount > 0)
            {
                combo.ApplyEffect(this, flowerCounts, minCount);

                var applicableThresholds = combo.ExtraEffects
                    .Where(t => minCount >= t.Key)
                    .OrderBy(t => t.Key);

                foreach (var threshold in applicableThresholds)
                {
                    threshold.Value.ApplyEffect(this);
                }

                foreach (var flowerType in combo.RequiredFlowers)
                {
                    consumedFlowers.Add(flowerType);
                }
            }
        }
    }
    #endregion

    #region Display Info
    public List<string> GetDisplayEffects()
    {
        var displayEffects = new List<string>();

        displayEffects.AddRange(comboEffects);

        if (Main.keyState.IsKeyDown(Keys.LeftAlt))
        {
            displayEffects.AddRange(individualFlowerEffects);
        }
        else if (individualFlowerEffects.Count > 0)
        {
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
        var summaryParts = new List<string>();

        var activeCombos = GetActiveCombos();
        summaryParts.AddRange(activeCombos);

        var activeFlowers = GetActiveIndividualFlowers();
        if (activeFlowers.Count > 0)
        {
            var flowerNames = string.Join(", ", activeFlowers);
            var effectText = "effects";
            summaryParts.Add($"{flowerNames} {effectText}");
        }

        if (summaryParts.Count == 0)
            return "No active effects";

        return string.Join(" | ", summaryParts);
    }

    private List<string> GetActiveCombos()
    {
        var activeCombos = new List<string>();

        if (activeSatchel?.ModItem is not FlowerSatchelItem satchel)
            return activeCombos;

        var flowerCounts = GetFlowerCounts();

        foreach (var combo in FlowerEffectConfig.ComboEffects)
        {
            var hasAllFlowers = combo.RequiredFlowers.All(f => flowerCounts.ContainsKey(f));
            if (hasAllFlowers)
            {
                var minCount = combo.RequiredFlowers.Select(f => flowerCounts[f]).Min();
                if (minCount > 0)
                {
                    activeCombos.Add(combo.Name);
                }
            }
        }

        return activeCombos;
    }

    private List<string> GetActiveIndividualFlowers()
    {
        var activeFlowers = new List<string>();

        if (activeSatchel?.ModItem is not FlowerSatchelItem satchel)
            return activeFlowers;

        var flowerCounts = GetFlowerCounts();
        var flowersInCombos = new HashSet<int>();

        foreach (var combo in FlowerEffectConfig.ComboEffects)
        {
            var hasAllFlowers = combo.RequiredFlowers.All(f => flowerCounts.ContainsKey(f));
            if (hasAllFlowers)
            {
                var minCount = combo.RequiredFlowers.Select(f => flowerCounts[f]).Min();
                if (minCount > 0)
                {
                    foreach (var flowerType in combo.RequiredFlowers)
                    {
                        flowersInCombos.Add(flowerType);
                    }
                }
            }
        }

        foreach (var kvp in flowerCounts)
        {
            if (!flowersInCombos.Contains(kvp.Key) && kvp.Value > 0)
            {
                var flowerName = GetFlowerDisplayName(kvp.Key);
                if (!string.IsNullOrEmpty(flowerName))
                {
                    activeFlowers.Add(flowerName);
                }
            }
        }

        return activeFlowers;
    }

    private Dictionary<int, int> GetFlowerCounts()
    {
        var flowerCounts = new Dictionary<int, int>();

        if (activeSatchel?.ModItem is not FlowerSatchelItem satchel)
            return flowerCounts;

        foreach (var item in satchel.items)
        {
            if (!item.IsAir)
            {
                var effectiveStack = Math.Min(item.stack, MAX_STACK_FOR_STATS);
                flowerCounts[item.type] = flowerCounts.GetValueOrDefault(item.type) + effectiveStack;
            }
        }

        return flowerCounts;
    }

    private string GetFlowerDisplayName(int itemType)
    {
        if (FlowerEffectConfig.FlowerEffects.ContainsKey(itemType))
        {
            return Lang.GetItemNameValue(itemType);
        }

        return "";
    }
    #endregion
}
public static class FlowerEffectConfig
{
    public class ComboEffect_Special
    {
        public Action<SatchelPlayer> ApplyEffect { get; set; }
    }

    public class FlowerEffect
    {
        public int ItemType { get; set; }
        public string Name { get; set; }
        public Action<SatchelPlayer, int> ApplyEffect { get; set; }
    }

    public class ComboEffect
    {
        public string Name { get; set; }
        public short[] RequiredFlowers { get; set; }

        public Action<SatchelPlayer, Dictionary<int, int>, int> ApplyEffect { get; set; }

        /// <summary>
        /// Additional effects that can be applied depending on minimum flower count.
        /// </summary>
        public Dictionary<int, ComboEffect_Special> ExtraEffects { get; set; } = [];
    }

    public static readonly Dictionary<int, FlowerEffect> FlowerEffects = new()
    {
        [ItemID.Blinkroot] = new FlowerEffect
        {
            ItemType = ItemID.Blinkroot,
            Name = $"[i:{ItemID.Blinkroot}]",
            ApplyEffect = (player, count) =>
            {
                var pickBonus = count * 0.003f;
                player.pickSpeedBonus += pickBonus;
                player.individualFlowerEffects.Add($"[i:{ItemID.Blinkroot}]+{pickBonus:P1} mining speed");

                var damageBonus = count * 0.0005f;
                player.damageBonus += damageBonus;
                player.individualFlowerEffects.Add($"[i:{ItemID.Blinkroot}]+{damageBonus:P1} damage");
            },
        },

        [ItemID.Daybloom] = new FlowerEffect
        {
            ItemType = ItemID.Daybloom,
            Name = $"[i:{ItemID.Daybloom}]",
            ApplyEffect = (player, count) =>
            {
                var hpBonus = (int)(count * 0.5f);
                if (hpBonus > 0)
                {
                    player.lifeBonus += hpBonus;
                    player.individualFlowerEffects.Add($"[i:{ItemID.Daybloom}]+{hpBonus} maximum life");
                }

                var defBonus = count / 10;
                if (defBonus > 0)
                {
                    player.defenseBonus += defBonus;
                    player.individualFlowerEffects.Add($"[i:{ItemID.Daybloom}]+{defBonus} defense");
                }
            },
        },

        [ItemID.Fireblossom] = new FlowerEffect
        {
            ItemType = ItemID.Fireblossom,
            Name = $"[i:{ItemID.Fireblossom}]",
            ApplyEffect = (player, count) =>
            {
                var meleeBonus = count * 0.002f;
                player.meleeDamageBonus += meleeBonus;
                player.individualFlowerEffects.Add($"[i:{ItemID.Fireblossom}]+{meleeBonus:P1} melee damage");

                var critBonus = count / 10;
                if (critBonus > 0)
                {
                    player.critBonus += critBonus;
                    player.individualFlowerEffects.Add($"[i:{ItemID.Fireblossom}]+{critBonus}% critical strike chance");
                }
            },
        },

        [ItemID.Sunflower] = new FlowerEffect
        {
            ItemType = ItemID.Sunflower,
            Name = $"[i:{ItemID.Sunflower}]",
            ApplyEffect = (player, count) =>
            {
                var speedBonus = count * 0.003f;
                player.moveSpeedBonus += speedBonus;
                player.individualFlowerEffects.Add($"[i:{ItemID.Sunflower}]+{speedBonus:P1} movement speed");

                var buildBonus = count * 0.001f;
                player.buildSpeedBonus += buildBonus;
                player.individualFlowerEffects.Add($"[i:{ItemID.Sunflower}]+{buildBonus:P1} placement speed");
            }
        },

        [ItemID.Shiverthorn] = new FlowerEffect
        {
            ItemType = ItemID.Shiverthorn,
            Name = $"[i:{ItemID.Shiverthorn}]",
            ApplyEffect = (player, count) =>
            {
                var critBonus = count * 0.1f;
                player.critBonus += critBonus;
                player.individualFlowerEffects.Add($"[i:{ItemID.Shiverthorn}]+{critBonus:F1}% critical strike chance");

                var buildBonus = count * 0.001f;
                player.buildSpeedBonus += buildBonus;
                player.individualFlowerEffects.Add($"[i:{ItemID.Shiverthorn}]+{buildBonus:P1} placement speed");
            }
        },

        [ItemID.Moonglow] = new FlowerEffect
        {
            ItemType = ItemID.Moonglow,
            Name = $"[i:{ItemID.Moonglow}]",
            ApplyEffect = (player, count) =>
            {
           
                var manaBonus = (int)(count * 0.5f);
                if (manaBonus > 0)
                {
                    player.manaBonus += manaBonus;
                    player.individualFlowerEffects.Add($"[i:{ItemID.Moonglow}]+{manaBonus} maximum mana");
                }

                var manaRegenBonus = (int)(count * 0.25f);
                if (manaRegenBonus > 0)
                {
                    player.manaRegenBonus += manaRegenBonus;
                    player.individualFlowerEffects.Add($"[i:{ItemID.Moonglow}]+{manaRegenBonus} mana regeneration");
                }
            }
        },

        [ItemID.Waterleaf] = new FlowerEffect
        {
            ItemType = ItemID.Moonglow,
            Name = $"[i:{ItemID.Moonglow}]",
            ApplyEffect = (player, count) =>
            {
                var kbBonus = count * 0.001f;
                player.kbBonus += kbBonus;
                player.individualFlowerEffects.Add($"[i:{ItemID.Moonglow}]+{kbBonus:P1} knockback");

                var magicBonus = count * 0.001f;
                player.magicDamageBonus += magicBonus;
                player.individualFlowerEffects.Add($"[i:{ItemID.Moonglow}]+{magicBonus:P1} magic damage");
            }
        },

        [ItemID.Deathweed] = new FlowerEffect
        {
            ItemType = ItemID.Deathweed,
            Name = $"[i:{ItemID.Deathweed}]",
            ApplyEffect = (player, count) =>
            {
                var thornsBonus = count * 0.001f;
                player.thornsBonus += thornsBonus;
                player.individualFlowerEffects.Add($"[i:{ItemID.Deathweed}]+{thornsBonus:P1} thorn damage");

                var critBonus = count * 0.001f;
                player.critBonus += critBonus;
                player.individualFlowerEffects.Add($"[i:{ItemID.Deathweed}]+{critBonus:P1} critical strike chance");
            }
        },
    };

    public static readonly List<ComboEffect> ComboEffects =
    [
        new ComboEffect
        {
            Name = "Dawn's Grace Bouquet",
            RequiredFlowers = [ItemID.Daybloom, ItemID.Blinkroot, ItemID.Shiverthorn],
            ApplyEffect = (player, flowerCounts, minCount) =>
            {
                var shiverthorn = flowerCounts[ItemID.Shiverthorn];
                var daybloom = flowerCounts[ItemID.Daybloom];
                var blinkroot = flowerCounts[ItemID.Blinkroot];

                var enduranceBonus = shiverthorn * 0.002f;
                var lifeBonus = daybloom * 2;
                var miningBonus = blinkroot * 0.0075f;

                player.enduranceBonus += enduranceBonus;
                player.lifeBonus += lifeBonus;
                player.pickSpeedBonus += miningBonus;

                player.comboEffects.Add($"[i:{ItemID.Shiverthorn}]+{enduranceBonus:P1} damage reduction ({shiverthorn})");
                player.comboEffects.Add($"[i:{ItemID.Daybloom}]+{lifeBonus} maximum life ({daybloom})");
                player.comboEffects.Add($"[i:{ItemID.Blinkroot}]+{miningBonus:P1} mining speed ({blinkroot})");
            },
            ExtraEffects = new()
            {
                [30] = new ComboEffect_Special
                {
                    ApplyEffect = (player) =>
                    {
                        player.Combo_DawnsGrace = true;
                        player.comboEffects.Add($"[i:{ModContent.ItemType<FlowerSatchelItem>()}][c/a0fea3:Hunter & Trapsight effects when above 90% HP]");
                    }
                }
            }
        },

        new ComboEffect
        {
            Name = "Duskwilt Bouquet",
            RequiredFlowers = [ItemID.Moonglow, ItemID.Deathweed, (short)ModContent.ItemType<MagnoliaItem>()],
            ApplyEffect = (player, flowerCounts, minCount) =>
            {
                var moonglow = flowerCounts[ItemID.Moonglow];
                var deathweed = flowerCounts[ItemID.Deathweed];
                var magnolia = flowerCounts[(short)ModContent.ItemType<MagnoliaItem>()];

                var thornBonus = deathweed * 0.003f;
                var meleeDmgBonus = moonglow * 0.0031515f;
                var critBonus = magnolia * 0.005f;

                player.thornsBonus += thornBonus;
                player.meleeDamageBonus += meleeDmgBonus;
                player.critBonus += critBonus;

                player.comboEffects.Add($"[i:{ItemID.Deathweed}]+{thornBonus:P1} thorn damage ({deathweed})");
                player.comboEffects.Add($"[i:{ItemID.Moonglow}]+{meleeDmgBonus:P1} melee damage ({moonglow})");
                player.comboEffects.Add($"[i:{ModContent.ItemType<MagnoliaItem>()}]+{critBonus:P1} critical strike chance ({magnolia})");
            },
            ExtraEffects = new()
            {
                [30] = new ComboEffect_Special
                {
                    ApplyEffect = (player) =>
                    {
                        player.comboEffects.Add($"[i:{ModContent.ItemType<FlowerSatchelItem>()}][c/a0fea3:Enemies have a chance to plant a sapling on death (not done)]");
                    }
                }
            }
        },

        new ComboEffect
        {
            Name = "Emberfrost Arrangement",
            RequiredFlowers = [ItemID.Fireblossom, ItemID.Shiverthorn],
            ApplyEffect = (player, flowerCounts, minCount) =>
            {
                var fireblossom = flowerCounts[ItemID.Fireblossom];
                var shiverthorn = flowerCounts[ItemID.Shiverthorn];

                var critChance = shiverthorn * 0.35f;
                var meleeDamage = fireblossom * 0.004f;
                var kbBoost = fireblossom * 0.003f;

                player.critBonus += critChance;
                player.meleeDamageBonus += meleeDamage;
                player.kbBonus += kbBoost;

                player.comboEffects.Add($"[i:{ItemID.Shiverthorn}]+{critChance:F1}% critical strike chance ({shiverthorn})");
                player.comboEffects.Add($"[i:{ItemID.Fireblossom}]+{meleeDamage:P1} melee damage ({fireblossom})");
                player.comboEffects.Add($"[i:{ItemID.Fireblossom}]+{kbBoost:P0} knockback ({fireblossom})");
            }
        },

        new ComboEffect
        {
            Name = "Sunrise Spray",
            RequiredFlowers = [ItemID.Fireblossom, ItemID.Daybloom, ItemID.Sunflower],
            ApplyEffect = (player, flowerCounts, minCount) =>
            {
                var fireblossom = flowerCounts[ItemID.Fireblossom];
                var daybloom = flowerCounts[ItemID.Daybloom];
                var sunflower = flowerCounts[ItemID.Sunflower];

                var lifeRegen = fireblossom / 3;
                var defenseBonus = daybloom / 4;
                var moveSpeed = sunflower * 0.003f;

                player.lifeRegenBonus += lifeRegen;
                player.defenseBonus += defenseBonus;
                player.moveSpeedBonus += moveSpeed;

                player.comboEffects.Add($"[i:{ItemID.Fireblossom}]+{lifeRegen} life regeneration ({fireblossom})");
                player.comboEffects.Add($"[i:{ItemID.Daybloom}]+{defenseBonus} defense ({daybloom})");
                player.comboEffects.Add($"[i:{ItemID.Sunflower}]+{moveSpeed:P0} movement speed ({sunflower})");
            },
            ExtraEffects = new()
            {
                [30] = new ComboEffect_Special
                {
                    ApplyEffect = (player) =>
                    {
                        player.Combo_SunriseSpray = true;
                        player.comboEffects.Add($"[i:{ModContent.ItemType<FlowerSatchelItem>()}][c/a0fea3:+4% speed during day]");
                    }
                },
            }
        },

        new ComboEffect
        {
            Name = "Explorer's Corsage",
            RequiredFlowers = [ItemID.Blinkroot, ItemID.Moonglow, ItemID.Shiverthorn],
            ApplyEffect = (player, flowerCounts, minCount) =>
            {
                var blinkroot = flowerCounts[ItemID.Blinkroot];
                var moonglow = flowerCounts[ItemID.Moonglow];
                var shiverthorn = flowerCounts[ItemID.Shiverthorn];

                var buildSpeed = moonglow * 0.005f;
                var critChance = shiverthorn * 0.19f;
                var miningSpeed = blinkroot * 0.005f;

                player.buildSpeedBonus += buildSpeed;
                player.critBonus += critChance;
                player.pickSpeedBonus += miningSpeed;

                player.comboEffects.Add($"[i:{ItemID.Moonglow}]+{buildSpeed:P1} placement speed ({moonglow})");
                player.comboEffects.Add($"[i:{ItemID.Shiverthorn}]+{critChance:F1}% critical strike chance ({shiverthorn})");
                player.comboEffects.Add($"[i:{ItemID.Blinkroot}]+{miningSpeed:P0} mining speed ({blinkroot})");
            },
            ExtraEffects = new()
            {
                [15] = new ComboEffect_Special
                {
                    ApplyEffect = (player) =>
                    {
                        player.Player.nightVision = true;
                        player.comboEffects.Add($"[i:{ModContent.ItemType<FlowerSatchelItem>()}][c/a0fea3:Gain Night Owl Vision]");
                    }
                },
                [30] = new ComboEffect_Special
                {
                    ApplyEffect = (player) =>
                    {
                        player.Combo_ExplorersCorsage = true;
                        player.comboEffects.Add($"[i:{ModContent.ItemType<FlowerSatchelItem>()}][c/a0fea3:Gain Dangersense when hit]");
                    }
                },
            }
        }
    ];
}