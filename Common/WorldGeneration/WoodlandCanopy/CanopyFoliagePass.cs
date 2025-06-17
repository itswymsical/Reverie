﻿using Reverie.Content.Tiles.Canopy;
using Reverie.Content.Tiles.Canopy.Surface;
using Reverie.lib;
using Terraria.GameContent.Biomes;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.WorldGeneration.WoodlandCanopy;

public class CanopyFoliagePass : GenPass
{
    #region Fields
    private CanopyBounds _canopyBounds;
    private FastNoiseLite _decorationNoise;
    #endregion

    public CanopyFoliagePass() : base("Woodland Canopy Foliage", 160f)
    {
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = "Detecting Canopy boundaries...";

        _canopyBounds = DetectCanopyBounds();

        if (!_canopyBounds.IsValid)
        {
            progress.Message = "No Canopy biome detected - skipping foliage generation";
            return;
        }

        progress.Message = "Sprucing up the Woodland Canopy...";
        Intitialize_DecoNoise();
        DoGrassAndFoliage(progress);
    }

    private CanopyBounds DetectCanopyBounds()
    {
        var bounds = new CanopyBounds
        {
            MinX = Main.maxTilesX,
            MaxX = 0,
            SurfaceY = Main.maxTilesY,
            DepthY = 0
        };

        for (int x = 200; x < Main.maxTilesX - 200; x += 3)
        {
            for (int y = 50; y < Main.maxTilesY - 100; y += 2)
            {
                if (WorldGen.InWorld(x, y) && IsCanopyTile(x, y))
                {
                    bounds.MinX = Math.Min(bounds.MinX, x);
                    bounds.MaxX = Math.Max(bounds.MaxX, x);
                    bounds.SurfaceY = Math.Min(bounds.SurfaceY, y);
                    bounds.DepthY = Math.Max(bounds.DepthY, y);
                }
            }
        }

        if (bounds.IsValid)
        {
            bounds.MinX = Math.Max(bounds.MinX - 10, 200);
            bounds.MaxX = Math.Min(bounds.MaxX + 10, Main.maxTilesX - 200);
            bounds.SurfaceY = Math.Max(bounds.SurfaceY - 5, 50);
            bounds.DepthY = Math.Min(bounds.DepthY + 10, Main.maxTilesY - 100);
        }

        return bounds;
    }

    private bool IsCanopyTile(int x, int y)
    {
        if (!WorldGen.InWorld(x, y)) return false;

        Tile tile = Main.tile[x, y];
        return tile.HasTile && (tile.TileType == TileID.LivingWood ||
                               tile.TileType == (ushort)ModContent.TileType<OxisolTile>());
    }

    private void Intitialize_DecoNoise()
    {
        _decorationNoise = new FastNoiseLite(WorldGen.genRand.Next());
        _decorationNoise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
        _decorationNoise.SetFrequency(0.05f);
    }

    private void SpreadGrass(int x, int y)
    {
        Tile tile = Framing.GetTileSafely(x, y);

        if (!tile.HasTile) return;

        // Surface grass: Convert OxisolTile to CanopyGrassTile if exposed to air
        if (tile.TileType == (ushort)ModContent.TileType<OxisolTile>())
        {
            if (IsExposedToAir(x, y))
            {
                TryPlaceCanopyGrass(x, y, (ushort)ModContent.TileType<CanopyGrassTile>());
            }
        }
        // Underground grass: Convert LivingWood to WoodgrassTile if exposed to air
        else if (tile.TileType == TileID.LivingWood)
        {
            if (IsExposedToAir(x, y))
            {
                TryPlaceCanopyGrass(x, y, (ushort)ModContent.TileType<WoodgrassTile>());
            }
        }
    }

