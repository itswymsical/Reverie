using Reverie.Common.Players;
using Reverie.Core.Missions;
using Terraria.ModLoader.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reverie.Core.Missions.Core;

namespace Reverie;

public sealed partial class Reverie : Mod
{
    public enum MessageType : byte
    {
        AddExperience,
        ClassStatPlayerSync,
        UnlockMainlineMission,
        StartMainlineMission,
        RequestMissionStateClone,
        SendMissionStateClone
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

                if (unlockPlayerID == -1) // All players (for mainline missions)
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

                if (startPlayerID == -1) // All players (for mainline missions)
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

            case MessageType.RequestMissionStateClone:
                int requestingPlayer = reader.ReadInt32();

                // Only server handles clone requests
                if (Main.netMode == NetmodeID.Server)
                {
                    SendMissionStateCloneToPlayer(requestingPlayer);
                }
                break;

            case MessageType.SendMissionStateClone:
                // Only clients receive clone data
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    var localMissionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
                    localMissionPlayer.ReceiveMissionStateClone(reader);
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
    public static void SendUnlockMainlineMission(int missionId, bool broadcast = false)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return;

        ModPacket packet = Instance.GetPacket();
        packet.Write((byte)MessageType.UnlockMainlineMission);
        packet.Write(-1); // -1 means all players
        packet.Write(missionId);
        packet.Write(broadcast);
        packet.Send();
    }

    /// <summary>
    /// Sends a packet to start a mainline mission for all players
    /// </summary>
    public static void SendStartMainlineMission(int missionId)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return;

        ModPacket packet = Instance.GetPacket();
        packet.Write((byte)MessageType.StartMainlineMission);
        packet.Write(-1); // -1 means all players
        packet.Write(missionId);
        packet.Send();
    }

    /// <summary>
    /// Requests a complete mission state clone from the server (client only)
    /// </summary>
    public static void RequestMissionStateClone()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
            return;

        ModPacket packet = Instance.GetPacket();
        packet.Write((byte)MessageType.RequestMissionStateClone);
        packet.Write(Main.LocalPlayer.whoAmI);
        packet.Send();
    }

    /// <summary>
    /// Server method: Creates authoritative mission state and sends to requesting player
    /// </summary>
    private static void SendMissionStateCloneToPlayer(int targetPlayer)
    {
        if (Main.netMode != NetmodeID.Server)
            return;

        try
        {
            // Create authoritative mission state by aggregating all players' progress
            var authoritativeState = new Dictionary<int, (int Progress, int Status, bool Unlocked, int CurrentIndex, bool IsMainline)>();

            // Scan all active players to build authoritative state
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                var player = Main.player[i];
                if (player.active)
                {
                    var missionPlayer = player.GetModPlayer<MissionPlayer>();

                    foreach (var mission in missionPlayer.missionDict.Values)
                    {
                        var missionId = mission.ID;
                        var currentData = ((int)mission.Progress, (int)mission.Status, mission.Unlocked, mission.CurrentIndex, mission.IsMainline);

                        if (!authoritativeState.ContainsKey(missionId))
                        {
                            // First time seeing this mission - use this state
                            authoritativeState[missionId] = currentData;
                        }
                        else
                        {
                            // Merge with existing state - keep furthest progress for mainline missions
                            var existing = authoritativeState[missionId];

                            if (mission.IsMainline)
                            {
                                if (currentData.Item1 > existing.Item1 ||
                                    (currentData.Item1 == existing.Item1 && currentData.Item4 > existing.Item4))
                                {
                                    authoritativeState[missionId] = currentData;
                                }
                            }
                        }
                    }
                }
            }

            // Always ensure Journey's Begin is unlocked in the authoritative state
            if (!authoritativeState.ContainsKey(MissionID.JourneysBegin))
            {
                authoritativeState[MissionID.JourneysBegin] = ((int)MissionProgress.Inactive, (int)MissionStatus.Unlocked, true, 0, true);
            }

            // Send the simplified state data to the requesting player
            ModPacket packet = Instance.GetPacket();
            packet.Write((byte)MessageType.SendMissionStateClone);
            packet.Write(authoritativeState.Count);

            foreach (var (missionId, (progress, status, unlocked, currentIndex, isMainline)) in authoritativeState)
            {
                packet.Write(missionId);
                packet.Write(progress);
                packet.Write(status);
                packet.Write(unlocked);
                packet.Write(currentIndex);
                packet.Write(isMainline);
            }

            packet.Send(targetPlayer);

            Instance.Logger.Info($"Sent mission state clone to player {targetPlayer}: {authoritativeState.Count} missions");
        }
        catch (Exception ex)
        {
            Instance.Logger.Error($"Error sending mission state clone: {ex}");
        }
    }
}