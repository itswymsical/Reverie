using ReLogic.Content;
using Reverie.Common.Players;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;

namespace Reverie.Common.UI.LevelUI
{
    public class LevelNotification : IInGameNotification
	{
		public bool ShouldBeRemoved => timeLeft <= 0;

		private int timeLeft = 5 * 60;

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
			
			var player = Main.LocalPlayer;
			var modPlayer = player.GetModPlayer<ExperiencePlayer>();
			GetTextureBasedOnLevel(player);

			var title = $"{player.name} Reached Level {modPlayer.experienceLevel}!" +
				$"\nSkill Points Available {modPlayer.skillPoints}";

			var effectiveScale = Scale * 1.1f;
			var size = (FontAssets.ItemStack.Value.MeasureString(title) + new Vector2(58f, 10f)) * effectiveScale;
			var panelSize = Utils.CenteredRectangle(bottomAnchorPosition + new Vector2(0f, (0f - size.Y) * 0.5f), size);

			var hovering = panelSize.Contains(Main.MouseScreen.ToPoint());

			Utils.DrawInvBG(spriteBatch, panelSize, new Color(64, 109, 164) * (hovering ? 0.75f : 0.5f));
			var iconScale = effectiveScale * 0.7f;
			var vector = panelSize.Right() - Vector2.UnitX * effectiveScale * (12f + iconScale * iconTexture.Width());
			spriteBatch.Draw(iconTexture.Value, vector, null, Color.White * Opacity, 0f, new Vector2(0f, iconTexture.Width() / 2f), iconScale, SpriteEffects.None, 0f);
			Utils.DrawBorderString(color: new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor / 5, Main.mouseTextColor) * Opacity, sb: spriteBatch, text: title, pos: vector - Vector2.UnitX * 10f, scale: effectiveScale * 0.9f, anchorx: 1f, anchory: 0.4f);

			if (hovering)
				OnMouseOver();
		}
		public void GetTextureBasedOnLevel(Player player)
		{
            var modPlayer = player.GetModPlayer<ExperiencePlayer>();
            if (modPlayer.experienceLevel <= 1)
            {
                iconTexture = TextureAssets.Item[ItemID.Wood];

            }
            else if (modPlayer.experienceLevel <= 2)
            {
                iconTexture = TextureAssets.Item[ItemID.StoneBlock];

            }
            else if (modPlayer.experienceLevel <= 3)
            {
                iconTexture = TextureAssets.Item[ItemID.IronBar];

            }
            else if (modPlayer.experienceLevel <= 5)
            {
                iconTexture = TextureAssets.Item[ItemID.SilverBar];

            }
            else if (modPlayer.experienceLevel <= 10)
            {
                iconTexture = TextureAssets.Item[ItemID.GoldBar];

            }
            else if (modPlayer.experienceLevel <= 15)
            {
                iconTexture = TextureAssets.Item[ItemID.DemoniteBar];
            }
            else if (modPlayer.experienceLevel <= 20)
            {
                iconTexture = TextureAssets.Item[ItemID.HellstoneBar];

            }
            else if (modPlayer.experienceLevel <= 25)
            {
                iconTexture = TextureAssets.Item[ItemID.CobaltBar];

            }
            else if (modPlayer.experienceLevel <= 30)
            {
                iconTexture = TextureAssets.Item[ItemID.MythrilBar];
            }
            else if (modPlayer.experienceLevel <= 35)
            {
                iconTexture = TextureAssets.Item[ItemID.AdamantiteBar];
            }
            else if (modPlayer.experienceLevel <= 40)
            {
                iconTexture = TextureAssets.Item[ItemID.ChlorophyteBar];
            }
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
}