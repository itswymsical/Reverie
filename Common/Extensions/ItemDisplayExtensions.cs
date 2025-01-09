using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;
using System;

namespace Reverie.Common.Extensions
{
    public static class ItemDisplayExtensions
    {
        public static void DrawItemBehindPlayer(this Player player, ref PlayerDrawSet drawInfo)
        {
            if (player.HeldItem == null || player.HeldItem.IsAir) return;

            Item item = player.HeldItem;
            Main.instance.LoadItem(item.type);
            Texture2D itemTexture = ModContent.Request<Texture2D>(item.ModItem?.Texture ?? $"Terraria/Images/Item_{item.type}").Value;

            Vector2 itemPosition = player.Center - Main.screenPosition;

            itemPosition.X -= itemTexture.Width / 16;
            itemPosition.Y -= itemTexture.Height / 16;

            float tilt = player.velocity.X * 0.125f;
            float currentTime = Main.GameUpdateCount / 60f;
            float animationTime = currentTime - Main.GameUpdateCount / 60f;
            
            float dir = MathHelper.ToRadians(182) * player.direction;

            SpriteEffects flip;
            if (player.direction == -1)
            {
                flip = SpriteEffects.FlipHorizontally;
            }
            else
            {
                flip = SpriteEffects.None;
            }
            DrawData itemDrawData = new(
                itemTexture,
                itemPosition,
                null,
                item.GetAlpha(Color.White),
                player.bodyRotation + dir,
                new Vector2(itemTexture.Width / 2, itemTexture.Height / 2),
                1.07f,
                flip,
                0
            );
            drawInfo.DrawDataCache.Insert(0, itemDrawData);
        }
    }
}
