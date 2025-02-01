namespace Reverie.Utilities;

/// <summary>
///     Provides utility methods for input handling.
/// </summary>
public static class InputUtils
{
    public static Vector2 CursorPosition => Main.SmartCursorWanted ? new Vector2(Main.SmartCursorX, Main.SmartCursorY) : Main.MouseWorld;
}