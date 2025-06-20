﻿using Reverie.Common.Subworlds.Archaea;
using Reverie.Common.UI.Missions;

using Reverie.Content.Dusts;
using Reverie.Content.Tiles.Archaea;

using Reverie.Core.Cinematics;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Core.Missions.Core;
using SubworldLibrary;

using System.Collections.Generic;
using System.Linq;
using Terraria.UI;
using Terraria.WorldBuilding;

namespace Reverie.Common.Players;

public class ReveriePlayer : ModPlayer
{
    private bool inventoryInit = false;
    private Mission currentMission;
    public bool magnetizedFall;
    public bool lodestoneKB;

    public override void PostUpdate()
    {
        if (Main.LocalPlayer.ZoneDesert || SubworldSystem.IsActive<ArchaeaSub>())
            DrawSandHaze();

        if (!Cutscene.IsPlayerVisible)
            Player.AddBuff(BuffID.Invisibility, 1, true);

        if (Cutscene.NoFallDamage)
            Player.noFallDmg = true;

        if (Main.playerInventory && !inventoryInit)
        {
            currentMission = Main.LocalPlayer.GetModPlayer<MissionPlayer>().ActiveMissions().FirstOrDefault();

            InGameNotificationsTracker.AddNotification(new MissionNotification(currentMission));
            inventoryInit = true;
        }

        DialogueManager.Instance.UpdateActive();
    }

    public override void SetControls()
    {
        if (Cutscene.DisableInputs)
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
            Player.controlMap = false;
            Player.controlMount = false;
        }
    }

    public override void ResetEffects()
    {
        magnetizedFall = false;
        lodestoneKB = false;
    }
    public override void ModifyDrawLayerOrdering(IDictionary<PlayerDrawLayer, PlayerDrawLayer.Position> positions)
    {
        base.ModifyDrawLayerOrdering(positions);

        if (Main.gameMenu) return;

        //if (positions.ContainsKey(ModContent.GetInstance<AuraLayer>()))
        //{
        //    positions[ModContent.GetInstance<AuraLayer>()] =
        //        new PlayerDrawLayer.AfterParent(PlayerDrawLayers.BackAcc);
        //}
    }

    //public override void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath)
    //{
    //    itemsByMod["Terraria"].RemoveAll(item => item.type == ItemID.CopperShortsword);
    //    itemsByMod["Terraria"].RemoveAll(item => item.type == ItemID.CopperPickaxe);
    //    itemsByMod["Terraria"].RemoveAll(item => item.type == ItemID.CopperAxe);
    //}

    private const int SAND_HAZE_RANGE_X = 55;
    private const int SAND_HAZE_RANGE_Y = 30;
    private const int DUST_SPAWN_CHANCE = 95;
    private const int DUST_SIZE = 5;
    private const float WIND_VELOCITY_FACTOR = 5.6f;
    private const float SANDSTORM_UPWARD_VELOCITY = 0.07f;

    private bool IsSandTile(Tile tile) => tile.HasTile && (
        tile.TileType == TileID.Sand ||
        tile.TileType == ModContent.TileType<PrimordialSandTile>()
    );

    private bool HasAirAbove(int x, int y) =>
        !Main.tile[x, y - 1].HasTile &&
        !Main.tile[x, y - 2].HasTile;

    private void DrawSandHaze()
    {
        int startX = (int)(Player.position.X / 16) - SAND_HAZE_RANGE_X;
        int endX = (int)(Player.position.X / 16) + SAND_HAZE_RANGE_X;
        int startY = (int)(Player.position.Y / 16) - SAND_HAZE_RANGE_Y;
        int endY = (int)(Player.position.Y / 16) + SAND_HAZE_RANGE_Y;

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                if (x < 0 || x >= Main.maxTilesX || y < 2 || y >= Main.maxTilesY)
                    continue;

                if (IsSandTile(Main.tile[x, y]) && HasAirAbove(x, y) && Main.rand.NextBool(DUST_SPAWN_CHANCE))
                {
                    SpawnSandDust(x, y);
                }
            }
        }
    }

    private void SpawnSandDust(int tileX, int tileY)
    {
        Vector2 dustPosition = new((tileX - 2) * 16, (tileY - 1) * 16);
        int dustIndex = Dust.NewDust(dustPosition, DUST_SIZE, DUST_SIZE, ModContent.DustType<SandHazeDust>());
        
        Main.dust[dustIndex].velocity.X -= Main.windSpeedCurrent;
        //Main.dust[dustIndex].velocity.Y -= Main.windSpeedCurrent;
        if (Player.ZoneSandstorm)
        {
            Main.dust[dustIndex].velocity.Y += SANDSTORM_UPWARD_VELOCITY;
        }
    }
}