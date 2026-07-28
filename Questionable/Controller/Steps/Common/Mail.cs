using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;
using Questionable.Controller.Steps.Movement;
using Questionable.Model.Questing;
using Dalamud.Game.ClientState.Objects;

namespace Questionable.Controller.Steps.Common;

internal static class Mail
{
    internal sealed class Factory(MogmailIpc mogmailIpc, IObjectTable objectTable, Configuration configuration, ILogger<Mail.Factory> logger) : ITaskFactory
    {
        public IEnumerable<ITask> CreateAllTasks(Domain.Quest quest, QuestSequence sequence, QuestStep step)
        {
            if (step.InteractionType != EInteractionType.AcceptQuest)
                yield break;

            logger.LogDebug($"ClaimMail: {configuration.General.ClaimMail} / MogmailIpc.IsInstalled: {mogmailIpc.IsInstalled} / LetterCount: {MogmailIpc.LetterCount ?? 0}");
            if (!configuration.General.ClaimMail)
                yield break;
            if (!mogmailIpc.IsInstalled)
                yield break;
            if (MogmailIpc.LetterCount == 0)
                yield break;

            var objList = FindDeliveryMoogle(objectTable);
            if (objList.TryGetFirst(out IGameObject? moogle) && moogle != null)
            {
                logger.LogDebug("Moving to moogle");
                yield return new MoveTask(step, moogle.Position);
                yield return new ClaimMailTask(moogle);
            }
        }
    }

    internal sealed record ClaimMailTask(IGameObject GameObject) : ITask
    {
        public bool ShouldRedoOnInterrupt() => true;
        public override string ToString() => "ClaimMail()";
    }

    internal sealed class ClaimMailExecutor(ITargetManager targetManager, MogmailIpc mogmailIpc) : TaskExecutor<ClaimMailTask>
    {
        protected unsafe override bool Start()
        {
            targetManager.Target = null;
            targetManager.Target = Task.GameObject;
            var result = (long)TargetSystem.Instance()->InteractWithObject((GameObject*)Task.GameObject.Address, checkLineOfSight: false);
            mogmailIpc.ClaimAll();
            CloseMail();
            return true;
        }

        public override ETaskResult Update()
        {
            if (mogmailIpc.IsBusy)
                mogmailIpc.ClaimAll();
            return ETaskResult.StillRunning;
        }

        public override bool ShouldInterruptOnDamage() => false;
    }

    internal static IEnumerable<IGameObject> FindDeliveryMoogle(IObjectTable objectTable)
    {
        var sheet = Svc.Data.Excel.GetSheet<ENpcBase>();
        foreach (IGameObject? obj in objectTable)
        {
            if (sheet.TryGetRow(obj.DataId, out var enpcsheet))
                if (enpcsheet.ENpcData.Any(x => x.RowId == 720898))
                    yield return obj;
        }
    }

    private static unsafe bool CloseMail()
    {
        // B1(api13): FFXIVClientStructs 6966 has no AgentLetter / AgentLetterView (it carries
        // LetterDataModule, InfoProxyLetter and LetterNumberArray instead), so the whole
        // close-the-mail-window path cannot be expressed here. Reporting 'not closed' makes the
        // caller fall through to its own timeout rather than act on a fabricated success.
        return false;
    }
}
