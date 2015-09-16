using System;
using System.Drawing;

namespace Gwen.Skin
{
    /// <summary>
    /// Simple skin (non-textured). Deprecated and incomplete, do not use.
    /// </summary>
    [Obsolete]
    public class Simple : Skin.SkinBase
    {
        private readonly Color borderColor;
        private readonly Color controlOutlineLightColor;
        private readonly Color controlOutlineLighterColor;
        private readonly Color backgroundDarkColor;
        private readonly Color controlColor;
        private readonly Color controlDarkerColor;
        private readonly Color controlOutlineNormalColor;
        private readonly Color controlBrightColor;
        private readonly Color controlDarkColor;
        private readonly Color highlightBackgroundColor;
        private readonly Color highlightBorderColor;
        private readonly Color toolTipBackgroundColor;
        private readonly Color toolTipBorderColor;
        private readonly Color modalColor;

        public Simple(Renderer.RendererBase renderer) : base(renderer)
        {
            borderColor = Color.FromArgb(255, 80, 80, 80);
            //m_colBG = Color.FromArgb(255, 248, 248, 248);
            backgroundDarkColor = Color.FromArgb(255, 235, 235, 235);

            controlColor = Color.FromArgb(255, 240, 240, 240);
            controlBrightColor = Color.FromArgb(255, 255, 255, 255);
            controlDarkColor = Color.FromArgb(255, 214, 214, 214);
            controlDarkerColor = Color.FromArgb(255, 180, 180, 180);

            controlOutlineNormalColor = Color.FromArgb(255, 112, 112, 112);
            controlOutlineLightColor = Color.FromArgb(255, 144, 144, 144);
            controlOutlineLighterColor = Color.FromArgb(255, 210, 210, 210);

            highlightBackgroundColor = Color.FromArgb(255, 192, 221, 252);
            highlightBorderColor = Color.FromArgb(255, 51, 153, 255);

            toolTipBackgroundColor = Color.FromArgb(255, 255, 255, 225);
            toolTipBorderColor = Color.FromArgb(255, 0, 0, 0);

            modalColor = Color.FromArgb(150, 25, 25, 25);
        }

        #region UI elements
        public override void DrawButton(Control.ControlBase control, bool depressed, bool hovered, bool disabled)
        {
            int w = control.Width;
            int h = control.Height;

            DrawButton(w, h, depressed, hovered);
        }

        public override void DrawMenuItem(Control.ControlBase control, bool submenuOpen, bool isChecked)
        {
            Rectangle rect = control.RenderBounds;
            if (submenuOpen || control.IsHovered)
            {
                renderer.DrawColor = highlightBackgroundColor;
                renderer.DrawFilledRect(rect);

                renderer.DrawColor = highlightBorderColor;
                renderer.DrawLinedRect(rect);
            }

            if (isChecked)
            {
                renderer.DrawColor = Color.FromArgb(255, 0, 0, 0);

                Rectangle r = new Rectangle(control.Width / 2 - 2, control.Height / 2 - 2, 5, 5);
                DrawCheck(r);
            }
        }

        public override void DrawMenuStrip(Control.ControlBase control)
        {
            int w = control.Width;
            int h = control.Height;

            renderer.DrawColor = Color.FromArgb(255, 246, 248, 252);
            renderer.DrawFilledRect(new Rectangle(0, 0, w, h));

            renderer.DrawColor = Color.FromArgb(150, 218, 224, 241);

            renderer.DrawFilledRect(Util.FloatRect(0, h * 0.4f, w, h * 0.6f));
            renderer.DrawFilledRect(Util.FloatRect(0, h * 0.5f, w, h * 0.5f));
        }

        public override void DrawMenu(Control.ControlBase control, bool paddingDisabled)
        {
            int w = control.Width;
            int h = control.Height;

            renderer.DrawColor = controlBrightColor;
            renderer.DrawFilledRect(new Rectangle(0, 0, w, h));

            if (!paddingDisabled)
            {
                renderer.DrawColor = controlColor;
                renderer.DrawFilledRect(new Rectangle(1, 0, 22, h));
            }

            renderer.DrawColor = controlOutlineNormalColor;
            renderer.DrawLinedRect(new Rectangle(0, 0, w, h));
        }

