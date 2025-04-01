namespace Reverie.Common.NPCs;

public abstract class WorldNPC : ModNPC
{
    public virtual int HairType { get; set; }
    public virtual Color HairColor { get; set; }
    public virtual Color SkinColor { get; set; }

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 19;
    }

    public override void SetDefaults()
    {
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.width = 32;
        NPC.height = 46;
        NPC.aiStyle = 7;
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
    }
    public override void FindFrame(int frameHeight)
    {
        NPC.frame.Width = 40;
        var hieght = NPC.frame.Height = 56;
        if (NPC.velocity.Y != 0) // If in air (jumping)
        {
            NPC.frame.Y = 5 * hieght;
        }
        else if (NPC.velocity.X != 0) // If moving horizontally
        {
            // Increment frame counter
            NPC.frameCounter++;

            // Change frames every 5 ticks
            if (NPC.frameCounter >= 5)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += hieght;

                // Reset to first walking frame if we've gone past the last one
                if (NPC.frame.Y >= 19 * hieght)
                {
                    NPC.frame.Y = 6 * hieght;
                }
                // Initialize to first walking frame if we're not in walking frames
                else if (NPC.frame.Y < 6 * hieght)
                {
                    NPC.frame.Y = 6 * hieght;
                }
            }
        }
        else // If idle
        {
            NPC.frame.Y = 0;
            NPC.frameCounter = 0;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D legsTexture = ModContent.Request<Texture2D>(Texture + "_Legs").Value;
        Texture2D bodyTexture = ModContent.Request<Texture2D>(Texture + "_Body").Value;
        Texture2D armsTexture = ModContent.Request<Texture2D>(Texture + "_Arms").Value;
        Texture2D faceTexture = ModContent.Request<Texture2D>(Texture + "_Head").Value;
        Texture2D hair = Main.Assets.Request<Texture2D>($"Images/Player_Hair_{HairType}").Value;
        // Calculate draw position - don't add origin here
        Vector2 drawPos = NPC.position - screenPos;
        SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        Rectangle frame = NPC.frame;

        // Set origin to the center horizontally, but bottom vertically 
        // This is important for proper grounding
        Vector2 origin = new Vector2(frame.Width / 2f, frame.Height);

        // Draw each body part
        spriteBatch.Draw(
            legsTexture,
            drawPos + new Vector2(NPC.width / 2f, NPC.height),
            frame,
            SkinColor,
            NPC.rotation,
            origin,
            NPC.scale,
            effects,
            0f
        );

        // Same for other body parts
        spriteBatch.Draw(
            bodyTexture,
            drawPos + new Vector2(NPC.width / 2f, NPC.height),
            frame,
            SkinColor,
            NPC.rotation,
            origin,
            NPC.scale,
            effects,
            0f
        );

        spriteBatch.Draw(
            armsTexture,
            drawPos + new Vector2(NPC.width / 2f, NPC.height),
            frame,
            SkinColor,
            NPC.rotation,
            origin,
            NPC.scale,
            effects,
            0f
        );

        spriteBatch.Draw(
            faceTexture,
            drawPos + new Vector2(NPC.width / 2f, NPC.height),
            frame,
            SkinColor,
            NPC.rotation,
            origin,
            NPC.scale,
            effects,
            0f
        );

        spriteBatch.Draw(
            hair,
            drawPos + new Vector2(NPC.width / 2f, NPC.height),
            frame,
            HairColor,
            NPC.rotation,
            origin,
            NPC.scale,
            effects,
            0f
        );

        return false;
    }
}