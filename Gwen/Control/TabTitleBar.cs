﻿using Gwen.DragDrop;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Titlebar for DockedTabControl.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class TabTitleBar : Label
    {
        public TabTitleBar(ControlBase parent) : base(parent)
        {
			AutoSizeToContents = false;
            MouseInputEnabled = true;
            TextPadding = new Padding(5, 2, 5, 2);
            Padding = new Padding(1, 2, 1, 2);

            DragAndDrop_SetPackage(true, "TabWindowMove");
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            skin.DrawTabTitleBar(this);
        }

        public override void DragAndDrop_StartDragging(Package package, int x, int y)
        {
            DragAndDrop.SourceControl = Parent;
            DragAndDrop.SourceControl.DragAndDrop_StartDragging(package, x, y);
        }

        public void UpdateFromTab(TabButton button)
        {
            Text = button.Text;
            SizeToContents();
        }
    }
}
