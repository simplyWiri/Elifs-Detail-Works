using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ElifsDecorations
{
    public class MapComponent_Windows : MapComponent
    {
        public List<Building_Window> windows = new List<Building_Window>();
        public Dictionary<IntVec3, List<Building_Window>> WindowCells = new Dictionary<IntVec3, List<Building_Window>>();
        public bool dirty = false;
        public List<IntVec3> DirtyCells = new List<IntVec3>();

        public MapComponent_Windows(Map map) : base(map)
        {
        }

        public void RegisterWindow(Building_Window window)
        {
            if (!windows.Contains(window))
            {
                windows.Add(window);
                // when we attempt to resolve the facing, we also query cells, and push them to the calculation
                window.Cells = WindowUtility.CalculateWindowLightCells(window, window.WindowComp.TryResolveFacing());
                UpdateWindowCells(window, true);
            }
        }

        public void DeRegisterWindow(Building_Window window)
        {
            if (windows.Contains(window))
            {
                windows.Remove(window);
                UpdateWindowCells(window, false);
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (DirtyCells.Count != 0)
            {
                HashSet<Building_Window> windowsToUpdate = new HashSet<Building_Window>();
                foreach (var c in DirtyCells)
                {
                    try
                    {
                        foreach (var window in WindowCells[c])
                        {
                            windowsToUpdate.Add(window);
                        }
                    } catch
                    {
                        DirtyCells.Remove(c);
                    }
                }

                foreach (var window in windowsToUpdate)
                {
                    UpdateWindowCells(window, false);
                    // when we attempt to resolve the facing, we also query cells, and push them to the calculation
                    window.Cells = WindowUtility.CalculateWindowLightCells(window, window.WindowComp.TryResolveFacing());
                    UpdateWindowCells(window, true);

                    DirtyCells.Clear();
                }
            }
        }

        public void UpdateAllWindowCells()
        {
            WindowCells.Clear();
            foreach (Building_Window window in windows)
            {
                UpdateWindowCells(window, true);
            }
        }

        public void UpdateWindowCells(Building_Window window, bool register)
        {
            foreach (IntVec3 c in window.Cells)
            {
                if (register)
                    if (WindowCells.ContainsKey(c)) // if we already have a window here (crossover) add our window to the list
                        WindowCells[c].Add(window);
                    else
                        WindowCells.Add(c, new List<Building_Window>() { window });
                else
                {
                    if (WindowCells.ContainsKey(c))
                    {
                        if (WindowCells[c].Count != 1) // if we have multiple entries, simply remove our current window from the list
                            WindowCells[c].Remove(window);
                        else // else just remove the entry entirely
                            WindowCells.Remove(c);
                    }
                }
            }

            LightingOverlay_Regenerate.dirty = true;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref DirtyCells, "DirtyCells", LookMode.Value);
            Scribe_Values.Look(ref dirty, "Dirty");
        }
    }
}