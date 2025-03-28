namespace Reverie.Core.Dialogue;

/// <summary>
/// A utility class to store dialogue keys as constants
/// to avoid hardcoding string literals throughout the codebase.
/// </summary>
public static class DialogueKeys
{
    // Chronicle dialogues
    public static class Chronicle
    {
        private const string PREFIX = "Chronicle_01";

        public static string Chapter1 => $"{PREFIX}.Chapter1";
    }

    // Tamer missions
    public static class TamerMissions
    {
        private const string PREFIX = "TamerTestMission";

        public static string Chapter1 => $"{PREFIX}.Chapter1";
    }

    // Crash Landing dialogues
    public static class CrashLanding
    {
        private const string PREFIX = "CrashLanding";

        public static string Intro => $"{PREFIX}.Intro";
        public static string Cutscene => $"{PREFIX}.Cutscene";
        public static string SettlingIn => $"{PREFIX}.SettlingIn";
        public static string GatheringResources => $"{PREFIX}.GatheringResources";
        public static string BuildShelter => $"{PREFIX}.BuildShelter";
        public static string WildlifeWoes => $"{PREFIX}.WildlifeWoes";
        public static string SlimeInfestation => $"{PREFIX}.SlimeInfestation";
        public static string SlimeInfestationCommentary => $"{PREFIX}.SlimeInfestation_Commentary";
        public static string SlimeRain => $"{PREFIX}.SlimeRain";
        public static string SlimeRainCommentary => $"{PREFIX}.SlimeRain_Commentary";
        public static string KSEncounter => $"{PREFIX}.KS_Encounter";
        public static string KSVictory => $"{PREFIX}.KS_Victory";
    }

    // Add more dialogue categories as needed

    // Helper method to create custom dialogue keys
    public static string Custom(string category, string sequence) => $"{category}.{sequence}";
}