using System;
using System.Windows.Forms;
using Gwen.ControlInternal;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Properties table.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class Properties : ControlBase
    {
        private readonly SplitterBar splitterBar;

        /// <summary>
        /// Returns the width of the first column (property names).
        /// </summary>
        public int SplitWidth { get { return splitterBar.X; } } // todo: rename?

        /// <summary>
        /// Invoked when a property value has been changed.
        /// </summary>
		public event GwenEventHandler<EventArgs> ValueChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="Properties"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Properties(ControlBase parent)
            : base(parent)
        {
            splitterBar = new SplitterBar(this);
            splitterBar.SetPosition(80, 0);
            splitterBar.Cursor = Cursors.SizeWE;
            splitterBar.Dragged += onSplitterMoved;
            splitterBar.ShouldDrawBackground = false;
        }

        /// <summary>
        /// Function invoked after layout.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void postLayout(Skin.SkinBase skin)
        {
            splitterBar.Height = 0;

            if (SizeToChildren(false, true))
            {
                InvalidateParent();
            }

            splitterBar.SetSize(3, Height);
        }

        /// <summary>
        /// Handles the splitter moved event.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected virtual void onSplitterMoved(ControlBase control, EventArgs args)
        {
            invalidateChildren();
        }

        /// <summary>
        /// Adds a new text property row.
        /// </summary>
        /// <param name="label">Property name.</param>
        /// <param name="value">Initial value.</param>
        /// <returns>Newly created row.</returns>
        public PropertyRow Add(string label, string value="")
        {
            return Add(label, new Property.Text(this), value);
        }

        /// <summary>
        /// Adds a new property row.
        /// </summary>
        /// <param name="label">Property name.</param>
        /// <param name="prop">Property control.</param>
        /// <param name="value">Initial value.</param>
        /// <returns>Newly created row.</returns>
        public PropertyRow Add(string label, Property.PropertyBase prop, string value="")
        {
            PropertyRow row = new PropertyRow(this, prop);
            row.Dock = Pos.Top;
            row.Label = label;
            row.ValueChanged += onRowValueChanged;

            prop.setValue(value, true);

            splitterBar.BringToFront();
            return row;
        }

		private void onRowValueChanged(ControlBase control, EventArgs args)
        {
            if (ValueChanged != null)
				ValueChanged.Invoke(control, EventArgs.Empty);
        }

        /// <summary>
        /// Deletes all rows.
        /// </summary>
        public void DeleteAll()
        {
            innerPanel.DeleteAllChildren();
        }
    }
}
