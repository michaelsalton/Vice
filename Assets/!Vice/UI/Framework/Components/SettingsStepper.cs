using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vice.UI
{
    [UxmlElement]
    public partial class SettingsStepper : SettingsField<int>
    {
        public const string UssClassName = "settings-stepper";
        public const string ArrowDisabledUssClassName = SettingsDropdown.ArrowUssClassName + "--disabled";

        readonly Label valueLabel;
        readonly Label previousArrow;
        readonly Label nextArrow;
        List<string> choices = new();

        [UxmlAttribute]
        public string Options
        {
            get => string.Join(",", choices);
            set
            {
                choices = string.IsNullOrEmpty(value) ? new List<string>() : value.Split(',').Select(option => option.Trim()).ToList();
                Refresh();
            }
        }

        public SettingsStepper() : this(null) { }

        public SettingsStepper(string label) : base(label)
        {
            AddToClassList(UssClassName);
            previousArrow = CreateArrow("<", -1);
            valueLabel = new Label();
            valueLabel.AddToClassList(SettingsDropdown.ValueUssClassName);
            nextArrow = CreateArrow(">", 1);
            Input.Add(previousArrow);
            Input.Add(valueLabel);
            Input.Add(nextArrow);

            RegisterCallback<NavigationMoveEvent>(OnNavigationMove);
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            base.SetValueWithoutNotify(newValue);
            Refresh();
        }

        void Step(int direction)
        {
            if (choices.Count == 0) return;
            // clamped, not wrapped: Ultra plus one stays Ultra
            value = Mathf.Clamp(value + direction, 0, choices.Count - 1);
        }

        void OnNavigationMove(NavigationMoveEvent moveEvent)
        {
            int direction = HorizontalDirection(moveEvent);
            if (direction == 0) return;
            Step(direction);
            ConsumeNavigation(moveEvent);
        }

        void Refresh()
        {
            bool hasChoice = value >= 0 && value < choices.Count;
            valueLabel.text = hasChoice ? choices[value] : "-";
            previousArrow.EnableInClassList(ArrowDisabledUssClassName, value <= 0);
            nextArrow.EnableInClassList(ArrowDisabledUssClassName, value >= choices.Count - 1);
        }

        Label CreateArrow(string glyph, int direction)
        {
            var arrow = new Label(glyph);
            arrow.AddToClassList(SettingsDropdown.ArrowUssClassName);
            arrow.RegisterCallback<ClickEvent>(clickEvent =>
            {
                Step(direction);
                clickEvent.StopPropagation();
            });
            return arrow;
        }
    }
}
