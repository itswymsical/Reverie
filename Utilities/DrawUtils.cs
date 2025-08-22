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

namespace Reverie.Utilities;

internal static class DrawUtils
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

    public static void DrawAdditive(Texture2D tex, Vector2 position, Color colour, float scale)
    {
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null, Main.GameViewMatrix.TransformationMatrix);

        Main.spriteBatch.Draw(tex, position, tex.Bounds, colour, 0f, tex.TextureCenter(), scale, SpriteEffects.None, 0f);

        Main.spriteBatch.RestartToDefault();
    }   

    /// <summary>
    /// Ends & restarts the spriteBatch with default vanilla parameters.
    /// </summary>
    public static void RestartToDefault(this SpriteBatch batch)
    {
        batch.End();
        batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None,
            Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }

    public static Vector2 TextureCenter(this Texture2D texture) => new Vector2(texture.Width / 2, texture.Height / 2);

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

    // Updated DrawPanel method in DrawUtils.cs

    // OPTION 1: 9-slice approach (use this if your texture has borders that shouldn't stretch)
    public static void DrawPanel(SpriteBatch sb, int x, int y, int w, int h, Color c = default)
    {
        if (c == default)
            c = new Color(63, 65, 151, 255) * 0.785f;

        var value = ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/PortraitBox").Value;

        // ADJUST THESE VALUES to match your sprite's actual border sizes!
        int borderLeft = 20;   // Measure the left border in your 480x106 sprite
        int borderRight = 20;  // Measure the right border in your 480x106 sprite  
        int borderTop = 30;    // Measure the top border in your 480x106 sprite
        int borderBottom = 30; // Measure the bottom border in your 480x106 sprite

        // Ensure minimum size
        int minWidth = borderLeft + borderRight;
        int minHeight = borderTop + borderBottom;

        if (w < minWidth) w = minWidth;
        if (h < minHeight) h = minHeight;

        // Calculate source texture regions
        int srcWidth = value.Width;
        int srcHeight = value.Height;
        int srcCenterWidth = srcWidth - borderLeft - borderRight;
        int srcCenterHeight = srcHeight - borderTop - borderBottom;

        // Draw the 9 sections of the panel

        // Top-left corner
        sb.Draw(value, new Rectangle(x, y, borderLeft, borderTop),
                new Rectangle(0, 0, borderLeft, borderTop), c);

        // Top edge (stretch horizontally)
        sb.Draw(value, new Rectangle(x + borderLeft, y, w - borderLeft - borderRight, borderTop),
                new Rectangle(borderLeft, 0, srcCenterWidth, borderTop), c);

        // Top-right corner
        sb.Draw(value, new Rectangle(x + w - borderRight, y, borderRight, borderTop),
                new Rectangle(srcWidth - borderRight, 0, borderRight, borderTop), c);

        // Left edge (stretch vertically)
        sb.Draw(value, new Rectangle(x, y + borderTop, borderLeft, h - borderTop - borderBottom),
                new Rectangle(0, borderTop, borderLeft, srcCenterHeight), c);

        // Center (stretch both ways)
        sb.Draw(value, new Rectangle(x + borderLeft, y + borderTop, w - borderLeft - borderRight, h - borderTop - borderBottom),
                new Rectangle(borderLeft, borderTop, srcCenterWidth, srcCenterHeight), c);

        // Right edge (stretch vertically)
        sb.Draw(value, new Rectangle(x + w - borderRight, y + borderTop, borderRight, h - borderTop - borderBottom),
                new Rectangle(srcWidth - borderRight, borderTop, borderRight, srcCenterHeight), c);

        // Bottom-left corner
        sb.Draw(value, new Rectangle(x, y + h - borderBottom, borderLeft, borderBottom),
                new Rectangle(0, srcHeight - borderBottom, borderLeft, borderBottom), c);

        // Bottom edge (stretch horizontally)
        sb.Draw(value, new Rectangle(x + borderLeft, y + h - borderBottom, w - borderLeft - borderRight, borderBottom),
                new Rectangle(borderLeft, srcHeight - borderBottom, srcCenterWidth, borderBottom), c);

        // Bottom-right corner
        sb.Draw(value, new Rectangle(x + w - borderRight, y + h - borderBottom, borderRight, borderBottom),
                new Rectangle(srcWidth - borderRight, srcHeight - borderBottom, borderRight, borderBottom), c);
    }

    // OPTION 2: Simple stretch approach (use this if your texture should just be stretched)
    public static void DrawPanelSimple(SpriteBatch sb, int x, int y, int w, int h, Color c = default)
    {
        if (c == default)
            c = new Color(63, 65, 151, 255) * 0.785f;

        var value = ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}Dialogue/PortraitBox").Value;

        // Simply stretch the entire texture to fit the desired rectangle
        sb.Draw(value, new Rectangle(x, y, w, h), null, c);
    }
    public static void DrawPanel(SpriteBatch sb, Rectangle R, Color c = default) => DrawPanel(sb, R.X, R.Y, R.Width, R.Height, c);
    public static void DrawPanelSimple(SpriteBatch sb, Rectangle R, Color c = default) => DrawPanelSimple(sb, R.X, R.Y, R.Width, R.Height, c);

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
