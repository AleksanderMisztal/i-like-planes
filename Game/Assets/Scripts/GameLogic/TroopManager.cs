﻿using System.Collections.Generic;
using System.Linq;
using Planes262.GameLogic.Troops;
using Planes262.GameLogic.Utils;

namespace Planes262.GameLogic
{
    public class TroopManager
    {
        private readonly Score score;
        private readonly TroopMap troopMap;
        private PlayerSide activePlayer = PlayerSide.Red;


        public TroopManager(TroopMap troopMap, Score score)
        {
            this.troopMap = troopMap;
            this.score = score;
        }

        public void ResetForNewGame()
        {
            foreach (ITroop troop in troopMap.Troops) 
                troop.CleanUpSelf();
            troopMap.ResetForNewGame();
            score.Reset();
        }
        
        public virtual void BeginNextRound(IEnumerable<Troop> troops)
        {
            IEnumerable<DamageScoringTroopDecorator> damageScoringTroops = troops.Select(t => new DamageScoringTroopDecorator(t, score));
            troopMap.SpawnWave(damageScoringTroops);
            
            HashSet<ITroop> beginningTroops = troopMap.GetTroops(activePlayer.Opponent());
            foreach (ITroop troop in beginningTroops)
                troop.ResetMovePoints();
            
            activePlayer = activePlayer.Opponent();
        }

        public void MoveTroop(VectorTwo position, int direction, List<BattleResult> battleResults)
        {
            int battleId = 0;
            ITroop troop = troopMap.Get(position);
            VectorTwo startingPosition = troop.Position;
            troop.MoveInDirection(direction);

            ITroop encounter = troopMap.Get(troop.Position);
            if (encounter == null)
            {
                troopMap.AdjustPosition(troop, startingPosition);
                return;
            }
            BattleResult result = battleResults[battleId++];

            if (result.AttackerDamaged) ApplyDamage(troop, startingPosition);
            if (result.DefenderDamaged) ApplyDamage(encounter, encounter.Position);

            troop.FlyOverOtherTroop();
            
            while ((encounter = troopMap.Get(troop.Position)) != null && !troop.Destroyed)
            {
                result = battleResults[battleId++];
                if (result.AttackerDamaged) ApplyDamage(troop, startingPosition);
                if (result.DefenderDamaged) ApplyDamage(encounter, encounter.Position);

                troop.FlyOverOtherTroop();
            }

            if (!troop.Destroyed)
                troopMap.AdjustPosition(troop, startingPosition);
        }

        private void ApplyDamage(ITroop troop, VectorTwo startingPosition)
        {
            troop.ApplyDamage();
            if (troop.Destroyed)
                troopMap.Remove(troop, startingPosition);
        }
    }
}
