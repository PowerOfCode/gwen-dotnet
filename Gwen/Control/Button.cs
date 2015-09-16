using System;
using Gwen.Input;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Button control.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class Button : Label
    {
        private bool depressed;
        private bool toggle;
        private bool toggleStatus;
        private bool centerImage;
        private ImagePanel image;

        /// <summary>
        /// Invoked when the button is pressed.
        /// </summary>
		public event GwenEventHandler<EventArgs> Pressed;

        /// <summary>
        /// Invoked when the button is released.
        /// </summary>
		public event GwenEventHandler<EventArgs> Released;

        /// <summary>
        /// Invoked when the button's toggle state has changed.
        /// </summary>
		public event GwenEventHandler<EventArgs> Toggled;

        /// <summary>
        /// Invoked when the button's toggle state has changed to On.
        /// </summary>
		public event GwenEventHandler<EventArgs> ToggledOn;

        /// <summary>
        /// Invoked when the button's toggle state has changed to Off.
        /// </summary>
		public event GwenEventHandler<EventArgs> ToggledOff;

        /// <summary>
        /// Indicates whether the button is depressed.
        /// </summary>
        public bool IsDepressed
        {
            get { return depressed; }
            set
            {
                if (depressed == value)
                    return;
                depressed = value;
                Redraw();
            }
        }

        /// <summary>
        /// Indicates whether the button is toggleable.
        /// </summary>
        [JsonProperty]
        public bool IsToggle { get { return toggle; } set { toggle = value; } }

        /// <summary>
        /// Determines the button's toggle state.
        /// </summary>
        [JsonProperty]
        public bool ToggleState
        {
            get { return toggleStatus; }
            set
            {
                if (!toggle) return;
                if (toggleStatus == value) return;

                toggleStatus = value;

                if (Toggled != null)
					Toggled.Invoke(this, EventArgs.Empty);

                if (toggleStatus)
                {
                    if (ToggledOn != null)
						ToggledOn.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    if (ToggledOff != null)
						ToggledOff.Invoke(this, EventArgs.Empty);
                }

                Redraw();
            }
        }

        /// <summary>
        /// Control constructor.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Button(ControlBase parent)
            : base(parent)
        {
			AutoSizeToContents = false;
            SetSize(100, 20);
            MouseInputEnabled = true;
            Alignment = Pos.Center;
            TextPadding = new Padding(3, 3, 3, 3);
        }

        /// <summary>
        /// Toggles the button.
        /// </summary>
        public virtual void Toggle()
        {
            ToggleState = !ToggleState;
        }

        /// <summary>
        /// "Clicks" the button.
        /// </summary>
        public virtual void Press(ControlBase control = null)
        {
            onClicked(0, 0);
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            base.render(skin);

            if (ShouldDrawBackground)
            {
                bool drawDepressed = IsDepressed && IsHovered;
                if (IsToggle)
                    drawDepressed = drawDepressed || ToggleState;

                bool bDrawHovered = IsHovered && shouldDrawHover;

                skin.DrawButton(this, drawDepressed, bDrawHovered, IsDisabled);
            }
        }

        /// <summary>
        /// Handler invoked on mouse click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="down">If set to <c>true</c> mouse button is down.</param>
        protected override void onMouseClickedLeft(int x, int y, bool down)
        {
            //base.onMouseClickedLeft(x, y, down);
            if (down)
            {
                IsDepressed = true;
                InputHandler.MouseFocus = this;
                if (Pressed != null)
                    Pressed.Invoke(this, EventArgs.Empty);
            }
            else
            {
                if (IsHovered && depressed)
                {
                    onClicked(x, y);
                }

                IsDepressed = false;
                InputHandler.MouseFocus = null;
                if (Released != null)
					Released.Invoke(this, EventArgs.Empty);
            }

            Redraw();
        }

        /// <summary>
        /// Internal OnPressed implementation.
        /// </summary>
        protected virtual void onClicked(int x, int y)
        {
            if (IsToggle)
            {
                Toggle();
            }

			base.onMouseClickedLeft(x, y, true);
        }

        /// <summary>
        /// Sets the button's image.
        /// </summary>
        /// <param name="textureName">Texture name. Null to remove.</param>
        /// <param name="center">Determines whether the image should be centered.</param>
        public virtual void SetImage(string textureName, bool center = false)
        {
            if (String.IsNullOrEmpty(textureName))
            {
                if (image != null)
                    image.Dispose();
                image = null;
                return;
            }

            if (image == null)
            {
                image = new ImagePanel(this);
            }

            image.ImageName = textureName;
            image.SizeToContents( );
            image.SetPosition(Math.Max(Padding.Left, 2), 2);
            image.KeyboardInputEnabled = false;
            image.MouseInputEnabled = false;
            centerImage = center;

            TextPadding = new Padding(image.Right + 2, TextPadding.Top, TextPadding.Right, TextPadding.Bottom);
        }

        /// <summary>
        /// Sizes to contents.
        /// </summary>
        public override void SizeToContents()
        {
            base.SizeToContents();
            if (image != null)
            {
                int height = image.Height + 4;
                if (Height < height)
                {
                    Height = height;
                }
            }
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
			return base.onKeySpace(down);
			//if (down)
			//    onClicked(0, 0);
			//return true;
        }

        /// <summary>
        /// Default accelerator handler.
        /// </summary>
        protected override void onAccelerator()
        {
            onClicked(0, 0);
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            base.layout(skin);
            if (image != null)
            {
                Align.CenterVertically(image);

                if (centerImage)
                    Align.CenterHorizontally(image);
            }
        }

        /// <summary>
        /// Updates control colors.
        /// </summary>
        public override void UpdateColors()
        {
            if (IsDisabled)
            {
                TextColor = Skin.Colors.Button.Disabled;
                return;
            }

            if (IsDepressed || ToggleState)
            {
                TextColor = Skin.Colors.Button.Down;
                return;
            }

            if (IsHovered)
            {
                TextColor = Skin.Colors.Button.Hover;
                return;
            }

            TextColor = Skin.Colors.Button.Normal;
        }

        /// <summary>
        /// Handler invoked on mouse double click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        protected override void onMouseDoubleClickedLeft(int x, int y)
        {
			base.onMouseDoubleClickedLeft(x, y);
            onMouseClickedLeft(x, y, true);
        }
    }
}
