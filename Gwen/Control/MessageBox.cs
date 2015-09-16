using System;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Simple message box.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class MessageBox : WindowControl
    {
        private readonly Button button;
        private readonly Label label; // should be rich label with maxwidth = parent

        /// <summary>
        /// Invoked when the message box has been dismissed.
        /// </summary>
        public GwenEventHandler<EventArgs> Dismissed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageBox"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        /// <param name="text">Message to display.</param>
        /// <param name="caption">Window caption.</param>
        public MessageBox(ControlBase parent, string text, string caption = "")
            : base(parent, caption, true)
        {
            DeleteOnClose = true;

            label = new Label(innerPanel);
            label.Text = text;
            label.Margin = Margin.Five;
            label.Dock = Pos.Top;
            label.Alignment = Pos.Center;

            button = new Button(innerPanel);
            button.Text = "OK"; // todo: parametrize buttons
            button.Clicked += closeButtonPressed;
            button.Clicked += dismissedHandler;
            button.Margin = Margin.Five;
            button.SetSize(50, 20);

            base.DisableResizing();

            Align.Center(this);
        }

		private void dismissedHandler(ControlBase control, EventArgs args)
        {
            if (Dismissed != null)
                Dismissed.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            base.layout(skin);

            Align.PlaceDownLeft(button, label, 10);
            Align.CenterHorizontally(button);
            innerPanel.SizeToChildren();
            innerPanel.Height += 10;
            SizeToChildren();
        }
    }
}
