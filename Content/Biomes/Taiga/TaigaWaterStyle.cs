using ReLogic.Content;

namespace Reverie.Content.Biomes.Taiga;

	public class TaigaWaterStyle : ModWaterStyle
	{
    public override string Texture => NAME + "/Assets/Textures/Biomes/Taiga/TaigaWaterStyle";
    public override string BlockTexture => Texture + "_Block";
    public override string SlopeTexture => Texture + "_Slope";
    private Asset<Texture2D> rainTexture;
		public override void Load() {
			rainTexture = Mod.Assets.Request<Texture2D>("Assets/Textures/Biomes/Taiga/TaigaRain");
		}

		public override int ChooseWaterfallStyle() {
			return ModContent.GetInstance<TaigaWaterfallStyle>().Slot;
		}

		public override int GetSplashDust() {
			return DustID.Water_Snow;
		}

		public override int GetDropletGore() {
			return GoreID.WaterDripIce;
		}

		public override Color BiomeHairColor() {
			return new Color(88, 150, 112);
		}

		public override byte GetRainVariant() {
			return (byte)Main.rand.Next(3);
		}

		public override Asset<Texture2D> GetRainTexture() => rainTexture;
	}