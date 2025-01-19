using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace Reverie.Core.Missions
{
    public class Objective(string description, int requiredCount = 1)
    {
        public string Description { get; set; } = description;
        public bool IsCompleted { get; set; } = false;
        public int RequiredCount { get; set; } = requiredCount;
        public int CurrentCount { get; set; } = 0;

        public bool UpdateProgress(int amount = 1)
        {
            CurrentCount += amount;
            if (CurrentCount >= RequiredCount)
            {
                IsCompleted = true;
                CurrentCount = RequiredCount; // Cap at required count
                return true;
            }
            return false;
        }

        public TagCompound Save()
        {
            return new TagCompound
            {
                ["Description"] = Description,
                ["IsCompleted"] = IsCompleted,
                ["RequiredCount"] = RequiredCount,
                ["CurrentCount"] = CurrentCount
            };
        }

        public static Objective Load(TagCompound tag)
        {
            var objective = new Objective(tag.GetString("Description"), tag.GetInt("RequiredCount"))
            {
                IsCompleted = tag.GetBool("IsCompleted"),
                CurrentCount = tag.GetInt("CurrentCount")
            };
            return objective;
        }
    }

    public class ObjectiveSetState
    {
        public List<ObjectiveState> Objectives { get; set; } = [];

        public TagCompound Serialize()
        {
            return new TagCompound
            {
                ["Objectives"] = Objectives.Select(obj => obj.Serialize()).ToList()
            };
        }

        public static ObjectiveSetState Deserialize(TagCompound tag)
        {
            try
            {
                return new ObjectiveSetState
                {
                    Objectives = tag.GetList<TagCompound>("Objectives")
                        .Select(t => ObjectiveState.Deserialize(t))
                        .Where(obj => obj != null)
                        .ToList()
                };
            }
            catch
            {
                return new ObjectiveSetState();
            }
        }
    }

    public class ObjectiveState
    {
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public int RequiredCount { get; set; }
        public int CurrentCount { get; set; }

        public TagCompound Serialize()
        {
            return new TagCompound
            {
                ["Description"] = Description,
                ["IsCompleted"] = IsCompleted,
                ["RequiredCount"] = RequiredCount,
                ["CurrentCount"] = CurrentCount
            };
        }

        public static ObjectiveState Deserialize(TagCompound tag)
        {
            try
            {
                return new ObjectiveState
                {
                    Description = tag.GetString("Description"),
                    IsCompleted = tag.GetBool("IsCompleted"),
                    RequiredCount = tag.GetInt("RequiredCount"),
                    CurrentCount = tag.GetInt("CurrentCount")
                };
            }
            catch
            {
                return null;
            }
        }
    }

    public class ObjectiveSet(List<Objective> objectives)
    {
        public List<Objective> Objectives { get; } = objectives;
        public bool IsCompleted => Objectives.All(o => o.IsCompleted);

        public void Reset()
        {
            foreach (var objective in Objectives)
            {
                objective.IsCompleted = false;
                objective.CurrentCount = 0;
            }
        }
    }
}