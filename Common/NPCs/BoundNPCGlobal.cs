using Reverie.Core.Dialogue;
using Terraria.Audio;
using Terraria.DataStructures;

namespace Reverie.Common.NPCs;

/// <summary>
/// Used to handle the cutscenes for bound NPCs.
/// </summary>
public class BoundNPCGlobal : GlobalNPC
{
    public override bool InstancePerEntity => true;

    private const int CUTSCENE_DURATION = 8 * 60;
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.friendly && entity.aiStyle == 0;
    }

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        base.OnSpawn(npc, source);

        if (npc.type == NPCID.WebbedStylist && !NPC.savedStylist)
        {
            StartCutscene(npc);
        }
    }

    private static void StartCutscene(NPC npc)
    {
        Vector2 targetPosition = npc.Center;

        Systems.Camera.CameraSystem.DoPanAnimation(
            CUTSCENE_DURATION,
            targetPosition,
            Vector2.Zero
        );
    }

    public override void GetChat(NPC npc, ref string chat)
    {
        base.GetChat(npc, ref chat);

        if (npc.type == NPCID.WebbedStylist)
        {
            npc.Transform(NPCID.Stylist);
            NPC.savedStylist = true;
            for (int i = 0; i < 16; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(1.8f, 1.2f);
                Gore.NewGoreDirect(default, npc.position, speed, GoreID.Smoke1, Main.rand.NextFloat(0.3f, 0.7f));

                Dust webDust = Dust.NewDustDirect(
                    npc.position,
                    npc.width,
                    npc.height,
                    DustID.Web,
                    speed.X,
                    speed.Y,
                    0,
                    default,
                    Main.rand.NextFloat(1f, 2f)
                );
                webDust.noGravity = true;
                webDust.fadeIn = 1.2f;

                if (i % 5 == 0)
                {
                    Dust sparkle = Dust.NewDustDirect(
                        npc.position,
                        npc.width,
                        npc.height,
                        DustID.DirtSpray,
                        speed.X * 0.5f,
                        speed.Y * 0.5f,
                        0,
                        default,
                        Main.rand.NextFloat(0.8f, 1.5f)
                    );
                    sparkle.noGravity = true;
                }
            }

            SoundEngine.PlaySound(SoundID.Grass, npc.position);

            DialogueManager.Instance.StartDialogue(
                NPCManager.Default, DialogueKeys.Stylist.StylistRescue, lineCount: 3, zoomIn: false);
        }
    }
}
