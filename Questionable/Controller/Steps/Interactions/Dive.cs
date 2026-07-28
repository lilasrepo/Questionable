using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Questionable.Controller.Steps.Common;
using Questionable.Model.Questing;
namespace Questionable.Controller.Steps.Interactions;

internal static class Dive
{
    // API12 / 7.1: sig from HEAD (game 7.5) may not match. Lazy-init with try/catch so the
    // type doesn't fail to load if the sig is absent — Dive task will throw at execute time instead.
    private static DiveDelegate? DiveFunc = TryResolveDive();

    private static DiveDelegate? TryResolveDive()
    {
        try
        {
            return Marshal.GetDelegateForFunctionPointer<DiveDelegate>(Svc.SigScanner.ScanText("48 89 5C 24 ?? 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 8B 1D ?? ?? ?? ?? 48 8D 54 24"));
        }
        catch
        {
            Svc.Log?.Warning("Dive sig not found on game 7.1; ExecuteDive will throw if invoked.");
            return null;
        }
    }

    public static unsafe void ExecuteDive()
    {
        if (DiveFunc is null)
            throw new InvalidOperationException("Dive function not available on this game version.");
        DiveFunc(Control.Instance());
    }
    private static unsafe void Dismount() => ActionManager.Instance()->UseAction(ActionType.GeneralAction, 23);
    private unsafe delegate byte DiveDelegate(void* control);

    internal sealed class Factory : SimpleTaskFactory
    {
        public override ITask? CreateTask(Quest quest, QuestSequence sequence, QuestStep step)
        {
            if (step.InteractionType != EInteractionType.Dive)
                return null;

            return new Task();
        }
    }

    internal sealed class Task : ITask
    {
        public override string ToString() => "Dive";
    }

    internal sealed class DoDive(ICondition condition)
        : AbstractDelayedTaskExecutor<Task>(TimeSpan.FromSeconds(5))
    {
        private int _attempts;

        protected override bool StartInternal()
        {
            if (condition[ConditionFlag.Diving])
                return false;

            if (PerformDive())
                return true;

            throw new TaskException("You aren't swimming, so we can't dive.");
        }

        private bool PerformDive()
        {
            if (condition[ConditionFlag.Swimming] || condition[ConditionFlag.Mounted])
            {
                ExecuteDive();
                Dismount();
                return true;
            }

            return false;
        }

        public override bool ShouldInterruptOnDamage() => false;

        protected override ETaskResult UpdateInternal()
        {
            if (condition[ConditionFlag.Diving])
                return ETaskResult.TaskComplete;

            if (_attempts >= 3)
                throw new TaskException("Please dive manually.");

            PerformDive();
            _attempts++;
            return ETaskResult.StillRunning;
        }
    }
}
