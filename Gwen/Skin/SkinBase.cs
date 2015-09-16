using System;
using System.Drawing;

namespace Gwen.Skin
{
    /// <summary>
    /// Base skin.
    /// </summary>
    public class SkinBase : IDisposable
    {
        protected Font defaultFont;
        protected readonly Renderer.RendererBase renderer;

        /// <summary>
        /// Colors of various UI elements.
        /// </summary>
        public SkinColors Colors;

        /// <summary>
        /// Default font to use when rendering text if none specified.
        /// </summary>
        public Font DefaultFont
        {
            get { return defaultFont; }
            set
            {
                defaultFont.Dispose();
                defaultFont = value;
            }
        }

        /// <summary>
        /// Renderer used.
        /// </summary>
        public Renderer.RendererBase Renderer { get { return renderer; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Base"/> class.
        /// </summary>
        /// <param name="renderer">Renderer to use.</param>
        protected SkinBase(Renderer.RendererBase renderer)
        {
            defaultFont = new Font(renderer);
            this.renderer = renderer;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            defaultFont.Dispose();
            GC.SuppressFinalize(this);
        }

#if DEBUG
        ~SkinBase()
        {
            throw new InvalidOperationException(String.Format("IDisposable object finalized: {0}", GetType()));
            //Debug.Print(String.Format("IDisposable object finalized: {0}", GetType()));
        }
#endif

        /// <summary>
        /// Releases the specified font.
        /// </summary>
        /// <param name="font">Font to release.</param>
        protected virtual void releaseFont(Font font)
        {
            if (font == null)
                return;
            if (renderer == null)
                return;
            renderer.FreeFont(font);
        }
        
        /// <summary>
        /// Sets the default text font.
        /// </summary>
        /// <param name="faceName">Font name. Meaning can vary depending on the renderer.</param>
        /// <param name="size">Font size.</param>
        public virtual void SetDefaultFont(string faceName, int size = 10)
        {
            defaultFont.FaceName = faceName;
            defaultFont.Size = size;
        }

        #region UI elements
        public virtual void DrawButton(Control.ControlBase control, bool depressed, bool hovered, bool disabled) { }
        public virtual void DrawTabButton(Control.ControlBase control, bool active, Pos dir) { }
        public virtual void DrawTabControl(Control.ControlBase control) { }
        public virtual void DrawTabTitleBar(Control.ControlBase control) { }
        public virtual void DrawMenuItem(Control.ControlBase control, bool submenuOpen, bool isChecked) { }
        public virtual void DrawMenuRightArrow(Control.ControlBase control) { }
        public virtual void DrawMenuStrip(Control.ControlBase control) { }
        public virtual void DrawMenu(Control.ControlBase control, bool paddingDisabled) { }
        public virtual void DrawRadioButton(Control.ControlBase control, bool selected, bool depressed) { }
        public virtual void DrawCheckBox(Control.ControlBase control, bool selected, bool depressed) { }
        public virtual void DrawGroupBox(Control.ControlBase control, int textStart, int textHeight, int textWidth) { }
        public virtual void DrawTextBox(Control.ControlBase control) { }
        public virtual void DrawWindow(Control.ControlBase control, int topHeight, bool inFocus) { }
        public virtual void DrawWindowCloseButton(Control.ControlBase control, bool depressed, bool hovered, bool disabled) { }
        public virtual void DrawHighlight(Control.ControlBase control) { }
        public virtual void DrawStatusBar(Control.ControlBase control) { }
        public virtual void DrawShadow(Control.ControlBase control) { }
        public virtual void DrawScrollBarBar(Control.ControlBase control, bool depressed, bool hovered, bool horizontal) { }
        public virtual void DrawScrollBar(Control.ControlBase control, bool horizontal, bool depressed) { }
        public virtual void DrawScrollButton(Control.ControlBase control, Pos direction, bool depressed, bool hovered, bool disabled) { }
        public virtual void DrawProgressBar(Control.ControlBase control, bool horizontal, float progress) { }
        public virtual void DrawListBox(Control.ControlBase control) { }
        public virtual void DrawListBoxLine(Control.ControlBase control, bool selected, bool even) { }
        public virtual void DrawSlider(Control.ControlBase control, bool horizontal, int numNotches, int barSize) { }
        public virtual void DrawSliderButton(Control.ControlBase control, bool depressed, bool horizontal) { }
        public virtual void DrawComboBox(Control.ControlBase control, bool down, bool isMenuOpen) { }
        public virtual void DrawComboBoxArrow(Control.ControlBase control, bool hovered, bool depressed, bool open, bool disabled) { }
        public virtual void DrawKeyboardHighlight(Control.ControlBase control, Rectangle rect, int offset) { }
        public virtual void DrawToolTip(Control.ControlBase control) { }
        public virtual void DrawNumericUpDownButton(Control.ControlBase control, bool depressed, bool up) { }
        public virtual void DrawTreeButton(Control.ControlBase control, bool open) { }
        public virtual void DrawTreeControl(Control.ControlBase control) { }

        public virtual void DrawDebugOutlines(Control.ControlBase control)
        {
            renderer.DrawColor = control.PaddingOutlineColor;
            Rectangle inner = new Rectangle(control.Bounds.Left + control.Padding.Left,
                                            control.Bounds.Top + control.Padding.Top,
                                            control.Bounds.Width - control.Padding.Right - control.Padding.Left,
                                            control.Bounds.Height - control.Padding.Bottom - control.Padding.Top);
            renderer.DrawLinedRect(inner);

            renderer.DrawColor = control.MarginOutlineColor;
            Rectangle outer = new Rectangle(control.Bounds.Left - control.Margin.Left,
                                            control.Bounds.Top - control.Margin.Top,
                                            control.Bounds.Width + control.Margin.Right + control.Margin.Left,
                                            control.Bounds.Height + control.Margin.Bottom + control.Margin.Top);
            renderer.DrawLinedRect(outer);

            renderer.DrawColor = control.BoundsOutlineColor;
            renderer.DrawLinedRect(control.Bounds);
        }

        public virtual void DrawTreeNode(Control.ControlBase ctrl, bool open, bool selected, int labelHeight, int labelWidth, int halfWay, int lastBranch, bool isRoot)
        {
            Renderer.DrawColor = Colors.Tree.Lines;

            if (!isRoot)
                Renderer.DrawFilledRect(new Rectangle(8, halfWay, 16 - 9, 1));

            if (!open) return;

            Renderer.DrawFilledRect(new Rectangle(14 + 7, labelHeight + 1, 1, lastBranch + halfWay - labelHeight));
        }

        public virtual void DrawPropertyRow(Control.ControlBase control, int iWidth, bool bBeingEdited, bool hovered)
        {
            Rectangle rect = control.RenderBounds;

            if (bBeingEdited)
                renderer.DrawColor = Colors.Properties.Column_Selected;
            else if (hovered)
                renderer.DrawColor = Colors.Properties.Column_Hover;
            else
                renderer.DrawColor = Colors.Properties.Column_Normal;

            renderer.DrawFilledRect(new Rectangle(0, rect.Y, iWidth, rect.Height));

            if (bBeingEdited)
                renderer.DrawColor = Colors.Properties.Line_Selected;
            else if (hovered)
                renderer.DrawColor = Colors.Properties.Line_Hover;
            else
                renderer.DrawColor = Colors.Properties.Line_Normal;

            renderer.DrawFilledRect(new Rectangle(iWidth, rect.Y, 1, rect.Height));

            rect.Y += rect.Height - 1;
            rect.Height = 1;

            renderer.DrawFilledRect(rect);
        }

        public virtual void DrawColorDisplay(Control.ControlBase control, Color color) { }
        public virtual void DrawModalControl(Control.ControlBase control) { }
        public virtual void DrawMenuDivider(Control.ControlBase control) { }
        public virtual void DrawCategoryHolder(Control.ControlBase control) { }
        public virtual void DrawCategoryInner(Control.ControlBase control, bool collapsed) { }

        public virtual void DrawPropertyTreeNode(Control.ControlBase control, int BorderLeft, int BorderTop)
        {
            Rectangle rect = control.RenderBounds;

            renderer.DrawColor = Colors.Properties.Border;

            renderer.DrawFilledRect(new Rectangle(rect.X, rect.Y, BorderLeft, rect.Height));
            renderer.DrawFilledRect(new Rectangle(rect.X + BorderLeft, rect.Y, rect.Width - BorderLeft, BorderTop));
        }
#endregion

        #region Symbols for Simple skin
        /*
        Here we're drawing a few symbols such as the directional arrows and the checkbox check

        Texture'd skins don't generally use these - but the Simple skin does. We did originally
        use the marlett font to draw these.. but since that's a Windows font it wasn't a very
        good cross platform solution.
        */
        
        public virtual void DrawArrowDown(Rectangle rect)
        {
            float x = (rect.Width / 5.0f);
            float y = (rect.Height / 5.0f);

            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 0.0f, rect.Y + y * 1.0f, x, y * 1.0f));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 1.0f, rect.Y + y * 1.0f, x, y * 2.0f));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 2.0f, rect.Y + y * 1.0f, x, y * 3.0f));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 3.0f, rect.Y + y * 1.0f, x, y * 2.0f));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 4.0f, rect.Y + y * 1.0f, x, y * 1.0f));
        }

        public virtual void DrawArrowUp(Rectangle rect)
        {
            float x = (rect.Width / 5.0f);
            float y = (rect.Height / 5.0f);

            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 0.0f, rect.Y + y * 3.0f, x, y * 1.0f));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 1.0f, rect.Y + y * 2.0f, x, y * 2.0f));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 2.0f, rect.Y + y * 1.0f, x, y * 3.0f));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 3.0f, rect.Y + y * 2.0f, x, y * 2.0f));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 4.0f, rect.Y + y * 3.0f, x, y * 1.0f));
        }

        public virtual void DrawArrowLeft(Rectangle rect)
        {
            float x = (rect.Width / 5.0f);
            float y = (rect.Height / 5.0f);

            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 3.0f, rect.Y + y * 0.0f, x * 1.0f, y));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 2.0f, rect.Y + y * 1.0f, x * 2.0f, y));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 1.0f, rect.Y + y * 2.0f, x * 3.0f, y));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 2.0f, rect.Y + y * 3.0f, x * 2.0f, y));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 3.0f, rect.Y + y * 4.0f, x * 1.0f, y));
        }

        public virtual void DrawArrowRight(Rectangle rect)
        {
            float x = (rect.Width / 5.0f);
            float y = (rect.Height / 5.0f);

            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 1.0f, rect.Y + y * 0.0f, x * 1.0f, y));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 1.0f, rect.Y + y * 1.0f, x * 2.0f, y));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 1.0f, rect.Y + y * 2.0f, x * 3.0f, y));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 1.0f, rect.Y + y * 3.0f, x * 2.0f, y));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 1.0f, rect.Y + y * 4.0f, x * 1.0f, y));
        }

        public virtual void DrawCheck(Rectangle rect)
        {
            float x = (rect.Width / 5.0f);
            float y = (rect.Height / 5.0f);

            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 0.0f, rect.Y + y * 3.0f, x * 2, y * 2));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 1.0f, rect.Y + y * 4.0f, x * 2, y * 2));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 2.0f, rect.Y + y * 3.0f, x * 2, y * 2));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 3.0f, rect.Y + y * 1.0f, x * 2, y * 2));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + x * 4.0f, rect.Y + y * 0.0f, x * 2, y * 2));
        }
        #endregion
    }
}
