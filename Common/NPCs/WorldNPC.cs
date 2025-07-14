using Terraria.GameContent;

namespace Reverie.Common.NPCs;

public abstract class WorldNPC : ModNPC
{
    public virtual int HairType { get; set; }
    public virtual Color HairColor { get; set; }
    public virtual Color SkinColor { get; set; }
    public virtual int ArmorType { get; set; } = -1;
    public virtual bool WearsHelmet { get; set; }
    public virtual bool WearsChestplate { get; set; }
    public virtual bool WearsLeggings { get; set; }

    public int SexType { get; set; } = 1; //default to male

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

    public override bool PreAI()
    {
        if (ArmorType == -1)
            return false;

        // armor stat implementation
        return true;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        DrawParts(spriteBatch, screenPos);
        DrawArmor(spriteBatch, screenPos);
        return false;
    }

    public void DrawParts(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        Texture2D legsTexture = ModContent.Request<Texture2D>(Texture + "_Legs").Value;
        Texture2D bodyTexture = ModContent.Request<Texture2D>(Texture + "_Body").Value;
        Texture2D armsTexture = ModContent.Request<Texture2D>(Texture + "_Arms").Value;
        Texture2D faceTexture = ModContent.Request<Texture2D>(Texture + "_Head").Value;
        Texture2D hair = Main.Assets.Request<Texture2D>($"Images/Player_Hair_{HairType}").Value;

        Vector2 drawPos = (NPC.position + new Vector2(0, 4)) - screenPos;
        Rectangle frame = NPC.frame;

        SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        int currentFrame = frame.Y / 56;
        int hairFrameIndex = currentFrame;

        if (currentFrame >= 6 && currentFrame <= 18)
        {
            hairFrameIndex = 0 + ((currentFrame - 6));
        }
        else if (currentFrame == 5)
            hairFrameIndex = 0;
        else
            hairFrameIndex = 0;

        Rectangle hairFrame = new Rectangle(
            frame.X,
            hairFrameIndex * (hair.Height / 14),
            frame.Width,
            hair.Height / 14
        );

        Vector2 origin = new Vector2(frame.Width / 2f, frame.Height);
        Vector2 hairOrigin = new Vector2(hairFrame.Width / 2f, hairFrame.Height);

        if (ArmorType !> -1)
        {
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
        }

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
            hairFrame,
            HairColor,
            NPC.rotation,
            hairOrigin,
            NPC.scale,
            effects,
            0f
        );
    }

    public void DrawArmor(SpriteBatch spriteBatch, Vector2 screenPos)
    {
        if (ArmorType == -1)
            return;

        // Calculate frame sizes
        int frameWidth = 40;
        int frameHeight = 56;

        // Get current frame index (0-18)
        int currentFrame = NPC.frame.Y / frameHeight;

        Vector2 drawPos = (NPC.position + new Vector2(0, 4)) - screenPos;
        Vector2 origin = new Vector2(frameWidth / 2f, frameHeight);
        SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        if (WearsChestplate)
        {
            Texture2D armorBody = Main.Assets.Request<Texture2D>($"Images/Armor/Armor_{ArmorType}").Value;

            int bodyFrameX = SexType == 1 ? 0 : frameWidth; // Male or Female
            int bodyFrameY = 0;

            int leftArmFrameX = 2 * frameWidth; // Idle arm position
            int leftArmFrameY = 0;
            int rightArmFrameX = 5 * frameWidth; // Idle arm position 
            int rightArmFrameY = 3 * frameHeight;

            if (NPC.velocity.Y != 0) // Jumping
            {
                // Jumping body frame
                bodyFrameX = SexType == 1 ? 0 : frameWidth; // Male or Female
                bodyFrameY = 2 * frameHeight;

                // Jumping arms
                leftArmFrameX = 3 * frameWidth; // Left arm jump
                leftArmFrameY = 0;
                rightArmFrameX = 2 * frameWidth;
                rightArmFrameY = 3 * frameHeight;
            }

            else if (NPC.velocity.X != 0) // Walking
            {
                // Body frame stays the same for walking

                // Calculate arm frame based on the walking animation frame
                int walkFrame = currentFrame - 6; // Walking frames start at 6

                if (walkFrame >= 0 && walkFrame <= 12) // Valid walking frame
                {
                    // Map the current walking frame (0-12) to arm frames (0-3)
                    int armIndex = walkFrame % 4;

                    // Left arm walking frames
                    leftArmFrameX = (3 + armIndex) * frameWidth;
                    leftArmFrameY = frameHeight;

                    // Right arm walking frames
                    rightArmFrameX = (3 + armIndex) * frameWidth;
                    rightArmFrameY = 3 * frameHeight;
                }
            }

            Rectangle bodyFrame = new Rectangle(bodyFrameX, bodyFrameY, frameWidth, frameHeight);
            Rectangle leftArmFrame = new Rectangle(leftArmFrameX, leftArmFrameY, frameWidth, frameHeight);
            Rectangle rightArmFrame = new Rectangle(rightArmFrameX, rightArmFrameY, frameWidth, frameHeight);

            Rectangle shoulderFrame = new Rectangle(0, frameHeight, frameWidth, frameHeight);
            spriteBatch.Draw(
                armorBody,
                drawPos + new Vector2(NPC.width / 2f, NPC.height),
                shoulderFrame,
                Color.White,
                NPC.rotation,
                origin,
                NPC.scale,
                effects,
                0f
            );

            // Draw body
            spriteBatch.Draw(
                armorBody,
                drawPos + new Vector2(NPC.width / 2f, NPC.height),
                bodyFrame,
                Color.White,
                NPC.rotation,
                origin,
                NPC.scale,
                effects,
                0f
            );

            // Draw left arm
            spriteBatch.Draw(
                armorBody,
                drawPos + new Vector2(NPC.width / 2f, NPC.height),
                leftArmFrame,
                Color.White,
                NPC.rotation,
                origin,
                NPC.scale,
                effects,
                0f
            );

            // Draw right arm
            spriteBatch.Draw(
                armorBody,
                drawPos + new Vector2(NPC.width / 2f, NPC.height),
                rightArmFrame,
                Color.White,
                NPC.rotation,
                origin,
                NPC.scale,
                effects,
                0f
            );
        }

        // Only draw helmet if the NPC wears one
        if (WearsHelmet)
        {
            Texture2D armorHead = Main.Assets.Request<Texture2D>($"Images/Armor_Head_{ArmorType}").Value;
            Rectangle headFrame = new Rectangle(0, NPC.frame.Y, frameWidth, frameHeight);

            spriteBatch.Draw(
                armorHead,
                drawPos + new Vector2(NPC.width / 2f, NPC.height),
                headFrame,
                Color.White,
                NPC.rotation,
                origin,
                NPC.scale,
                effects,
                0f
            );
        }

        // Only draw leggings if the NPC wears them
        if (WearsLeggings)
        {
            Texture2D armorLegs = Main.Assets.Request<Texture2D>($"Images/Armor_Legs_{ArmorType}").Value;
            Rectangle legsFrame = new Rectangle(0, NPC.frame.Y, frameWidth, frameHeight);

            spriteBatch.Draw(
                armorLegs,
                drawPos + new Vector2(NPC.width / 2f, NPC.height),
                legsFrame,
                Color.White,
                NPC.rotation,
                origin,
                NPC.scale,
                effects,
                0f
            );
        }
    }
}