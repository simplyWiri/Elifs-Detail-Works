using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using Verse;

namespace ElifsDecorations
{
    public class Building_Window : Building
    {
        public int Size => Math.Max(def.size.x, def.size.z);

        private CompWindow windowComp = null;
        private int LastUpdate = -1;

        public CompWindow WindowComp
        {
            get
            {
                if (windowComp == null)
                {
                    windowComp = GetComp<CompWindow>();
                }
                return windowComp;
            }
        }

        public List<IntVec3> Cells = new List<IntVec3>();

        public override Graphic Graphic
        {
            get
            {
                return WindowComp.CurrentGraphic;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            
            WindowCache.WindowComponent.RegisterWindow(this);

            if (ElifsDecorationsSettings.BeautyEnabled)
                WindowComp.GetBeauty();
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            WindowCache.WindowComponent.DeRegisterWindow(this);
            base.DeSpawn(mode);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref Cells, "Cells", LookMode.Value);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
        }

        public void ChangeState()
        {
            WindowComp.ChangeState();
            this.DirtyMapMesh(this.Map);
        }

        public override void TickRare()
        {
            base.TickRare();

            if (WindowComp.state == State.Ajar)
            {
                float rate = Size * 14f; // following in suit of the default '14f' of a vent in vanilla
                GenTemperature.EqualizeTemperaturesThroughBuilding(this, rate, true);
            }

            if (LastUpdate-- <= 0)
            {
                if (WindowComp.facing == LinkDirections.None)
                {
                    WindowComp.TryResolveFacing();
                }

                LastUpdate = 25;
                if(ElifsDecorationsSettings.BeautyEnabled)
                    WindowComp.GetBeauty();
            }

        }

    }
}