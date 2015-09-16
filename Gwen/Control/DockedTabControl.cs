using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Docked tab control.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class DockedTabControl : TabControl
    {
        private readonly TabTitleBar titleBar;

        /// <summary>
        /// Determines whether the title bar is visible.
        /// </summary>
        public bool TitleBarVisible { get { return !titleBar.IsHidden; } set { titleBar.IsHidden = !value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="DockedTabControl"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public DockedTabControl(ControlBase parent)
            : base(parent)
        {
            Dock = Pos.Fill;

            titleBar = new TabTitleBar(this);
            titleBar.Dock = Pos.Top;
            titleBar.IsHidden = true;
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            TabStrip.IsHidden = (TabCount <= 1);
            updateTitleBar();
            base.layout(skin);
        }

        private void updateTitleBar()
        {
            if (CurrentButton == null)
                return;

            titleBar.UpdateFromTab(CurrentButton);
        }

        public override void DragAndDrop_StartDragging(DragDrop.Package package, int x, int y)
        {
            base.DragAndDrop_StartDragging(package, x, y);

            IsHidden = true;
            // This hiding our parent thing is kind of lousy.
            Parent.IsHidden = true;
        }

        public override void DragAndDrop_EndDragging(bool success, int x, int y)
        {
            IsHidden = false;
            if (!success)
            {
                Parent.IsHidden = false;
            }
        }

        public void MoveTabsTo(DockedTabControl target)
        {
            var children = TabStrip.Children.ToArray(); // copy because collection will be modified
            foreach (ControlBase child in children)
            {
                TabButton button = child as TabButton;
                if (button == null)
                    continue;
                target.AddPage(button);
            }
            Invalidate();
        }
    }
}
