using System;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Tree control.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class TreeControl : TreeNode
    {
        private readonly ScrollControl scrollControl;
        private bool multiSelect;

        /// <summary>
        /// Determines if multiple nodes can be selected at the same time.
        /// </summary>
        public bool AllowMultiSelect { get { return multiSelect; } set { multiSelect = value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeControl"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public TreeControl(ControlBase parent)
            : base(parent)
        {
            treeControl = this;

            RemoveChild(toggleButton, true);
            toggleButton = null;
            RemoveChild(title, true);
            title = null;
            RemoveChild(innerPanel, true);
            innerPanel = null;

            multiSelect = false;

            scrollControl = new ScrollControl(this);
            scrollControl.Dock = Pos.Fill;
            scrollControl.EnableScroll(false, true);
            scrollControl.AutoHideBars = true;
            scrollControl.Margin = Margin.One;

            innerPanel = scrollControl;

            scrollControl.SetInnerSize(1000, 1000); // todo: why such arbitrary numbers?

			Dock = Pos.None;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            if (ShouldDrawBackground)
                skin.DrawTreeControl(this);
        }

        /// <summary>
        /// Handler invoked when control children's bounds change.
        /// </summary>
        /// <param name="oldChildBounds"></param>
        /// <param name="child"></param>
        protected override void onChildBoundsChanged(System.Drawing.Rectangle oldChildBounds, ControlBase child)
        {
            if (scrollControl != null)
                scrollControl.UpdateScrollBars();
        }

        /// <summary>
        /// Removes all child nodes.
        /// </summary>
        public virtual void RemoveAll()
        {
            scrollControl.DeleteAll();
        }

        /// <summary>
        /// Handler for node added event.
        /// </summary>
        /// <param name="node">Node added.</param>
        public virtual void OnNodeAdded(TreeNode node)
        {
            node.LabelPressed += onNodeSelected;
        }

        /// <summary>
        /// Handler for node selected event.
        /// </summary>
        /// <param name="Control">Node selected.</param>
		protected virtual void onNodeSelected(ControlBase Control, EventArgs args)
        {
            if (!multiSelect /*|| InputHandler.InputHandler.IsKeyDown(Key.Control)*/)
                UnselectAll();
        }
    }
}
