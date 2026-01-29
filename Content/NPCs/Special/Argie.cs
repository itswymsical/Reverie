using Reverie.Common.Systems;
using Reverie.Content.Cutscenes;
using Reverie.Core.Cinematics;
using Reverie.Core.NPCs.Actors;
using Terraria.DataStructures;
using Terraria.ModLoader.Utilities;

namespace Reverie.Content.NPCs.Special;

[AutoloadHead]
public class Argie : WorldNPCActor
{
    private const float APPROACH_DISTANCE = 200f;
    private bool hasTriggeredCutscene = false;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.npcFrameCount[Type] = 1;
    }

    public override void SetDefaults()
    {
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.immortal = true;
        NPC.width = 50;
        NPC.height = 74;
        NPC.aiStyle = 7;
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0f;
    }

    public override void OnSpawn(IEntitySource source)
    {
        base.OnSpawn(source);
        hasTriggeredCutscene = DownedSystem.argieCutscene;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        return NPC.AnyNPCs(Type) ? 0 : SpawnCondition.UndergroundMushroom.Chance * 0.5f;
    }

    public override void AI()
    {
        base.AI();

        if (!hasTriggeredCutscene && !DownedSystem.argieCutscene)
        {
            Player closestPlayer = Main.player[Player.FindClosest(NPC.position, NPC.width, NPC.height)];

            if (closestPlayer.active && Vector2.Distance(NPC.Center, closestPlayer.Center) <= APPROACH_DISTANCE)
            {
                TriggerIntroCutscene();
            }
        }
    }

    private void TriggerIntroCutscene()
    {
        hasTriggeredCutscene = true;
    }

    public override string GetChat()
    {
        if (!DownedSystem.argieCutscene)
        {
            return "...";
        }

        return Main.rand.Next() switch
        {
            _ => "Mycelia is my brand! Could you lend me a hand? Perhaps two...?",
        };
    }
}