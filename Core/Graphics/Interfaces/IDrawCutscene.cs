namespace Reverie.Core.Graphics.Interfaces;

/// <summary>
/// Interface for cutscenes that need custom drawing logic.
/// </summary>
public interface IDrawCutscene
{
    void CustomDraw(SpriteBatch spriteBatch);
}
