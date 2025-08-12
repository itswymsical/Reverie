using Reverie.Common.Players;
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
        ClassStatPlayerSync
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

            default:
                Logger.WarnFormat($"{NAME + NAME_PREFIX} Unknown Message type: {0}", msgType);
                break;
        }
    }
}