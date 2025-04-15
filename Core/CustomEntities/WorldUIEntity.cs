namespace Reverie.Core.CustomEntities;

/// <summary>
/// A world entity that acts as a UI element, allowing for both world positioning and UI interactions
/// </summary>
public class WorldUIEntity
{
    #region Properties
    public Vector2 WorldPosition { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    private Entity trackingEntity;
    private Vector2 trackingOffset;
    private bool isTracking => trackingEntity != null && trackingEntity.active;

    public bool IsHovering { get; private set; }
    public bool WasHovering { get; private set; }
    public bool JustHovered => IsHovering && !WasHovering;
    public bool JustStoppedHovering => !IsHovering && WasHovering;
    public bool IsVisible { get; set; } = true;

    public float AnimationTimer { get; private set; } = 0f;
    public Vector2 Offset { get; set; } = Vector2.Zero;

    public event Action OnClick;
    #endregion

    public delegate void DrawDelegate(SpriteBatch spriteBatch, Vector2 screenPos, float opacity);
    public DrawDelegate CustomDraw;

    public WorldUIEntity(Vector2 worldPosition, int width, int height)
    {
        WorldPosition = worldPosition;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Sets this entity to track another entity's position
    /// </summary>
    public void TrackEntity(Entity entity, Vector2 offset)
    {
        trackingEntity = entity;
        trackingOffset = offset;

        if (entity != null && entity.active)
        {
            UpdateTrackingPosition();
        }
    }

    private void UpdateTrackingPosition()
    {
        if (trackingEntity is NPC npc)
        {
            WorldPosition = npc.Top + trackingOffset;
        }
        else
        {
            WorldPosition = trackingEntity.position + trackingOffset;
        }
    }

    public virtual void Update()
    {
        AnimationTimer += 0.1f;

        if (isTracking)
        {
            UpdateTrackingPosition();
        }

        WasHovering = IsHovering;
        IsHovering = CheckHovering();

        if (IsHovering && Main.mouseLeft && Main.mouseLeftRelease)
        {
            OnClick?.Invoke();
        }

        CustomUpdate();
    }

    /// <summary>
    /// Override in derived classes for custom update logic
    /// </summary>
    protected virtual void CustomUpdate() { }

    /// <summary>
    /// Checks if the mouse is hovering over this entity
    /// </summary>
    public bool CheckHovering()
    {
        if (!IsVisible)
            return false;

        var zoom = Main.GameViewMatrix.Zoom.Y;

        // Convert world position to screen position with proper zoom handling
        var screenPos = WorldToScreen(WorldPosition + Offset);

        var scaledWidth = (int)(Width * zoom);
        var scaledHeight = (int)(Height * zoom);

        return new Rectangle(
            (int)screenPos.X - scaledWidth / 2,
            (int)screenPos.Y - scaledHeight / 2,
            scaledWidth,
            scaledHeight
        ).Contains(Main.MouseScreen.ToPoint());
    }

    /// <summary>
    /// Draws the entity in the world
    /// </summary>
    public virtual void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible)
            return;

        // Get screen position with animation offset
        var screenPos = WorldToScreen(WorldPosition + Offset);

        // Call custom draw method if provided
        CustomDraw?.Invoke(spriteBatch, screenPos, 1f);
    }

    /// <summary>
    /// Converts world coordinates to screen coordinates
    /// </summary>
    public static Vector2 WorldToScreen(Vector2 worldPos)
    {
        return Vector2.Transform(worldPos - Main.screenPosition, Main.GameViewMatrix.TransformationMatrix);
    }

    /// <summary>
    /// Converts screen coordinates to world coordinates
    /// </summary>
    public static Vector2 ScreenToWorld(Vector2 screenPos)
    {
        return Vector2.Transform(screenPos, Matrix.Invert(Main.GameViewMatrix.TransformationMatrix)) + Main.screenPosition;
    }
}