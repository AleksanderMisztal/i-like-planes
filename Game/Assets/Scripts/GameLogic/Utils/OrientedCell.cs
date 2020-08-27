﻿using Planes262.GameLogic.Exceptions;

namespace Planes262.GameLogic.Utils
{
    public class OrientedCell
    {
        public VectorTwo Position { get; }
        public int Orientation { get; }

        public OrientedCell(VectorTwo position, int orientation)
        {
            Position = position;
            Orientation = orientation;
        }

        public OrientedCell[] GetControllZone()
        {
            OrientedCell[] cs = new OrientedCell[3];
            for (int i = -1; i < 2; i++)
            {
                cs[i + 1] = GetAdjacent(i);
            }
            return cs;
        }

        private OrientedCell GetAdjacent(int direction)
        {
            VectorTwo position = Hex.GetAdjacentHex(Position, Orientation + direction);
            return new OrientedCell(position, Orientation + direction);
        }

        public int GetDirection(OrientedCell coords)
        {
            for (int i = -1; i < 2; i++)
            {
                OrientedCell c = GetAdjacent(i);
                if (c == coords) return i;
            }
            throw new PathFindingException("Can't get there!");
        }

        public override int GetHashCode()
        {
            return Orientation + 7 * (Position.X + 101 * Position.Y);
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType())) return false;
            OrientedCell c = (OrientedCell)obj;
            return Position.X == c.Position.X && Position.Y == c.Position.Y && Orientation == c.Orientation;
        }
        public static bool operator ==(OrientedCell a, OrientedCell b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }
        public static bool operator !=(OrientedCell a, OrientedCell b)
        {
            return !(a == b);
        }
    }
}