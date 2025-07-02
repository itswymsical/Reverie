using System.IO;
using Terraria.ModLoader.IO;

namespace Reverie.Common.Systems;

/// <summary>
/// Acts as a container for "downed boss" flags and other event flags.
/// </summary>
public class DownedSystem : ModSystem
{
    public static bool foundChronicleI = false;
    public static bool initialCutscene = false;

    public override void ClearWorld()
    {
        foundChronicleI = false;
        initialCutscene = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        if (foundChronicleI)
        {
            tag["foundChronicleI"] = true;
        }
        if (initialCutscene)
        {
            tag["initialCutscene"] = true;
        }
    }

    public override void LoadWorldData(TagCompound tag)
    {
        foundChronicleI = tag.ContainsKey("foundChronicleI");
        initialCutscene = tag.ContainsKey("initialCutscene");
    }

    public override void NetSend(BinaryWriter writer)
    {
        // Order of parameters is important and has to match that of NetReceive
        writer.WriteFlags(foundChronicleI, initialCutscene);
        // WriteFlags supports up to 8 entries, if you have more than 8 flags to sync, call WriteFlags again.

        // If you need to send a large number of flags, such as a flag per item type or something similar, BitArray can be used to efficiently send them. See Utils.SendBitArray documentation.
    }

    public override void NetReceive(BinaryReader reader)
    {
        // Order of parameters is important and has to match that of NetSend
        reader.ReadFlags(out foundChronicleI, out initialCutscene);
        // ReadFlags supports up to 8 entries, if you have more than 8 flags to sync, call ReadFlags again.
    }
}
