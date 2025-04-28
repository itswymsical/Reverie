﻿using System.Collections.Generic;
using System.IO;
using System.Linq;

using Terraria.ModLoader.IO;

namespace Reverie.Core.Missions;

public class Objective(string description, int requiredCount = 1)
{
    public string Description { get; set; } = description;
    public bool IsCompleted { get; set; } = false;
    public int RequiredCount { get; set; } = requiredCount;
    public int CurrentCount { get; set; } = 0;
    public bool IsVisible { get; set; } = true;

    public delegate bool VisibilityCondition(Mission mission);
    public VisibilityCondition VisibilityCheck { get; set; } = null;

    public bool ShouldBeVisible(Mission mission)
    {
        if (VisibilityCheck != null)
            return VisibilityCheck(mission);

        return IsVisible;
    }
    public bool UpdateProgress(int amount = 1)
    {
        CurrentCount += amount;
        if (CurrentCount >= RequiredCount)
        {
            IsCompleted = true;
            CurrentCount = RequiredCount;
            return true;
        }
        return false;
    }

    public void WriteData(BinaryWriter writer)
    {
        writer.Write(Description);
        writer.Write(IsCompleted);
        writer.Write(RequiredCount);
        writer.Write(CurrentCount);
        writer.Write(IsVisible);
    }

    public void ReadData(BinaryReader reader)
    {
        Description = reader.ReadString();
        IsCompleted = reader.ReadBoolean();
        RequiredCount = reader.ReadInt32();
        CurrentCount = reader.ReadInt32();
        IsVisible = reader.ReadBoolean();
    }

    public byte[] Serialize()
    {
        using MemoryStream ms = new();
        using (BinaryWriter writer = new(ms))
        {
            WriteData(writer);
        }
        return ms.ToArray();
    }

    public void Deserialize(byte[] data)
    {
        using MemoryStream ms = new(data);
        using BinaryReader reader = new(ms);
        ReadData(reader);
    }
}

public class ObjectiveIndexState
{
    public List<ObjectiveState> Objectives { get; set; } = [];

    public TagCompound Serialize()
    {
        return new TagCompound
        {
            ["Objectives"] = Objectives.Select(obj => obj.Serialize()).ToList()
        };
    }

    public static ObjectiveIndexState Deserialize(TagCompound tag)
    {
        try
        {
            return new ObjectiveIndexState
            {
                Objectives = tag.GetList<TagCompound>("Objectives")
                    .Select(t => ObjectiveState.Deserialize(t))
                    .Where(obj => obj != null)
                    .ToList()
            };
        }
        catch
        {
            return new ObjectiveIndexState();
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
    public bool HasCheckedInitialInventory { get; set; } = false;

    public void Reset()
    {
        foreach (var objective in Objectives)
        {
            objective.IsCompleted = false;
            objective.CurrentCount = 0;
        }
        HasCheckedInitialInventory = false;
    }
    public void WriteData(BinaryWriter writer)
    {
        writer.Write(Objectives.Count);
        writer.Write(HasCheckedInitialInventory);
        foreach (var objective in Objectives)
        {
            objective.WriteData(writer);
        }
    }

    public void ReadData(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        HasCheckedInitialInventory = reader.ReadBoolean();
        Objectives.Clear();
        for (int i = 0; i < count; i++)
        {
            var objective = new Objective("", 1);
            objective.ReadData(reader);
            Objectives.Add(objective);
        }
    }

    public byte[] Serialize()
    {
        using MemoryStream ms = new();
        using (BinaryWriter writer = new(ms))
        {
            WriteData(writer);
        }
        return ms.ToArray();
    }

    public void Deserialize(byte[] data)
    {
        using MemoryStream ms = new(data);
        using BinaryReader reader = new(ms);
        ReadData(reader);
    }
}