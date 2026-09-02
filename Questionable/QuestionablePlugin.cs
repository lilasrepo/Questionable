using System;
using System.IO;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using ECommons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Questionable.Logging;
using Questionable.Controller;
using Questionable.Controller.CombatModules;
using Questionable.Controller.GameUi;
using Questionable.Controller.NavigationOverrides;
using Questionable.Controller.Steps;
using PunishLib;
using Questionable.AutoGen;
using Questionable.AutoGen.Generation;
using Questionable.Controller.Steps.Common;
using Questionable.Controller.Steps.Fishing;
using Questionable.Controller.Steps.Gathering;
using Questionable.Controller.Steps.Interactions;
using Questionable.Controller.Steps.Movement;
using Questionable.Controller.Steps.Shared;
using Questionable.Controller.Utils;
using Questionable.Data;
using Questionable.External;
using Questionable.Functions;
using Questionable.Gear;
using Questionable.PathData;
using Questionable.Utils;
using Questionable.Validation;
using Questionable.Validation.Validators;
using Questionable.Windows;
using Questionable.Windows.ConfigComponents;
using Questionable.Windows.JournalComponents;
using Questionable.Windows.QuestComponents;
using Questionable.Windows.Utils;
using static Questionable.Utils.LocalizeShortcut;
using Action = Questionable.Controller.Steps.Interactions.ActionStep;
using WrathCombo.API;
using WrathError = WrathCombo.API.WrathIPCWrapper.ErrorType;

namespace Questionable;

// porting-note(api13): upstream declares this as a primary-constructor
// IAsyncDalamudPlugin and lets the host await LoadAsync. IAsyncDalamudPlugin does not exist
// at api13 -- the host constructs the plugin and never awaits anything -- so the primary
// constructor is expanded into a real one that stores the services in same-named readonly
// fields (every member below closes over them unchanged) and performs the load inline.
// A field initializer cannot do the job: it has no `this`, and BuildAndInitialize needs it
// for ECommonsMain.Init. Keep this shape on every refresh; the file is pinned.
public sealed class QuestionablePlugin : IDalamudPlugin
{
    private readonly IDalamudPluginInterface pluginInterface;
    private readonly IClientState clientState;
    private readonly ITargetManager targetManager;
    private readonly IFramework framework;
    private readonly IGameGui gameGui;
    private readonly IDataManager dataManager;
    private readonly ISigScanner sigScanner;
    private readonly IObjectTable objectTable;
    private readonly IPluginLog pluginLog;
    private readonly ICondition condition;
    private readonly IChatGui chatGui;
    private readonly ICommandManager commandManager;
    private readonly IAddonLifecycle addonLifecycle;
    private readonly IKeyState keyState;
    private readonly IContextMenu contextMenu;
    private readonly IToastGui toastGui;
    private readonly IGameInteropProvider gameInteropProvider;
    private readonly ITextureProvider textureProvider;

    public QuestionablePlugin(
        IDalamudPluginInterface pluginInterface,
        IClientState clientState,
        ITargetManager targetManager,
        IFramework framework,
        IGameGui gameGui,
        IDataManager dataManager,
        ISigScanner sigScanner,
        IObjectTable objectTable,
        IPluginLog pluginLog,
        ICondition condition,
        IChatGui chatGui,
        ICommandManager commandManager,
        IAddonLifecycle addonLifecycle,
        IKeyState keyState,
        IContextMenu contextMenu,
        IToastGui toastGui,
        IGameInteropProvider gameInteropProvider,
        ITextureProvider textureProvider)
    {
        ArgumentNullException.ThrowIfNull(pluginInterface);
        ArgumentNullException.ThrowIfNull(chatGui);
        this.pluginInterface = pluginInterface;
        this.clientState = clientState;
        this.targetManager = targetManager;
        this.framework = framework;
        this.gameGui = gameGui;
        this.dataManager = dataManager;
        this.sigScanner = sigScanner;
        this.objectTable = objectTable;
        this.pluginLog = pluginLog;
        this.condition = condition;
        this.chatGui = chatGui;
        this.commandManager = commandManager;
        this.addonLifecycle = addonLifecycle;
        this.keyState = keyState;
        this.contextMenu = contextMenu;
        this.toastGui = toastGui;
        this.gameInteropProvider = gameInteropProvider;
        this.textureProvider = textureProvider;

        try
        {
            _serviceProvider = BuildAndInitialize();
        }
        catch (Exception)
        {
            Dispose();
            chatGui.PrintError(_L("Unable to load plugin, check /xllog for details"), _L("Questionable"));
            throw;
        }
    }

