using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Reverie.Content.Terraria.NPCs.WorldNPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Core.Dialogue
{
    public class NPCPortrait(Asset<Texture2D> texture, int frameCount)
    {
        public Asset<Texture2D> Texture { get; } = texture;
        public int FrameCount { get; } = frameCount;
        public int FrameWidth = 92;
        public int FrameHeight = 92;

        public Rectangle GetFrameRect(int frameIndex)
        {
            return new Rectangle(frameIndex * FrameWidth, 0, FrameWidth, FrameHeight);
        }
    }

    public class NPCData(NPCPortrait portrait, string npcName, int npcID, Color dialogueColor, SoundStyle characterSound)
    {
        public NPCPortrait Portrait { get; } = portrait;
        public string NpcName { get; } = npcName;
        public int NpcID { get; } = npcID;
        public Color DialogueColor { get; } = dialogueColor;
        public SoundStyle CharacterSound { get; } = characterSound;
        public Dictionary<DialogueID, DialogueSequence> DialogueSequences { get; } = [];

        public void AddDialogueSequence(DialogueID dialogueId, DialogueSequence sequence)
        {
            DialogueSequences[dialogueId] = sequence;
        }
        public string GetNpcGivenName(int npcID)
        {
            var thisNPC = NPC.FindFirstNPC(npcID);
            NPC npc = Main.npc.FirstOrDefault(n => n.active && n.TypeName == NpcName);
            return npc?.GivenOrTypeName ?? NpcName;
        }
    }

    public static class NPCDataManager
    {
        public static NPCData Default { get; private set; }
        public static NPCData GuideData { get; private set; }
        public static NPCData GoblinData { get; private set; }
        public static NPCData StumpyData { get; private set; }
        public static NPCData SophieData { get; private set; }
        public static NPCData CowboyData { get; private set; }

        public static void Initialize()
        {
            var basicPortrait = new NPCPortrait(
                ModContent.Request<Texture2D>($"{Assets.UI.DialogueUI}PortraitFrame"),
                frameCount: 1
            );

            Default = new NPCData(
                basicPortrait,
                "",
                -1,
                Color.LightBlue,
                SoundID.MenuOpen
            );

            var guidePortrait = new NPCPortrait(
                ModContent.Request<Texture2D>($"{Assets.UI.DialogueCharacters}Guide"),
                frameCount: 2
            );

            GuideData = new NPCData(
                guidePortrait,
                "Guide",
                NPCID.Guide,
                new Color(64, 109, 164),
                SoundID.MenuOpen
            );

            var goblinPortrait = new NPCPortrait(
            ModContent.Request<Texture2D>($"{Assets.UI.DialogueCharacters}Goblin"),
            frameCount: 2
            );

            GoblinData = new NPCData(
                goblinPortrait,
                "Goblin Tinkerer",
                NPCID.GoblinTinkerer,
                Color.LightSlateGray,
                new SoundStyle($"{Assets.SFX.Dialogue}Goblin")
                {
                    MaxInstances = 1,
                    Volume = 0.8f,
                    PitchVariance = 0f,
                    PlayOnlyIfFocused = true
                });

            var stumpyPortrait = new NPCPortrait(
            ModContent.Request<Texture2D>($"{Assets.UI.DialogueCharacters}Unknown"),
            frameCount: 1
            );

            StumpyData = new NPCData(
                stumpyPortrait,
                "Stumpy",
                ModContent.NPCType<Stumpy>(),
                Color.LightGreen,
                new SoundStyle($"{Assets.SFX.Dialogue}Fungore")
                {
                    MaxInstances = 1,
                    Volume = 0.8f,
                    PitchVariance = 0f,
                    Pitch = 0.7f,
                    PlayOnlyIfFocused = true
                });

            var sophiePortrait = new NPCPortrait(
            ModContent.Request<Texture2D>($"{Assets.UI.DialogueCharacters}Sophie"),
            frameCount: 2
            );

            SophieData = new NPCData(
                sophiePortrait,
                "Sophie",
                ModContent.NPCType<Sophie>(),
                Color.Violet,
                new SoundStyle($"{Assets.SFX.Dialogue}Sophie")
                {
                    MaxInstances = 1,
                    Volume = 0.8f,
                    PitchVariance = 0f,
                    PlayOnlyIfFocused = true
                });

            var cowboyPortrait = new NPCPortrait(
            ModContent.Request<Texture2D>($"{Assets.UI.DialogueCharacters}Cowboy"),
            frameCount: 2
            );

            CowboyData = new NPCData(
                cowboyPortrait,
                "Cowboy",
                NPCID.Frog, //placeholder lol
                Color.SaddleBrown,
                new SoundStyle($"{Assets.SFX.Dialogue}Gearhead")
                {
                    MaxInstances = 1,
                    Volume = 0.8f,
                    PitchVariance = 0f,
                    Pitch = -.4f,
                    PlayOnlyIfFocused = true
                });

            foreach (DialogueID dialogueId in Enum.GetValues(typeof(DialogueID)))
            {
                var dialogue = DialogueList.GetDialogueById(dialogueId);
                if (dialogue != null)
                {
                    Default.AddDialogueSequence(dialogueId, dialogue);
                    GuideData.AddDialogueSequence(dialogueId, dialogue);
                    GoblinData.AddDialogueSequence(dialogueId, dialogue);
                    StumpyData.AddDialogueSequence(dialogueId, dialogue);
                    SophieData.AddDialogueSequence(dialogueId, dialogue);
                    CowboyData.AddDialogueSequence(dialogueId, dialogue);
                }
            }

            DialogueManager.Instance.RegisterNPC("Default", Default);
            DialogueManager.Instance.RegisterNPC("Guide", GuideData);
            DialogueManager.Instance.RegisterNPC("Goblin", GoblinData);
            DialogueManager.Instance.RegisterNPC("Stumpy", StumpyData);
            DialogueManager.Instance.RegisterNPC("Sophie", SophieData);
            DialogueManager.Instance.RegisterNPC("Cowboy", CowboyData);
        }
    }
}