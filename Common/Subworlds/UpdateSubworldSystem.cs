using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria;
using Reverie.Common.Subworlds.Archaea;
using Reverie.Common.Subworlds.Sylvanwalde;

namespace Reverie.Common.Subworlds;

public class UpdateSubworldSystem : ModSystem
{
    public override void PreUpdateWorld()
    {
        if (SubworldSystem.IsActive<ArchaeaSub>())
        {
            // Update mechanisms
            Wiring.UpdateMech();

            // Update tile entities
            TileEntity.UpdateStart();
            foreach (var te in TileEntity.ByID.Values)
            {
                te.Update();
            }
            TileEntity.UpdateEnd();

            // Update liquid
            if (++Liquid.skipCount > 1)
            {
                Liquid.UpdateLiquid();
                Liquid.skipCount = 0;
            }
        }

        if (SubworldSystem.IsActive<SylvanSub>())
        {
            // Update mechanisms
            Wiring.UpdateMech();

            // Update tile entities
            TileEntity.UpdateStart();
            foreach (var te in TileEntity.ByID.Values)
            {
                te.Update();
            }
            TileEntity.UpdateEnd();

            // Update liquid
            if (++Liquid.skipCount > 1)
            {
                Liquid.UpdateLiquid();
                Liquid.skipCount = 0;
            }
        }
    }
}