        public override void DrawShadow(Control.ControlBase control)
        {
            int w = control.Width;
            int h = control.Height;

            int x = 4;
            int y = 6;

            renderer.DrawColor = Color.FromArgb(10, 0, 0, 0);

            renderer.DrawFilledRect(new Rectangle(x, y, w, h));
            x += 2;
            renderer.DrawFilledRect(new Rectangle(x, y, w, h));
            y += 2;
            renderer.DrawFilledRect(new Rectangle(x, y, w, h));
        }

        public virtual void DrawButton(int w, int h, bool depressed, bool bHovered, bool bSquared = false)
        {
            if (depressed) renderer.DrawColor = controlDarkColor;
            else if (bHovered) renderer.DrawColor = controlBrightColor;
            else renderer.DrawColor = controlColor;

            renderer.DrawFilledRect(new Rectangle(1, 1, w - 2, h - 2));
            
            if (depressed) renderer.DrawColor = controlDarkColor;
            else if (bHovered) renderer.DrawColor = controlColor;
            else renderer.DrawColor = controlDarkColor;

            renderer.DrawFilledRect(Util.FloatRect(1, h * 0.5f, w - 2, h * 0.5f - 2));

            if (!depressed)
            {
                renderer.DrawColor = controlBrightColor;
            }
            else
            {
                renderer.DrawColor = controlDarkerColor;
            }
            renderer.DrawShavedCornerRect(new Rectangle(1, 1, w - 2, h - 2), bSquared);

            // Border
            renderer.DrawColor = controlOutlineNormalColor;
            renderer.DrawShavedCornerRect(new Rectangle(0, 0, w, h), bSquared);
        }

