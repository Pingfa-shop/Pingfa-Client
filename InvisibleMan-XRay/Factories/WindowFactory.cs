using System.Windows;

namespace InvisibleManXRay.Factories
{
    using Core;
    using Models;
    using Managers;
    using Services;
    using Handlers;
    using Values;

    public class WindowFactory
    {
        private InvisibleManXRayCore core;
        private HandlersManager handlersManager;

        private LocalizationService LocalizationService => ServiceLocator.Get<LocalizationService>();

        public void Setup(InvisibleManXRayCore core, HandlersManager handlersManager)
        {
            this.core = core;
            this.handlersManager = handlersManager;
        }

        public MainWindow GetMainWindow() => Application.Current.MainWindow as MainWindow;

        public MainWindow CreateMainWindow()
        {
            ConfigHandler configHandler = handlersManager.GetHandler<ConfigHandler>();
            SettingsHandler settingsHandler = handlersManager.GetHandler<SettingsHandler>();
            LinkHandler linkHandler = handlersManager.GetHandler<LinkHandler>();

            MainWindow mainWindow = new MainWindow();
            mainWindow.Setup(
                getConfig: configHandler.GetCurrentConfig,
                loadConfig: core.LoadConfig,
                enableMode: core.EnableMode,
                openServerWindow: CreateServerWindow,
                openSettingsWindow: CreateSettingsWindow,
                onRunServer: core.Run,
                onStopServer: core.Stop,
                onCancelServer: core.Cancel,
                onDisableMode: core.DisableMode,
                onGenerateClientId: settingsHandler.GenerateClientId,
                onCustomLinkClick: linkHandler.OpenCustomLink
            );
            
            return mainWindow;

            bool IsNeedToShowPolicyWindow() => false;
        }

        public SettingsWindow CreateSettingsWindow()
        {
            SettingsHandler settingsHandler = handlersManager.GetHandler<SettingsHandler>();
            NotifyHandler notifyHandler = handlersManager.GetHandler<NotifyHandler>();
            LocalizationHandler localizationHandler = handlersManager.GetHandler<LocalizationHandler>();

            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Setup(
                getLanguage: settingsHandler.UserSettings.GetLanguage,
                getMode: settingsHandler.UserSettings.GetMode,
                getProtocol: settingsHandler.UserSettings.GetProtocol,
                getSystemProxyUsed: settingsHandler.UserSettings.GetSystemProxyUsed,
                getUdpEnabled: settingsHandler.UserSettings.GetUdpEnabled,
                getRunningAtStartupEnabled: settingsHandler.UserSettings.GetRunningAtStartupEnabled,
                getProxyPort: settingsHandler.UserSettings.GetProxyPort,
                getTunPort: settingsHandler.UserSettings.GetTunPort,
                getTestPort: settingsHandler.UserSettings.GetTestPort,
                getDeviceIp: settingsHandler.UserSettings.GetTunIp,
                getDns: settingsHandler.UserSettings.GetDns,
                getLogLevel: settingsHandler.UserSettings.GetLogLevel,
                getLogPath: settingsHandler.UserSettings.GetLogPath,
                onUpdateUserSettings: UpdateUserSettings
            );

            SetupLocalizedWindowTitle(
                window: settingsWindow,
                term: Localization.WINDOW_TITLE_SETTINGS
            );

            return settingsWindow;

            void UpdateUserSettings(UserSettings userSettings)
            {
                settingsHandler.UpdateUserSettings(userSettings);
                localizationHandler.TryApplyCurrentLanguage();
                notifyHandler.InitializeNotifyIcon();
                notifyHandler.CheckModeItem(userSettings.GetMode());
                GetMainWindow().TryDisableModeAndRerun();
            }
        }

        public ServerWindow CreateServerWindow()
        {
            ConfigHandler configHandler = handlersManager.GetHandler<ConfigHandler>();
            TemplateHandler templateHandler = handlersManager.GetHandler<TemplateHandler>();
            SettingsHandler settingsHandler = handlersManager.GetHandler<SettingsHandler>();
            MainWindow mainWindow = GetMainWindow();
            
            ServerWindow serverWindow = new ServerWindow();
            serverWindow.Setup(
                getCurrentConfigPath: settingsHandler.UserSettings.GetCurrentConfigPath,
                isCurrentPathEqualRootConfigPath: configHandler.IsCurrentPathEqualRootConfigPath,
                getAllGeneralConfigs: configHandler.GetAllGeneralConfigs,
                getAllSubscriptionConfigs: configHandler.GetAllSubscriptionConfigs,
                getAllGroups: configHandler.GetAllGroups,
                convertLinkToConfig: templateHandler.ConverLinkToConfig,
                convertLinkToSubscription: templateHandler.ConvertLinkToSubscription,
                loadConfig: core.LoadConfig,
                testConnection: core.Test,
                getLogPath: settingsHandler.UserSettings.GetLogPath,
                onCopyConfig: configHandler.CopyConfig,
                onCreateConfig: configHandler.CreateConfig,
                onCreateSubscription: configHandler.CreateSubscription,
                onDeleteSubscription: configHandler.DeleteSubscription,
                onDeleteConfig: configHandler.LoadFiles,
                onUpdateConfig: UpdateConfig
            );

            SetupLocalizedWindowTitle(
                window: serverWindow,
                term: Localization.WINDOW_TITLE_SERVER
            );
            
            return serverWindow;

            void UpdateConfig(string path)
            {
                settingsHandler.UpdateCurrentConfigPath(path);
                mainWindow.UpdateUI();
                mainWindow.TryRerun();
            }
        }

        private void SetupLocalizedWindowTitle(Window window, string term)
        {
            window.Title = $"Pingfa - {LocalizationService.GetTerm(term)}";
        }
    }
}