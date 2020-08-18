﻿using System.Linq;
using System.Collections.Generic;
using Planes262.GameLogic.Utils;
using Planes262.GameLogic.Exceptions;
using System.Diagnostics;

namespace Planes262.GameLogic
{
    public class PathFinder
    {
        private readonly TroopMap map;

        private PlayerSide side;

        private HashSet<OrientedCell> reachableCells = new HashSet<OrientedCell>();
        private Dictionary<OrientedCell, OrientedCell> parent = new Dictionary<OrientedCell, OrientedCell>();
        private Dictionary<VectorTwo, OrientedCell> orient = new Dictionary<VectorTwo, OrientedCell>();

        public PathFinder(TroopMap map)
        {
            this.map = map;
        }


        public HashSet<VectorTwo> GetReachableCells(VectorTwo position)
        {
            ResetMembers();
            var troop = map.Get(position);
            if (troop == null) throw new PathFindingException("No troop on this hex!");
            return GetReachableCells(troop);
        }

        private void ResetMembers()
        {
            reachableCells = new HashSet<OrientedCell>();
            parent = new Dictionary<OrientedCell, OrientedCell>();
            orient = new Dictionary<VectorTwo, OrientedCell>();
        }

        private HashSet<VectorTwo> GetReachableCells(Troop troop)
        {
            side = troop.Player;
            var initialPosition = new OrientedCell(troop.Position, troop.Orientation);
            AddReachableCells(initialPosition, troop.MovePoints);
            return new HashSet<VectorTwo>(reachableCells.Select(c => c.Position));
        }

        private void AddReachableCells(OrientedCell sourceCell, int movePoints)
        {
            if (movePoints <= 0) return;
            Trace.WriteLine($"Source: {sourceCell.Position}, {sourceCell.Orientation}, mp: {movePoints}");
            foreach (var cell in sourceCell.GetControllZone())
            {
                Trace.WriteLine($"Cell: {cell.Position}, {cell.Orientation}");
                if (reachableCells.Contains(cell)) continue;
                AddCell(sourceCell, movePoints - 1, cell);
            }
        }

        private void AddCell(OrientedCell sourceCell, int movePoints, OrientedCell cell)
        {
            var encounter = map.Get(cell.Position);
            if (encounter == null || encounter.Player != side)
            {
                reachableCells.Add(cell);
                parent[cell] = sourceCell;
                orient[cell.Position] = cell;
            }
            if (encounter == null)
                AddReachableCells(cell, movePoints);
        }

        public List<int> GetDirections(VectorTwo start, VectorTwo end)
        {
            var directions = new List<int>();
            var coords = orient[end];
            while (coords.Position != start)
            {
                var prevCoords = parent[coords];
                var direction = prevCoords.GetDirection(coords);
                directions.Add(direction);
                coords = prevCoords;
            }
            directions.Reverse();
            return directions;
        }
    }
}
