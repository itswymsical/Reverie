/*
 * Copyright (C) 2024 Project Starlight River
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using Terraria.GameContent;
using Terraria.UI.Chat;
using static Reverie.Reverie;

namespace Reverie.Utilities;

internal static class DrawUtilities
{
    public static Vector2 GetMiddleBetween(this Vector2 start, Vector2 end) => (start + end) / 2;

    public static Vector2 TurnRight(this Vector2 vector) => new Vector2(-vector.Y, vector.X);

    public static Vector2 TurnLeft(this Vector2 vector) => new Vector2(vector.Y, -vector.X);

    public static Vector2 ToVector2(this Vector3 vector) => new Vector2(vector.X, vector.Y);

    public static Vector3 ToVector3(this Vector2 vector) => new Vector3(vector.X, vector.Y, 0f);

    public static Vector2 ToDrawPosition(this Vector2 vector) => vector - Main.screenPosition;

    public static Matrix DefaultEffectMatrix
    {
        get
        {
            var device = Main.instance.GraphicsDevice;

            var width = device.Viewport.Width > 0 ? 2f / device.Viewport.Width : 0;
            var height = device.Viewport.Height > 0 ? -2f / device.Viewport.Height : 0;

            return new Matrix
            {
                M11 = width,
                M22 = height,
                M33 = 1,
                M44 = 1,
                M41 = -1 - width / 2f,
                M42 = 1 - height / 2f
            };
        }
    }

    public static void DrawLine(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color? color = null, int lineWidth = 1)
    {
        var dist = end - start;

        var rotation = dist.ToRotation();

        var lineColor = color ?? Color.White;

        var destRect = new Rectangle((int)start.X, (int)start.Y, (int)dist.Length(), lineWidth);

        spriteBatch.Draw(TextureAssets.MagicPixel.Value, destRect, null, lineColor, rotation, default, SpriteEffects.None, 0);
    }

    public static void DrawTriangle(Vector2[] vertices, Color? color = null)
    {
        var device = Main.graphics.GraphicsDevice;

        var basicEffect = Main.dedServ ? null : new BasicEffect(device)
        {
            VertexColorEnabled = true,
            View = DefaultEffectMatrix
        };

        if (basicEffect is null)
        {
            return;
        }

        var points = new VertexPositionColor[3];

        for (var i = 0; i < 3; i++)
        {
            points[i] = new VertexPositionColor(vertices[i].ToVector3(), color ?? Color.White);
        }

        device.SetVertexBuffer(null);

        var vertexBuffer = new VertexBuffer(device, typeof(VertexPositionColor), 3, BufferUsage.WriteOnly);
        vertexBuffer.SetData(points);

        device.SetVertexBuffer(vertexBuffer);

        foreach (var pass in basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();

            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 3);
        }
    }

    public static void DrawTriangle(Texture2D texture, Vector2[] vertices, Vector2[] texCoords)
    {
        var device = Main.graphics.GraphicsDevice;

        var basicEffect = Main.dedServ ? null : new BasicEffect(device)
        {
            TextureEnabled = true,
            Texture = texture,
            View = DefaultEffectMatrix
        };

        if (basicEffect is null)
        {
            return;
        }

        var points = new VertexPositionTexture[3];

        for (var i = 0; i < 3; i++)
        {
            points[i] = new VertexPositionTexture(vertices[i].ToVector3(), texCoords[i]);
        }

        device.SetVertexBuffer(null);

        var vertexBuffer = new VertexBuffer(device, typeof(VertexPositionTexture), 3, BufferUsage.WriteOnly);
        vertexBuffer.SetData(points);

        device.SetVertexBuffer(vertexBuffer);

        foreach (var pass in basicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();

            device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
        }
    }

    public static void DrawPanel(SpriteBatch sb, int x, int y, int w, int h, Color c = default)
    {
        if (c == default)
            c = new Color(63, 65, 151, 255) * 0.785f;

        var value = ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/PortraitBox").Value;
        if (w < 20)
            w = 20;

        if (h < 20)
            h = 20;

        sb.Draw(value, new Rectangle(x, y, 10, 10), new Rectangle(0, 0, 10, 10), c);
        sb.Draw(value, new Rectangle(x + 10, y, w - 20, 10), new Rectangle(10, 0, 10, 10), c);
        sb.Draw(value, new Rectangle(x + w - 10, y, 10, 10), new Rectangle(value.Width - 10, 0, 10, 10), c);
        sb.Draw(value, new Rectangle(x, y + 10, 10, h - 20), new Rectangle(0, 10, 10, 10), c);
        sb.Draw(value, new Rectangle(x + 10, y + 10, w - 20, h - 20), new Rectangle(10, 10, 10, 10), c);
        sb.Draw(value, new Rectangle(x + w - 10, y + 10, 10, h - 20), new Rectangle(value.Width - 10, 10, 10, 10), c);
        sb.Draw(value, new Rectangle(x, y + h - 10, 10, 10), new Rectangle(0, value.Height - 10, 10, 10), c);
        sb.Draw(value, new Rectangle(x + 10, y + h - 10, w - 20, 10), new Rectangle(10, value.Height - 10, 10, 10), c);
        sb.Draw(value, new Rectangle(x + w - 10, y + h - 10, 10, 10), new Rectangle(value.Width - 10, value.Height - 10, 10, 10), c);
    }

    public static void DrawPanel(SpriteBatch sb, Rectangle R, Color c = default) => DrawPanel(sb, R.X, R.Y, R.Width, R.Height, c);

    public static void DrawText(SpriteBatch spriteBatch, Vector2 position, string text, Color? color = null, Color? shadowColor = null, float scale = 1f)
    {
        var font = FontAssets.DeathText.Value;

        var textSize = font.MeasureString(text) * scale;
        var textPosition = position - textSize / 2f;

        var drawTextColor = color ?? Color.White;
        var shadowTextColor = shadowColor ?? Color.Black;

        shadowTextColor.A = drawTextColor.A;

        ChatManager.DrawColorCodedStringShadow(spriteBatch, font, text, textPosition, shadowTextColor, 0f, default, new Vector2(scale));
        ChatManager.DrawColorCodedString(spriteBatch, font, text, textPosition, drawTextColor, 0f, default, new Vector2(scale));
    }

    public static void DrawText(SpriteBatch spriteBatch, Color color, string text, Vector2 position, float scale = 1f)
    {
        var font = FontAssets.DeathText.Value;

        var textSize = font.MeasureString(text) * scale;
        var textPosition = position - textSize / 2f;

        var shadowColor = Color.Black;
        shadowColor.A = color.A;

        ChatManager.DrawColorCodedStringShadow(spriteBatch, font, text, textPosition, shadowColor, 0f, default, Vector2.One * scale);
        ChatManager.DrawColorCodedString(spriteBatch, font, text, textPosition, color, 0f, default, Vector2.One * scale);
    }

    public static void DrawTextCollumn(SpriteBatch spriteBatch, Color color, string text, ref Vector2 position, float scale = 1f, float spacement = 5f)
    {
        var font = FontAssets.DeathText.Value;

        var textSize = font.MeasureString(text) * scale;
        var textPosition = position - textSize / 2f;
        position.Y += textSize.Y + spacement;

        // Maybe we could use DrawText here?

        var shadowColor = Color.Black;
        shadowColor.A = color.A;

        ChatManager.DrawColorCodedStringShadow(spriteBatch, font, text, textPosition, shadowColor, 0f, default, Vector2.One * scale);
        ChatManager.DrawColorCodedString(spriteBatch, font, text, textPosition, color, 0f, default, Vector2.One * scale);
    }

    public static T[] FastUnion<T>(this T[] front, T[] back)
    {
        var combined = new T[front.Length + back.Length];

        Array.Copy(front, combined, front.Length);
        Array.Copy(back, 0, combined, front.Length, back.Length);

        return combined;
    }
}
