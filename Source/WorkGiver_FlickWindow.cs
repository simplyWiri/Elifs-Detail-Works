using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ElifsDecorations
{
    [DefOf]
    public static class Window_DefOf
    {
        public static JobDef FlickWindow;
        public static DesignationDef SwitchWindow;
    }

    public class JobDriver_Flick : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOn(() => (base.Map.designationManager.DesignationOn(base.TargetThingA, Window_DefOf.SwitchWindow) == null) ? true : false);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_General.Wait(15).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);

            Toil finalize = new Toil();
            finalize.initAction = delegate
            {
                Pawn actor = finalize.actor;
                var thingWithComps = (ThingWithComps)actor.CurJob.targetA.Thing;

                var window = thingWithComps as Building_Window;
                window.ChangeState();

                actor.records.Increment(RecordDefOf.SwitchesFlicked);
                Map.designationManager.DesignationOn(thingWithComps, Window_DefOf.SwitchWindow)?.Delete();
            };
            finalize.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return finalize;
        }
    }

    public class WorkGiver_Flick : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            List<Designation> desList = pawn.Map.designationManager.allDesignations;
            for (int i = 0; i < desList.Count; i++)
            {
                if (desList[i].def == Window_DefOf.SwitchWindow)
                {
                    yield return desList[i].target.Thing;
                }
            }
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(Window_DefOf.SwitchWindow);
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn.Map.designationManager.DesignationOn(t, Window_DefOf.SwitchWindow) == null)
                return false;
            if (!pawn.CanReserve(t, 1, -1, null, forced))
                return false;
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return JobMaker.MakeJob(Window_DefOf.FlickWindow, t);
        }
    }
}