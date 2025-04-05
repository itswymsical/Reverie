using Terraria.DataStructures;
using Terraria.Audio;

using System.Collections.Generic;
using System.Linq;

using Reverie.Utilities;
using Reverie.Core.Dialogue;
using Reverie.Core.Missions;

namespace Reverie.Content.Missions.Argie;

public class FungalFracas : Mission //todo: implement mission logic
{
    public FungalFracas() : base(MissionID.FUNGAL_FRACAS,
      "Fungal Fracas",
      "'A fungus goliath is causing commotion'" +
      "\nInvestigate the forest and the strange mushrooms growing.",
      [
        [("Talk to Guide", 1)],
        [("Clear the dancing mushrooms", 5)],
        [("Let Guide inspect samples", 1)],
        [("Track down escaped mushrooms", 1)],
        [("Clear mushroom colonies", 3)],
        [("Investigate the giant mushroom", 1)],
        [("Defeat Fungore", 1)]
      ],

      [new Item(ItemID.GoldCoin, Main.rand.Next(4, 8))],
      isMainline: false,
      NPCID.Guide,
      xpReward: 100)
    {
        ModContent.GetInstance<Reverie>().Logger.Info("[Fungal Fracas] Mission constructed");
    }

    internal enum Objectives
    {
        TalkToGuide = 0,
        ClearShrooms = 1,
        InspectSamples = 2,
        TrackDownShrooms = 3,
        ClearColonies = 4,
        InvestigateCap = 5,
        DefeatFungore = 6,
    }

    public override void Update()
    {
        base.Update();
        Main.bloodMoon = false;
    }
}
