using Reverie.Common.Subworlds.Archaea;
using Reverie.Common.Systems;
using Reverie.Common.UI.Missions;
using Reverie.Content.Cutscenes;
using Reverie.Content.Dusts;
using Reverie.Content.Tiles.Archaea;

using Reverie.Core.Cinematics;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Core.Missions.Core;
using SubworldLibrary;
using System.Linq;
using Terraria.UI;

namespace Reverie.Common.Players;

public class ReveriePlayer : ModPlayer
{
    private bool notificationExists = false;
    private Mission currentMission;
    public bool magnetizedFall;
    public bool lodestoneKB;
    public bool microlithEquipped;
    public override void PostUpdate()
    {
        if (Main.LocalPlayer.ZoneDesert || SubworldSystem.IsActive<ArchaeaSub>())
            DrawSandHaze();

        if (!Cutscene.IsPlayerVisible)
            Player.AddBuff(BuffID.Invisibility, 1, true);

        if (Cutscene.NoFallDamage)
            Player.noFallDmg = true;

        var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        bool hasMissions = missionPlayer.ActiveMissions().Any() || missionPlayer.AvailableMissions().Any();

        if (hasMissions && !notificationExists)
        {
            var currentMission = missionPlayer.ActiveMissions().FirstOrDefault() ??
                               missionPlayer.AvailableMissions().FirstOrDefault();

            if (currentMission != null)
            {
                MissionSidebarManager.Instance.SetNotification(new MissionSidebar(currentMission));
                notificationExists = true;
            }
        }
        else if (!hasMissions)
        {
            MissionSidebarManager.Instance.ClearNotification();
            notificationExists = false;
        }

        DialogueManager.Instance.Update();
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
        microlithEquipped = false;
    }
    public override void OnEnterWorld()
    {
        base.OnEnterWorld();

        if (!DownedSystem.initialCutscene && Main.netMode != NetmodeID.MultiplayerClient)
            CutsceneSystem.PlayCutscene<IntroCutscene>();

        notificationExists = false;
    }

    private bool IsSandTile(Tile tile) => tile.HasTile && (
        tile.TileType == TileID.Sand ||
        tile.TileType == ModContent.TileType<PrimordialSandTile>()
    );

    private bool HasAirAbove(int x, int y) =>
        !Main.tile[x, y - 1].HasTile &&
        !Main.tile[x, y - 2].HasTile;

    private void DrawSandHaze()
    {
        var config = ModContent.GetInstance<SandHazeConfig>();

        if (!config.EffectiveEnableSandHaze)
            return;

        int startX = (int)(Player.position.X / 16) - config.EffectiveHorizontalRange;
        int endX = (int)(Player.position.X / 16) + config.EffectiveHorizontalRange;
        int startY = (int)(Player.position.Y / 16) - config.EffectiveVerticalRange;
        int endY = (int)(Player.position.Y / 16) + config.EffectiveVerticalRange;

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                if (x < 0 || x >= Main.maxTilesX || y < 2 || y >= Main.maxTilesY)
                    continue;

                if (IsSandTile(Main.tile[x, y]) && HasAirAbove(x, y) && Main.rand.NextBool(config.EffectiveDustSpawnChance))
                {
                    SpawnSandDust(x, y);
                }
            }
        }
    }

    private void SpawnSandDust(int tileX, int tileY)
    {
        var config = ModContent.GetInstance<SandHazeConfig>();

        Vector2 dustPosition = new((tileX - 2) * 16, (tileY - 1) * 16);
        int dustIndex = Dust.NewDust(dustPosition, config.EffectiveDustSize, config.EffectiveDustSize, ModContent.DustType<SandHazeDust>());
        Main.dust[dustIndex].velocity.X -= Main.windSpeedCurrent * config.EffectiveWindVelocityFactor;

        if (Player.ZoneSandstorm)
        {
            Main.dust[dustIndex].velocity.Y += config.EffectiveSandstormUpwardVelocity;
        }
    }
}