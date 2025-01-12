using Microsoft.Xna.Framework;
using Reverie.Common.MissionAttributes;
using Reverie.Common.Systems;
using Reverie.Core.Cutscenes;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;
using Reverie.Cutscenes;
using Terraria;
using Terraria.ID;

namespace Reverie.Common.Players
{
    partial class MissionPlayer
    {
        public override void OnEnterWorld()
        {
            MissionHandlerManager.Instance.Reset(); // Reset handlers on world enter

            Mission Reawakening = GetMission(MissionID.Reawakening);
            ReveriePlayer player = Main.LocalPlayer.GetModPlayer<ReveriePlayer>();

            if (Reawakening != null && Reawakening.State != MissionState.Completed)
            {
                if (Reawakening.Progress != MissionProgress.Active)
                {
                    CutsceneLoader.PlayCutscene(new IntroCutscene());
                    UnlockMission(MissionID.Reawakening);
                    StartMission(MissionID.Reawakening);

                    Reawakening.Progress = MissionProgress.Active;
                }

                if (Reawakening.CurrentSetIndex == 1)
                {
                    if (!player.pathWarrior && !player.pathMarksman && !player.pathMage && !player.pathConjurer)
                    {
                        ReverieUISystem.Instance.ClassInterface.SetState(ReverieUISystem.Instance.classUI);
                    }
                }
            }
        }
        public override bool OnPickup(Item item)
        {
            MissionHandlerManager.Instance.OnItemPickup(item);
            return base.OnPickup(item);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            base.OnHitNPC(target, hit, damageDone);
            MissionHandlerManager.Instance.OnNPCHit(target, hit.Damage);
        }
    }
}