    private ServiceProvider? _serviceProvider;
    private bool _ecommonsInitialized;

    public void Dispose()
    {
        // porting-note(api13): replaces upstream's DisposeAsync. IDalamudPlugin is IDisposable
        // only, so the ServiceProvider is disposed synchronously; ECommons must come down after
        // it, and only if Init actually ran (the constructor calls Dispose on a failed load).
        // Unhook Dalamud event sources (Framework.Update, UiBuilder callbacks, toast hooks, ...)
        // before disposing the container: MS.DI marks the root scope disposed at the START of
        // Dispose, and a framework tick in that window would hit a disposed scope. Its Dispose is
        // idempotent, so MS.DI's own disposal pass is a no-op afterwards. (upstream f8739e3e)
        try
        {
            _serviceProvider?.GetService<DalamudInitializer>()?.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // Container already torn down elsewhere - nothing to unhook.
        }

        _serviceProvider?.Dispose();
        _serviceProvider = null;

        if (_ecommonsInitialized)
        {
            _ecommonsInitialized = false;
            ECommonsMain.Dispose();
        }
    }

    private ServiceProvider BuildAndInitialize()
    {
        ECommonsMain.Init(pluginInterface, this, Module.DalamudReflector);
        _ecommonsInitialized = true;
        WrathIPCWrapper.Init(pluginInterface, WrathError.IPCNotReady | WrathError.Unexpected);
        PunishLibMain.Init(pluginInterface, "Questionable", new AboutPlugin()
        {
            Developer = "alydev",
            Sponsor = "https://ko-fi.com/alydev"
        });

        ServiceCollection serviceCollection = [];
        serviceCollection.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Trace)
            .ClearProviders()
            .AddDalamudLogger(pluginLog, t => t[(t.LastIndexOf('.') + 1)..]));

        // Dalamud services supplied to the plugin constructor - Injectio can't discover these.
        serviceCollection.AddSingleton<IDalamudPlugin>(this);
        serviceCollection.AddSingleton(pluginInterface);
        serviceCollection.AddSingleton(clientState);
        serviceCollection.AddSingleton(targetManager);
        serviceCollection.AddSingleton(framework);
        serviceCollection.AddSingleton(gameGui);
        serviceCollection.AddSingleton(dataManager);
        serviceCollection.AddSingleton(sigScanner);
        serviceCollection.AddSingleton(objectTable);
        serviceCollection.AddSingleton(pluginLog);
        serviceCollection.AddSingleton(condition);
        serviceCollection.AddSingleton(chatGui);
        serviceCollection.AddSingleton(commandManager);
        serviceCollection.AddSingleton(addonLifecycle);
        serviceCollection.AddSingleton(keyState);
        serviceCollection.AddSingleton(contextMenu);
        serviceCollection.AddSingleton(toastGui);
        serviceCollection.AddSingleton(gameInteropProvider);
        serviceCollection.AddSingleton(textureProvider);
        serviceCollection.AddSingleton(new WindowSystem(nameof(Questionable)));

        var savedConfig = (Configuration?)pluginInterface.GetPluginConfig();
        if (savedConfig != null && savedConfig.Version != Configuration.PluginConfigVersion)
        {
            // Backup config when version changes
            pluginInterface.ConfigFile.CopyTo(Path.ChangeExtension(pluginInterface.ConfigFile.FullName, ".json.bak"), overwrite: true);
            savedConfig.Version = Configuration.PluginConfigVersion;
        }

        var configuration = savedConfig ?? new Configuration();
        if (!configuration.AutoRedeemOffResetApplied)
        {
            configuration.ApplyAutoRedeemRewardItemsInitialReset();
            configuration.AutoRedeemOffResetApplied = true;
            pluginInterface.SavePluginConfig(configuration);
        }

        serviceCollection.AddSingleton(configuration);
        Questionable.Utils.LocalizeShortcut.Initialize(configuration, dataManager, clientState);
        Windows.Common.Ui.QstTheme.Initialize(configuration);

        // Injectio-discovered registrations: every class carrying [RegisterSingleton]/[RegisterTransient],
        // and the [RegisterServices] task-registration module in ServiceCollectionExtensions.
        serviceCollection.AddQuestionable();

        // Questpath auto-generation (Questionable/AutoGen): reads game data through Dalamud's Lumina
        // instance, which QuestGameData borrows without disposing.
        serviceCollection.AddSingleton(sp =>
            new QuestGameData(sp.GetRequiredService<IDataManager>().GameData));

        // Same-instance forwards: AetheryteData and JsonSchemaValidator each satisfy an additional
        // service registration that must resolve to the singleton already registered above, not to
        // a second independently-constructed instance.
        serviceCollection.AddSingleton<IAetheryteTerritoryProvider>(sp => sp.GetRequiredService<AetheryteData>());
        serviceCollection.AddSingleton<IQuestValidator>(sp => sp.GetRequiredService<JsonSchemaValidator>());

        // Breaks the QuestController <-> MovementController ctor cycle without handing MovementController
        // the whole IServiceProvider. Once .Value is evaluated the container isn't touched again, so a
        // framework tick during shutdown can't hit a disposed scope through this path.
        // (upstream f8739e3e; this file is pinned, so every new registration upstream adds must be
        // carried over by hand — a missing one is a load-time DI failure the compiler cannot see.)
        serviceCollection.AddSingleton(sp => new Lazy<QuestController>(sp.GetRequiredService<QuestController>));

        var serviceProvider = serviceCollection.BuildServiceProvider();
        Initialize(serviceProvider);
        return serviceProvider;
    }

    // Task factories and executors are now registered in ServiceCollectionExtensions:AddTaskRegistrations

    private static void Initialize(IServiceProvider serviceProvider)
    {
        // Resolve before the registry loads — its constructor discards a bundle left by an older
        // plugin version, so the registry doesn't pick up a stale one.
        PathDataUpdater pathDataUpdater = serviceProvider.GetRequiredService<PathDataUpdater>();
        serviceProvider.GetRequiredService<QuestRegistry>().Reload();
        serviceProvider.GetRequiredService<GatheringPointRegistry>().Reload();
        serviceProvider.GetRequiredService<SinglePlayerDutyConfigComponent>().Reload();
        serviceProvider.GetRequiredService<CommandHandler>();
        serviceProvider.GetRequiredService<ContextMenuController>();
        serviceProvider.GetRequiredService<CraftworksSupplyController>();
        serviceProvider.GetRequiredService<CreditsController>();
        serviceProvider.GetRequiredService<HelpUiController>();
        serviceProvider.GetRequiredService<PointMenuHandler>();
        serviceProvider.GetRequiredService<HousingSelectBlockHandler>();
        serviceProvider.GetRequiredService<YesNoChoiceHandler>();
        serviceProvider.GetRequiredService<DialogueChoiceHandler>();
        serviceProvider.GetRequiredService<ShopController>();
        serviceProvider.GetRequiredService<GrandCompanyExchangeController>();
        serviceProvider.GetRequiredService<ChocoboNamingController>();
        serviceProvider.GetRequiredService<QuestionableIpc>();
        serviceProvider.GetRequiredService<DalamudInitializer>();
        serviceProvider.GetRequiredService<TextAdvanceIpc>();
        serviceProvider.GetRequiredService<YesAlreadyIpc>();

        pathDataUpdater.CheckForUpdates();
    }
}
