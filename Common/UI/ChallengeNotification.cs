using ReLogic.Content;
using Reverie.Common.Players;
using Reverie.Core.ChallengeSystem;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;

namespace Reverie.Common.UI;

public class ChallengeNotification : IInGameNotification
{
    public bool ShouldBeRemoved => timeLeft <= 0;

    private int timeLeft = 5 * 60;
    private Challenge currentChallenge;
    private string challengeName;
    private int currentProgress;
    private int maxProgress;
    private int currentTier;
    private int maxTier;

    private ChallengeCategory category;

    private bool[] progressMilestonesShown = new bool[4] { false, false, false, false }; // 33%, 50%, 75%, 100%

    public ChallengeNotification(Challenge challenge, string name, int current, int max, int tier, int maxTiers = 5)
    {
        currentChallenge = challenge;
        challengeName = name;
        currentProgress = current;
        maxProgress = max;
        currentTier = tier;
        maxTier = maxTiers;
    }

    public Asset<Texture2D> iconTexture = TextureAssets.Item[ItemID.Wood]; //Default Texture

    private float Scale
    {
        get
        {
            if (timeLeft < 30)
                return MathHelper.Lerp(0f, 1f, timeLeft / 30f);

            if (timeLeft > 285)
                return MathHelper.Lerp(1f, 0f, (timeLeft - 285) / 15f);

            return 1f;
        }
    }

    private float Opacity
    {
        get
        {
            if (Scale <= 0.5f)
                return 0f;

            return (Scale - 0.5f) / 0.5f;
        }
    }

    // Check if a progress milestone has been reached
    public bool CheckProgressMilestone(int currentProgress, int maxProgress)
    {
        float progress = (float)currentProgress / maxProgress;

        if (progress >= 0.33f && !progressMilestonesShown[0])
        {
            progressMilestonesShown[0] = true;
            return true;
        }
        else if (progress >= 0.5f && !progressMilestonesShown[1])
        {
            progressMilestonesShown[1] = true;
            return true;
        }
        else if (progress >= 0.75f && !progressMilestonesShown[2])
        {
            progressMilestonesShown[2] = true;
            return true;
        }
        else if (progress >= 1f && !progressMilestonesShown[3])
        {
            progressMilestonesShown[3] = true;
            return true;
        }

        return false;
    }


    public void Update()
    {
        timeLeft--;

        if (timeLeft < 0)
            timeLeft = 0;
    }

    public void DrawInGame(SpriteBatch spriteBatch, Vector2 bottomAnchorPosition)
    {
        if (Opacity <= 0f)
            return;

        GetTextureBasedOnCategory();

        var title = $"{challengeName} | {currentProgress}/{maxProgress} - Tier {currentTier}/{maxTier}";
        var progressPercent = maxProgress > 0 ? (100 * currentProgress / maxProgress) : 0;

        var effectiveScale = Scale * 1.1f;
        var size = (FontAssets.ItemStack.Value.MeasureString(title) + new Vector2(58f, 10f)) * effectiveScale;
        var panelSize = Utils.CenteredRectangle(bottomAnchorPosition + new Vector2(0f, (0f - size.Y) * 0.5f), size);

        var hovering = panelSize.Contains(Main.MouseScreen.ToPoint());

        Color bgColor = new Color(64, 109, 164);

        Utils.DrawInvBG(spriteBatch, panelSize, bgColor * (hovering ? 0.75f : 0.5f));

        var iconScale = effectiveScale * 0.7f;
        var vector = panelSize.Right() - Vector2.UnitX * effectiveScale * (12f + iconScale * iconTexture.Width());

        spriteBatch.Draw(iconTexture.Value, vector, null, Color.White * Opacity, 0f, new Vector2(0f, iconTexture.Width() / 2f), iconScale, SpriteEffects.None, 0f);
        Utils.DrawBorderString(color: new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor / 5, Main.mouseTextColor) * Opacity, sb: spriteBatch, text: title, pos: vector - Vector2.UnitX * 10f, scale: effectiveScale * 0.9f, anchorx: 1f, anchory: 0.4f);

        if (hovering)
            OnMouseOver();
    }

    public void GetTextureBasedOnCategory()
    {
        iconTexture = category switch
        {
            ChallengeCategory.Combat => TextureAssets.Item[ItemID.PlatinumBroadsword],
            ChallengeCategory.Exploration => TextureAssets.Item[ItemID.Compass],
            ChallengeCategory.Building => TextureAssets.Item[ItemID.Toolbox],
            ChallengeCategory.Fishing => TextureAssets.Item[ItemID.ReinforcedFishingPole],
            ChallengeCategory.Mining => TextureAssets.Item[ItemID.CopperPickaxe],
            ChallengeCategory.Crafting => TextureAssets.Item[ItemID.WorkBench],
            ChallengeCategory.Boss => TextureAssets.NpcHeadBoss[0],
            ChallengeCategory.Event => TextureAssets.Item[ItemID.GoblinBattleStandard],
            ChallengeCategory.Collection => TextureAssets.Item[ItemID.GoldChest],
            ChallengeCategory.Quest => TextureAssets.Item[ItemID.LuckyCoin],
            _ => TextureAssets.Item[ItemID.Book],
        };
    }

    private void OnMouseOver()
    {
        if (PlayerInput.IgnoreMouseInterface)
            return;

        Main.LocalPlayer.mouseInterface = true;

        if (!Main.mouseLeft || !Main.mouseLeftRelease)
            return;

        Main.mouseLeftRelease = false;

        if (timeLeft > 30)
        {
            timeLeft = 30;
        }
    }

    public void PushAnchor(ref Vector2 positionAnchorBottom) => positionAnchorBottom.Y -= 50f * Opacity;
}
