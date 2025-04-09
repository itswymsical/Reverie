namespace Reverie.Core.CustomEntities;

/// <summary>
/// A world entity that acts as a UI element, allowing for both world positioning and UI interactions
/// </summary>
public class WorldUIEntity
{
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

    private Entity parentEntity;
    private Vector2 parentOffset;

    public event Action OnClick;

    public delegate void DrawDelegate(SpriteBatch spriteBatch, Vector2 screenPos, float opacity);
    public DrawDelegate CustomDraw;

    public WorldUIEntity(Vector2 worldPosition, int width, int height)
    {
        WorldPosition = worldPosition;
        Width = width;
        Height = height;
    }
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
            // Get the top of the NPC and add the offset
            WorldPosition = npc.Top + trackingOffset;
        }
        else
        {
            WorldPosition = trackingEntity.position + trackingOffset;
        }
    }

    public WorldUIEntity(Entity parent, Vector2 offset, int width, int height)
    {
        parentEntity = parent;
        parentOffset = offset;
        WorldPosition = parent.position + offset;
        Width = width;
        Height = height;
    }

    public virtual void Update()
    {
        AnimationTimer += 0.1f;

        if (parentEntity != null && parentEntity.active)
        {
            if (parentEntity is NPC npc)
            {
                WorldPosition = npc.Top + parentOffset;
            }
            else
            {
                WorldPosition = parentEntity.position + parentOffset;
            }
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

    public bool CheckHovering()
    {
        if (!IsVisible)
            return false;

        // Convert world position to screen position
        var screenPos = WorldToScreen(WorldPosition);

        // Check if mouse is within bounds
        return new Rectangle(
            (int)screenPos.X,
            (int)screenPos.Y,
            Width,
            Height
        ).Contains(Main.MouseScreen.ToPoint());
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        if (!IsVisible)
            return;

        var screenPos = WorldToScreen(WorldPosition);

        CustomDraw?.Invoke(spriteBatch, screenPos, 1f);
    }

    /// <summary>
    /// Converts world coordinates to screen coordinates
    /// </summary>
    public static Vector2 WorldToScreen(Vector2 worldPos)
    {
        return Vector2.Transform(worldPos - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix);
    }

    /// <summary>
    /// Converts screen coordinates to world coordinates
    /// </summary>
    public static Vector2 ScreenToWorld(Vector2 screenPos)
    {
        return Vector2.Transform(screenPos, Matrix.Invert(Main.GameViewMatrix.ZoomMatrix)) + Main.screenPosition;
    }
}