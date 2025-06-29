using Reverie.Core.Dialogue;
using System.Collections.Generic;
using Terraria.UI;

namespace Reverie.Content.Items.Debugging;

// Add this to your debug item to trace exactly where the freeze happens

public class FreezeTrackingDialogueDebug : ModItem
{
    public override string Texture => PLACEHOLDER;

    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 20;
        Item.value = Item.buyPrice(0);
        Item.rare = ItemRarityID.Quest;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            TrackedDialogueStart();
        }
        return true;
    }

    private void TrackedDialogueStart()
    {
        try
        {
            Main.NewText("[STEP 1] Starting tracked dialogue", Color.Green);

            // Test localization access first
            Main.NewText("[STEP 2] Testing localization access", Color.Green);
            var testLoc = Reverie.Instance.GetLocalization("DialogueLibrary.AFallingStar.CrashLanding.Line1.Text");
            Main.NewText($"[STEP 3] Localization result: {testLoc.Value[..Math.Min(20, testLoc.Value.Length)]}...", Color.Green);

            // Test DialogueBuilder next
            Main.NewText("[STEP 4] Testing DialogueBuilder", Color.Green);
            var dialogueData = DialogueBuilder.BuildLineFromLocalization("AFallingStar.CrashLanding", "Line1");

            if (dialogueData == null)
            {
                Main.NewText("[ERROR] DialogueBuilder returned null", Color.Red);
                return;
            }

            Main.NewText($"[STEP 5] Built dialogue: '{dialogueData.PlainText[..Math.Min(15, dialogueData.PlainText.Length)]}...'", Color.Green);
            Main.NewText($"[STEP 6] Speaker: {dialogueData.Speaker}, Color: {dialogueData.SpeakerColor}", Color.Green);

            // Test DialogueBox creation
            Main.NewText("[STEP 7] Creating DialogueBox", Color.Green);
            var dialogueList = new List<DialogueData> { dialogueData };
            var dialogueBox = DialogueBox.Create(dialogueList, false);

            if (dialogueBox == null)
            {
                Main.NewText("[ERROR] DialogueBox.Create returned null", Color.Red);
                return;
            }

            Main.NewText("[STEP 8] DialogueBox created successfully", Color.Green);

            // Test adding to notifications
            Main.NewText("[STEP 9] Adding to InGameNotificationsTracker", Color.Green);
            InGameNotificationsTracker.AddNotification(dialogueBox);

            Main.NewText("[SUCCESS] Dialogue should now be visible!", Color.Lime);
        }
        catch (Exception ex)
        {
            Main.NewText($"[FREEZE POINT] Exception: {ex.Message}", Color.Red);
            Main.NewText($"[STACK TRACE] {ex.StackTrace[..Math.Min(100, ex.StackTrace.Length)]}...", Color.Orange);
        }
    }
}