using System;
using UnityEngine.UIElements;
using Vice.UI;

namespace Vice
{
    public class PauseScreen : UIScreen
    {
        readonly Button saveButton;
        readonly string saveLabel;

        public PauseScreen(VisualTreeAsset template, UIScreen settingsScreen, Func<bool> save, Action quitToMenu) : base(template)
        {
            saveButton = Root.Q<Button>("save");
            saveLabel = saveButton.text;
            Root.Q<Button>("resume").clicked += Close;
            // the label is the only confirmation, so a failed write has to say so
            saveButton.clicked += () => saveButton.text = save() ? "Saved" : "Save failed";
            Root.Q<Button>("settings").clicked += () => Open(settingsScreen);
            Root.Q<Button>("quit").clicked += quitToMenu;
        }

        // the next pause starts at "Save", not at the last result
        protected override void OnClosed() => saveButton.text = saveLabel;
    }
}