        public override void DrawRadioButton(Control.ControlBase control, bool selected, bool depressed)
        {
            Rectangle rect = control.RenderBounds;

            // Inside colour
            if (control.IsHovered) renderer.DrawColor = Color.FromArgb(255, 220, 242, 254);
            else renderer.DrawColor = controlBrightColor;

            renderer.DrawFilledRect(new Rectangle(1, 1, rect.Width - 2, rect.Height - 2));

            // Border
            if (control.IsHovered) renderer.DrawColor = Color.FromArgb(255, 85, 130, 164);
            else renderer.DrawColor = controlOutlineLightColor;

            renderer.DrawShavedCornerRect(rect);

            renderer.DrawColor = Color.FromArgb(15, 0, 50, 60);
            renderer.DrawFilledRect(new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + 2, rect.Y + 2, rect.Width * 0.3f, rect.Height - 4));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height * 0.3f));

            if (control.IsHovered) renderer.DrawColor = Color.FromArgb(255, 121, 198, 249);
            else renderer.DrawColor = Color.FromArgb(50, 0, 50, 60);

            renderer.DrawFilledRect(new Rectangle(rect.X + 2, rect.Y + 3, 1, rect.Height - 5));
            renderer.DrawFilledRect(new Rectangle(rect.X + 3, rect.Y + 2, rect.Width - 5, 1));


            if (selected)
            {
                renderer.DrawColor = Color.FromArgb(255, 40, 230, 30);
                renderer.DrawFilledRect(new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4));
            }
        }

        public override void DrawCheckBox(Control.ControlBase control, bool selected, bool depressed)
        {
            Rectangle rect = control.RenderBounds;

            // Inside colour
            if (control.IsHovered) renderer.DrawColor = Color.FromArgb(255, 220, 242, 254);
            else renderer.DrawColor = controlBrightColor;

            renderer.DrawFilledRect(rect);

            // Border
            if (control.IsHovered) renderer.DrawColor = Color.FromArgb(255, 85, 130, 164);
            else renderer.DrawColor = controlOutlineLightColor;

            renderer.DrawLinedRect(rect);

            renderer.DrawColor = Color.FromArgb(15, 0, 50, 60);
            renderer.DrawFilledRect(new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height - 4));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + 2, rect.Y + 2, rect.Width * 0.3f, rect.Height - 4));
            renderer.DrawFilledRect(Util.FloatRect(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height * 0.3f));

            if (control.IsHovered) renderer.DrawColor = Color.FromArgb(255, 121, 198, 249);
            else renderer.DrawColor = Color.FromArgb(50, 0, 50, 60);

            renderer.DrawFilledRect(new Rectangle(rect.X + 2, rect.Y + 2, 1, rect.Height - 4));
            renderer.DrawFilledRect(new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, 1));

            if (depressed)
            {
                renderer.DrawColor = Color.FromArgb(255, 100, 100, 100);
                Rectangle r = new Rectangle(control.Width / 2 - 2, control.Height / 2 - 2, 5, 5);
                DrawCheck(r);
            }
            else if (selected)
            {
                renderer.DrawColor = Color.FromArgb(255, 0, 0, 0);
                Rectangle r = new Rectangle(control.Width / 2 - 2, control.Height / 2 - 2, 5, 5);
                DrawCheck(r);
            }
        }

        public override void DrawGroupBox(Control.ControlBase control, int textStart, int textHeight, int textWidth)
        {
            Rectangle rect = control.RenderBounds;

            rect.Y += (int)(textHeight * 0.5f);
            rect.Height -= (int)(textHeight * 0.5f);

            Color m_colDarker = Color.FromArgb(50, 0, 50, 60);
            Color m_colLighter = Color.FromArgb(150, 255, 255, 255);

            renderer.DrawColor = m_colLighter;

            renderer.DrawFilledRect(new Rectangle(rect.X + 1, rect.Y + 1, textStart - 3, 1));
            renderer.DrawFilledRect(new Rectangle(rect.X + 1 + textStart + textWidth, rect.Y + 1,
                                                  rect.Width - textStart + textWidth - 2, 1));
            renderer.DrawFilledRect(new Rectangle(rect.X + 1, (rect.Y + rect.Height) - 1, rect.Width - 2, 1));

            renderer.DrawFilledRect(new Rectangle(rect.X + 1, rect.Y + 1, 1, rect.Height));
            renderer.DrawFilledRect(new Rectangle((rect.X + rect.Width) - 2, rect.Y + 1, 1, rect.Height - 1));

            renderer.DrawColor = m_colDarker;

            renderer.DrawFilledRect(new Rectangle(rect.X + 1, rect.Y, textStart - 3, 1));
            renderer.DrawFilledRect(new Rectangle(rect.X + 1 + textStart + textWidth, rect.Y,
                                                  rect.Width - textStart - textWidth - 2, 1));
            renderer.DrawFilledRect(new Rectangle(rect.X + 1, (rect.Y + rect.Height) - 1, rect.Width - 2, 1));

            renderer.DrawFilledRect(new Rectangle(rect.X, rect.Y + 1, 1, rect.Height - 1));
            renderer.DrawFilledRect(new Rectangle((rect.X + rect.Width) - 1, rect.Y + 1, 1, rect.Height - 1));
        }

        public override void DrawTextBox(Control.ControlBase control)
        {
            Rectangle rect = control.RenderBounds;
            bool bHasFocus = control.HasFocus;

            // Box inside
            renderer.DrawColor = Color.FromArgb(255, 255, 255, 255);
            renderer.DrawFilledRect(new Rectangle(1, 1, rect.Width - 2, rect.Height - 2));

            renderer.DrawColor = controlOutlineLightColor;
            renderer.DrawFilledRect(new Rectangle(rect.X + 1, rect.Y, rect.Width - 2, 1));
            renderer.DrawFilledRect(new Rectangle(rect.X, rect.Y + 1, 1, rect.Height - 2));

            renderer.DrawColor = controlOutlineLighterColor;
            renderer.DrawFilledRect(new Rectangle(rect.X + 1, (rect.Y + rect.Height) - 1, rect.Width - 2, 1));
            renderer.DrawFilledRect(new Rectangle((rect.X + rect.Width) - 1, rect.Y + 1, 1, rect.Height - 2));

            if (bHasFocus)
            {
                renderer.DrawColor = Color.FromArgb(150, 50, 200, 255);
                renderer.DrawLinedRect(rect);
            }
        }

        public override void DrawTabButton(Control.ControlBase control, bool active, Pos dir)
        {
            Rectangle rect = control.RenderBounds;
            bool bHovered = control.IsHovered;

            if (bHovered) renderer.DrawColor = controlBrightColor;
            else renderer.DrawColor = controlColor;

            renderer.DrawFilledRect(new Rectangle(1, 1, rect.Width - 2, rect.Height - 1));

            if (bHovered) renderer.DrawColor = controlColor;
            else renderer.DrawColor = controlDarkColor;

            renderer.DrawFilledRect(Util.FloatRect(1, rect.Height*0.5f, rect.Width - 2, rect.Height*0.5f - 1));

            renderer.DrawColor = controlBrightColor;
            renderer.DrawShavedCornerRect(new Rectangle(1, 1, rect.Width - 2, rect.Height));

            renderer.DrawColor = borderColor;

            renderer.DrawShavedCornerRect(new Rectangle(0, 0, rect.Width, rect.Height));
        }

        public override void DrawTabControl(Control.ControlBase control)
        {
            Rectangle rect = control.RenderBounds;

            renderer.DrawColor = controlColor;
            renderer.DrawFilledRect(rect);

            renderer.DrawColor = borderColor;
            renderer.DrawLinedRect(rect);

            //m_Renderer.DrawColor = m_colControl;
            //m_Renderer.DrawFilledRect(CurrentButtonRect);
        }

        public override void DrawWindow(Control.ControlBase control, int topHeight, bool inFocus)
        {
            Rectangle rect = control.RenderBounds;

            // Titlebar
            if (inFocus)
                renderer.DrawColor = Color.FromArgb(230, 87, 164, 232);
            else
                renderer.DrawColor = Color.FromArgb(230, (int)(87 * 0.70f), (int)(164 * 0.70f),
                                                    (int)(232 * 0.70f));

            int iBorderSize = 5;
            renderer.DrawFilledRect(new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, topHeight - 1));
            renderer.DrawFilledRect(new Rectangle(rect.X + 1, rect.Y + topHeight - 1, iBorderSize,
                                                  rect.Height - 2 - topHeight));
            renderer.DrawFilledRect(new Rectangle(rect.X + rect.Width - iBorderSize, rect.Y + topHeight - 1, iBorderSize,
                                                  rect.Height - 2 - topHeight));
            renderer.DrawFilledRect(new Rectangle(rect.X + 1, rect.Y + rect.Height - iBorderSize, rect.Width - 2,
                                                  iBorderSize));

            // Main inner
            renderer.DrawColor = Color.FromArgb(230, controlDarkColor.R, controlDarkColor.G, controlDarkColor.B);
            renderer.DrawFilledRect(new Rectangle(rect.X + iBorderSize + 1, rect.Y + topHeight,
                                                  rect.Width - iBorderSize * 2 - 2,
                                                  rect.Height - topHeight - iBorderSize - 1));

            // Light inner border
            renderer.DrawColor = Color.FromArgb(100, 255, 255, 255);
            renderer.DrawShavedCornerRect(new Rectangle(rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2));

            // Dark line between titlebar and main
            renderer.DrawColor = borderColor;

            // Inside border
            renderer.DrawColor = controlOutlineNormalColor;
            renderer.DrawLinedRect(new Rectangle(rect.X + iBorderSize, rect.Y + topHeight - 1, rect.Width - 10,
                                                 rect.Height - topHeight - (iBorderSize - 1)));

            // Dark outer border
            renderer.DrawColor = borderColor;
            renderer.DrawShavedCornerRect(new Rectangle(rect.X, rect.Y, rect.Width, rect.Height));
        }

        public override void DrawWindowCloseButton(Control.ControlBase control, bool depressed, bool hovered, bool disabled)
        {
            // TODO
            DrawButton(control, depressed, hovered, disabled);
        }

        public override void DrawHighlight(Control.ControlBase control)
        {
            Rectangle rect = control.RenderBounds;
            renderer.DrawColor = Color.FromArgb(255, 255, 100, 255);
            renderer.DrawFilledRect(rect);
        }

        public override void DrawScrollBar(Control.ControlBase control, bool horizontal, bool depressed)
        {
            Rectangle rect = control.RenderBounds;
            if (depressed)
                renderer.DrawColor = controlDarkerColor;
            else
                renderer.DrawColor = controlBrightColor;
            renderer.DrawFilledRect(rect);
        }

        public override void DrawScrollBarBar(Control.ControlBase control, bool depressed, bool hovered, bool horizontal)
        {
            //TODO: something specialized
            DrawButton(control, depressed, hovered, false);
        }

        public override void DrawTabTitleBar(Control.ControlBase control)
        {
            Rectangle rect = control.RenderBounds;

            renderer.DrawColor = Color.FromArgb(255, 177, 193, 214);
            renderer.DrawFilledRect(rect);

            renderer.DrawColor = borderColor;
            rect.Height += 1;
            renderer.DrawLinedRect(rect);
        }

        public override void DrawProgressBar(Control.ControlBase control, bool horizontal, float progress)
        {
            Rectangle rect = control.RenderBounds;
            Color FillColour = Color.FromArgb(255, 0, 211, 40);

            if (horizontal)
            {
                //Background
                renderer.DrawColor = controlDarkColor;
                renderer.DrawFilledRect(new Rectangle(1, 1, rect.Width - 2, rect.Height - 2));

                //Right half
                renderer.DrawColor = FillColour;
                renderer.DrawFilledRect(Util.FloatRect(1, 1, rect.Width * progress - 2, rect.Height - 2));

                renderer.DrawColor = Color.FromArgb(150, 255, 255, 255);
                renderer.DrawFilledRect(Util.FloatRect(1, 1, rect.Width - 2, rect.Height * 0.45f));
            }
            else
            {
                //Background 
                renderer.DrawColor = controlDarkColor;
                renderer.DrawFilledRect(new Rectangle(1, 1, rect.Width - 2, rect.Height - 2));

                //Top half
                renderer.DrawColor = FillColour;
                renderer.DrawFilledRect(Util.FloatRect(1, 1 + (rect.Height * (1 - progress)), rect.Width - 2,
                                                         rect.Height * progress - 2));

                renderer.DrawColor = Color.FromArgb(150, 255, 255, 255);
                renderer.DrawFilledRect(Util.FloatRect(1, 1, rect.Width * 0.45f, rect.Height - 2));
            }

            renderer.DrawColor = Color.FromArgb(150, 255, 255, 255);
            renderer.DrawShavedCornerRect(new Rectangle(1, 1, rect.Width - 2, rect.Height - 2));

            renderer.DrawColor = Color.FromArgb(70, 255, 255, 255);
            renderer.DrawShavedCornerRect(new Rectangle(2, 2, rect.Width - 4, rect.Height - 4));

            renderer.DrawColor = borderColor;
            renderer.DrawShavedCornerRect(rect);
        }

        public override void DrawListBox(Control.ControlBase control)
        {
            Rectangle rect = control.RenderBounds;

            renderer.DrawColor = controlBrightColor;
            renderer.DrawFilledRect(rect);

            renderer.DrawColor = borderColor;
            renderer.DrawLinedRect(rect);
        }

        public override void DrawListBoxLine(Control.ControlBase control, bool selected, bool even)
        {
            Rectangle rect = control.RenderBounds;

            if (selected)
            {
                renderer.DrawColor = highlightBorderColor;
                renderer.DrawFilledRect(rect);
            }
            else if (control.IsHovered)
            {
                renderer.DrawColor = highlightBackgroundColor;
                renderer.DrawFilledRect(rect);
            }
        }

        public override void DrawSlider(Control.ControlBase control, bool horizontal, int numNotches, int barSize)
        {
            Rectangle rect = control.RenderBounds;
            Rectangle notchRect = rect;

            if (horizontal)
            {
                rect.Y += (int)(rect.Height * 0.4f);
                rect.Height -= (int)(rect.Height * 0.8f);
            }
            else
            {
                rect.X += (int)(rect.Width * 0.4f);
                rect.Width -= (int)(rect.Width * 0.8f);
            }

            renderer.DrawColor = backgroundDarkColor;
            renderer.DrawFilledRect(rect);

            renderer.DrawColor = controlDarkerColor;
            renderer.DrawLinedRect(rect);
        }

        public override void DrawComboBox(Control.ControlBase control, bool down, bool open)
        {
            DrawTextBox(control);
        }

        public override void DrawKeyboardHighlight(Control.ControlBase control, Rectangle r, int iOffset)
        {
            Rectangle rect = r;

            rect.X += iOffset;
            rect.Y += iOffset;
            rect.Width -= iOffset * 2;
            rect.Height -= iOffset * 2;

            //draw the top and bottom
            bool skip = true;
            for (int i = 0; i < rect.Width * 0.5; i++)
            {
                renderer.DrawColor = Color.FromArgb(255, 0, 0, 0);
                if (!skip)
                {
                    renderer.DrawPixel(rect.X + (i * 2), rect.Y);
                    renderer.DrawPixel(rect.X + (i * 2), rect.Y + rect.Height - 1);
                }
                else
                    skip = false;
            }

            for (int i = 0; i < rect.Height * 0.5; i++)
            {
                renderer.DrawColor = Color.FromArgb(255, 0, 0, 0);
                renderer.DrawPixel(rect.X, rect.Y + i * 2);
                renderer.DrawPixel(rect.X + rect.Width - 1, rect.Y + i * 2);
            }
        }

        public override void DrawToolTip(Control.ControlBase control)
        {
            Rectangle rct = control.RenderBounds;
            rct.X -= 3;
            rct.Y -= 3;
            rct.Width += 6;
            rct.Height += 6;

            renderer.DrawColor = toolTipBackgroundColor;
            renderer.DrawFilledRect(rct);

            renderer.DrawColor = toolTipBorderColor;
            renderer.DrawLinedRect(rct);
        }

        public override void DrawScrollButton(Control.ControlBase control, Pos direction, bool depressed, bool hovered, bool disabled)
        {
            DrawButton(control, depressed, false, false);

            renderer.DrawColor = Color.FromArgb(240, 0, 0, 0);

            Rectangle r = new Rectangle(control.Width / 2 - 2, control.Height / 2 - 2, 5, 5);

            if (direction == Pos.Top) DrawArrowUp(r);
            else if (direction == Pos.Bottom) DrawArrowDown(r);
            else if (direction == Pos.Left) DrawArrowLeft(r);
            else DrawArrowRight(r);
        }

        public override void DrawComboBoxArrow(Control.ControlBase control, bool hovered, bool depressed, bool open, bool disabled)
        {
            //DrawButton( control.Width, control.Height, depressed, false, true );

            renderer.DrawColor = Color.FromArgb(240, 0, 0, 0);

            Rectangle r = new Rectangle(control.Width / 2 - 2, control.Height / 2 - 2, 5, 5);
            DrawArrowDown(r);
        }

        public override void DrawNumericUpDownButton(Control.ControlBase control, bool depressed, bool up)
        {
            //DrawButton( control.Width, control.Height, depressed, false, true );

            renderer.DrawColor = Color.FromArgb(240, 0, 0, 0);

            Rectangle r = new Rectangle(control.Width / 2 - 2, control.Height / 2 - 2, 5, 5);

            if (up) DrawArrowUp(r);
            else DrawArrowDown(r);
        }

        public override void DrawTreeButton(Control.ControlBase control, bool open)
        {
            Rectangle rect = control.RenderBounds;
            rect.X += 2;
            rect.Y += 2;
            rect.Width -= 4;
            rect.Height -= 4;

            renderer.DrawColor = controlBrightColor;
            renderer.DrawFilledRect(rect);

            renderer.DrawColor = borderColor;
            renderer.DrawLinedRect(rect);

            renderer.DrawColor = borderColor;

            if (!open) // ! because the button shows intention, not the current state
                renderer.DrawFilledRect(new Rectangle(rect.X + rect.Width / 2, rect.Y + 2, 1, rect.Height - 4));

            renderer.DrawFilledRect(new Rectangle(rect.X + 2, rect.Y + rect.Height / 2, rect.Width - 4, 1));
        }

        public override void DrawTreeControl(Control.ControlBase control)
        {
            Rectangle rect = control.RenderBounds;

            renderer.DrawColor = controlBrightColor;
            renderer.DrawFilledRect(rect);

            renderer.DrawColor = borderColor;
            renderer.DrawLinedRect(rect);
        }

        public override void DrawTreeNode(Control.ControlBase ctrl, bool open, bool selected, int labelHeight, int labelWidth, int halfWay, int lastBranch, bool isRoot)
        {
            if (selected)
            {
                Renderer.DrawColor = Color.FromArgb(100, 0, 150, 255);
                Renderer.DrawFilledRect(new Rectangle(17, 0, labelWidth + 2, labelHeight - 1));
                Renderer.DrawColor = Color.FromArgb(200, 0, 150, 255);
                Renderer.DrawLinedRect(new Rectangle(17, 0, labelWidth + 2, labelHeight - 1));
            }

            base.DrawTreeNode(ctrl, open, selected, labelHeight, labelWidth, halfWay, lastBranch, isRoot);
        }

        public override void DrawStatusBar(Control.ControlBase control)
        {
            // todo
        }

        public override void DrawColorDisplay(Control.ControlBase control, Color color)
        {
            Rectangle rect = control.RenderBounds;

            if (color.A != 255)
            {
                Renderer.DrawColor = Color.FromArgb(255, 255, 255, 255);
                Renderer.DrawFilledRect(rect);

                Renderer.DrawColor = Color.FromArgb(128, 128, 128, 128);

                Renderer.DrawFilledRect(Util.FloatRect(0, 0, rect.Width * 0.5f, rect.Height * 0.5f));
                Renderer.DrawFilledRect(Util.FloatRect(rect.Width * 0.5f, rect.Height * 0.5f, rect.Width * 0.5f,
                                                         rect.Height * 0.5f));
            }

            Renderer.DrawColor = color;
            Renderer.DrawFilledRect(rect);

            Renderer.DrawColor = Color.FromArgb(255, 0, 0, 0);
            Renderer.DrawLinedRect(rect);
        }

        public override void DrawModalControl(Control.ControlBase control)
        {
            if (control.ShouldDrawBackground)
            {
                Rectangle rect = control.RenderBounds;
                Renderer.DrawColor = modalColor;
                Renderer.DrawFilledRect(rect);
            }
        }

        public override void DrawMenuDivider(Control.ControlBase control)
        {
            Rectangle rect = control.RenderBounds;
            Renderer.DrawColor = backgroundDarkColor;
            Renderer.DrawFilledRect(rect);
            Renderer.DrawColor = controlDarkerColor;
            Renderer.DrawLinedRect(rect);
        }

        public override void DrawMenuRightArrow(Control.ControlBase control)
        {
            DrawArrowRight(control.RenderBounds);
        }

        public override void DrawSliderButton(Control.ControlBase control, bool depressed, bool horizontal)
        {
            DrawButton(control, depressed, control.IsHovered, control.IsDisabled);
        }

        public override void DrawCategoryHolder(Control.ControlBase control)
        {
            // todo
        }

        public override void DrawCategoryInner(Control.ControlBase control, bool collapsed)
        {
            // todo
        }
        #endregion
    }
}
