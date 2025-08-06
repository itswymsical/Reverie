using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Reverie.Common.Players;
using Reverie.Core.Loaders;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace Reverie.Content.Items.Lodestone;

[AutoloadEquip(EquipType.Legs)]
public class LodestoneLeggings : ModItem
{
    public override void SetDefaults()
    {
        Item.defense = 4;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 2);
        Item.width = Item.height = 34;
    }

    public override void UpdateEquip(Player player)
    {
        player.runAcceleration *= 1.1f;
    }

    public override void AddRecipes()
    {
        var recipe = CreateRecipe();
        recipe.AddIngredient(ModContent.ItemType<LodestoneItem>(), 10);
        recipe.AddIngredient(ModContent.ItemType<MagnetizedCoil>(), 2);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}

[AutoloadEquip(EquipType.Body)]
public class LodestoneChestplate : ModItem
{
    public override void SetDefaults()
    {
        Item.defense = 5;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 2);
        Item.width = Item.height = 34;
    }
    public override void UpdateEquip(Player player)
    {
        player.endurance += 0.02f;
    }
    public override void AddRecipes()
    {
        CreateRecipe()
       .AddIngredient(ModContent.ItemType<LodestoneItem>(), 12)
       .AddIngredient(ModContent.ItemType<MagnetizedCoil>(), 1)
       .AddIngredient(ItemID.Silk, 6)
       .AddTile(TileID.Anvils)
       .Register();
    }
}

[AutoloadEquip(EquipType.Head)]
public class LodestoneHelmet : ModItem
{
    public override void Load()
    {
        On_Main.DrawInfernoRings += DrawLodestoneField;
    }

    public override void Unload()
    {
        On_Main.DrawInfernoRings -= DrawLodestoneField;
    }

    private void DrawLodestoneField(On_Main.orig_DrawInfernoRings orig, Main self)
    {
        bool effectFound = false;
        for (int i = 0; i < 255; i++)
        {
            if (!Main.player[i].active || Main.player[i].outOfRange || Main.player[i].dead)
                continue;
            Player player = Main.player[i];
            if (!HasLodestoneSet(player))
                continue;
            if (!effectFound)
            {
                try
                {
                    var fieldEffect = ShaderLoader.GetShader("MagnetShader").Value;
                    if (fieldEffect == null)
                    {
                        continue;
                    }

                    Vector2 screenPos = player.Center - Main.screenPosition;
                    float fieldRadius = 800f;

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
                        SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                        fieldEffect, Main.GameViewMatrix.TransformationMatrix);

                    // Set parameters
                    fieldEffect.Parameters["uPlayerPosition"]?.SetValue(screenPos);
                    fieldEffect.Parameters["uScreenResolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
                    fieldEffect.Parameters["uSourceRect"]?.SetValue(new Vector4(0, 0, Main.screenWidth, Main.screenHeight));
                    fieldEffect.Parameters["uTime"]?.SetValue(Main.GameUpdateCount * 0.03f);
                    fieldEffect.Parameters["uFieldRadius"]?.SetValue(fieldRadius);
                    fieldEffect.Parameters["uRippleFrequency"]?.SetValue(4f);
                    fieldEffect.Parameters["uDistortionStrength"]?.SetValue(16f);
                    fieldEffect.Parameters["uFalloffPower"]?.SetValue(2f);

                    fieldEffect.Parameters["uPulseSpeed"]?.SetValue(1.5f);
                    fieldEffect.Parameters["uPulseIntensity"]?.SetValue(0.4f);
                    fieldEffect.Parameters["uBaseOpacity"]?.SetValue(0f);

                    effectFound = true;
                }
                catch (Exception ex)
                {
                    Main.NewText($"Shader error: {ex.Message}", Color.Red);
                    continue;
                }
            }

            Vector2 playerScreenPos = player.Center - Main.screenPosition;
            float effectSize = 200;

            Rectangle effectRect = new Rectangle(
                (int)(playerScreenPos.X - effectSize / 2),
                (int)(playerScreenPos.Y - effectSize / 2),
                (int)effectSize,
                (int)effectSize
            );

            var color = Lighting.GetColor(effectRect.X, effectRect.Y);

            Rectangle screenBounds = new Rectangle(-50, -50, Main.screenWidth + 100, Main.screenHeight + 100);
            if (effectRect.Intersects(screenBounds))
            {
                Main.spriteBatch.Draw(
                   ModContent.Request<Texture2D>($"{VFX_DIRECTORY}InverseMask").Value,
                    effectRect,
                    null,
                    color,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    1f
                );
            }
        }

        if (effectFound)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer,
                null, Main.Transform);
        }
        orig(self);
    }

    private bool HasLodestoneSet(Player player)
    {
        return player.armor[0].type == ModContent.ItemType<LodestoneHelmet>() &&
               player.armor[1].type == ModContent.ItemType<LodestoneChestplate>() &&
               player.armor[2].type == ModContent.ItemType<LodestoneLeggings>();
    }

    public override void SetStaticDefaults()
    {
        ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
    }

    public override void SetDefaults()
    {
        Item.defense = 3;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(silver: 2);
        Item.width = Item.height = 34;
    }

    public override void UpdateEquip(Player player)
    {
        player.pickSpeed += 0.1f;
    }

    public override bool IsArmorSet(Item head, Item body, Item legs)
    {
        return body.type == ModContent.ItemType<LodestoneChestplate>() &&
               legs.type == ModContent.ItemType<LodestoneLeggings>();
    }

    public override void UpdateArmorSet(Player player)
    {
        player.setBonus = "You emit a magnetic field that gravitates items towards you" +
            "\nIncreases pickup range substantially";

        for (var i = 0; i < Main.maxItems; i++)
        {
            var targetItem = Main.item[i];
            if (targetItem.active && !targetItem.beingGrabbed && targetItem.noGrabDelay == 0)
            {
                var distance = Vector2.Distance(player.Center, targetItem.Center);
                if (distance <= 100f)
                {
                    var movement = Vector2.Normalize(player.Center - targetItem.Center);
                    var speedFactor = 1f - distance / 100f;
                    var speed = 3f * speedFactor;
                    targetItem.velocity = movement * speed;
                }
            }
        }
    }

    public override void AddRecipes()
    {
        CreateRecipe()
        .AddIngredient(ModContent.ItemType<LodestoneItem>(), 8)
        .AddIngredient(ModContent.ItemType<MagnetizedCoil>(), 2)
        .AddIngredient(ItemID.CopperBar, 2)
        .AddTile(TileID.Anvils)
        .Register();
    }
}