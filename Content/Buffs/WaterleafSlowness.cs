namespace Reverie.Content.Buffs
{
    public class WaterleafSlowness : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.pvpBuff[Type] = true; 
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.velocity /= 1.33f;
            npc.AddBuff(BuffID.Wet, 1);
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.moveSpeed /= 1.33f;
            player.AddBuff(BuffID.Wet, 1);
        }
    }
}
