using Microsoft.Xna.Framework;
using Reverie.Core.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Reverie.Common.Players
{
    public partial class SkillPlayer : ModPlayer
    {
        private Dictionary<int, SkillData> unlockedSkills = [];

        public override void Initialize()
        {
            base.Initialize();
            unlockedSkills.Clear();
        }

        public int GetSkillStack(int skillId)
        {
            return unlockedSkills.TryGetValue(skillId, out var data) ? data.CurrentStack : 0;
        }

        public void AddSkillStack(int skillId)
        {
            Skill skill = SkillList.GetSkillById(skillId);
            if (skill != null)
            {
                if (!unlockedSkills.TryGetValue(skillId, out var data))
                {
                    data = new SkillData { ID = skillId, CurrentStack = 0 };
                    unlockedSkills[skillId] = data;
                }

                if (data.CurrentStack < skill.MaxStack)
                {
                    data.CurrentStack++;
                    Main.NewText($"Skill {(data.CurrentStack > 1 ? "upgraded" : "unlocked")}: {skill.Name} (Stack: {data.CurrentStack})");
                }
            }
        }

        public override void UpdateEquips()
        {
            base.UpdateEquips();
            foreach (var skillData in unlockedSkills.Values)
            {
                Skill skill = SkillList.GetSkillById(skillData.ID);
                if (skill != null)
                {
                    if (skill.ID == SkillList.IDs.WithHaste)
                    {
                        Player.pickSpeed -= skill.GetEffectForStack(skillData.CurrentStack);
                    }
                    // Add other skill effects here
                }
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["UnlockedSkills"] = unlockedSkills.Values.Select(s => s.Save()).ToList();
        }

        public override void LoadData(TagCompound tag)
        {
            unlockedSkills.Clear();
            var loadedSkills = tag.GetList<TagCompound>("UnlockedSkills")
                .Select(SkillData.Load);
            foreach (var skillData in loadedSkills)
            {
                unlockedSkills[skillData.ID] = skillData;
            }
        }
    }
}