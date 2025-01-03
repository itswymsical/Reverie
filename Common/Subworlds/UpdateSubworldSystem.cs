﻿using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria;
using Reverie.Common.Systems.Subworlds.Archaea;

namespace Reverie.Common.Systems.Subworlds
{
    public class UpdateSubworldSystem : ModSystem
    {
        public override void PreUpdateWorld()
        {
            if (SubworldSystem.IsActive<ArchaeaSubworld>())
            {
                // Update mechanisms
                Wiring.UpdateMech();

                // Update tile entities
                TileEntity.UpdateStart();
                foreach (TileEntity te in TileEntity.ByID.Values)
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
}