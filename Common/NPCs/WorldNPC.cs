using Terraria.DataStructures;
using Terraria.GameContent;

public class ClothingID
{
    public static int None = 0;
    public static int Basic = 1;
}

public abstract class WorldNPC : ModNPC
{
    public virtual int HairType { get; set; }
    public virtual Color HairColor { get; set; }
    public virtual Color SkinColor { get; set; }
    public override string Texture => INVIS;
    public virtual int ClothingType { get; set; } = ClothingID.Basic;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 24;
    }

    public override void SetDefaults()
    {
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.width = 28;
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
        const int FRAME_WIDTH = 42;
        const int FRAME_HEIGHT = 58;

        NPC.frame.Width = FRAME_WIDTH;
        NPC.frame.Height = FRAME_HEIGHT;

        if (NPC.velocity.Y != 0) // jumping
        {
            // Row 0, Column 5 = Jump frame
            NPC.frame.X = 5 * FRAME_WIDTH;
            NPC.frame.Y = 0;
        }
        else if (NPC.velocity.X != 0) // moving - walking animation
        {
            if (NPC.velocity.X > 0)
                NPC.spriteDirection = 1;
            else if (NPC.velocity.X < 0)
                NPC.spriteDirection = -1;

            NPC.frameCounter++;

            if (NPC.frameCounter >= 3)
            {
                NPC.frameCounter = 0;

                int walkFrameIndex = (NPC.frame.Y / FRAME_HEIGHT) * 8 + (NPC.frame.X / FRAME_WIDTH);

                if (walkFrameIndex < 8 || walkFrameIndex > 20)
                {
                    walkFrameIndex = 8;
                }
                else
                {
                    walkFrameIndex++;
                    // Skip column 7 (special animation) and wrap around
                    if (walkFrameIndex == 15)
                        walkFrameIndex = 16;
                    else if (walkFrameIndex > 22)
                        walkFrameIndex = 8;
                    else if (walkFrameIndex % 8 == 7)
                        walkFrameIndex++;
                }

                int row = walkFrameIndex / 8;
                int col = walkFrameIndex % 8;

                NPC.frame.X = col * FRAME_WIDTH;
                NPC.frame.Y = row * FRAME_HEIGHT;
            }
        }
        else // idle
        {
            // Row 0, Column 0 = Idle frame
            NPC.frame.X = 0;
            NPC.frame.Y = 0;
            NPC.frameCounter = 0;
        }
    }

    private void DrawClothing(SpriteBatch spriteBatch, Vector2 drawPos, Rectangle sourceFrame, Vector2 origin, SpriteEffects effects)
    {
        if (ClothingType == ClothingID.None)
            return;

        Texture2D clothingTexture = ModContent.Request<Texture2D>($"Reverie/Assets/Textures/NPCs/WorldNPCs/Clothing_{ClothingType}").Value;
        Color lightColor = Lighting.GetColor((int)(NPC.position.X + NPC.width * 0.5f) / 16, (int)(NPC.position.Y + NPC.height * 0.5f) / 16);

        spriteBatch.Draw(
            clothingTexture,
            drawPos + new Vector2(NPC.width / 2f, NPC.height),
            sourceFrame,
            lightColor,
            NPC.rotation,
            origin,
            NPC.scale,
            effects,
            0f
        );
    }

    public void DrawParts(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D npcTexture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/NPCs/WorldNPCs/Civilian_Terrarian").Value;
        Texture2D hairTexture = Main.Assets.Request<Texture2D>($"Images/Player_Hair_{HairType}").Value;

        const int FRAME_WIDTH = 42;
        const int FRAME_HEIGHT = 58;

        int currentCol = NPC.frame.X / FRAME_WIDTH;
        int currentRow = NPC.frame.Y / FRAME_HEIGHT;

        Vector2 drawPos = (NPC.position + new Vector2(0, 4)) - screenPos;
        Vector2 origin = new Vector2(FRAME_WIDTH / 2f, FRAME_HEIGHT);
        SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        Rectangle sourceFrame = new Rectangle(
            currentCol * FRAME_WIDTH,
            currentRow * FRAME_HEIGHT,
            FRAME_WIDTH,
            FRAME_HEIGHT
        );

        int hairFrameIndex = 0; // Default to idle hair frame

        if ((currentRow == 1 || currentRow == 2) && currentCol <= 6) // Walk cycle
        {
            switch (currentRow)
            {
                case 1: // cycle 1
                    switch (currentCol)
                    {
                        case 0: hairFrameIndex = 7; break;  // Step up frame
                        case 1: hairFrameIndex = 8; break;  // Step up frame
                        case 2: hairFrameIndex = 9; break;  // Step up frame
                        case 3: hairFrameIndex = 9; break;  // Step up frame
                        default: hairFrameIndex = 0; break; //use idle hair
                    }
                    break;
                case 2: //cycle 2
                    switch (currentCol)
                    {
                        case 0: hairFrameIndex = 7; break;  // Step up frame
                        case 1: hairFrameIndex = 8; break;  // Step up frame
                        case 2: hairFrameIndex = 9; break;  // Step up frame
                        case 3: hairFrameIndex = 9; break;  // Step up frame
                        default: hairFrameIndex = 0; break; //use idle hair
                    }
                    break;
            }
        }

        Rectangle hairFrame = new Rectangle(
            0,
            hairFrameIndex * (hairTexture.Height / 14),
            FRAME_WIDTH,
            hairTexture.Height / 14
        );

        Vector2 hairOrigin = new Vector2(hairFrame.Width / 2f, hairFrame.Height);

        Color hairColor = Lighting.GetColor((int)(NPC.position.X + NPC.width * 0.5f) / 16, (int)(NPC.position.Y + NPC.height * 0.5f) / 16, HairColor);
        Color skinColor = Lighting.GetColor((int)(NPC.position.X + NPC.width * 0.5f) / 16, (int)(NPC.position.Y + NPC.height * 0.5f) / 16, SkinColor);

        spriteBatch.Draw(
            npcTexture,
            drawPos + new Vector2(NPC.width / 2f, NPC.height),
            sourceFrame,
            skinColor,
            NPC.rotation,
            origin,
            NPC.scale,
            effects,
            0f
        );

        DrawClothing(spriteBatch, drawPos, sourceFrame, origin, effects);

        spriteBatch.Draw(
            hairTexture,
            drawPos + new Vector2((NPC.width / 2f) + (NPC.spriteDirection * 2), NPC.height - 2),
            hairFrame,
            hairColor,
            NPC.rotation,
            hairOrigin,
            NPC.scale,
            effects,
            0f
        );
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        DrawParts(spriteBatch, screenPos, drawColor);
        return false;
    }
}
