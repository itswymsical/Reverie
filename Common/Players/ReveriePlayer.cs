using Microsoft.Xna.Framework;
using Reverie.Content.Dusts;
using Reverie.Core.Cutscenes;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Reverie.Reverie;

namespace Reverie.Common.Players
{
    public class ReveriePlayer : ModPlayer
    {
        public bool ZoneWoodlandCanopy;
        public bool borealCutter;
        public bool onSand;

        public bool pathWarrior;
        public bool pathMarksman;
        public bool pathMage;
        public bool pathConjurer;

        public override void ResetEffects()
        {
            onSand = false;
        }

        public override void PostUpdateMiscEffects()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                Player.ManageSpecialBiomeVisuals("HeatDistortion", Player.ZoneEmberiteCaverns(), Player.Center);
                //Player.ManageSpecialBiomeVisuals("EmberiteCavernsScreenShader", Player.ZoneEmberiteCaverns(), Player.Center);
            }
        }

        public override void PostUpdate()
        {
            if (Main.LocalPlayer.ZoneDesert)
            {
                DrawSand();
            }
        }

        public override void SetControls()
        {
            if (CutsceneSystem.DisableMoment)
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

        private void DrawSand()
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
                            int Index = Dust.NewDust(new Vector2((k - 2) * 16, (i - 1) * 16), 5, 5, ModContent.DustType<SandHaze>());
                            Main.dust[Index].velocity.X -= Main.windSpeedCurrent / 5.6f;
                            if (Player.ZoneSandstorm)
                            {
                                Main.dust[Index].velocity.Y += 0.07f;
                            }
                        }
                    }
                }
            }
        }

        public override void ModifyStartingInventory(IReadOnlyDictionary<string, List<Item>> itemsByMod, bool mediumCoreDeath)
        {
            itemsByMod["Terraria"].RemoveAll(item => item.type == ItemID.CopperShortsword);
            itemsByMod["Terraria"].RemoveAll(item => item.type == ItemID.CopperPickaxe);
            itemsByMod["Terraria"].RemoveAll(item => item.type == ItemID.CopperAxe);
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)MessageType.ClassType);
            packet.Write((byte)Player.whoAmI);
            packet.Write(pathWarrior);
            packet.Write(pathMarksman);
            packet.Write(pathMage);
            packet.Write(pathConjurer);
            packet.Send(toWho, fromWho);
        }

        public void ReceivePlayerSync(BinaryReader reader)
        {
            pathWarrior = reader.ReadBoolean();
            pathMarksman = reader.ReadBoolean();
            pathMage = reader.ReadBoolean();
            pathConjurer = reader.ReadBoolean();
        }
        public override void CopyClientState(ModPlayer targetCopy)
        {
            ReveriePlayer clone = (ReveriePlayer)targetCopy;
            clone.pathWarrior = pathWarrior;
            clone.pathMarksman = pathMarksman;
            clone.pathMage = pathMage;
            clone.pathConjurer = pathConjurer;
        }

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            ReveriePlayer clone = (ReveriePlayer)clientPlayer;

            if (pathWarrior != clone.pathWarrior || pathMarksman != clone.pathMarksman
                || pathMage != clone.pathMage || pathConjurer != clone.pathConjurer)
                SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
        }

        public override void SaveData(TagCompound tag)
        {
            tag["pathWarrior"] = pathWarrior;
            tag["pathMarksman"] = pathMarksman;
            tag["pathMage"] = pathMage;
            tag["pathConjurer"] = pathConjurer;
        }
        public override void LoadData(TagCompound tag)
        {
            pathWarrior = tag.GetBool("pathWarrior");
            pathMarksman = tag.GetBool("pathMarksman");
            pathMage = tag.GetBool("pathMage");
            pathConjurer = tag.GetBool("pathConjurer");
        }
    }
    public class IdleAnimationPlayer : ModPlayer
    {
        private float breathingSpeed = 2.5f;
        private float armAmplitude = 0.1f;
        private float bodyAmplitude = 0.05f;
        private float startTime;
        private bool wasMoving = true;
        private bool wasUsingItem = false;

        private float defaultBodyRotation;
        private Vector2 defaultBodyPosition;
        private Vector2 defaultLegPosition;
        private float defaultLegRotation;

        public override void Initialize()
        {
            defaultBodyRotation = Player.bodyRotation;
            defaultBodyPosition = Player.bodyPosition;
            defaultLegPosition = Player.legPosition;
            defaultLegRotation = Player.legRotation;
        }

        public override void PostUpdate()
        {
            bool isMoving = Player.velocity != Vector2.Zero;
            bool isUsingItem = Player.controlUseItem;

            if (isMoving || isUsingItem != wasUsingItem)
            {
                wasMoving = true;
                wasUsingItem = isUsingItem;
                startTime = Main.GameUpdateCount / 60f;

                ResetPositions();
            }
            else if (!isMoving && !isUsingItem && wasMoving)
            {
                wasMoving = false;
            }

            if (!isMoving && !isUsingItem && Player.itemAnimation <= 0)
            {
                ApplyIdleAnimation();
            }
        }

        private void ApplyIdleAnimation()
        {
            float currentTime = Main.GameUpdateCount / 60f;
            float animationTime = currentTime - startTime;

            float breathingCycle = (float)Math.Sin(animationTime * breathingSpeed);
            float frontArmOffset = (float)Math.Sin((animationTime + 0.2f) * breathingSpeed);

            float backArmRotation = (breathingCycle * armAmplitude);
            float frontArmRotation = (frontArmOffset * armAmplitude);

            Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, backArmRotation);
            Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, frontArmRotation);

            Player.bodyRotation = breathingCycle * (bodyAmplitude * 1.55f);
            Player.bodyPosition.Y = breathingCycle * bodyAmplitude;

            Player.legRotation = breathingCycle * (bodyAmplitude * 0.55f);
            Player.legPosition.Y = breathingCycle * bodyAmplitude;
        }

        private void ResetPositions()
        {
            Player.bodyRotation = defaultBodyRotation;
            Player.bodyPosition = defaultBodyPosition;
            Player.legPosition = defaultLegPosition;
            Player.legRotation = defaultLegRotation;
        }
    }
}