    private bool IsExposedToAir(int x, int y)
    {
        // Check all 8 neighbors (including diagonals) for ANY air exposure
        for (int checkX = x - 1; checkX <= x + 1; checkX++)
        {
            for (int checkY = y - 1; checkY <= y + 1; checkY++)
            {
                if (checkX == x && checkY == y) continue; // Skip center tile
                if (!WorldGen.InWorld(checkX, checkY)) continue;

                Tile neighborTile = Framing.GetTileSafely(checkX, checkY);

                // If ANY neighbor is air, this tile should become grass
                if (!neighborTile.HasTile)
                {
                    return true;
                }
            }
        }

        return false; // No air exposure found
    }

    private void TryPlaceCanopyGrass(int tileX, int tileY, ushort grassType)
    {
        Tile tile = Framing.GetTileSafely(tileX, tileY);

        if (!tile.HasTile) return;

        tile.TileType = grassType;

        WorldGen.SquareTileFrame(tileX, tileY, true);

        if (Main.netMode == NetmodeID.Server)
        {
            NetMessage.SendTileSquare(-1, tileX, tileY, 1, TileChangeType.None);
        }
    }

    private void DoGrassAndFoliage(GenerationProgress progress)
    {
        Rectangle canopyRect = _canopyBounds.ToRectangle();
        int totalTiles = canopyRect.Width * canopyRect.Height;
        int processedTiles = 0;

        // First Pass: Spread grass on appropriate tiles (selective conversion)
        progress.Message = "Spreading canopy grass...";

        for (int x = canopyRect.Left; x <= canopyRect.Right; x++)
        {
            for (int y = canopyRect.Top; y <= canopyRect.Bottom; y++)
            {
                processedTiles++;
                if (processedTiles % 500 == 0)
                {
                    progress.Set((double)processedTiles / (totalTiles * 3));
                }

                if (!WorldGen.InWorld(x, y)) continue;
                SpreadGrass(x, y);
            }
        }

        // Second Pass: Cleanup isolated tiles
        progress.Message = "Cleaning up isolated tiles...";
        processedTiles = 0;

        for (int x = canopyRect.Left; x <= canopyRect.Right; x++)
        {
            for (int y = canopyRect.Top; y <= canopyRect.Bottom; y++)
            {
                processedTiles++;
                if (processedTiles % 500 == 0)
                {
                    progress.Set(0.33 + (double)processedTiles / (totalTiles * 3));
                }

                if (!WorldGen.InWorld(x, y)) continue;
                CleanupIsolatedTiles(x, y);
            }
        }

        // Third Pass: Add decorations
        progress.Message = "Adding foliage and decorations...";
        processedTiles = 0;

        for (int x = canopyRect.Left; x <= canopyRect.Right; x++)
        {
            for (int y = canopyRect.Top; y <= canopyRect.Bottom; y++)
            {
                processedTiles++;
                if (processedTiles % 500 == 0)
                {
                    progress.Set(0.66 + (double)processedTiles / (totalTiles * 3));
                }

                if (!WorldGen.InWorld(x, y)) continue;
                AddDecorations(x, y);
            }
        }
    }

