using System.Collections.Generic;
using System.Linq;
using Reverie.Common.Players;
using System;

namespace Reverie.Core.Skills
{
    public class Skill
    {
        public int ID { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int MaxStack { get; private set; }
        public List<float> EffectPerStack { get; private set; }
        public bool IsAdvancement { get; private set; }
        public bool IsAugment { get; private set; }
        public Action<SkillPlayer> OnUnlock { get; private set; }

        private SkillData _data;

        public Skill(int id, string name, string description, int maxStack, List<float> effectPerStack, bool isAdvancement = false, bool isAugment = false)
        {
            ID = id;
            Name = name;
            Description = description;
            MaxStack = maxStack;
            EffectPerStack = effectPerStack;
            IsAdvancement = isAdvancement;
            IsAugment = isAugment;
        }

        public float GetEffectForStack(int stack)
        {
            return EffectPerStack.Take(stack).Sum();
        }

        public float GetNextLevelEffect(int currentStack)
        {
            if (currentStack < MaxStack)
            {
                return GetEffectForStack(currentStack) + EffectPerStack[currentStack];
            }
            return GetEffectForStack(currentStack);
        }

        public SkillData GetData()
        {
            return _data;
        }

        public void SetData(SkillData data)
        {
            _data = data;
        }
    }
}