using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace ElifsDecorations
{
    public class WindowUtility
    {
        public static List<IntVec3> CalculateWindowLightCells(Building_Window window, List<IntVec3> cells = null)
        {
            return CalculateWindowLightCells(window, window.def.size, window.Position, window.Rotation, window.Map, cells);
        }

        public static List<IntVec3> CalculateWindowLightCells(Building_Window window, IntVec2 size, IntVec3 center, Rot4 rot, Map map, List<IntVec3> cells = null)
        {
            List<IntVec3> returnVec = new List<IntVec3>();
            if (window.WindowComp.facing == LinkDirections.None) return returnVec;



            if (cells == null)
            {
                foreach (IntVec3 c in GetWindowCells(window, center, rot, size, map))
                {
                    ValidateCell(c);
                }
            }
            else
            {
                foreach (IntVec3 c in cells)
                {
                    ValidateCell(c);
                }
            }            
            
            void ValidateCell(IntVec3 c)
            {
                bool flag = false;
                foreach (var cell in window.OccupiedRect())
                {
                    if (LightReaches(c, cell, map))
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                    returnVec.Add(c);
            }
            return returnVec;
        }

        private static bool LightReaches(IntVec3 cell, IntVec3 observer, Map map)
        {
            return (cell.Walkable(map) && GenSight.LineOfSightToEdges(cell, observer, map, true));
        }

        public static List<IntVec3> GetWindowCells(Building_Window window, bool forceAll = false)
        {
            return GetWindowCells(window, window.Position, window.Rotation, window.def.Size, window.Map, forceAll);
        }

        private static List<IntVec3> GetWindowCells(Building_Window window, IntVec3 center, Rot4 rot, IntVec2 size, Map map, bool forceAll = false)
        {
            if (window.WindowComp.state == State.Closed)
                return new List<IntVec3>();

            List<IntVec3> returnVec = new List<IntVec3>();

            var extent = (size.x > size.z) ? size.x : size.z;
            extent += window.WindowComp.Props.radius;

            size = new IntVec2(extent, extent);

            switch (ElifsDecorationsSettings.focalType)
            {
                case WindowFocalType.Circular:
                    foreach (var c in GenRadial.RadialCellsAround(center, extent, true))
                    {
                        if (InvalidCell(c, center, rot, window, forceAll))
                            continue;

                        returnVec.Add(c);
                    }
                    break;

                case WindowFocalType.Rectangular:
                    foreach (IntVec3 c in GenAdj.OccupiedRect(center, rot, size))
                    {
                        if (InvalidCell(c, center, rot, window, forceAll))
                            continue;

                        returnVec.Add(c);
                    }
                    break;
            }

            return returnVec;
        }

        public static bool InvalidCell(IntVec3 c, IntVec3 center, Rot4 rot, Building_Window window, bool AllCells = false)
        {
            if (!c.InBounds(window.Map))
                return true; // out of map

            if (!AllCells)
            {
                if (!rot.IsHorizontal && ((window.WindowComp.facing == LinkDirections.Up && c.z < center.z) || (window.WindowComp.facing == LinkDirections.Down && c.z > center.z)))
                    return true;

                if (rot.IsHorizontal && ((window.WindowComp.facing == LinkDirections.Right && c.x < center.x) || (window.WindowComp.facing == LinkDirections.Left && c.x > center.x)))
                    return true;
            }

            if ((!rot.IsHorizontal && c.z == center.z) || (rot.IsHorizontal && c.x == center.x))
                return true; // we are inline of the window


            return false;
        }

    }
}