using Terraria;
using Terraria.ID;
using Reverie.Core.Missions;
using Reverie.Core.Dialogue;

namespace Reverie.Content.Terraria.NPCs.WorldNPCs
{
    public class Stumpy : MissionNPC
    {
        public override string Texture => Assets.Terraria.NPCs.WorldNPCs + Name;
        public override string HeadTexture => Assets.Terraria.NPCs.WorldNPCs + Name + "_Head";
        public override void SetStaticDefaults()
        {
            //Main.npcFrameCount[Type] = 1;
            NPCID.Sets.HasNoPartyText[Type] = true;
            NPCID.Sets.NoTownNPCHappiness[Type] = true;

            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
            {
                Velocity = 2f,
                Direction = 1
            };

            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }
        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.width = 58;
            NPC.height = 76;

            NPC.damage = 0;
            NPC.defense = 15;
            NPC.lifeMax = 250;

            NPC.GivenName = "Stumpy";
        }
        public override void OnChatButtonClicked(bool firstButton, ref string shopName)
        {
            base.OnChatButtonClicked(firstButton, ref shopName);
            if (firstButton)
            {
                DialogueManager.Instance.StartDialogue(NPCDataManager.StumpyData, DialogueID.Mission_04_StumpyIntro);
            }
        }
        public override bool CanTownNPCSpawn(int numTownNPCs) => false;
    }
}