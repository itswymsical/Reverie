namespace Reverie
{
	/// <summary>
	/// Contains directories for all asset paths.
	/// </summary>
	internal static class Assets
	{
		public const string Path = $"{nameof(Reverie)}/Assets/";

        public const string Music = "Assets/Music/";

        public const string Backgrounds = "Assets/Backgrounds/";

        public const string Icon = $"{nameof(Reverie)}/icon";
		public const string Dusts = Path + "Dusts/";

        public const string PlaceholderTexture = Path + "UnrenderedTexture";

        internal class Terraria
        {
            public const string Directory = Path + "Terraria/";
            internal class Items
            {
                public const string Dir = Directory + "Items/";
                public const string Food = Dir + "Food/";
                public const string Frostbark = Dir + "Frostbark/";
                public const string Lodestone = Dir + "Lodestone/";
                public const string Weapons = Dir + "Weapons/";
                public const string Shovels = Dir + "Shovels/";
                public const string Mission = Dir + "Mission/";

                public const string Fungore = Dir + "Fungore/";
                public const string WoodenWarden = Dir + "WoodenWarden/";
            }

            internal class NPCs
            {
                public const string Path = Directory + "NPCs/";
                public const string BloodMoon = Path + "RedDesert/";
                public const string Canopy = Path + "Canopy/";
                public const string Corruption = Path + "Corruption/";
                public const string Crimson = Path + "Crimson/";
                public const string Cumulor = Path + "Cumulor/";
                public const string Eosforos = Path + "Eosforos/";
                public const string FoodLegion = Path + "FoodLegion/";
                public const string Fungore = Path + "Fungore/";
                public const string Underground = Path + "Emberite/";
                public const string Warden = Path + "Warden/";
                public const string WorldNPCs = Path + "WorldNPCs/";
            }

            
            internal class Projectiles
            {
                public const string Dir = Directory + "Projectiles/";
                public const string Frostbark = Dir + "Frostbark/";
                public const string Fungore = Dir + "Fungore/";
                public const string FoodLegion = Dir + "FoodLegion/";
                public const string WoodenWarden = Dir + "WoodenWarden/";
                public const string Lodestone = Dir + "Lodestone/";
            }
            internal class Tiles
            {
                public const string Dir = Directory + "Tiles/";

            }
        }

        internal class Archaea
        {
            public const string Directory = Path + "Archaea/";
            public const string Items = Directory + "Items/";
            public const string Weapons = Items + "Weapons/";

            public const string Projectiles = Directory + "Projectiles/";
            internal class NPCs
            {
                public const string Path = Directory + "NPCs/";
                public const string Surface = Path + "Surface/";
                public const string Emberite = Path + "Emberite/";
                public const string RedDesert = Path + "RedDesert/";
            }
            internal class Tiles
            {
                public const string Path = Directory + "Tiles/";
                public const string Emberite = Path + "Emberite/";
                public const string Projectiles = Path + "Projectiles/";
                public const string RedDesert = Path + "RedDesert/";              
            }
        }

        internal class Sylvanwalde
        {
            public const string Directory = Path + "Sylvanwalde/";
            public const string Items = Directory + "Items/";
            public const string Weapons = Items + "Weapons/";

            internal class NPCs
            {
                public const string Path = Directory + "NPCs/";
                public const string WorldNPCs = Path + "WorldNPCs/";
            }

            public const string Projectiles = Directory + "Projectiles/";
            internal class Tiles
            {
                public const string Path = Directory + "Tiles/";
                public const string DruidsGarden = Path + "DruidsGarden/";
                public const string Canopy = Path + "WoodlandCanopy/";
            }
        }

        internal class Seaforth
        {
            public const string Directory = Path + "Seaforth/";
            public const string Items = Directory + "Items/";
            public const string Weapons = Items + "Weapons/";

            internal class NPCs
            {
                public const string Path = Directory + "NPCs/";
                public const string WorldNPCs = Path + "WorldNPCs/";
            }
            public const string Projectiles = Directory + "Projectiles/";
            internal class Tiles
            {
                public const string Path = Directory + "Tiles/";
            }
        }

        internal class SFX
        {
            public const string Directory = Path + "SFX/";
            public const string Dialogue = Directory + "Dialogue/";
        }

        internal class VFX 
        {
            public const string Directory = Path + "VFX/";
        } 

        internal class UI
        {
            public const string Directory = Path + "UI/";
            public const string ClassSelection = Directory + "ClassSelection/";
            public const string DialogueUI = Directory + "DialogueUI/";
            public const string DialogueCharacters = DialogueUI + "Characters/";
            public const string ExperienceMeter = Directory + "ExperienceMeter/";
            public const string MagicMirror = Directory + "MagicMirror/";
            public const string MissionUI = Directory + "MissionUI/";
            public const string SkillTree = Directory + "SkillTree/";
        }

		internal class Buffs
		{
			public const string Directory = Path + "Buffs/";
			public const string Minions = Directory + "Minions/";
			public const string Potions = Directory + "Potions/";
			public const string Debuffs = Directory + "Debuffs/";

		}
    }
}
