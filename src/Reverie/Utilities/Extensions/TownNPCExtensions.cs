
namespace Reverie.Utilities.Extensions;
/// <summary>
///     Provides <see cref="NPC"/> extension methods, specifically for Town NPCs.
/// </summary>
public static class TownNPCExtensions
{
    /// <summary>
    /// Makes a Town <see cref="NPC"/> perform rock, paper, scissors.
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public static bool ForceRockPaperScissors(this NPC npc) => npc.ai[0] == 16f;

    /// <summary>
    /// Makes a Town <see cref="NPC"/> do that talking bubble thingy. Specifically used for Dialogue Sequences.
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    //public static void ForceBubbleChatState(this NPC npc)
    //{
    //    static bool IsNPCInActiveDialogue(NPC npc)
    //    {
    //        var activeDialogue = DialogueManager.Instance.GetActiveDialogue();
    //        if (activeDialogue != null)
    //        {
    //            return activeDialogue.npcData.NpcID == npc.type;
    //        }
    //        return false;
    //    }

    //    if (!IsNPCInActiveDialogue(npc))
    //    {
    //        npc.immortal = false;
    //        return;
    //    }
    //    else
    //    {
    //        npc.ai[0] = 3f;
    //        npc.immortal = true;
    //        npc.velocity = Vector2.Zero;

    //        Player player = Main.player[Main.myPlayer];
    //        npc.direction = player.Center.X < npc.Center.X ? -1 : 1;
    //        npc.spriteDirection = npc.direction;
    //    }
    //}
}
