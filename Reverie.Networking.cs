using Reverie.Common.Players;
using Reverie.Core.Missions;
using Reverie.Core.Missions.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reverie;

public sealed partial class Reverie : Mod
{
    public enum MessageType : byte
    {
        AddExperience,
        ClassStatPlayerSync,
        UnlockMainlineMission,
        StartMainlineMission,
        RequestMainlineMissionSync,
        SendMainlineMissionSync
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        MessageType msgType = (MessageType)reader.ReadByte();

        switch (msgType)
        {
            case MessageType.AddExperience:
                int playerID = reader.ReadInt32();
                int experience = reader.ReadInt32();
                if (playerID >= 0 && playerID < Main.maxPlayers)
                {
                    Player player = Main.player[playerID];
                    ExperiencePlayer.AddExperience(player, experience);
                    CombatText.NewText(player.Hitbox, Color.LightGoldenrodYellow, $"+{experience} Exp", true);
                }
                break;

            case MessageType.UnlockMainlineMission:
                int unlockPlayerID = reader.ReadInt32();
                int unlockMissionID = reader.ReadInt32();
                bool broadcast = reader.ReadBoolean();

                if (unlockPlayerID == -1)
                {
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        if (Main.player[i].active)
                        {
                            var missionPlayer = Main.player[i].GetModPlayer<MissionPlayer>();
                            missionPlayer.UnlockMissionLocal(unlockMissionID, broadcast);
                        }
                    }
                }
                else if (unlockPlayerID >= 0 && unlockPlayerID < Main.maxPlayers && Main.player[unlockPlayerID].active)
                {
                    var missionPlayer = Main.player[unlockPlayerID].GetModPlayer<MissionPlayer>();
                    missionPlayer.UnlockMissionLocal(unlockMissionID, broadcast);
                }
                break;

            case MessageType.StartMainlineMission:
                int startPlayerID = reader.ReadInt32();
                int startMissionID = reader.ReadInt32();

                if (startPlayerID == -1)
                {
                    for (int i = 0; i < Main.maxPlayers; i++)
                    {
                        if (Main.player[i].active)
                        {
                            var missionPlayer = Main.player[i].GetModPlayer<MissionPlayer>();
                            missionPlayer.StartMissionLocal(startMissionID);
                        }
                    }
                }
                else if (startPlayerID >= 0 && startPlayerID < Main.maxPlayers && Main.player[startPlayerID].active)
                {
                    var missionPlayer = Main.player[startPlayerID].GetModPlayer<MissionPlayer>();
                    missionPlayer.StartMissionLocal(startMissionID);
                }
                break;

            case MessageType.RequestMainlineMissionSync:
                int requestingPlayer = reader.ReadInt32();

                if (Main.netMode == NetmodeID.Server)
                {
                    SendMainlineMissionSyncToPlayer(requestingPlayer);
                }
                break;

            case MessageType.SendMainlineMissionSync:
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    var localMissionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
                    localMissionPlayer.ReceiveMainlineMissionSyncData(reader);
                }
                break;

            default:
                Logger.WarnFormat($"{NAME + NAME_PREFIX} Unknown Message type: {0}", msgType);
                break;
        }
    }

    /// <summary>
    /// Sends a packet to unlock a mainline mission for all players
    /// </summary>
    /// <param name="missionId">ID of the mission to unlock</param>
    /// <param name="broadcast">Whether to show notification</param>
    public static void SendUnlockMainlineMission(int missionId, bool broadcast = false)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return; // No networking needed in single player

        ModPacket packet = Instance.GetPacket();
        packet.Write((byte)MessageType.UnlockMainlineMission);
        packet.Write(-1);
        packet.Write(missionId);
        packet.Write(broadcast);
        packet.Send();
    }

    /// <summary>
    /// Sends a packet to start a mainline mission for all players
    /// </summary>
    /// <param name="missionId">ID of the mission to start</param>
    public static void SendStartMainlineMission(int missionId)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return; // No networking needed in single player

        ModPacket packet = Instance.GetPacket();
        packet.Write((byte)MessageType.StartMainlineMission);
        packet.Write(-1);
        packet.Write(missionId);
        packet.Send();
    }

    /// <summary>
    /// Requests mainline mission sync data from the server (client only)
    /// </summary>
    public static void RequestMainlineMissionSync()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;

        ModPacket packet = Instance.GetPacket();
        packet.Write((byte)MessageType.RequestMainlineMissionSync);
        packet.Write(Main.LocalPlayer.whoAmI);
        packet.Send();
    }

    /// <summary>
    /// Server method: Collects mainline mission progress and sends to requesting player
    /// </summary>
    private static void SendMainlineMissionSyncToPlayer(int targetPlayer)
    {
        if (Main.netMode != NetmodeID.Server)
            return;

        try
        {
            var syncData = new Dictionary<int, (int CurrentIndex, int Progress, bool IsUnlocked, bool IsStarted)>();

            // Collect mainline mission progress from all active players
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                var player = Main.player[i];
                if (player.active)
                {
                    var missionPlayer = player.GetModPlayer<MissionPlayer>();

                    foreach (var mission in missionPlayer.missionDict.Values.Where(m => m.IsMainline))
                    {
                        var missionId = mission.ID;

                        if (!syncData.ContainsKey(missionId))
                        {
                            syncData[missionId] = (mission.CurrentIndex, (int)mission.Progress, mission.Unlocked, mission.Progress == MissionProgress.Ongoing);
                        }
                        else
                        {
                            var current = syncData[missionId];

                            if (mission.CurrentIndex > current.CurrentIndex ||
                                (mission.CurrentIndex == current.CurrentIndex && mission.Progress == MissionProgress.Ongoing && !current.IsStarted))
                            {
                                syncData[missionId] = (mission.CurrentIndex, (int)mission.Progress,
                                    current.IsUnlocked || mission.Unlocked,
                                    current.IsStarted || mission.Progress == MissionProgress.Ongoing);
                            }
                        }
                    }
                }
            }

            ModPacket packet = Instance.GetPacket();
            packet.Write((byte)MessageType.SendMainlineMissionSync);
            packet.Write(syncData.Count);

            foreach (var (missionId, (currentIndex, progress, isUnlocked, isStarted)) in syncData)
            {
                packet.Write(missionId);
                packet.Write(currentIndex);
                packet.Write(progress);
                packet.Write(isUnlocked);
                packet.Write(isStarted);
            }

            packet.Send(targetPlayer);

            Instance.Logger.Info($"Sent mainline mission sync data to player {targetPlayer}: {syncData.Count} missions");
        }
        catch (Exception ex)
        {
            Instance.Logger.Error($"Error sending mainline mission sync: {ex}");
        }
    }
}