    private void CleanupIsolatedTiles(int x, int y)
    {
        Tile tile = Framing.GetTileSafely(x, y);

        if (!tile.HasTile) return;

        bool isCanopyTile = tile.TileType == (ushort)ModContent.TileType<CanopyGrassTile>() ||
                           tile.TileType == (ushort)ModContent.TileType<WoodgrassTile>() ||
                           tile.TileType == (ushort)ModContent.TileType<OxisolTile>() ||
                           tile.TileType == TileID.LivingWood ||
                           tile.TileType == TileID.Dirt;

        if (!isCanopyTile) return;

        int solidNeighbors = 0;

        for (int checkX = x - 1; checkX <= x + 1; checkX++)
        {
            for (int checkY = y - 1; checkY <= y + 1; checkY++)
            {
                if (checkX == x && checkY == y) continue;

                if (!WorldGen.InWorld(checkX, checkY)) continue;

                Tile neighborTile = Framing.GetTileSafely(checkX, checkY);
                if (neighborTile.HasTile && Main.tileSolid[neighborTile.TileType])
                {
                    solidNeighbors++;
                }
            }
        }

        if (solidNeighbors < 3)
        {
            tile.HasTile = false;
            tile.WallType = 0;

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendTileSquare(-1, x, y, 1, TileChangeType.None);
            }
        }
    }

    private void AddDecorations(int x, int y)
    {
        Tile tile = Framing.GetTileSafely(x, y);
        Tile tileAbove = Framing.GetTileSafely(x, y - 1);
        Tile tileBelow = Framing.GetTileSafely(x, y + 1);

        if (!tile.HasTile) return;

        // Only decorate on grass tiles
        bool validTile = tile.TileType == (ushort)ModContent.TileType<CanopyGrassTile>() || tile.TileType == (ushort)ModContent.TileType<WoodgrassTile>();

        if (!validTile) return;

        // Get decoration noise for varied placement
        float decorationValue = _decorationNoise.GetNoise(x, y);

        // GRASS PLANTS (only on surface grass)
        if (validTile && decorationValue > 0.2f && !tileAbove.HasTile &&
            !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock)
        {
            if (WorldGen.genRand.NextBool(3))
            {
                WorldGen.PlaceTile(x, y - 1, (ushort)ModContent.TileType<CanopyFoliageTile>());
                tileAbove = Framing.GetTileSafely(x, y - 1); // Refresh reference
                tileAbove.TileFrameY = 0;
                tileAbove.TileFrameX = (short)(WorldGen.genRand.Next(10) * 18);
                WorldGen.SquareTileFrame(x, y - 1, true);

                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendTileSquare(-1, x, y - 1, 1, TileChangeType.None);
                }
            }
        }

        // VINES (only on surface grass)
        if (validTile && decorationValue > 0.4f && !tileBelow.HasTile &&
            !tile.BottomSlope && WorldGen.genRand.NextBool(8))
        {
            tileBelow.TileType = (ushort)ModContent.TileType<CanopyVineTile>();
            tileBelow.HasTile = true;
            WorldGen.SquareTileFrame(x, y + 1, true);

            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendTileSquare(-1, x, y + 1, 3, TileChangeType.None);
            }
        }

        // SAPLINGS (only on surface grass)
        if (validTile && decorationValue > 0.6f && !tileAbove.HasTile &&
            !tile.LeftSlope && !tile.RightSlope && !tile.IsHalfBlock &&
            WorldGen.genRand.NextBool(18))
        {
            if (tile.BlockType == BlockType.Solid)
            {
                WorldGen.PlaceTile(x, y - 1, TileID.Saplings, mute: true);
                WorldGen.GrowTree(x, y - 1);
            }
        }

        // DECORATIVE PILES (both grass types, but different chances)
        float pileChance = validTile ? 0.14f : 0.08f; // More piles on surface
        if (decorationValue > 0.7f && WorldGen.genRand.NextFloat() < pileChance)
        {
            if (HasSpaceForDecoration(x, y, 3, 2))
            {
                int decorationType = WorldGen.genRand.NextBool() ?
                    ModContent.TileType<CanopyLogTile>() :
                    ModContent.TileType<CanopyRockTile>();

                WorldGen.PlaceTile(x, y - 1, decorationType, mute: true);

                if (Main.netMode == NetmodeID.Server)
                {
                    NetMessage.SendTileSquare(-1, x, y - 1, 3, TileChangeType.None);
                }
            }
        }
    }

    private bool HasSpaceForDecoration(int centerX, int centerY, int width, int height)
    {
        for (int x = centerX; x < centerX + width; x++)
        {
            for (int y = centerY - height; y < centerY; y++)
            {
                if (!WorldGen.InWorld(x, y)) return false;

                Tile checkTile = Framing.GetTileSafely(x, y);
                if (checkTile.HasTile) return false;
            }
        }
        return true;
    }
}