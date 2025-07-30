namespace Reverie.Utilities.Extensions;


public static class NPCSequences
{
    /// <summary>
    /// NPC thinks, then offers an item
    /// </summary>
    public static void ThinkThenOffer(this NPC npc, int thinkDuration = 90, int offerDuration = 300)
    {
        npc.PerformActionSequence(
            (2f, thinkDuration),  // Blink/think
            (9f, offerDuration)   // Hold out item
        );
    }

    /// <summary>
    /// NPC greets enthusiastically 
    /// </summary>
    public static void EnthusiasticGreeting(this NPC npc)
    {
        npc.PerformActionSequence(
            (19f, 60),  // Talk
            (6f, 120),  // Celebrate
            (19f, 180)  // Talk more
        );
    }

    /// <summary>
    /// NPC has a realization moment
    /// </summary>
    public static void HasRealization(this NPC npc)
    {
        npc.PerformActionSequence(
            (2f, 30),   // Blink
            (2f, 60),   // Blink longer (thinking)
            (19f, 120), // Excited talking
            (6f, 90)    // Celebrate realization
        );
    }

    /// <summary>
    /// Merchant-style interaction: think about offer, present item, celebrate sale
    /// </summary>
    public static void MerchantInteraction(this NPC npc)
    {
        npc.PerformActionSequence(
            (2f, 60),   // Consider offer
            (3f, 180),  // Talk while showing item
            (6f, 120)   // Happy about transaction
        );
    }
}