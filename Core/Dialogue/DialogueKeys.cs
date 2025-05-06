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
        public static string CopperStandardMidway => $"{PREFIX}.CopperStandardMidway";
        public static string CopperCoinsInProgress => $"{PREFIX}.CopperCoinsInProgress";
        public static string CopperCoinsInProgress_Alt => $"{PREFIX}.CopperCoinsInProgress_Alt";
        public static string MineCopperInProgress => $"{PREFIX}.MineCopperInProgress";
        public static string MerchantIntro => $"{PREFIX}.MerchantIntro";

    }

    public static class Stylist
    {
        private const string PREFIX = "Stylist";

        public static string StylistRescue => $"{PREFIX}.StylistRescue";
    }

    public static class Demolitionist
    {
        private const string PREFIX = "Demolitionist";
        public static string DemolitionistIntro => $"{PREFIX}.DemolitionistIntro";
        public static string TorchGodStart => $"{PREFIX}.TorchGodStart";
        public static string TorchGodComplete => $"{PREFIX}.TorchGodComplete";
        public static string TorchPlacement_01 => $"{PREFIX}.TorchPlacement_01";
        public static string TorchPlacement_02 => $"{PREFIX}.TorchPlacement_01";
        public static string TorchPlacement_03 => $"{PREFIX}.TorchPlacement_01";
        public static string TorchPlacement_04 => $"{PREFIX}.TorchPlacement_01";
    }

    public static class Argie
    {
        private const string PREFIX = "Argie";

        public static string Introduction => $"{PREFIX}.Introduction";
        public static string BloomcapIntro => $"{PREFIX}.BloomcapIntro";
        public static string BloomcapCollected => $"{PREFIX}.BloomcapCollected";
        public static string BloomcapCollectedHalf => $"{PREFIX}.BloomcapCollectedHalf";
        public static string BloomcapCollectedAll => $"{PREFIX}.BloomcapCollectedAll";
        public static string BloomcapComplete => $"{PREFIX}.BloomcapComplete";
    }

    public static class Nurse
    {
        private const string PREFIX = "Nurse";
        public static string NurseIntro => $"{PREFIX}.NurseIntro";
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
        public static string Cutscene => $"{PREFIX}.Cutscene";
        public static string CrashLanding => $"{PREFIX}.CrashLanding";
        public static string GatheringResources => $"{PREFIX}.GatheringResources";
        public static string ArchiverChronicleIFound => $"{PREFIX}.ArchiverChronicleIFound";
        public static string GuideReadsChronicleI => $"{PREFIX}.GuideReadsChronicleI";
        public static string ChronicleDecoded => $"{PREFIX}.ChronicleDecoded";
        public static string WildlifeWoes => $"{PREFIX}.WildlifeWoes";
        public static string SlimeInfestation => $"{PREFIX}.SlimeInfestation";
        public static string SlimeInfestationCommentary => $"{PREFIX}.SlimeInfestation_Commentary";
        public static string SlimeRain => $"{PREFIX}.SlimeRain";
        public static string SlimeRainCommentary => $"{PREFIX}.SlimeRain_Commentary";
        public static string ChimeResponse => $"{PREFIX}.ChimeResponse";
        public static string SlimeRainWarning => $"{PREFIX}.SlimeRain_Warning";
        public static string KSEncounter => $"{PREFIX}.KS_Encounter";
        public static string KingSlimeDefeat => $"{PREFIX}.KingSlimeDefeat";
    }

    public static string Custom(string category, string sequence) => $"{category}.{sequence}";
}