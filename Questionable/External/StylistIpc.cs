using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using static Questionable.External.IPCUtils;
namespace Questionable.External;

internal sealed class StylistIpc(IDalamudPluginInterface pluginInterface, ILogger<StylistIpc> logger)
{
    private readonly ICallGateSubscriber<bool> _isBusy;
    private readonly ILogger<AutomatonIpc> _logger;
    private readonly ICallGateSubscriber<bool?, bool?, object?> _updateGearset; //bool? moveItemsFromInventory, bool? shouldEquip
    private bool _loggedIpcError;

    public static bool IsInstalled => IPCSubscriber.IsInstalled("Stylist");

    public bool IsBusy => !IsInstalled || _isBusy.InvokeFunc();

    public void UpdateGearset()
    {
        try
        {
            _updateGearset.InvokeAction(true, true);
        }
        catch (IpcError e)
        {
            if (!_loggedIpcError)
            {
                _loggedIpcError = true;
                _logger.LogWarning(e, "Could not query stylist to update gearset, probably not installed");
            }
        }
    }
}
