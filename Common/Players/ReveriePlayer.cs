using Reverie.Common.Systems;
using Reverie.Common.UI.Missions;
using Reverie.Content.Cutscenes;
using Reverie.Content.Dusts;
using Reverie.Content.Projectiles.Desert;

using Reverie.Core.Cinematics;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using SubworldLibrary;
using System.Linq;

namespace Reverie.Common.Players;

public class ReveriePlayer : ModPlayer
{
    public bool magnetizedFall;
    public bool lodestoneKB;
    public bool microlithEquipped;
    public override void ResetEffects()
    {
        magnetizedFall = false;
        lodestoneKB = false;
        microlithEquipped = false;
    }


    public override void PostUpdate()
    {
        if (Main.LocalPlayer.ZoneDesert)
        {
            DrawSandHaze();
            SpawnTumbleweed();
        }

        if (!Cutscene.IsPlayerVisible)
            Player.AddBuff(BuffID.Invisibility, 1, true);

        if (Cutscene.NoFallDamage)
            Player.noFallDmg = true;


        if (Player == Main.LocalPlayer)
        {
            DialogueManager.Instance.Update();
        }
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
    private bool IsSandTile(Tile tile) =>
    tile.HasTile && (tile.TileType == TileID.Sand);

    private bool HasAirAbove(int x, int y) => !Main.tile[x, y - 1].HasTile && !Main.tile[x, y - 2].HasTile;

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
    #endregion
}