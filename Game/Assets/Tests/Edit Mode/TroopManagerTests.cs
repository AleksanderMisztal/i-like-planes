﻿using System.Collections.Generic;
using NUnit.Framework;
using Planes262.GameLogic;
using Planes262.GameLogic.Utils;

namespace Planes262.Tests.Edit_Mode
{
    public class TroopManagerTests
    {
        private TroopManager troopManager;
        private List<Troop> troops;

        private void Move(int x, int y, int direction, int battleCount)
        {
            List<BattleResult> battleResults = new List<BattleResult>();
            for (int i = 0; i < battleCount; i++) 
                battleResults.Add(new BattleResult(true, true));
            troopManager.MoveTroop(new VectorTwo(x, y), direction, battleResults);
        }
        
        [Test]
        public void GameTestsSimplePasses()
        {
            TroopMap troopMap = new TroopMap();
            troopManager = new TroopManager(troopMap);
            troops = new List<Troop>
            {
                Troop.Blue(1,1),
                Troop.Blue(1,2),
                Troop.Blue(1,3),
                Troop.Red(4, 1),
                Troop.Red(4, 2),
                Troop.Red(4, 3),
            };
            
            troopManager.ResetForNewGame();
            troopManager.BeginNextRound(troops);
            Move(1, 1, 0, 0);
            Move(2, 1, 0, 0);
            Move(3, 1, 0, 1);
            
            Assert.IsTrue(troops[0].Position == new VectorTwo(5, 1));
        }
    }
}
