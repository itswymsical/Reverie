using ReLogic.Content;

namespace Reverie.Content.Biomes.Canopy
{
	public class CanopyWaterStyle : ModWaterStyle
	{
        public override string Texture => NAME + "/Assets/Textures/Biomes/Canopy/CanopyWaterStyle";
        public override string BlockTexture => Texture + "_Block";
        public override string SlopeTexture => Texture + "_Slope";
        private Asset<Texture2D> rainTexture;
		public override void Load() {
			rainTexture = Mod.Assets.Request<Texture2D>("Assets/Textures/Biomes/Canopy/CanopyRain");
		}

		public override int ChooseWaterfallStyle() {
			return ModContent.GetInstance<CanopyWaterfallStyle>().Slot;
		}

		public override int GetSplashDust() {
			return DustID.Water_Jungle;
		}

		public override int GetDropletGore() {
			return GoreID.WaterDripJungle;
		}

		public override Color BiomeHairColor() {
			return Color.LimeGreen;
		}

		public override byte GetRainVariant() {
			return (byte)Main.rand.Next(3);
		}

		public override Asset<Texture2D> GetRainTexture() => rainTexture;
	}
}