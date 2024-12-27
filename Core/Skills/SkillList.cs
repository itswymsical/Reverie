using System.Collections.Generic;
using System.Linq;

namespace Reverie.Core.Skills
{
    public static class SkillList
    {
        public static class IDs
        {
            public const int WithHaste = 1;
            public const int YearnForTheMines = 2;
            public const int Fortune = 3;
            public const int Loothound = 4;
            public const int Groundhog = 5;
            public const int YouCanSeeIt = 6;
        }

        private static Dictionary<int, Skill> _skills;

        public static void Initialize()
        {
            _skills = new Dictionary<int, Skill>
        {
            {
                IDs.WithHaste,
                new Skill(
                    IDs.WithHaste,
                    "With Haste!",
                    "Increases mining speed",
                    3,
                    [0.06f, 0.04f, 0.07f],
                    isAdvancement: true)
                },
            {
                IDs.Fortune,
                new Skill(
                    IDs.Fortune,
                    "Fortune",
                    "Shinnies have a small chance to drop extra on destruction\nA blast of gold will appear on harvest",
                    1,
                    [0.17f],
                    isAugment: true)
                }
            };
        }

        public static Skill GetSkillById(int id)
        {
            if (_skills == null)
            {
                Initialize();
            }

            return _skills.TryGetValue(id, out Skill skill) ? skill : null;
        }

        public static IEnumerable<Skill> GetAllSkills()
        {
            if (_skills == null)
            {
                Initialize();
            }

            return _skills.Values;
        }

        public static IEnumerable<Skill> GetSkillsByType(bool isAdvancement, bool isAugment)
        {
            return GetAllSkills().Where(s => s.IsAdvancement == isAdvancement && s.IsAugment == isAugment);
        }

        public static IEnumerable<int> GetAllSkillIds()
        {
            return GetAllSkills().Select(s => s.ID);
        }
    }
}