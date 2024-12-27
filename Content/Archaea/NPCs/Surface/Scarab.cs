using Microsoft.Xna.Framework;
using Reverie.Helpers;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Archaea.NPCs.Surface
{
    public class Scarab : ModNPC
    {
        public override string Texture => Assets.Archaea.NPCs.Surface + Name;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 52;
            NPC.height = 30;
            NPC.damage = 22;
            NPC.defense = 16;
            NPC.lifeMax = 97;
            NPC.knockBackResist = .7f;
            NPC.HitSound = SoundID.NPCHit32;
            NPC.DeathSound = SoundID.NPCDeath36;
        }

        public override void FindFrame(int frameHeight)
        {
            int frameTimer = 5;
            int numFrames = 4;

            NPC.frameCounter++;
            if (NPC.frameCounter >= frameTimer)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;

                if (NPC.frame.Y >= frameHeight * numFrames)
                {
                    NPC.frame.Y = 0;
                }
            }
            NPC.spriteDirection = (NPC.direction == 1) ? 1 : -1;
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];

            NPC.GenericFighterAI();

            if (Vector2.Distance(NPC.Center, target.Center) >= 200f)
            {
                NPC.Transform(ModContent.NPCType<AirborneScarab>());
            }
        }
    }
    public class AirborneScarab : ModNPC
    {
        public override string Texture => Assets.Archaea.NPCs.Surface + Name;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.width = 62;
            NPC.height = 50;
            NPC.damage = 22;
            NPC.defense = 16;
            NPC.lifeMax = 97;
            NPC.knockBackResist = .7f;
            NPC.HitSound = SoundID.NPCHit32;
            NPC.DeathSound = SoundID.NPCDeath36;
        }
        public override void FindFrame(int frameHeight)
        {
            int frameTimer = 5;
            int numFrames = 4;
            NPC.frameCounter++;
            if (NPC.frameCounter >= frameTimer)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;

                if (NPC.frame.Y >= frameHeight * numFrames)
                {
                    NPC.frame.Y = 0;
                }
            }
            NPC.spriteDirection = (NPC.direction == 1) ? 1 : -1;
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];

            NPC.GenericFlyerAI(maxSpeedX: 6, accelerationX: 0.09f);

            if (Vector2.Distance(NPC.Center, target.Center) <= 200f)
            {
                NPC.Transform(ModContent.NPCType<Scarab>());
            }
        }
    }
}