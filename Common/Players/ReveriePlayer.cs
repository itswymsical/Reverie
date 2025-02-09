using Reverie.Content.Dusts;
using Reverie.Core.Cinematics;
using System.Collections.Generic;

namespace Reverie.Common.Players;

public class ReveriePlayer : ModPlayer
{
    public override void PostUpdate()
    {
        if (Main.LocalPlayer.ZoneDesert)
        {
            DrawSandHaze();
        }
        if (!Cutscene.IsPlayerVisible)
        {
            Player.AddBuff(BuffID.Invisibility, 1, true);
        }
        if (Cutscene.NoFallDamage)
        {
            Player.noFallDmg = true;
        }
    }

    public override void SetControls()
    {
        if (Cutscene.DisableMoment)
        {
            Player.controlLeft = false;
            Player.controlRight = false;
            Player.controlUp = false;
            Player.controlDown = false;
            Player.controlJump = false;
            Player.controlHook = false;
            Player.controlInv = false;
            Player.controlUseItem = false;
            Player.controlUseTile = false;
        }
    }
    public override void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath)
    {
        itemsByMod["Terraria"].RemoveAll(item => item.type == ItemID.CopperShortsword);
        itemsByMod["Terraria"].RemoveAll(item => item.type == ItemID.CopperPickaxe);
        itemsByMod["Terraria"].RemoveAll(item => item.type == ItemID.CopperAxe);
    }

    private void DrawSandHaze()
    {
        for (int k = (int)Math.Floor(Player.position.X / 16) - 55; k < (int)Math.Floor(Player.position.X / 16) + 55; k++)
        {
            for (int i = (int)Math.Floor(Player.position.Y / 16) - 30; i < (int)Math.Floor(Player.position.Y / 16) + 30; i++)
            {
                if (!Main.tile[k, i - 1].HasTile
                    && !Main.tile[k, i - 2].HasTile
                    && Main.tile[k, i].HasTile
                    && Main.tile[k, i].TileType == TileID.Sand
                    && Main.tile[k, i].TileType == TileID.Sand)
                {
                    if (Main.rand.Next(0, 95) == 2)
                    {
                        int Index = Dust.NewDust(new Vector2((k - 2) * 16, (i - 1) * 16), 5, 5, ModContent.DustType<SandHazeDust>());

                        Main.dust[Index].velocity.X -= Main.windSpeedCurrent / 5.6f;

                        if (Player.ZoneSandstorm)
                            Main.dust[Index].velocity.Y += 0.07f;                        
                    }
                }
            }
        }
    }
}