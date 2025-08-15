using System.Collections.Generic;
using System.IO;
using System.Linq;

using Terraria.ModLoader.IO;

namespace Reverie.Core.Missions;

public class Objective(string description, int requiredCount = 1)
{
    public string Description { get; set; } = description;
    public bool IsCompleted { get; set; } = false;
    public int RequiredCount { get; set; } = requiredCount;
    public int Count { get; set; } = 0;
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
        Count += amount;
        if (Count >= RequiredCount)
        {
            IsCompleted = true;
            Count = RequiredCount;
            return true;
        }
        return false;
    }

    public void WriteData(BinaryWriter writer)
    {
        writer.Write(Description);
        writer.Write(IsCompleted);
        writer.Write(RequiredCount);
        writer.Write(Count);
        writer.Write(IsVisible);
    }

    public void ReadData(BinaryReader reader)
    {
        Description = reader.ReadString();
        IsCompleted = reader.ReadBoolean();
        RequiredCount = reader.ReadInt32();
        Count = reader.ReadInt32();
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
            ["Objective"] = Objectives.Select(obj => obj.Serialize()).ToList()
        };
    }

    public static ObjectiveIndexState Deserialize(TagCompound tag)
    {
        try
        {
            return new ObjectiveIndexState
            {
                Objectives = tag.GetList<TagCompound>("Objective")
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
            ["Count"] = CurrentCount
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
                CurrentCount = tag.GetInt("Count")
            };
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// gets a list of objectives for a mission. Missions typically have serveral task lists that need to be completed.
/// </summary>
/// <param name="objectives"></param>
public class ObjectiveList(List<Objective> objectives)
{
    /// <summary>
    /// gets the current objective index.
    /// </summary>
    public List<Objective> Objective { get; } = objectives;
    public bool IsCompleted => Objective.All(o => o.IsCompleted);
    public bool HasCheckedInitialInventory { get; set; } = false;

    public void Reset()
    {
        foreach (var objective in Objective)
        {
            objective.IsCompleted = false;
            objective.Count = 0;
        }
        HasCheckedInitialInventory = false;
    }
    public void WriteData(BinaryWriter writer)
    {
        writer.Write(Objective.Count);
        writer.Write(HasCheckedInitialInventory);
        foreach (var objective in Objective)
        {
            objective.WriteData(writer);
        }
    }

    public void ReadData(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        HasCheckedInitialInventory = reader.ReadBoolean();
        Objective.Clear();
        for (var i = 0; i < count; i++)
        {
            var objective = new Objective("", 1);
            objective.ReadData(reader);
            Objective.Add(objective);
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