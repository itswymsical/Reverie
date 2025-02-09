﻿namespace Reverie.Core.Interfaces;

public interface ITrail
{
    bool IsActive { get; }
    void Update(Vector2 newPosition);
    void Draw(GraphicsDevice graphicsDevice, Matrix viewProjectionMatrix);
}