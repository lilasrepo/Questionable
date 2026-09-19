using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using Questionable.Model.Questing;
using static Questionable.External.IPCUtils;
namespace Questionable.External;

[RegisterSingleton]
internal sealed class ArtisanIpc(IDalamudPluginInterface pluginInterface, ILogger<ArtisanIpc> logger) : Ipc
{
    public override string InternalName => "Artisan";
    public override Version? GetVersion() => IPCSubscriber.Version(InternalName);
    public override bool IsReady() => IpcInvoke.SafeFunc(() => GetVersion() != null && !_isBusy.InvokeFunc(), fallback: false);

    private readonly ICallGateSubscriber<ushort, int, object> _craftItem = pluginInterface.GetIpcSubscriber<ushort, int, object>("Artisan.CraftItem");
    private readonly ICallGateSubscriber<bool> _getEnduranceStatus = pluginInterface.GetIpcSubscriber<bool>("Artisan.GetEnduranceStatus");
    private readonly ICallGateSubscriber<bool> _isListRunning = pluginInterface.GetIpcSubscriber<bool>("Artisan.IsListRunning");
    private readonly ICallGateSubscriber<int, object> _startListById = pluginInterface.GetIpcSubscriber<int, object>("Artisan.StartListById");
    private readonly ICallGateSubscriber<bool> _isBusy = pluginInterface.GetIpcSubscriber<bool>("Artisan.IsBusy");
    // MUST RE-APPLY (TC-only): sub-craft aware craft start, paired with Artisan's own
    // Artisan.CraftItemWithSubcrafts provider. Upstream has no equivalent; dropping it silently
    // reduces crafter quests to a single-item craft. See feedback_preserve_questionable_artisan_subcraft_ipc.
    private readonly ICallGateSubscriber<ushort, int, object> _craftItemWithSubcrafts = pluginInterface.GetIpcSubscriber<ushort, int, object>("Artisan.CraftItemWithSubcrafts");

    public bool CraftItem(ushort recipeId, int quantity)
    {
        try
        {
            logger.LogInformation("Attempting to craft {Quantity} items with recipe {RecipeId} with Artisan", quantity,
                recipeId);
            _craftItem.InvokeAction(recipeId, quantity);
            return true;
        }
        catch (IpcError e)
        {
            logger.LogError(e, "Unable to craft items");
            return false;
        }
    }

    public bool CraftList(int listId)
    {
        try
        {
            logger.LogInformation("Attempting to craft list {ListId} with Artisan", listId);
            _startListById.InvokeAction(listId);
            return true;
        }
        catch (IpcError e)
        {
            logger.LogError(e, "Unable to craft items");
            return false;
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "CraftList failed");
            return false;
        }
    }

    public bool CraftList(ElementId questId) => CraftList(questId.Value.ToInt() + 65536);

    // TC-only: builds a temporary Artisan list containing the recipe plus all its sub-crafts,
    // runs it, and auto-removes the temp list when finished. Degrades to false if the Artisan
    // build lacks this IPC (older Artisan / international HEAD don't expose it).
    public bool CraftListWithSubcrafts(ushort recipeId, int quantity)
    {
        try
        {
            logger.LogInformation(
                "Attempting to craft {Quantity} items (with sub-crafts) for recipe {RecipeId} with Artisan", quantity,
                recipeId);
            _craftItemWithSubcrafts.InvokeAction(recipeId, quantity);
            return true;
        }
        catch (IpcError e)
        {
            logger.LogError(e, "Unable to craft items");
            return false;
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "CraftListWithSubcrafts failed");
            return false;
        }
    }

    public bool IsCrafting()
    {
        try
        {
            return _getEnduranceStatus.InvokeFunc() || _isListRunning.InvokeFunc();
        }
        catch (IpcError e)
        {
            logger.LogError(e, "Unable to check for Artisanstatus");
            return false;
        }
    }
}
