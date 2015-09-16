using System;
using System.Drawing;
using System.Linq;
using Gwen.ControlInternal;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Movable window with title bar.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class WindowControl : ResizableControl
    {
        private readonly Dragger titleBar;
        private readonly Label title;
        private readonly CloseButton closeButton;
        private bool deleteOnClose;
        private Modal modal;

        /// <summary>
        /// Window caption.
        /// </summary>
        [JsonProperty]
        public string Title { get { return title.Text; } set { title.Text = value; } }

        /// <summary>
        /// Determines whether the window has close button.
        /// </summary>
        [JsonProperty]
        public bool IsClosable { get { return !closeButton.IsHidden; } set { closeButton.IsHidden = !value; } }

        /// <summary>
        /// Determines whether the control should be disposed on close.
        /// </summary>
        [JsonProperty]
        public bool DeleteOnClose { get { return deleteOnClose; } set { deleteOnClose = value; } }

        /// <summary>
        /// Indicates whether the control is hidden.
        /// </summary>
        public override bool IsHidden
        {
            get { return base.IsHidden; }
            set
            {
                if (!value)
                    BringToFront();
                base.IsHidden = value;
            }
        }

		public void ToggleHidden() {
			IsHidden = !IsHidden;
		}

        public WindowControl(ControlBase parent) : this(parent, "Window", false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowControl"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        /// <param name="caption">Window caption.</param>
        /// <param name="modal">Determines whether the window should be modal.</param>
        public WindowControl(ControlBase parent, string title = "", bool modal = false)
            : base(parent)
        {
            titleBar = new Dragger(this);
            titleBar.Height = 24;
            titleBar.Padding = Gwen.Padding.Zero;
            titleBar.Margin = new Margin(0, 0, 0, 4);
            titleBar.Target = this;
            titleBar.Dock = Pos.Top;

            this.title = new Label(titleBar);
            this.title.Alignment = Pos.Left | Pos.CenterV;
            this.title.Text = title;
            this.title.Dock = Pos.Fill;
            this.title.Padding = new Padding(8, 4, 0, 0);
            this.title.TextColor = Skin.Colors.Window.TitleInactive;

            closeButton = new CloseButton(titleBar, this);
            closeButton.SetSize(24, 24);
            closeButton.Dock = Pos.Right;
            closeButton.Clicked += closeButtonPressed;
            closeButton.IsTabable = false;
            closeButton.Name = "closeButton";

            //Create a blank content control, dock it to the top - Should this be a ScrollControl?
            innerPanel = new ControlBase(this);
            innerPanel.Dock = Pos.Fill;
            getResizer(8).Hide();
            BringToFront();
            IsTabable = false;
            Focus();
            MinimumSize = new Point(100, 40);
            ClampMovement = true;
            KeyboardInputEnabled = false;

            if (modal)
                MakeModal();
        }

		public override void DisableResizing() {
			base.DisableResizing();
			Padding = new Padding(6, 0, 6, 0);
		}

		public void Close() {
			closeButtonPressed(this, EventArgs.Empty);
		}

		protected virtual void closeButtonPressed(ControlBase control, EventArgs args)
        {
            IsHidden = true;

            if (modal != null)
            {
                modal.DelayedDelete();
                modal = null;
            }

            if (deleteOnClose)
            {
                Parent.RemoveChild(this, true);
            }
        }

        /// <summary>
        /// Makes the window modal: covers the whole canvas and gets all input.
        /// </summary>
        /// <param name="dim">Determines whether all the background should be dimmed.</param>
        public void MakeModal(bool dim = false)
        {
            if (modal != null)
                return;

            modal = new Modal(GetCanvas());
            Parent = modal;

            if (dim)
                modal.ShouldDrawBackground = true;
            else
                modal.ShouldDrawBackground = false;
        }

        /// <summary>
        /// Indicates whether the control is on top of its parent's children.
        /// </summary>
        public override bool IsOnTop
        {
            get { return Parent.Children.Where(x => x is WindowControl).Last() == this; }
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            bool hasFocus = IsOnTop;

            if (hasFocus)
				title.TextColor = Skin.Colors.Window.TitleActive;
            else
				title.TextColor = Skin.Colors.Window.TitleInactive;

            skin.DrawWindow(this, titleBar.Bottom, hasFocus);
        }

        /// <summary>
        /// Renders under the actual control (shadows etc).
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void renderUnder(Skin.SkinBase skin)
        {
            base.renderUnder(skin);
            skin.DrawShadow(this);
        }

        public override void Touch()
        {
            base.Touch();
            BringToFront();
        }

        /// <summary>
        /// Renders the focus overlay.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void renderFocus(Skin.SkinBase skin)
        {

        }
    }
}
