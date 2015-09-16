using System;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// CheckBox control.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class CheckBox : Button
    {
        private bool isChecked;

        /// <summary>
        /// Indicates whether the checkbox is checked.
        /// </summary>
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                if (isChecked == value) return;
                isChecked = value;
                onCheckChanged();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckBox"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public CheckBox(ControlBase parent)
            : base(parent)
        {
            SetSize(15, 15);
			IsToggle = true;
        }

        /// <summary>
        /// Toggles the checkbox.
        /// </summary>
        public override void Toggle()
        {
            base.Toggle();
            IsChecked = !IsChecked;
        }

        /// <summary>
        /// Invoked when the checkbox has been checked.
        /// </summary>
		public event GwenEventHandler<EventArgs> Checked;

        /// <summary>
        /// Invoked when the checkbox has been unchecked.
        /// </summary>
		public event GwenEventHandler<EventArgs> UnChecked;

        /// <summary>
        /// Invoked when the checkbox state has been changed.
        /// </summary>
		public event GwenEventHandler<EventArgs> CheckChanged;

        /// <summary>
        /// Determines whether unchecking is allowed.
        /// </summary>
        protected virtual bool allowUncheck { get { return true; } }

        /// <summary>
        /// Handler for CheckChanged event.
        /// </summary>
        protected virtual void onCheckChanged()
        {
            if (IsChecked)
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
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            base.render(skin);
            skin.DrawCheckBox(this, isChecked, IsDepressed);
        }

        /// <summary>
        /// Internal OnPressed implementation.
        /// </summary>
        protected override void onClicked(int x, int y)
        {
            if (IsDisabled)
                return;

            if (IsChecked && !allowUncheck)
            {
                return;
            }

			base.onClicked(x, y);
        }
    }
}
