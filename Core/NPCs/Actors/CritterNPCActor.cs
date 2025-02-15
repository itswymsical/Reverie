namespace Reverie.Core.NPCs.Actors;

public abstract class CritterNPCActor : ModNPC
{
    /// <summary>
    ///     Gets the maximum speed that the NPC can move at, in pixels per tick.
    /// </summary>
    public virtual float MaxSpeed { get; } = 1f;

    /// <summary>
    ///     Gets the time that the NPC should idle for, in ticks.
    /// </summary>
    public virtual float IdleTime { get; } = 300f;
    
    public override void AI()
    {
        var shouldWalk = NPC.ai[0] > IdleTime;

        if (NPC.direction == 0)
        {
            NPC.direction = Main.rand.NextBool() ? -1 : 1;
        }

        NPC.ai[0]++;

        if (NPC.ai[0] > IdleTime * 2)
        {
            if (Main.rand.NextBool(4))
            {
                NPC.direction = Main.rand.NextBool() ? -1 : 1;

                NPC.netUpdate = true;
            }

            NPC.ai[0] = 0;
        }

        if (shouldWalk)
        {
            if (NPC.velocity.X < -MaxSpeed || NPC.velocity.X > MaxSpeed)
            {
                if (NPC.velocity.Y == 0f)
                {
                    NPC.velocity *= 0.8f;
                }
            }
            else
            {
                if (NPC.velocity.X < MaxSpeed && NPC.direction == 1)
                {
                    NPC.velocity.X += 0.07f;
                }

                if (NPC.velocity.X > -MaxSpeed && NPC.direction == -1)
                {
                    NPC.velocity.X -= 0.07f;
                }

                NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -MaxSpeed, MaxSpeed);
            }
        }
        else
        {
            NPC.velocity.X *= 0.8f;
        }
        
        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
    }
}