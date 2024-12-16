namespace InvisibleManXRay.Managers.Initializers
{
    using Models;
    using Managers;
    using Services;
    using Handlers;

    public class ServicesInitializer
    {
        public ServicesManager ServicesManager { get; private set; }

        public void Register()
        {
            ServicesManager = new ServicesManager();

            ServicesManager.AddService(new LocalizationService());
        }

        public void Setup(HandlersManager handlersManager)
        {
            SetupServiceLocator();
            SetupLocalizationService();
            SetupMainService();

            void SetupServiceLocator()
            {
                ServiceLocator.Setup(
                    servicesManager: ServicesManager
                );
            }

            void SetupLocalizationService()
            {
                ServicesManager.GetService<LocalizationService>().Setup(
                    getLocalizationResource: handlersManager.GetHandler<LocalizationHandler>().GetLocalizationResource
                );
            }

            void SetupMainService()
            {
                SettingsHandler settingsHandler = handlersManager.GetHandler<SettingsHandler>();
            }
        }
    }
}