using Terraria.ModLoader.IO;

namespace Reverie.Common.Systems.Elemental;

public struct ElementalData
{
    public ElementType element;
    public int boundExp;

    public ElementalData(ElementType element, int boundExp)
    {
        this.element = element;
        this.boundExp = boundExp;
    }

    public readonly TagCompound Save()
    {
        return new TagCompound
        {
            ["element"] = (int)element,
            ["boundExp"] = boundExp
        };
    }

    public static ElementalData Load(TagCompound tag)
    {
        return new ElementalData(
            (ElementType)tag.GetInt("element"),
            tag.GetInt("boundExp")
        );
    }

    public readonly float GetDamageBonus()
    {
        return element switch
        {
            ElementType.Fire => boundExp * 0.001f,
            _ => 0f
        };
    }

    public readonly int GetEffectStrength()
    {
        return element switch
        {
            ElementType.Fire => Math.Max(1, boundExp / 10),
            _ => 0
        };
    }
}

public enum ElementType
{
    None = 0,
    Fire = 1
}