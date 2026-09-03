using Dalamud.Plugin.Ipc;
namespace Questionable.External;

[RegisterSingleton]
internal sealed class TextAdvanceIpc : IDisposable
{
    private readonly Configuration _configuration;
    private readonly ICallGateSubscriber<string, bool> _disableExternalControl;
    private readonly ICallGateSubscriber<string, ExternalTerritoryConfig, bool> _enableExternalControl;
    private readonly IFramework _framework;
    private readonly ICallGateSubscriber<bool> _isInExternalControl;
    private readonly string _pluginName;
    private readonly QuestController _questController;
    private bool _isExternalControlActivated;

    public TextAdvanceIpc(IDalamudPluginInterface pluginInterface, IFramework framework,
        QuestController questController, Configuration configuration)
    {
        _framework = framework;
        _questController = questController;
        _configuration = configuration;
        _isInExternalControl = pluginInterface.GetIpcSubscriber<bool>("TextAdvance.IsInExternalControl");
        _enableExternalControl =
            pluginInterface.GetIpcSubscriber<string, ExternalTerritoryConfig, bool>(
                "TextAdvance.EnableExternalControl");
        _disableExternalControl = pluginInterface.GetIpcSubscriber<string, bool>("TextAdvance.DisableExternalControl");
        _pluginName = pluginInterface.InternalName;
        _framework.Update += OnUpdate;
    }

    public void Dispose()
    {
        _framework.Update -= OnUpdate;
        if (_isExternalControlActivated)
            IpcInvoke.TryOnFrameworkThread(_framework, () => _disableExternalControl.InvokeFunc(_pluginName));
    }

    private void OnUpdate(IFramework framework)
    {
        bool hasActiveQuest = _questController.IsRunning ||
                              _questController.AutomationType != QuestController.EAutomationType.Manual;
        // porting-note(api12/TC): TextAdvance has no TC port, so its IPC is never registered.
        // Route every InvokeFunc through IpcInvoke.SafeFunc (silent overload) so IpcNotReadyError
        // degrades to the fallback instead of throwing every frame out of IFramework.Update.
        if (_configuration.General.ConfigureTextAdvance && hasActiveQuest)
        {
            if (!IpcInvoke.SafeFunc(() => _isInExternalControl.InvokeFunc(), true))
            {
                if (IpcInvoke.SafeFunc(() => _enableExternalControl.InvokeFunc(
                        _pluginName, CreateExternalTerritoryConfig(_configuration.General.DontSkipCutscenes)), false))
                    _isExternalControlActivated = true;
            }
        }
        else
        {
            if (_isExternalControlActivated)
            {
                if (IpcInvoke.SafeFunc(() => _disableExternalControl.InvokeFunc(_pluginName), true) ||
                    !IpcInvoke.SafeFunc(() => _isInExternalControl.InvokeFunc(), true))
                    _isExternalControlActivated = false;
            }
        }
    }

    private static ExternalTerritoryConfig CreateExternalTerritoryConfig(bool dontSkipCutscenes) => new()
    {
        EnableQuestAccept = true,
        EnableQuestComplete = true,
        EnableRewardPick = true,
        EnableRequestHandin = true,
        EnableCutsceneEsc = !dontSkipCutscenes,
        EnableCutsceneSkipConfirm = !dontSkipCutscenes,
        EnableTalkSkip = !dontSkipCutscenes,
        EnableRequestFill = true,
        EnableAutoInteract = false
    };

    private sealed class ExternalTerritoryConfig
    {
        public bool? EnableQuestAccept;
        public bool? EnableQuestComplete;
        public bool? EnableRewardPick;
        public bool? EnableRequestHandin;
        public bool? EnableCutsceneEsc;
        public bool? EnableCutsceneSkipConfirm;
        public bool? EnableTalkSkip;
        public bool? EnableRequestFill;
        public bool? EnableAutoInteract;
    }
}
