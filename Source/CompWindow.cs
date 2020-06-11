using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace ElifsDecorations
{
    // Default lets light & beauty through
    // Open lets light & beauty & temperature flow + shooting through
    // Closed lets none through
    public enum State { Open, Ajar, Closed }

    [StaticConstructorOnStartup]
    public class CompWindow : ThingComp
    {
        public Building_Window Parent => (Building_Window)parent;
        private Graphic offGraphic = null;
        private Graphic ventingGraphic = null;
        private static Texture2D OpenIcon = ContentFinder<Texture2D>.Get("UI/Open", false);
        private static Texture2D ClosedIcon = ContentFinder<Texture2D>.Get("UI/Closed", false);
        private static Texture2D AjarIcon = ContentFinder<Texture2D>.Get("UI/Ajar", false);
        private static Texture2D FlipIcon = ContentFinder<Texture2D>.Get("UI/Flip", false);
        public State state = State.Open;
        public State wantedState = State.Open;
        public LinkDirections facing = LinkDirections.None;
        public float CachedBeauty = 0f;

        public CompProperties_Window Props => (CompProperties_Window)props;

        public Graphic OffGraphic
        {
            get
            {
                if (offGraphic == null)
                    offGraphic = GraphicDatabase.Get(parent.def.graphicData.graphicClass, parent.def.graphicData.texPath + "_Off", parent.def.graphicData.shaderType.Shader, parent.def.graphicData.drawSize, parent.DrawColor, parent.DrawColorTwo);
                return offGraphic;
            }
        }
        public Graphic VentingGraphic
        {
            get
            {
                if (ventingGraphic == null)
                    ventingGraphic = GraphicDatabase.Get(parent.def.graphicData.graphicClass, parent.def.graphicData.texPath + "_Open", parent.def.graphicData.shaderType.Shader, parent.def.graphicData.drawSize, parent.DrawColor, parent.DrawColorTwo);

                return ventingGraphic;
            }
        }
        public Graphic CurrentGraphic
        {
            get
            {
                switch (state)
                {
                    case State.Open:        return parent.DefaultGraphic;
                    case State.Ajar:        return VentingGraphic;
                }
                return OffGraphic;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            Command_Action command = new Command_Action();
            switch (wantedState)
            {
                case State.Open:
                    command.icon = AjarIcon;
                    command.defaultLabel = "Open the window";
                    break;

                case State.Ajar:
                    command.icon = ClosedIcon;
                    command.defaultLabel = "Close the window";
                    break;

                case State.Closed:
                    command.icon = OpenIcon;
                    command.defaultLabel = "Open the blinds";
                    break;
            }
            command.defaultDesc = "Change the current state of the window";
            command.action = delegate
            {
                switch (wantedState)
                {
                    case State.Open: wantedState = State.Ajar; break;
                    case State.Ajar: wantedState = State.Closed; break;
                    case State.Closed: wantedState = State.Open; break;
                }

                if (state != wantedState)
                {
                    Designation designation = Parent.Map.designationManager.DesignationOn(Parent, Window_DefOf.SwitchWindow);
                    if (designation == null)
                    {
                        Parent.Map.designationManager.AddDesignation(new Designation(Parent, Window_DefOf.SwitchWindow));
                    }
                }
                else
                {
                    Designation designation = Parent.Map.designationManager.DesignationOn(Parent, Window_DefOf.SwitchWindow);
                    if (designation != null)
                    {
                        designation.Delete();
                    }
                }
            };

            yield return command;

            command = new Command_Action();
            command.icon = FlipIcon;
            command.defaultLabel = "Switch Facing Direction";
            command.defaultDesc = "Switch the direction that the window faces manually";
            command.action = delegate
            {
                if(Parent.Rotation.IsHorizontal)
                {
                    if (facing == LinkDirections.Left) 
                        facing = LinkDirections.Right;
                    else 
                        facing = LinkDirections.Left;
                } else
                {
                    if (facing == LinkDirections.Up)
                        facing = LinkDirections.Down;
                    else
                        facing = LinkDirections.Up;
                }

                UpdateWindow();
            };
            yield return command;
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"Window is currently {state.ToString()}, with the target state of {wantedState.ToString()}");
            builder.Append($"\nand is currently facing {facing.ToString()}");
            builder.Append(base.CompInspectStringExtra());
            return builder.ToString();
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            GenDraw.DrawFieldEdges(Parent.Cells.ToList(), Color.green);
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }

        public void ChangeState()
        {
            bool NeedRefresh = false;
            if (state == State.Closed || wantedState == State.Closed)
                NeedRefresh = true;

            state = wantedState;

            if (NeedRefresh) // this will cause some (possibly) mad spikes if you open/close a bunch of windows in a large base
            {
                UpdateWindow();
                GetBeauty();
            }
        }

        private void UpdateWindow()
        {
            WindowCache.WindowComponent.UpdateWindowCells(Parent, false);
            Parent.Cells = WindowUtility.CalculateWindowLightCells(Parent);
            WindowCache.WindowComponent.UpdateWindowCells(Parent, true);

            Find.CurrentMap.mapDrawer.MapMeshDirty(Parent.Position, MapMeshFlag.GroundGlow);
            GetBeauty();
        }

        // in short, get all the cells in the 'radius' of the window, whichever 'side' of the window has less roof cells becomes the side that is being faced, i.e. light goes to the side with more roofs
        public void TryResolveFacing()
        {
            var cells = WindowUtility.GetWindowCells(Parent, true).ToList();

            var leftCells = cells.Where(c =>
            {
                if (Parent.Rotation.IsHorizontal)
                {
                    if (c.x > Parent.Position.x) return true;
                    return false;
                }
                else
                {
                    if (c.z > Parent.Position.z) return true;
                        return false;
                }
            }).ToList();

            foreach (var c in leftCells) // we in effect make sure that 'cells' only contains cells from the 'right' side
                cells.Remove(c);

            var map = Parent.Map;

            int count = 0;
            foreach (var c in cells)
                if (c.Roofed(map))
                    count++;

            int leftCount = 0;
            foreach (var c in leftCells)
                if (c.Roofed(map))
                    leftCount++;

            if (count > leftCount)
            {
                if (Parent.Rotation.IsHorizontal) facing = LinkDirections.Right;
                else facing = LinkDirections.Up;
            }
            else if (count < leftCount)
            {
                if (Parent.Rotation.IsHorizontal) facing = LinkDirections.Left;
                else facing = LinkDirections.Down;
            }
            else
                facing = LinkDirections.None;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref state, "state");
            Scribe_Values.Look(ref wantedState, "wantedState");
            Scribe_Values.Look(ref facing, "facing");
        }
        public void GetBeauty()
        {
            CachedBeauty = 0f;

            if (state == State.Closed) return;

            var things = new List<Thing>();
            foreach (var cell in WindowUtility.GetWindowCells(Parent, true).Except(Parent.Cells))
                CachedBeauty += BeautyUtility.CellBeauty(cell, Parent.Map, things);


            CachedBeauty *= .9f;
        }
    }
}