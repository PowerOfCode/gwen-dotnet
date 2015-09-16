using Gwen.Input;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// RadioButton with label.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class LabeledRadioButton : ControlBase
    {
        private readonly RadioButton radioButton;
        private readonly Label label;

        /// <summary>
        /// Label text.
        /// </summary>
        public string Text { get { return label.Text; } set { label.Text = value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabeledRadioButton"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public LabeledRadioButton(ControlBase parent)
            : base(parent)
        {
			MouseInputEnabled = true;
            SetSize(100, 20);

            radioButton = new RadioButton(this);
            //m_RadioButton.Dock = Pos.Left; // no docking, it causes resizing
            //m_RadioButton.Margin = new Margin(0, 2, 2, 2);
            radioButton.IsTabable = false;
            radioButton.KeyboardInputEnabled = false;

            label = new Label(this);
            label.Alignment = Pos.CenterV | Pos.Left;
            label.Text = "Radio Button";
			label.Clicked += delegate(ControlBase control, ClickedEventArgs args) { radioButton.Press(control); };
            label.IsTabable = false;
            label.KeyboardInputEnabled = false;
        }

        protected override void layout(Skin.SkinBase skin)
        {
            // ugly stuff because we don't have anchoring without docking (docking resizes children)
            if (label.Height > radioButton.Height) // usually radio is smaller than label so it gets repositioned to avoid clipping with negative Y
            {
                radioButton.Y = (label.Height - radioButton.Height)/2;
            }
            Align.PlaceRightBottom(label, radioButton);
            SizeToChildren();
            base.layout(skin);
        }

        /// <summary>
        /// Renders the focus overlay.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void renderFocus(Skin.SkinBase skin)
        {
            if (InputHandler.KeyboardFocus != this) return;
            if (!IsTabable) return;

            skin.DrawKeyboardHighlight(this, RenderBounds, 0);
        }

        // todo: would be nice to remove that
        internal RadioButton RadioButton { get { return radioButton; } }

        /// <summary>
        /// Handler for Space keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onKeySpace(bool down)
        {
            if (down)
                radioButton.IsChecked = !radioButton.IsChecked;
            return true;
        }

        /// <summary>
        /// Selects the radio button.
        /// </summary>
        public virtual void Select()
        {
            radioButton.IsChecked = true;
        }
    }
}
