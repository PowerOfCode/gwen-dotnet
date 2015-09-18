using System.Drawing;
using Gwen.ControlInternal;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Static text label.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class Label : ControlBase
    {
        protected readonly TextControl text;
        private Pos align;
        private Padding textPadding;
        private bool autoSizeToContents;

        /// <summary>
        /// Text alignment.
        /// </summary>
        [JsonProperty]
        public Pos Alignment { get { return align; } set { align = value; Invalidate(); } }

        /// <summary>
        /// Text.
        /// </summary>
        [JsonProperty]
        public virtual string Text { get { return text.Text; } set { SetText(value); } }

        /// <summary>
        /// Font.
        /// </summary>
        [JsonProperty]
        public Font Font
        {
            get { return text.Font; }
            set
            {
                text.Font = value;
                if (autoSizeToContents)
                    SizeToContents();
                Invalidate();
            }
        }

        /// <summary>
        /// Internal Text color (changes will be lost!). Use TextColorOverride to set the TextColor.
        /// </summary>
        public Color TextColor { get { return text.TextColor; } set { text.TextColor = value; } }

        /// <summary>
        /// Custom Text color.
        /// </summary>
        [JsonProperty]
        public Color TextColorOverride { get { return text.TextColorOverride; } set { text.TextColorOverride = value; } }

        /// <summary>
        /// Text override - used to display different string.
        /// </summary>
        public string TextOverride { get { return text.TextOverride; } set { text.TextOverride = value; } }

        /// <summary>
        /// Width of the text (in pixels).
        /// </summary>
        public int TextWidth { get { return text.Width; } }

        /// <summary>
        /// Height of the text (in pixels).
        /// </summary>
        public int TextHeight { get { return text.Height; } }

        public int TextX { get { return text.X; } }
        public int TextY { get { return text.Y; } }

        /// <summary>
        /// Text length (in characters).
        /// </summary>
        public int TextLength { get { return text.Length; } }
        public int TextRight { get { return text.Right; } }
        public virtual void MakeColorNormal() { TextColor = Skin.Colors.Label.Default; }
        public virtual void MakeColorBright() { TextColor = Skin.Colors.Label.Bright; }
        public virtual void MakeColorDark() { TextColor = Skin.Colors.Label.Dark; }
        public virtual void MakeColorHighlight() { TextColor = Skin.Colors.Label.Highlight; }

        /// <summary>
        /// Specifies wheter the renderer should cache this text or not. Set this to false for dynamic or frequently changing texts.
        /// </summary>
        public bool ShouldCacheText { get { return text.ShouldCacheText; } set { text.ShouldCacheText = value; } }

        /// <summary>
        /// Determines if the control should autosize to its text.
        /// </summary>
        [JsonProperty]
        public bool AutoSizeToContents { get { return autoSizeToContents; } set { autoSizeToContents = value; Invalidate(); InvalidateParent(); } }

        /// <summary>
        /// Text padding.
        /// </summary>
        [JsonProperty]
        public Padding TextPadding { get { return textPadding; } set { textPadding = value; Invalidate(); InvalidateParent(); } }

		public override event ControlBase.GwenEventHandler<ClickedEventArgs> Clicked {
			add {
				base.Clicked += value;
				MouseInputEnabled = ClickEventAssigned;
			}
			remove {
				base.Clicked -= value;
				MouseInputEnabled = ClickEventAssigned;
			}
		}

		public override event ControlBase.GwenEventHandler<ClickedEventArgs> DoubleClicked {
			add {
				base.DoubleClicked += value;
				MouseInputEnabled = ClickEventAssigned;
			}
			remove {
				base.DoubleClicked -= value;
				MouseInputEnabled = ClickEventAssigned;
			}
		}

		public override event ControlBase.GwenEventHandler<ClickedEventArgs> RightClicked {
			add {
				base.RightClicked += value;
				MouseInputEnabled = ClickEventAssigned;
			}
			remove {
				base.RightClicked -= value;
				MouseInputEnabled = ClickEventAssigned;
			}
		}

		public override event ControlBase.GwenEventHandler<ClickedEventArgs> DoubleRightClicked {
			add {
				base.DoubleRightClicked += value;
				MouseInputEnabled = ClickEventAssigned;
			}
			remove {
				base.DoubleRightClicked -= value;
				MouseInputEnabled = ClickEventAssigned;
			}
		}


        /// <summary>
        /// Initializes a new instance of the <see cref="Label"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public Label(ControlBase parent) : base(parent)
        {
            text = new TextControl(this);
            //m_Text.Font = Skin.DefaultFont;

			MouseInputEnabled = false;
            SetSize(100, 10);
            Alignment = Pos.Left | Pos.Top;

            autoSizeToContents = true;
        }

        /// <summary>
        /// Returns index of the character closest to specified point (in canvas coordinates).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        protected virtual Point getClosestCharacter(int x, int y)
        {
            return new Point(text.GetClosestCharacter(text.CanvasPosToLocal(new Point(x, y))), 0);
        }

        /// <summary>
        /// Sets the position of the internal text control.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        protected void setTextPosition(int x, int y)
        {
            text.SetPosition(x, y);
        }

        /// <summary>
        /// Handler for text changed event.
        /// </summary>
        protected virtual void onTextChanged() {}

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            base.layout(skin);

            Pos align = this.align;

            if (autoSizeToContents)
                SizeToContents();

            int x = textPadding.Left + Padding.Left;
            int y = textPadding.Top + Padding.Top;

            if (0 != (align & Pos.Right))
                x = Width - text.Width - textPadding.Right - Padding.Right;
            if (0 != (align & Pos.CenterH))
                x = (int)((textPadding.Left + Padding.Left) + ((Width - text.Width - textPadding.Left - Padding.Left - textPadding.Right - Padding.Right) * 0.5f));

            if (0 != (align & Pos.CenterV))
                y = (int)((textPadding.Top + Padding.Top) + ((Height - text.Height) * 0.5f) - textPadding.Bottom - Padding.Bottom);
            if (0 != (align & Pos.Bottom))
                y = Height - text.Height - textPadding.Bottom - Padding.Bottom;

            text.SetPosition(x, y);
        }

        /// <summary>
        /// Sets the label text.
        /// </summary>
        /// <param name="str">Text to set.</param>
        /// <param name="doEvents">Determines whether to invoke "text changed" event.</param>
        public virtual void SetText(string str, bool doEvents = true)
        {
            if (Text == str)
                return;

            text.Text = str;
            if (autoSizeToContents)
                SizeToContents();
            Invalidate();
            InvalidateParent();

            if (doEvents)
                onTextChanged();
        }

        public virtual void SizeToContents()
        {
            text.SetPosition(textPadding.Left + Padding.Left, textPadding.Top + Padding.Top);
            text.SizeToContents();

            SetSize(text.Width + Padding.Left + Padding.Right + textPadding.Left + textPadding.Right,
                text.Height + Padding.Top + Padding.Bottom + textPadding.Top + textPadding.Bottom);
            InvalidateParent();
        }

        /// <summary>
        /// Gets the coordinates of specified character.
        /// </summary>
        /// <param name="index">Character index.</param>
        /// <returns>Character coordinates (local).</returns>
        public virtual Point GetCharacterPosition(int index)
        {
            Point p = text.GetCharacterPosition(index);
            return new Point(p.X + text.X, p.Y + text.Y);
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
        }

		/// <summary>
		/// Updates control colors.
		/// </summary>
		public override void UpdateColors() {
			if (IsDisabled) {
				TextColor = Skin.Colors.Button.Disabled;
				return;
			}

			if (IsHovered && ClickEventAssigned) {
				TextColor = Skin.Colors.Button.Hover;
				return;
			}

            TextColor = Skin.Colors.Button.Normal;
		}
    }
}
