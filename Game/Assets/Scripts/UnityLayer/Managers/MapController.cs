﻿using System;
using System.Collections.Generic;
using GameDataStructures;
using Planes262.GameLogic;
using Planes262.GameLogic.Troops;
using Planes262.GameLogic.Utils;
using UnityEngine;

namespace Planes262.UnityLayer.Managers
{
    public class MapController
    {
        public MapController(TileManager tileManager, TroopMap troopMap, Action<MoveAttemptEventArgs> troopMoveHandler)
        {
            this.tileManager = tileManager;
            this.troopMap = troopMap;
            this.troopMoveHandler = troopMoveHandler;
        }
        
        private readonly TileManager tileManager;
        private readonly TroopMap troopMap;
        private readonly Action<MoveAttemptEventArgs> troopMoveHandler;
        
        private PathFinder pathFinder;
        private PlayerSide playerSide;
        private PlayerSide activePlayer = PlayerSide.Red;
        public bool IsLocal = false;

        private bool isPositionSelected;
        private bool isTargetSelected;
        private VectorTwo selectedPosition;
        private ITroop selectedTroop;
        private HashSet<VectorTwo> reachableCells;
        private VectorTwo targetPosition;
        private List<int> directions;
        

        public void ResetForNewGame(PlayerSide side, Board board)
        {
            DeactivateTroops();
            playerSide = side;
            pathFinder = new PathFinder(troopMap, board);
        }

        public void OnCellClicked(VectorTwo cell)
        {
            if (isPositionSelected && cell == selectedPosition) return;
            if (isPositionSelected && reachableCells.Contains(cell))
                ChangePathOrSend(cell);
            else SelectTroop(cell);
        }

        private void ChangePathOrSend(VectorTwo cell)
        {
            if (isTargetSelected && targetPosition == cell)
                SendMoves(selectedPosition, selectedTroop.Orientation, directions);
            else SetAsTarget(cell);
        }

        private void SendMoves(VectorTwo position, int orientation, List<int> moveDirections)
        {
            Debug.Log("Sending moves");
            DeactivateTroops();
            foreach (int dir in moveDirections)
            {
                troopMoveHandler(new MoveAttemptEventArgs(activePlayer, position, dir));
                orientation += dir;
                position = Hex.GetAdjacentHex(position, orientation);
            }
        }

        private void SetAsTarget(VectorTwo cell)
        {
            isTargetSelected = true;
            targetPosition = cell;
            directions = pathFinder.GetDirections(selectedPosition, targetPosition);
            HighlightPath(selectedPosition, selectedTroop.Orientation, directions);
        }

        private void SelectTroop(VectorTwo cell)
        {
            DeactivateTroops();
            selectedTroop = troopMap.Get(cell);
            if (selectedTroop != null && (selectedTroop.Player == playerSide || IsLocal) && selectedTroop.Player == activePlayer)
                ActivateTroopAt(cell);
        }

        private void ActivateTroopAt(VectorTwo cell)
        {
            isPositionSelected = true;
            selectedPosition = cell;
            reachableCells = pathFinder.GetReachableCells(cell);
            tileManager.ActivateTiles(reachableCells);
        }

        private void DeactivateTroops()
        {
            isPositionSelected = false;
            selectedTroop = null;
            reachableCells = null;
            isTargetSelected = false;
            directions = null;
            tileManager.DeactivateTiles();
    }

        private void HighlightPath(VectorTwo position, int orientation, List<int> moveDirections)
        {
            List<VectorTwo> cells = new List<VectorTwo>();
            foreach (int dir in moveDirections)
            {
                orientation += dir;
                position = Hex.GetAdjacentHex(position, orientation);
                cells.Add(position);
            }
            tileManager.HighlightPath(cells);
        }

        public void ToggleActivePlayer()
        {
            activePlayer = activePlayer.Opponent();
        }
    }
}