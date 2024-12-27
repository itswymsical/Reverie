using Terraria.ModLoader.IO;
using Terraria;
using Microsoft.Xna.Framework;

namespace Reverie.Core.Skills
{
    public class SkillData
    {
        public int ID { get; set; }
        public int CurrentStack { get; set; }

        public TagCompound Save()
        {
            return new TagCompound {
                {"ID", ID},
                {"CurrentStack", CurrentStack}
            };
        }

        public static SkillData Load(TagCompound tag)
        {
            return new SkillData
            {
                ID = tag.GetInt("ID"),
                CurrentStack = tag.GetInt("CurrentStack")
            };
        }
    }
}