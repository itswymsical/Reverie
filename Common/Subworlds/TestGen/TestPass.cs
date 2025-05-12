using Reverie.Content.Tiles.Canopy;
using Reverie.Content.Tiles.Taiga;
using Reverie.lib;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace Reverie.Common.Subworlds.TestGen;

public class TestPass : GenPass
{
    public TestPass() : base("TestPass", 0.1f)
    {
    }

    private FastNoiseLite noise;
    protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        var origin = new Point(Main.maxTilesX / 2, Main.maxTilesX / 2);

        Main.spawnTileX = origin.X;
        Main.spawnTileY = origin.Y;

        //WorldUtils.Gen(
        //    origin,
        //    new Shapes.Circle((int)Math.PI * (origin.X / 20)),
        //    Actions.Chain(new Actions.SetTile(TileID.LivingWood))
        //);

        //Dirt hill
        WorldUtils.Gen(
            origin,
            new Shapes.Mound((int)Math.PI * (origin.X / 16), origin.Y / 8),
            Actions.Chain(new Actions.SetTile(TileID.Dirt))
        );

        //tree
        WorldUtils.Gen(
            new Point(origin.X, origin.Y - (origin.Y / 8)),
            new ShapeRoot(-1.2, 60, 9.6, 2),
            Actions.Chain(new Actions.SetTile(TileID.LivingMahogany))
        );

        // trunk 2
        WorldUtils.Gen(
            new Point(origin.X - 6, origin.Y - (origin.Y / 6)),
            new ShapeRoot(-2.1, 34, 5.6, 2),
            Actions.Chain(new Actions.SetTile(TileID.LivingMahogany))
        );

        // branch test
        WorldUtils.Gen(
            new Point(origin.X - 20, origin.Y - (origin.Y / 6)),
            new ShapeBranch(10, 16),
            Actions.Chain(new Actions.SetTile(TileID.Stone))
        );


        int totalWidth = 175;
        int quantity = 16;
        int spacing = totalWidth / quantity;

        Random random = new Random();

        // Generate roots across the underhill
        for (int i = 0; i < quantity; i++)
        {
            int xPosition = origin.X - (totalWidth / 2) + (i * spacing);
            double randomAngle = random.NextDouble() * Math.PI; // Random angle between 0 and π

            WorldUtils.Gen(
                new(xPosition, origin.Y),
                new ShapeRoot(
                    angle: randomAngle,
                    50 + random.Next(0, 30),
                    3 + random.NextDouble() * 2,
                    0.5
                ),
                Actions.Chain(new Actions.SetTile(TileID.LivingWood))
            );
        }
    }
}
