using System.IO;
using Terraria.ModLoader.IO;
using Terraria.ModLoader;
using Terraria;

namespace Reverie.Common.Systems
{
    public class DownedBossSystem : ModSystem
    {
        public static bool downedFungore = false;
        public static bool downedWarden = false;
        public static bool pickedUpAnAccessoryForTheFirstTime = false;
        public static bool enteredTheWoodlandCanopyBeforeProgression = false;
        public override void ClearWorld()
        {
            downedFungore = false;
            downedWarden = false;
            pickedUpAnAccessoryForTheFirstTime = false;
            enteredTheWoodlandCanopyBeforeProgression = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            if (downedFungore)
            {
                tag["downedFungore"] = true;
            }
            if (downedWarden)
            {
                tag["downedWarden"] = true;
            }
            if (pickedUpAnAccessoryForTheFirstTime)
            {
                tag["pickedUpAnAccessoryForTheFirstTime"] = true;
            }
            if (enteredTheWoodlandCanopyBeforeProgression)
            {
                tag["enteredTheWoodlandCanopyBeforeProgression"] = true;
            }
        }

        public override void LoadWorldData(TagCompound tag)
        {
            downedFungore = tag.ContainsKey("downedFungore");
            downedWarden = tag.ContainsKey("downedWarden");
            pickedUpAnAccessoryForTheFirstTime = tag.ContainsKey("pickedUpAnAccessoryForTheFirstTime");
            enteredTheWoodlandCanopyBeforeProgression = tag.ContainsKey("enteredTheWoodlandCanopyBeforeProgression");
        }

        public override void NetSend(BinaryWriter writer)
        {
            var flags = new BitsByte();
            flags[0] = downedFungore;
            flags[1] = downedWarden;
            writer.Write(flags);

            var randomEventFlags = new BitsByte();
            randomEventFlags[0] = pickedUpAnAccessoryForTheFirstTime;
            randomEventFlags[0] = enteredTheWoodlandCanopyBeforeProgression;
            writer.Write(randomEventFlags);
        }

        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            downedFungore = flags[0];
            downedWarden = flags[0];

            BitsByte randomEventFlags = reader.ReadByte();
            pickedUpAnAccessoryForTheFirstTime = randomEventFlags[0];
            enteredTheWoodlandCanopyBeforeProgression = randomEventFlags[0];
        }
    }
}
