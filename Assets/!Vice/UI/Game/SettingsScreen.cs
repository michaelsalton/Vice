using UnityEngine.UIElements;
using Vice.UI;

namespace Vice
{
    public class SettingsScreen : UIScreen
    {
        readonly GameSettings settings;

        public SettingsScreen(VisualTreeAsset template, GameSettings settings,
            UIScreen display, UIScreen graphics, UIScreen controls, UIScreen audio) : base(template)
        {
            this.settings = settings;
            Root.Q<Button>("display").clicked += () => Open(display);
            Root.Q<Button>("graphics").clicked += () => Open(graphics);
            Root.Q<Button>("controls").clicked += () => Open(controls);
            Root.Q<Button>("audio").clicked += () => Open(audio);
        }

        // one builder for every menu that offers settings, so the pause and main menus can't drift apart
        public static SettingsScreen Create(GameSettings settings, UIRoot uiRoot, VisualTreeAsset settingsTemplate,
            VisualTreeAsset displayTemplate, VisualTreeAsset graphicsTemplate, VisualTreeAsset controlsTemplate, VisualTreeAsset audioTemplate)
        {
            var displayPage = CreatePage(displayTemplate, new DisplaySettingsViewModel(settings));
            // rescaling the panel mid-drag would move the slider out from under the cursor, so it applies on leaving the page
            displayPage.Closed += () => uiRoot.Scale = settings.UiScale;
            return new SettingsScreen(settingsTemplate, settings,
                displayPage,
                CreatePage(graphicsTemplate, settings),
                CreatePage(controlsTemplate, settings),
                CreatePage(audioTemplate, settings));
        }

        // settings pages have no logic of their own; every control binds to the page's data source
        static UIScreen CreatePage(VisualTreeAsset template, object dataSource)
        {
            var page = new UIScreen(template);
            page.Root.dataSource = dataSource;
            return page;
        }

        // one write per visit to settings, instead of one per slider tick
        protected override void OnClosed() => settings.Save();
    }
}
