using SubworldLibrary;
using Terraria.GameContent;
using Terraria.GameInput;

namespace Reverie.Common.Subworlds;

public abstract class Otherworld : Subworld
{
    private int currentFrame;
    private double frameTime;
    private const int totalFrames = 19;
    private const double timePerFrame = 40;
    private readonly float fadeInDuration = 12f;
    private double elapsedTime;

    public virtual Texture2D LoadingScreen { get; protected set; }
    public virtual Texture2D OtherworldTitle { get; protected set; }
    public virtual Texture2D LoadingIcon { get; protected set; }

    public override void SetStaticDefaults()
    {
        LoadingScreen = (Texture2D)ModContent.Request<Texture2D>($"{nameof(Reverie)}/Assets/Textures/Backgrounds/ChronicleBG");
        OtherworldTitle = TextureAssets.Logo.Value;
        LoadingIcon = TextureAssets.LoadingSunflower.Value;
    }

    public override void DrawSetup(GameTime gameTime)
    {
        PlayerInput.SetZoom_UI();

        Main.instance.GraphicsDevice.Clear(Color.Black);
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.UIScaleMatrix);
        DrawMenu(gameTime);

        Main.DrawCursor(Main.DrawThickCursor());
        Main.spriteBatch.End();
    }

    public override void DrawMenu(GameTime gameTime)
    {
        var opacity = MathHelper.Clamp((float)(elapsedTime / fadeInDuration), 0f, 1f);

        elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;
        frameTime += gameTime.ElapsedGameTime.TotalMilliseconds;
        if (frameTime >= timePerFrame)
        {
            frameTime -= timePerFrame;
            currentFrame = (currentFrame + 1) % totalFrames;
        }
        // Draw background
        if (LoadingScreen != null)
        {
            Main.spriteBatch.Draw(LoadingScreen, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * opacity);
            LoadingTooltips(Main.spriteBatch);
        }

        // Draw title
        if (OtherworldTitle != null)
        {
            Vector2 nameTexturePosition = new(10, 10);
            Main.spriteBatch.Draw(OtherworldTitle, nameTexturePosition, Color.White * opacity);
        }

        // Draw loading icon
        if (LoadingIcon != null)
        {
            var frameWidth = LoadingIcon.Width;
            var frameHeight = LoadingIcon.Height / totalFrames;
            Rectangle sourceRectangle = new(0, currentFrame * frameHeight, frameWidth, frameHeight);
            Vector2 iconPosition = new(Main.screenWidth - LoadingIcon.Width - 10, Main.screenHeight - frameHeight - 10);
            Main.spriteBatch.Draw(LoadingIcon, iconPosition, sourceRectangle, Color.White);
        }
    }

    public virtual void LoadingTooltips(SpriteBatch spriteBatch) { }
}