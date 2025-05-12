using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.Subworlds.TestGen;

public class TestPass : GenPass
{
    public TestPass() : base("TestPass", 0.1f)
    {
    }

    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        var origin = new Point(Main.maxTilesX / 2, Main.maxTilesX / 2);

        Main.spawnTileX = origin.X;
        Main.spawnTileY = origin.Y;

        WorldUtils.Gen(
            origin,
            new Shapes.Mound((int)Math.PI * (origin.X / 16), origin.Y / 13),
            Actions.Chain(new Actions.SetTile(TileID.Dirt))
        );

        int treeX = origin.X;
        int treeY = origin.Y;

        var structureWidth = 53;
        var structureHeight = 192;

        while (Main.tile[treeX, treeY].HasTile && treeY > 0)
        {
            treeY--;
        }
        treeY++;

        if (Main.tile[treeX, treeY].HasTile)
        {
            var structX = origin.X - structureWidth;
            var structY = origin.Y - structureHeight;
            StructureHelper.API.Generator.GenerateStructure("Structures/CanopyTree", new Point16(structX, structY), Instance);
        }

        int totalWidth = 175;
        int quantity = 16;
        int spacing = totalWidth / quantity;

        // Generate roots across the underhill
        for (int i = 0; i < quantity; i++)
        {
            int xPosition = origin.X - (totalWidth / 2) + (i * spacing);
            double randomAngle = Main.rand.NextDouble() * Math.PI; // Random angle between 0 and π

            WorldUtils.Gen(
                new(xPosition + 10, origin.Y),
                new ShapeRoot(
                    angle: randomAngle,
                    50 + Main.rand.Next(0, 30),
                    3 + Main.rand.NextDouble() * 2,
                    0.5
                ),
                Actions.Chain(new Actions.SetTile(TileID.LivingWood))
            );
        }

        //Tunnel
        WorldUtils.Gen(
            new(origin.X, origin.Y - 30),
            new ShapeRoot(MathHelper.Pi / 2, origin.Y / 2, 8, 1),
            Actions.Chain(new Actions.ClearTile(), new Actions.PlaceWall(WallID.LivingWoodUnsafe))
        );
    }
}
