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
using Reverie.Core.Graphics.Interfaces;

namespace Reverie.Core.Graphics;

public class AdditiveDrawing : HookGroup
{
    public override void Load()
    {
        if (Main.dedServ)
            return;

        On_Main.DrawDust += DrawAdditive;
    }

    private void DrawAdditive(On_Main.orig_DrawDust orig, Main self)
    {
        orig(self);
        Main.spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

        for (var k = 0; k < Main.maxProjectiles; k++) //Projectiles
        {
            if (Main.projectile[k].active && Main.projectile[k].ModProjectile is IDrawAdditive additive)
                additive.DrawAdditive(Main.spriteBatch);
        }

        for (var k = 0; k < Main.maxNPCs; k++) //NPCs
        {
            if (Main.npc[k].active && Main.npc[k].ModNPC is IDrawAdditive)
                ((IDrawAdditive)Main.npc[k].ModNPC).DrawAdditive(Main.spriteBatch);
        }

        Main.spriteBatch.End();
    }
}

