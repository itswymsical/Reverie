using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Reverie.Common;
using Reverie.Content.Terraria.Tiles;

namespace Reverie.Content.Terraria.Items.Lodestone
{
    public class Lodestone : ModItem
    {
        public override string Texture => Assets.Terraria.Items.Lodestone + Name;

        private const float BaseMagnetRange = 95f;
        private const float BaseMaxSpeed = 1.7f;

        private const float MaxMagnetRange = 380f;
        private const float MaxSpeed = 6f;

        private const float ExponentialBase = 1.09f;

        public override void SetDefaults()
        {
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(silver: 10);
            Item.width = Item.height = 28;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<LodestoneTile>();
            Item.useTime = Item.useAnimation = 24;
            Item.useTurn = true;
            Item.maxStack = 999;
            Item.holdStyle = ItemHoldStyleID.HoldRadio;
        }

        public override void HoldItem(Player player)
        {
            base.HoldItem(player);
            MagnetizeItems_Held(player);
        }

        public override void Update(ref float gravity, ref float maxFallSpeed)
        {
            if (!Item.IsAir && Item.active)
            {
                MagnetizeItems_World();
            }
            base.Update(ref gravity, ref maxFallSpeed);
        }

        private void MagnetizeItems_Held(Player player)
        {
            (float range, float maxSpeed) = CalculateEnhancedValues();
            MagnetizeItems(player.Center, range, maxSpeed);
        }

        private void MagnetizeItems_World()
        {
            (float range, float maxSpeed) = CalculateEnhancedValues();
            MagnetizeItems(Item.Center, range, maxSpeed);
        }

        private void MagnetizeItems(Vector2 center, float range, float maxSpeed)
        {
            for (int i = 0; i < Main.maxItems; i++)
            {
                Item targetItem = Main.item[i];
                if (targetItem.active && !targetItem.beingGrabbed && targetItem.noGrabDelay == 0 &&
                    targetItem != Item && ReverieHooks.IsAMetalItem(targetItem))
                {
                    float distance = Vector2.Distance(center, targetItem.Center);
                    if (distance <= range)
                    {
                        Vector2 movement = Vector2.Normalize(center - targetItem.Center);
                        float speedFactor = 1f - (distance / range); // Items closer to the center move faster
                        float speed = maxSpeed * speedFactor;
                        targetItem.velocity = movement * speed;
                    }
                }
            }
        }

        private (float range, float maxSpeed) CalculateEnhancedValues()
        {
            float stackMultiplier = Math.Min(Item.stack, 500) - 1;

            float enhancedRange = Math.Min(BaseMagnetRange * (float)Math.Pow(ExponentialBase, stackMultiplier), MaxMagnetRange);
            float enhancedMaxSpeed = Math.Min(BaseMaxSpeed * (float)Math.Pow(ExponentialBase, stackMultiplier), MaxSpeed);

            return (enhancedRange, enhancedMaxSpeed);
        }
    }
}