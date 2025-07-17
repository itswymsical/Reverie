using Reverie.Common.NPCs;
using Reverie.Core.NPCs.Components;
using System.Collections.Generic;
using Terraria;

namespace Reverie.Content.NPCs.WorldNPCs;

public class CivilianNPC : WorldNPC
{
    // Static arrays for randomization
    private static readonly Color[] SkinColors = new Color[]
    {
        Color.BurlyWood,
        Color.PeachPuff,
        Color.SandyBrown,
        Color.Tan,
        Color.Wheat,
        new Color(139, 69, 19),    // SaddleBrown
        new Color(160, 82, 45),    // Sienna
        new Color(210, 180, 140),  // Tan
        new Color(222, 184, 135),  // BurlyWood
        new Color(245, 245, 220),  // Beige
    };

    private static readonly Color[] HairColors = new Color[]
    {
        Color.Black,
        Color.Brown,
        Color.RosyBrown,
        Color.BlanchedAlmond,
        Color.Yellow,
        Color.Orange,
        Color.Red,
        Color.DarkRed,
        Color.Purple,
        Color.Blue,
        Color.Green,
        Color.Gray,
        Color.White,
        new Color(139, 69, 19),    // SaddleBrown
        new Color(160, 82, 45),    // Sienna
        new Color(205, 133, 63),   // Peru
        new Color(222, 184, 135),  // BurlyWood
        new Color(245, 222, 179),  // Wheat
    };

    private static readonly string[] FirstNames = new string[]
    {
        "Alex", "Jordan", "Casey", "Morgan", "Riley", "Avery", "Quinn", "Sage",
        "Blake", "Cameron", "Dakota", "Emery", "Finley", "Hayden", "Jamie", "Kai",
        "Logan", "Micah", "Noah", "Parker", "Reese", "River", "Rowan", "Skylar",
        "Taylor", "Teagan", "Zion", "Ari", "Ash", "Bay", "Cedar", "Drew",
        "Ellis", "Gray", "Hunter", "Indigo", "Jesse", "Kendall", "Lane", "Max",
        "Nico", "Ocean", "Phoenix", "Rain", "Sam", "Tate", "Val", "Winter"
    };

    private static readonly string[] LastNames = new string[]
    {
        "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis",
        "Rodriguez", "Martinez", "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson", "Thomas",
        "Taylor", "Moore", "Jackson", "Martin", "Lee", "Perez", "Thompson", "White",
        "Harris", "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson", "Walker", "Young",
        "Allen", "King", "Wright", "Scott", "Torres", "Nguyen", "Hill", "Flores",
        "Green", "Adams", "Nelson", "Baker", "Hall", "Rivera", "Campbell", "Mitchell"
    };
    private WorldNPCComponent worldComponent;

    public override Color SkinColor { get; set; }
    public override int HairType { get; set; }
    public override Color HairColor { get; set; }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }

    public override void SetDefaults()
    {
        base.SetDefaults(); // Call the parent SetDefaults first

        // Randomize appearance
        RandomizeAppearance();

        // Set other NPC properties
        NPC.knockBackResist = 0.5f;

        if (!NPC.TryGetGlobalNPC(out worldComponent))
        {
            worldComponent = new WorldNPCComponent
            {
                Enabled = true,
                FollowDistance = 100f
            };
        }
    }

    private void RandomizeAppearance()
    {
        SkinColor = SkinColors[Main.rand.Next(SkinColors.Length)];

        HairType = Main.rand.Next(1, 52);

        HairColor = HairColors[Main.rand.Next(HairColors.Length)];

        string firstName = FirstNames[Main.rand.Next(FirstNames.Length)];
        string lastName = LastNames[Main.rand.Next(LastNames.Length)];
    }

    public override string GetChat()
    {
        return Main.rand.Next(4) switch
        {
            0 => "bogus.",
            1 => "bongus.",
            2 => "chongus.",
            _ => "boobus chongus.",
        };
    }

    public override void SetChatButtons(ref string button, ref string button2)
    {
        button = "Follow/Stay";
        button2 = "Wander/Stay";
    }

    public override List<string> SetNPCNameList()
    {
        var names = new List<string>();
        for (int i = 0; i < 100; i++)
        {
            string firstName = FirstNames[Main.rand.Next(FirstNames.Length)];
            string lastName = LastNames[Main.rand.Next(LastNames.Length)];
            string fullName = $"{firstName} {lastName}";

            if (!names.Contains(fullName))
            {
                names.Add(fullName);
            }
        }
        return names;
    }
    public override void OnChatButtonClicked(bool firstButton, ref string shopName)
    {
        if (worldComponent == null) return;

        if (firstButton)
        {
            switch (NPC.ai[3])
            {
                case 1f: // If following
                    worldComponent.Stay(NPC);
                    Main.NewText($"<{NPC.GivenName}> Okay! I'll be here if you need anything.");
                    break;
                case 2f: // If staying
                case 3f: // If wandering
                default:
                    worldComponent.Follow(NPC, Main.myPlayer);
                    Main.NewText($"<{NPC.GivenName}> Following you!");
                    break;
            }
        }
        else
        {
            switch (NPC.ai[3])
            {
                case 1f: // If following
                case 2f: // If staying
                    worldComponent.Wander(NPC);
                    Main.NewText($"<{NPC.GivenName}> I ought to talk to my friends...");
                    break;
                case 3f: // If wandering
                    worldComponent.Stay(NPC);
                    Main.NewText($"<{NPC.GivenName}> Okay! I'll be here if you need anything.");
                    break;
            }
        }
    }

}