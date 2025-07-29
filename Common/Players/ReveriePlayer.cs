using Reverie.Common.Subworlds.Archaea;
using Reverie.Common.Systems;
using Reverie.Common.UI.Missions;
using Reverie.Content.Cutscenes;
using Reverie.Content.Dusts;
using Reverie.Content.Projectiles.Desert;
using Reverie.Content.Tiles.Archaea;

using Reverie.Core.Cinematics;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using SubworldLibrary;
using System.Linq;

namespace Reverie.Common.Players;

public class ReveriePlayer : ModPlayer
{
    private bool notificationExists = false;
    public bool magnetizedFall;
    public bool lodestoneKB;
    public bool microlithEquipped;
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

    public override void PostUpdate()
    {
        if (Main.LocalPlayer.ZoneDesert || SubworldSystem.IsActive<ArchaeaSub>())
        {
            SpawnTumbleweed();
        }

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

    #region Desert Visuals
    private void SpawnTumbleweed()
    {
        if (!Main.rand.NextBool(1200))
            return;

        int existingTumbleweeds = 0;
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<TumbleweedProjectile>())
                existingTumbleweeds++;
        }

        if (existingTumbleweeds >= 3) 
            return;

        bool spawnLeft = Main.rand.NextBool();
        int spawnX = spawnLeft ?
            (int)(Player.position.X - Main.screenWidth / 2) :
            (int)(Player.position.X + Main.screenWidth / 2);

        int spawnY = (int)(Player.position.Y / 16);
        for (int y = spawnY; y < Main.maxTilesY - 10; y++)
        {
            if (Main.tile[spawnX / 16, y].HasTile && Main.tileSolid[Main.tile[spawnX / 16, y].TileType])
            {
                spawnY = y - 2;
                break;
            }
        }

        Vector2 spawnPos = new(spawnX, spawnY * 16);
        Vector2 velocity = new(spawnLeft ? Main.rand.NextFloat(1f, 3f) : Main.rand.NextFloat(-3f, -1f), 0);

        Projectile.NewProjectile(Player.GetSource_Misc("TumbleweedSpawn"), spawnPos, velocity,
            ModContent.ProjectileType<TumbleweedProjectile>(), 0, 0f, Player.whoAmI);
    }

    #endregion
}