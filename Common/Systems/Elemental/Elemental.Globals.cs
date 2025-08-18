using Reverie.Common.Players;
using Terraria.ModLoader.IO;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Reverie.Core.Graphics.Interfaces;
using Reverie.Core.Graphics;
using Reverie.Core.Loaders;
using Reverie.Utilities;
using System.Linq;
using Terraria;
using Microsoft.Xna.Framework.Graphics;

namespace Reverie.Common.Systems.Elemental;

public class ElementalGlobalItem : GlobalItem
{
    public override bool InstancePerEntity => true;

    public ElementalData elementalData = new(ElementType.None, 0);

    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        if (entity.damage <= 0)
            return false;

        if (entity.DamageType == DamageClass.Melee ||
            entity.DamageType == DamageClass.Ranged ||
            entity.DamageType == DamageClass.Magic ||
            entity.DamageType == DamageClass.Summon || entity.DamageType == DamageClass.SummonMeleeSpeed
            || entity.DamageType == DamageClass.MeleeNoSpeed || entity.DamageType == DamageClass.MagicSummonHybrid || entity.DamageType == DamageClass.Default)
            return true;
        return false;
    }

    public override void SaveData(Item item, TagCompound tag)
    {
        if (elementalData.element != ElementType.None)
        {
            tag["elementalData"] = elementalData.Save();
        }
    }

    public override void LoadData(Item item, TagCompound tag)
    {
        if (tag.ContainsKey("elementalData"))
        {
            elementalData = ElementalData.Load(tag.GetCompound("elementalData"));
        }
    }

    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
    {
        if (elementalData.element != ElementType.None)
        {
            var bonus = elementalData.GetDamageBonus();
            damage += bonus;
        }
    }

    //public override bool? UseItem(Item item, Player player)
    //{
    //    return base.UseItem(item, player);
    //}

    //public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    //{
    //    return base.PreDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    //}

    public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
    }

    public override void OnHitNPC(Item item, Player player, NPC target, NPC.HitInfo hit, int damageDone)
    {
        base.OnHitNPC(item, player, target, hit, damageDone);

        if (elementalData.element == ElementType.Fire && elementalData.boundExp > 0)
        {
            int burnDuration = elementalData.GetEffectStrength() * 60;
            target.AddBuff(BuffID.OnFire, burnDuration);
        }
    }
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (elementalData.element != ElementType.None && elementalData.boundExp > 0)
        {
            var elementName = elementalData.element.ToString();
            var color = GetElementColor(elementalData.element);
            var damageBonus = elementalData.GetDamageBonus();

            // Add binding info tooltip
            var bindingTooltip = $"{elementalData.boundExp} experience bound";
            tooltips.Add(new TooltipLine(Mod, "ElementalBinding", bindingTooltip)
            {
                OverrideColor = color
            });

            foreach (TooltipLine line in tooltips)
            {
                if (line.Mod == "Terraria" && line.Name == "Damage")
                {
                    var colorHex = $"{color.R:X2}{color.G:X2}{color.B:X2}";
                    var bonusDamage = (int)(item.damage * damageBonus);
                    line.Text += $" ([c/{colorHex}:+{bonusDamage}])";
                }
            }
        }
        else
        {
            int minRequired = GetMinRequirement(item);
            if (minRequired > 0)
            {
                var tooltip = $"Requires {minRequired} experience to bind Reverie";
                tooltips.Add(new TooltipLine(Mod, "ElementalBindingAvailable", tooltip)
                {
                    OverrideColor = Color.LightGray
                });
            }
        }
    }

    public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
    {
        if ((line.Name == "ItemName") && line.Mod == "Terraria" && elementalData.element != ElementType.None)
        {
            Vector2 textSize = line.Font.MeasureString(line.Text);
            Vector2 iconPosition = new Vector2(line.X + textSize.X + 6, line.Y + 2);

            if (elementalData.element == ElementType.Fire)
            {
                Texture2D iconTexture = ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Elements/Incendiary").Value;
                Rectangle sourceRect = new Rectangle(0, 0, 36, 36);
                Rectangle destRect = new Rectangle((int)iconPosition.X, (int)iconPosition.Y - 6, 30, 30);

                Main.spriteBatch.Draw(iconTexture, destRect, sourceRect, Color.White);
            }
        }
        return base.PreDrawTooltipLine(item, line, ref yOffset);
    }

    public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (elementalData.element != ElementType.None)
        {
            if (elementalData.element == ElementType.Fire)
            {
                Texture2D iconTexture = ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Elements/Incendiary").Value;
                Rectangle sourceRect = new Rectangle(0, 0, 36, 36);
                Rectangle destRect = new Rectangle((int)position.X + 8, (int)position.Y + 8, 20, 20);

                Main.spriteBatch.Draw(iconTexture, destRect, sourceRect, Color.White);
            }
        }
        base.PostDrawInInventory(item, spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }

    public bool TryBindExp(Player player, ElementType element, int amount)
    {
        var expPlayer = player.GetModPlayer<ExperiencePlayer>();
        if (!expPlayer.CanAfford(amount))
            return false;
        if (elementalData.element != ElementType.None && elementalData.element != element)
            return false;

        // This method should be called with the specific item being bound
        // The UI should pass the item from the slot, not rely on held item
        return false; // This method needs the item parameter
    }

    public bool TryBindExp(Player player, ElementType element, int amount, Item targetItem)
    {
        var expPlayer = player.GetModPlayer<ExperiencePlayer>();
        if (!expPlayer.CanAfford(amount))
            return false;
        if (elementalData.element != ElementType.None && elementalData.element != element)
            return false;

        int minRequired = GetMinRequirement(targetItem);
        if (amount < minRequired)
            return false;

        expPlayer.TrySpendExp(amount);
        if (elementalData.element == ElementType.None)
            elementalData.element = element;
        elementalData.boundExp += amount;
        return true;
    }

    private int GetMinRequirement(Item item)
    {
        int baseRequirement = item.rare switch
        {
            -1 => 5,
            0 => 10,
            1 => 15,
            2 => 25,
            3 => 45,
            4 => 70,
            5 => 100,
            6 => 140,
            7 => 60,
            8 => 90,
            9 => 180,
            10 => 300,
            11 => 450,
            _ => 600
        };
        float damageScale = 1f + (float)Math.Log(Math.Max(1, item.damage)) * 0.15f;
        float speedScale = item.useTime switch
        {
            <= 6 => 2.5f,
            <= 10 => 2.0f,
            <= 15 => 1.5f,
            <= 20 => 1.2f,
            <= 25 => 1.0f,
            <= 30 => 0.9f,
            <= 40 => 0.8f,
            _ => 0.7f
        };
        int requirement = (int)(baseRequirement * damageScale * speedScale);
        return Math.Max(5, Math.Min(requirement, 1500));
    }

    public int GetMinimumBinding(Item item) => GetMinRequirement(item);

    public void UnbindExperience(Player player)
    {
        if (elementalData.boundExp > 0)
        {
            ExperiencePlayer.AddExperience(player, elementalData.boundExp);
            elementalData = new(ElementType.None, 0);
        }
    }

    private static Color GetElementColor(ElementType element)
    {
        return element switch
        {
            ElementType.Fire => Color.Orange,
            _ => Color.White
        };
    }
}

