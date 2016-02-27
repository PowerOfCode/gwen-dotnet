using System;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// CheckBox with label.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class LabeledCheckBox : ControlBase
    {
        private readonly CheckBox checkBox;
        private readonly Label label;

        /// <summary>
        /// Invoked when the control has been checked.
        /// </summary>
        public event GwenEventHandler<EventArgs> Checked;

        /// <summary>
        /// Invoked when the control has been unchecked.
        /// </summary>
        public event GwenEventHandler<EventArgs> UnChecked;

        /// <summary>
        /// Invoked when the control's check has been changed.
        /// </summary>
        public event GwenEventHandler<EventArgs> CheckChanged;

        /// <summary>
        /// Indicates whether the control is checked.
        /// </summary>
        public bool IsChecked { get { return checkBox.IsChecked; } set { checkBox.IsChecked = value; } }

        /// <summary>
        /// Label text.
        /// </summary>
        public string Text { get { return label.Text; } set { label.Text = value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabeledCheckBox"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public LabeledCheckBox(ControlBase parent)
            : base(parent)
        {
            SetSize(200, 19);
            checkBox = new CheckBox(this);
            checkBox.Dock = Pos.Left;
            checkBox.Margin = new Margin(0, 2, 2, 2);
            checkBox.IsTabable = false;
            checkBox.CheckChanged += onCheckChanged;

            label = new Label(this);
            label.Dock = Pos.Fill;
			label.Clicked += delegate(ControlBase Control, ClickedEventArgs args) { checkBox.Press(Control); };
            label.IsTabable = false;

            IsTabable = false;
        }

        /// <summary>
        /// Handler for CheckChanged event.
        /// </summary>
        protected virtual void onCheckChanged(ControlBase control, EventArgs Args)
        {
            if (checkBox.IsChecked)
            {
                if (Checked != null)
					Checked.Invoke(this, EventArgs.Empty);
            }
            else
            {
                if (UnChecked != null)
					UnChecked.Invoke(this, EventArgs.Empty);
            }

            if (CheckChanged != null)
				CheckChanged.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handler for Space keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onKeySpace(bool down)
        {
            base.onKeySpace(down);
            if (!down)
                checkBox.IsChecked = !checkBox.IsChecked;
            return true;
        }
		
		/// <summary>
        /// Resizes the control to fit its children.
        /// </summary>
        /// <param name="width">Determines whether to change control's width.</param>
        /// <param name="height">Determines whether to change control's height.</param>
        /// <returns>True if bounds changed.</returns>
        public override bool SizeToChildren(bool width = true, bool height = true)
        {
            label.SizeToContents();

            int w = width ? label.Width + label.Margin.Left + label.Margin.Right + checkBox.Width + checkBox.Margin.Left + checkBox.Margin.Right : Width;
            int h = height ? Math.Max(label.Height + label.Margin.Top + label.Margin.Bottom, checkBox.Height + checkBox.Margin.Top + checkBox.Margin.Top) : Height;

            return SetSize(w, h);
        }
    }
}
