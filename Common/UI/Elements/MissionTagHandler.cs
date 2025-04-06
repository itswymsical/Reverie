using Terraria.Audio;
using Terraria.UI.Chat;

namespace Reverie.Common.UI.Elements;

public class MissionTagHandler : ITagHandler
{
    public TextSnippet Parse(string text, Color baseColor = default, string options = null)
    {
        return new MissionSnippet(text, baseColor);
    }

    public class MissionSnippet : TextSnippet
    {
        private bool _hovering;
        private bool _prevHover;

        public MissionSnippet(string text, Color colour) : base(text, colour)
        {
            CheckForHover = true;
        }

        public void PostUpdate()
        {
            _prevHover = _hovering;
            _hovering = false;
        }

        public override void OnHover()
        {
            _hovering = true;
            Main.LocalPlayer.mouseInterface = true;
            if (!_prevHover)
            {
                SoundEngine.PlaySound(SoundID.MenuTick);

                _hovering = true;
            }

            if (string.IsNullOrEmpty(Main.hoverItemName))
            {
                Main.instance.MouseText("Missions [!]");
                Main.mouseText = true;
            }
        }

        public override void OnClick()
        {
            Main.NewText($"The button was clicked. Comments, Questions, Concerns?");
        }
    }
}