public class ElementalGlobalProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    public ElementalData elementalData = new(ElementType.None, 0);

    private List<Vector2> trailCache;
    private const int TRAIL_LENGTH = 10;

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (source is EntitySource_ItemUse itemSource)
        {
            if (itemSource.Item.TryGetGlobalItem<ElementalGlobalItem>(out var globalItem))
            {
                if (globalItem.elementalData.element != ElementType.None)
                {
                    elementalData = globalItem.elementalData;
                }
            }
        }
    }

    public override void AI(Projectile projectile)
    {
        base.AI(projectile);

        if (elementalData.element != ElementType.None)
        {
            UpdateTrailCache(projectile);
        }

        if (elementalData.element == ElementType.Fire && elementalData.boundExp > 0)
        {
            if (Main.rand.NextBool(3))
            {
                var dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height,
                    DustID.Torch, 0, 0, 100, default, 1.2f);
                dust.velocity *= 0.3f;
                dust.noGravity = true;
            }

            Lighting.AddLight(projectile.Center, 0.8f, 0.4f, 0.1f);
        }
    }

    private void UpdateTrailCache(Projectile projectile)
    {
        trailCache ??= new List<Vector2>();

        trailCache.Add(projectile.Center);

        while (trailCache.Count > TRAIL_LENGTH)
        {
            trailCache.RemoveAt(0);
        }
    }

    public override void PostDraw(Projectile projectile, Color lightColor)
    {
        if (elementalData.element == ElementType.Fire && elementalData.boundExp > 0)
        {
            DrawElementalOverlay(projectile);
        }
    }

    private void DrawElementalOverlay(Projectile projectile)
    {
        Main.instance.LoadProjectile(projectile.type);
        var projectileTexture = TextureAssets.Projectile[projectile.type].Value;
        var bloomTexture = ModContent.Request<Texture2D>($"{VFX_DIRECTORY}Bloom").Value;

        // Calculate scale to match projectile sprite size to bloom texture
        float scaleX = (float)projectileTexture.Width / bloomTexture.Width;
        float scaleY = (float)projectileTexture.Height / bloomTexture.Height;
        var spriteScale = new Vector2(scaleX, scaleY) * projectile.scale;

        var bloomOrigin = bloomTexture.Size() * 0.5f;
        var glowColor = GetElementGlowColor(elementalData.element);
        var bloomColor = GetElementBloomColor(elementalData.element);

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

        if (trailCache != null && trailCache.Count > 1)
        {
            for (int i = 0; i < trailCache.Count; i++)
            {
                float trailProgress = (float)i / (trailCache.Count - 1);
                float intensity = 0.2f + (trailProgress * 0.8f);

                // Apply sprite-aligned scaling to trail
                var trailScale = spriteScale * (0.7f + (trailProgress * 0.3f));

                var trailPos = trailCache[i] - Main.screenPosition;
                Main.EntitySpriteDraw(bloomTexture, trailPos, null, glowColor * intensity, projectile.rotation,
                    bloomOrigin, trailScale, SpriteEffects.None, 0);
            }
        }

        var sunburstShader = ShaderLoader.GetShader("SunburstShader");
        if (sunburstShader?.Value != null)
        {
            var shader = sunburstShader.Value;

            float projectileSize = Math.Max(projectile.width, projectile.height);
            float baseScale = projectileSize / 32f;
            float sunburstScale = Math.Max(baseScale * 2.2f, 1.0f);
            int sunburstSize = (int)(projectileSize * 2.5f);
            int halfSize = sunburstSize / 2;

            shader.Parameters["uTime"]?.SetValue(Main.GameUpdateCount * 0.02f);
            shader.Parameters["uIntensity"]?.SetValue(0.95f);
            shader.Parameters["uColor"]?.SetValue(bloomColor.ToVector3());
            shader.Parameters["uCenter"]?.SetValue(Vector2.Zero);
            shader.Parameters["uScale"]?.SetValue(sunburstScale);
            shader.Parameters["uRayCount"]?.SetValue(12f);
            shader.CurrentTechnique.Passes[0].Apply();

            var sunburstDrawPos = projectile.Center - Main.screenPosition;
            var sunburstRect = new Rectangle((int)(sunburstDrawPos.X - halfSize), (int)(sunburstDrawPos.Y - halfSize), sunburstSize, sunburstSize);
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, sunburstRect, bloomColor * 0.6f);
        }

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
    }

    private static Color GetElementGlowColor(ElementType element)
    {
        return element switch
        {
            ElementType.Fire => new Color(255, 120, 60, 100),
            _ => new Color(255, 255, 255, 100)
        };
    }

    private static Color GetElementBloomColor(ElementType element)
    {
        return element switch
        {
            ElementType.Fire => new Color(255, 80, 20),
            _ => Color.White
        };
    }

    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (elementalData.element != ElementType.None)
        {
            float damageBonus = elementalData.GetDamageBonus();
            modifiers.SourceDamage += damageBonus;
        }
    }

    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (elementalData.element == ElementType.Fire && elementalData.boundExp > 0)
        {
            int burnDuration = elementalData.GetEffectStrength() * 60;
            target.AddBuff(BuffID.OnFire, burnDuration);

            for (int i = 0; i < 16; i++)
            {
                var dust = Dust.NewDustDirect(target.position, target.width, target.height,
                    DustID.Torch, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3),
                    100, default, 1.5f);
                dust.noGravity = true;
            }
        }
    }
}