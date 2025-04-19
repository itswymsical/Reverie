using Terraria.ModLoader.Config;
using Microsoft.Xna.Framework;

namespace Reverie.Common.Configs
{
    public class ExperienceMeterConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("ExperienceMeterColors")]
#pragma warning disable CS0618 // Type or member is obsolete
        [Label("ExperienceColors")]
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
        [Tooltip("Customize the RGB color of the Experience Meter")]
#pragma warning restore CS0618 // Type or member is obsolete
        public ColorSetting BarColor { get; set; }

        public ExperienceMeterConfig()
        {
            BarColor = new ColorSetting(255, 255, 255);
        }

        public class ColorSetting
        {
            public byte R { get; set; }
            public byte G { get; set; }
            public byte B { get; set; }

            public ColorSetting()
            {
                R = 255;
                G = 255;
                B = 255;
            }

            public ColorSetting(byte r, byte g, byte b)
            {
                R = r;
                G = g;
                B = b;
            }

            public static implicit operator Color(ColorSetting setting)
            {
                return new Color(setting.R, setting.G, setting.B);
            }

            public static implicit operator ColorSetting(Color color)
            {
                return new ColorSetting(color.R, color.G, color.B);
            }
        }
    }
}