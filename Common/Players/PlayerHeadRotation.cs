// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.
//https://github.com/Mirsario/TerrariaOverhaul/blob/dev/Common/EntityEffects/PlayerHeadRotation.cs

using Reverie.Core.Dialogue;
using Terraria.DataStructures;

namespace Reverie.Common.Players;

/// <summary>
/// Adapted from Terraria Overhaul, by Mirsario. Rotates the <see cref="Player"/> head by (<see cref="Main.MouseWorld"/> - <see cref="Player.Center"/>).
/// </summary>
public class PlayerHeadRotation : ModPlayer
{
    private static bool active;
    private float headRotation;
    private float targetHeadRotation;

    public override void Load()
    {
        // Hook into vanilla draw calls to ensure compatibility
        On_Main.DrawPlayers_AfterProjectiles += static (orig, main) =>
        {
            active = true;
            orig(main);
            active = false;
        };

        On_Main.DrawPlayers_BehindNPCs += static (orig, main) =>
        {
            active = true;
            orig(main);
            active = false;
        };
    }

    public override void PreUpdate()
    {
        const float LOOK_STRENGTH = 0.55f;

        if (Player.sleeping.isSleeping || DialogueManager.Instance.IsAnyActive() || Player.pulley)
        {
            targetHeadRotation = 0f;
        }
        else
        {
            var lookPosition = Main.MouseWorld;
            var offset = lookPosition - Player.Center;

            if (Math.Sign(offset.X) == Player.direction)
            {
                targetHeadRotation = (offset * Player.direction).ToRotation() * LOOK_STRENGTH;
            }
            else
            {
                targetHeadRotation = 0f;
            }

            //todo: make changes to vanilla direction changing.
            if (Player.velocity == Vector2.Zero)
            {
                if (lookPosition.X > Player.Center.X)
                    Player.direction = 1;
                else
                    Player.direction = -1;
            }
        }

        var lerpSpeed = 0.15f;
        headRotation = MathHelper.Lerp(headRotation, targetHeadRotation, lerpSpeed);
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        if (!Main.gameMenu && active)
        {
            drawInfo.drawPlayer.headRotation = headRotation;
        }
    }
}