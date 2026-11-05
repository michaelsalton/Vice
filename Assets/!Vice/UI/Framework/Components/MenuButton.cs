using UnityEngine.UIElements;

namespace Vice.UI
{
    [UxmlElement]
    public partial class MenuButton : Button
    {
        public const string UssClassName = "menu-button";

        public MenuButton()
        {
            AddToClassList(UssClassName);
            AddToClassList(UIScreen.MenuItemClass);
        }
    }
}
