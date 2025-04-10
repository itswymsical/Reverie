namespace Reverie.Core.Dialogue;

/// <summary>
/// A utility class to store dialogue keys as constants
/// to avoid hardcoding string literals throughout the codebase.
/// </summary>
public static class DialogueKeys
{

    public static class Chronicle
    {
        private const string PREFIX = "Chronicle_01";

        public static string Chapter1 => $"{PREFIX}.Chapter1";
    }
    public static class Merchant
    {
        private const string PREFIX = "Merchant";

        public static string CopperStandardStart => $"{PREFIX}.CopperStandardStart";
        public static string CopperStandardComplete => $"{PREFIX}.CopperStandardComplete";
        public static string CopperCoinsInProgress => $"{PREFIX}.CopperCoinsInProgress";
        public static string CopperCoinsInProgress_Alt => $"{PREFIX}.CopperCoinsInProgress_Alt";
        public static string MineCopperInProgress => $"{PREFIX}.MineCopperInProgress";
        public static string SmeltCopperInProgress => $"{PREFIX}.SmeltCopperInProgress";
        public static string MerchantIntro => $"{PREFIX}.MerchantIntro";

    }
    public static class ArgieDialogue
    {
        private const string PREFIX = "ArgieDialogue";

        public static string Introduction => $"{PREFIX}.Introduction";
        public static string BloomcapIntro => $"{PREFIX}.BloomcapIntro";
        public static string BloomcapCollected => $"{PREFIX}.BloomcapCollected";
        public static string BloomcapCollectedHalf => $"{PREFIX}.BloomcapCollectedHalf";
        public static string BloomcapCollectedAll => $"{PREFIX}.BloomcapCollectedAll";
        public static string BloomcapComplete => $"{PREFIX}.BloomcapComplete";
    }

    public static class TamerMissions
    {
        private const string PREFIX = "TamerTestMission";

        public static string Chapter1 => $"{PREFIX}.Chapter1";
    }

    public static class FallingStar
    {
        private const string PREFIX = "AFallingStar";

        public static string Intro => $"{PREFIX}.Intro";
        public static string MerchantIntro => $"{PREFIX}.MerchantIntro";
        public static string DemolitionistIntro => $"{PREFIX}.DemolitionistIntro";
        public static string NurseIntro => $"{PREFIX}.NurseIntro";
        public static string Cutscene => $"{PREFIX}.Cutscene";
        public static string CrashLanding => $"{PREFIX}.CrashLanding";
        public static string GatheringResources => $"{PREFIX}.GatheringResources";
        public static string BuildShelter => $"{PREFIX}.BuildShelter";
        public static string WildlifeWoes => $"{PREFIX}.WildlifeWoes";
        public static string SlimeInfestation => $"{PREFIX}.SlimeInfestation";
        public static string SlimeInfestationCommentary => $"{PREFIX}.SlimeInfestation_Commentary";
        public static string SlimeRain => $"{PREFIX}.SlimeRain";
        public static string SlimeRainCommentary => $"{PREFIX}.SlimeRain_Commentary";
        public static string ChimeResponse => $"{PREFIX}.ChimeResponse";
        public static string ChimeResponse_Merchant => $"{PREFIX}.ChimeResponse_Merchant";
        public static string SlimeRainWarning => $"{PREFIX}.SlimeRain_Warning";
        public static string KSEncounter => $"{PREFIX}.KS_Encounter";
        public static string KingSlimeDefeat => $"{PREFIX}.KingSlimeDefeat";
    }

    public static string Custom(string category, string sequence) => $"{category}.{sequence}";
}