using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Core.Dialogue
{
    public static class DialogueList
    {
        private static Dictionary<DialogueID, DialogueSequence> _dialogues;

        static DialogueList() => Initialize();

        private static void Initialize()
        {
            var builder = new DialogueBuilder();
            _dialogues = new Dictionary<DialogueID, DialogueSequence>
            {
                [DialogueID.CrashLanding_Intro] = DialogueBuilder.Build("CrashLanding", "Intro", 1, 
                modifications: [
                    (line: 1, delay: 1, emote: 0),
                ]),

                [DialogueID.CrashLanding_SettlingIn] = DialogueBuilder.Build("CrashLanding", "SettlingIn", 4),

                [DialogueID.CrashLanding_GatheringResources] = DialogueBuilder.Build("CrashLanding", "GatheringResources", 2),
                [DialogueID.CrashLanding_FixHouse] = DialogueBuilder.Build("CrashLanding", "FixHouse", 3),
                [DialogueID.CrashLanding_ArmYourself] = DialogueBuilder.Build("CrashLanding", "ArmYourself", 2),
                [DialogueID.CrashLanding_Crafting] = DialogueBuilder.Build("CrashLanding", "Crafting", 2),
            };
        }

        public static DialogueSequence GetDialogueById(DialogueID id) 
        => _dialogues.TryGetValue(id, out var dialogue) ? dialogue : null;

        public static IEnumerable<DialogueSequence> GetAllDialogues() 
        => _dialogues.Values;    
    }
}