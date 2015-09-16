using System;
using System.Drawing;
using System.IO;
using Gwen.Skin.Texturing;
using Single = Gwen.Skin.Texturing.Single;

namespace Gwen.Skin
{
    #region UI element textures
    public struct SkinTextures
    {
        public Bordered StatusBar;
        public Bordered Selection;
        public Bordered Shadow;
        public Bordered Tooltip;

        public struct _Panel
        {
            public Bordered Normal;
            public Bordered Bright;
            public Bordered Dark;
            public Bordered Highlight;
        }

        public struct _Window
        {
            public Bordered Normal;
            public Bordered Inactive;
            public Single Close;
            public Single Close_Hover;
            public Single Close_Down;
            public Single Close_Disabled;
        }

        public struct _CheckBox
        {
            public struct _Active
            {
                public Single Normal;
                public Single Checked;
            }
            public struct _Disabled
            {
                public Single Normal;
                public Single Checked;
            }

            public _Active Active;
            public _Disabled Disabled;
        }

        public struct _RadioButton
        {
            public struct _Active
            {
                public Single Normal;
                public Single Checked;
            }
            public struct _Disabled
            {
                public Single Normal;
                public Single Checked;
            }

            public _Active Active;
            public _Disabled Disabled;
        }

        public struct _TextBox
        {
            public Bordered Normal;
            public Bordered Focus;
            public Bordered Disabled;
        }

        public struct _Tree
        {
            public Bordered Background;
            public Single Minus;
            public Single Plus;
        }

        public struct _ProgressBar
        {
            public Bordered Back;
            public Bordered Front;
        }

        public struct _Scroller
        {
            public Bordered TrackV;
            public Bordered TrackH;
            public Bordered ButtonV_Normal;
            public Bordered ButtonV_Hover;
            public Bordered ButtonV_Down;
            public Bordered ButtonV_Disabled;
            public Bordered ButtonH_Normal;
            public Bordered ButtonH_Hover;
            public Bordered ButtonH_Down;
            public Bordered ButtonH_Disabled;

            public struct _Button
            {
                public Bordered[] Normal;
                public Bordered[] Hover;
                public Bordered[] Down;
                public Bordered[] Disabled;
            }

            public _Button Button;
        }

        public struct _Menu
        {
            public Single RightArrow;
            public Single Check;

            public Bordered Strip;
            public Bordered Background;
            public Bordered BackgroundWithMargin;
            public Bordered Hover;
        }

        public struct _Input
        {
            public struct _Button
            {
                public Bordered Normal;
                public Bordered Hovered;
                public Bordered Disabled;
                public Bordered Pressed;
            }

            public struct _ComboBox
            {
                public Bordered Normal;
                public Bordered Hover;
                public Bordered Down;
                public Bordered Disabled;

                public struct _Button
                {
                    public Single Normal;
                    public Single Hover;
                    public Single Down;
                    public Single Disabled;
                }

                public _Button Button;
            }

            public struct _Slider
            {
                public struct _H
                {
                    public Single Normal;
                    public Single Hover;
                    public Single Down;
                    public Single Disabled;
                }

                public struct _V
                {
                    public Single Normal;
                    public Single Hover;
                    public Single Down;
                    public Single Disabled;
                }

                public _H H;
                public _V V;
            }

            public struct _ListBox
            {
                public Bordered Background;
                public Bordered Hovered;
                public Bordered EvenLine;
                public Bordered OddLine;
                public Bordered EvenLineSelected;
                public Bordered OddLineSelected;
            }

            public struct _UpDown
            {
                public struct _Up
                {
                    public Single Normal;
                    public Single Hover;
                    public Single Down;
                    public Single Disabled;
                }

                public struct _Down
                {
                    public Single Normal;
                    public Single Hover;
                    public Single Down;
                    public Single Disabled;
                }

                public _Up Up;
                public _Down Down;
            }

            public _Button Button;
            public _ComboBox ComboBox;
            public _Slider Slider;
            public _ListBox ListBox;
            public _UpDown UpDown;
        }

        public struct _Tab
        {
            public struct _Bottom
            {
                public Bordered Inactive;
                public Bordered Active;
            }

            public struct _Top
            {
                public Bordered Inactive;
                public Bordered Active;
            }

            public struct _Left
            {
                public Bordered Inactive;
                public Bordered Active;
            }

            public struct _Right
            {
                public Bordered Inactive;
                public Bordered Active;
            }

            public _Bottom Bottom;
            public _Top Top;
            public _Left Left;
            public _Right Right;

            public Bordered Control;
            public Bordered HeaderBar;
        }

        public struct _CategoryList
        {
            public Bordered Outer;
            public Bordered Inner;
            public Bordered Header;
        }

        public _Panel Panel;
        public _Window Window;
        public _CheckBox CheckBox;
        public _RadioButton RadioButton;
        public _TextBox TextBox;
        public _Tree Tree;
        public _ProgressBar ProgressBar;
        public _Scroller Scroller;
        public _Menu Menu;
        public _Input Input;
        public _Tab Tab;
        public _CategoryList CategoryList;
    }
    #endregion

    /// <summary>
    /// Base textured skin.
    /// </summary>
    public class TexturedBase : Skin.SkinBase
    {
        protected SkinTextures textures;

        private readonly Texture texture;

        /// <summary>
        /// Initializes a new instance of the <see cref="TexturedBase"/> class.
        /// </summary>
        /// <param name="renderer">Renderer to use.</param>
        /// <param name="textureName">Name of the skin texture map.</param>
        public TexturedBase(Renderer.RendererBase renderer, string textureName)
            : base(renderer)
        {
            texture = new Texture(Renderer);
            texture.Load(textureName);

            initializeColors();
            initializeTextures();
        }

        public TexturedBase(Renderer.RendererBase renderer, Stream textureData)
            : base(renderer)
        {
            texture = new Texture(Renderer);
            texture.LoadStream(textureData);

            initializeColors();
            initializeTextures();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            texture.Dispose();
            base.Dispose();
        }

        #region Initialization
        private void initializeColors()
        {
            Colors.Window.TitleActive   = Renderer.PixelColor(texture, 4 + 8*0, 508, Color.Red);
            Colors.Window.TitleInactive = Renderer.PixelColor(texture, 4 + 8*1, 508, Color.Yellow);

            Colors.Button.Normal   = Renderer.PixelColor(texture, 4 + 8*2, 508, Color.Yellow);
            Colors.Button.Hover    = Renderer.PixelColor(texture, 4 + 8*3, 508, Color.Yellow);
            Colors.Button.Down     = Renderer.PixelColor(texture, 4 + 8*2, 500, Color.Yellow);
            Colors.Button.Disabled = Renderer.PixelColor(texture, 4 + 8*3, 500, Color.Yellow);

            Colors.Tab.Active.Normal     = Renderer.PixelColor(texture, 4 + 8*4, 508, Color.Yellow);
            Colors.Tab.Active.Hover      = Renderer.PixelColor(texture, 4 + 8*5, 508, Color.Yellow);
            Colors.Tab.Active.Down       = Renderer.PixelColor(texture, 4 + 8*4, 500, Color.Yellow);
            Colors.Tab.Active.Disabled   = Renderer.PixelColor(texture, 4 + 8*5, 500, Color.Yellow);
            Colors.Tab.Inactive.Normal   = Renderer.PixelColor(texture, 4 + 8*6, 508, Color.Yellow);
            Colors.Tab.Inactive.Hover    = Renderer.PixelColor(texture, 4 + 8*7, 508, Color.Yellow);
            Colors.Tab.Inactive.Down     = Renderer.PixelColor(texture, 4 + 8*6, 500, Color.Yellow);
            Colors.Tab.Inactive.Disabled = Renderer.PixelColor(texture, 4 + 8*7, 500, Color.Yellow);

            Colors.Label.Default   = Renderer.PixelColor(texture, 4 + 8*8, 508, Color.Yellow);
            Colors.Label.Bright    = Renderer.PixelColor(texture, 4 + 8*9, 508, Color.Yellow);
            Colors.Label.Dark      = Renderer.PixelColor(texture, 4 + 8*8, 500, Color.Yellow);
            Colors.Label.Highlight = Renderer.PixelColor(texture, 4 + 8*9, 500, Color.Yellow);

            Colors.Tree.Lines    = Renderer.PixelColor(texture, 4 + 8*10, 508, Color.Yellow);
            Colors.Tree.Normal   = Renderer.PixelColor(texture, 4 + 8*11, 508, Color.Yellow);
            Colors.Tree.Hover    = Renderer.PixelColor(texture, 4 + 8*10, 500, Color.Yellow);
            Colors.Tree.Selected = Renderer.PixelColor(texture, 4 + 8*11, 500, Color.Yellow);

            Colors.Properties.Line_Normal     = Renderer.PixelColor(texture, 4 + 8*12, 508, Color.Yellow);
            Colors.Properties.Line_Selected   = Renderer.PixelColor(texture, 4 + 8*13, 508, Color.Yellow);
            Colors.Properties.Line_Hover      = Renderer.PixelColor(texture, 4 + 8*12, 500, Color.Yellow);
            Colors.Properties.Title           = Renderer.PixelColor(texture, 4 + 8*13, 500, Color.Yellow);
            Colors.Properties.Column_Normal   = Renderer.PixelColor(texture, 4 + 8*14, 508, Color.Yellow);
            Colors.Properties.Column_Selected = Renderer.PixelColor(texture, 4 + 8*15, 508, Color.Yellow);
            Colors.Properties.Column_Hover    = Renderer.PixelColor(texture, 4 + 8*14, 500, Color.Yellow);
            Colors.Properties.Border          = Renderer.PixelColor(texture, 4 + 8*15, 500, Color.Yellow);
            Colors.Properties.Label_Normal    = Renderer.PixelColor(texture, 4 + 8*16, 508, Color.Yellow);
            Colors.Properties.Label_Selected  = Renderer.PixelColor(texture, 4 + 8*17, 508, Color.Yellow);
            Colors.Properties.Label_Hover     = Renderer.PixelColor(texture, 4 + 8*16, 500, Color.Yellow);

            Colors.ModalBackground = Renderer.PixelColor(texture, 4 + 8*18, 508, Color.Yellow);
            
            Colors.TooltipText = Renderer.PixelColor(texture, 4 + 8*19, 508, Color.Yellow);

            Colors.Category.Header                  = Renderer.PixelColor(texture, 4 + 8*18, 500, Color.Yellow);
            Colors.Category.Header_Closed           = Renderer.PixelColor(texture, 4 + 8*19, 500, Color.Yellow);
            Colors.Category.Line.Text               = Renderer.PixelColor(texture, 4 + 8*20, 508, Color.Yellow);
            Colors.Category.Line.Text_Hover         = Renderer.PixelColor(texture, 4 + 8*21, 508, Color.Yellow);
            Colors.Category.Line.Text_Selected      = Renderer.PixelColor(texture, 4 + 8*20, 500, Color.Yellow);
            Colors.Category.Line.Button             = Renderer.PixelColor(texture, 4 + 8*21, 500, Color.Yellow);
            Colors.Category.Line.Button_Hover       = Renderer.PixelColor(texture, 4 + 8*22, 508, Color.Yellow);
            Colors.Category.Line.Button_Selected    = Renderer.PixelColor(texture, 4 + 8*23, 508, Color.Yellow);
            Colors.Category.LineAlt.Text            = Renderer.PixelColor(texture, 4 + 8*22, 500, Color.Yellow);
            Colors.Category.LineAlt.Text_Hover      = Renderer.PixelColor(texture, 4 + 8*23, 500, Color.Yellow);
            Colors.Category.LineAlt.Text_Selected   = Renderer.PixelColor(texture, 4 + 8*24, 508, Color.Yellow);
            Colors.Category.LineAlt.Button          = Renderer.PixelColor(texture, 4 + 8*25, 508, Color.Yellow);
            Colors.Category.LineAlt.Button_Hover    = Renderer.PixelColor(texture, 4 + 8*24, 500, Color.Yellow);
            Colors.Category.LineAlt.Button_Selected = Renderer.PixelColor(texture, 4 + 8*25, 500, Color.Yellow);
        }

        private void initializeTextures()
        {
            textures.Shadow    = new Bordered(texture, 448, 0, 31, 31, Margin.Eight);
            textures.Tooltip   = new Bordered(texture, 128, 320, 127, 31, Margin.Eight);
            textures.StatusBar = new Bordered(texture, 128, 288, 127, 31, Margin.Eight);
            textures.Selection = new Bordered(texture, 384, 32, 31, 31, Margin.Four);

            textures.Panel.Normal    = new Bordered(texture, 256, 0, 63, 63, new Margin(16, 16, 16, 16));
            textures.Panel.Bright    = new Bordered(texture, 256 + 64, 0, 63, 63, new Margin(16, 16, 16, 16));
            textures.Panel.Dark      = new Bordered(texture, 256, 64, 63, 63, new Margin(16, 16, 16, 16));
            textures.Panel.Highlight = new Bordered(texture, 256 + 64, 64, 63, 63, new Margin(16, 16, 16, 16));

            textures.Window.Normal   = new Bordered(texture, 0, 0, 127, 127, new Margin(8, 32, 8, 8));
            textures.Window.Inactive = new Bordered(texture, 128, 0, 127, 127, new Margin(8, 32, 8, 8));

            textures.CheckBox.Active.Checked  = new Single(texture, 448, 32, 15, 15);
            textures.CheckBox.Active.Normal   = new Single(texture, 464, 32, 15, 15);
            textures.CheckBox.Disabled.Normal = new Single(texture, 448, 48, 15, 15);
            textures.CheckBox.Disabled.Normal = new Single(texture, 464, 48, 15, 15);

            textures.RadioButton.Active.Checked  = new Single(texture, 448, 64, 15, 15);
            textures.RadioButton.Active.Normal   = new Single(texture, 464, 64, 15, 15);
            textures.RadioButton.Disabled.Normal = new Single(texture, 448, 80, 15, 15);
            textures.RadioButton.Disabled.Normal = new Single(texture, 464, 80, 15, 15);

            textures.TextBox.Normal   = new Bordered(texture, 0, 150, 127, 21, Margin.Four);
            textures.TextBox.Focus    = new Bordered(texture, 0, 172, 127, 21, Margin.Four);
            textures.TextBox.Disabled = new Bordered(texture, 0, 193, 127, 21, Margin.Four);

            textures.Menu.Strip                = new Bordered(texture, 0, 128, 127, 21, Margin.One);
            textures.Menu.BackgroundWithMargin = new Bordered(texture, 128, 128, 127, 63, new Margin(24, 8, 8, 8));
            textures.Menu.Background           = new Bordered(texture, 128, 192, 127, 63, Margin.Eight);
            textures.Menu.Hover                = new Bordered(texture, 128, 256, 127, 31, Margin.Eight);
            textures.Menu.RightArrow           = new Single(texture, 464, 112, 15, 15);
            textures.Menu.Check                = new Single(texture, 448, 112, 15, 15);

            textures.Tab.Control         = new Bordered(texture, 0, 256, 127, 127, Margin.Eight);
            textures.Tab.Bottom.Active   = new Bordered(texture, 0, 416, 63, 31, Margin.Eight);
            textures.Tab.Bottom.Inactive = new Bordered(texture, 0 + 128, 416, 63, 31, Margin.Eight);
            textures.Tab.Top.Active      = new Bordered(texture, 0, 384, 63, 31, Margin.Eight);
            textures.Tab.Top.Inactive    = new Bordered(texture, 0 + 128, 384, 63, 31, Margin.Eight);
            textures.Tab.Left.Active     = new Bordered(texture, 64, 384, 31, 63, Margin.Eight);
            textures.Tab.Left.Inactive   = new Bordered(texture, 64 + 128, 384, 31, 63, Margin.Eight);
            textures.Tab.Right.Active    = new Bordered(texture, 96, 384, 31, 63, Margin.Eight);
            textures.Tab.Right.Inactive  = new Bordered(texture, 96 + 128, 384, 31, 63, Margin.Eight);
            textures.Tab.HeaderBar       = new Bordered(texture, 128, 352, 127, 31, Margin.Four);

            textures.Window.Close       = new Single(texture, 0, 224, 24, 24);
            textures.Window.Close_Hover = new Single(texture, 32, 224, 24, 24);
            textures.Window.Close_Hover = new Single(texture, 64, 224, 24, 24);
            textures.Window.Close_Hover = new Single(texture, 96, 224, 24, 24);

            textures.Scroller.TrackV           = new Bordered(texture, 384, 208, 15, 127, Margin.Four);
            textures.Scroller.ButtonV_Normal   = new Bordered(texture, 384 + 16, 208, 15, 127, Margin.Four);
            textures.Scroller.ButtonV_Hover    = new Bordered(texture, 384 + 32, 208, 15, 127, Margin.Four);
            textures.Scroller.ButtonV_Down     = new Bordered(texture, 384 + 48, 208, 15, 127, Margin.Four);
            textures.Scroller.ButtonV_Disabled = new Bordered(texture, 384 + 64, 208, 15, 127, Margin.Four);
            textures.Scroller.TrackH           = new Bordered(texture, 384, 128, 127, 15, Margin.Four);
            textures.Scroller.ButtonH_Normal   = new Bordered(texture, 384, 128 + 16, 127, 15, Margin.Four);
            textures.Scroller.ButtonH_Hover    = new Bordered(texture, 384, 128 + 32, 127, 15, Margin.Four);
            textures.Scroller.ButtonH_Down     = new Bordered(texture, 384, 128 + 48, 127, 15, Margin.Four);
            textures.Scroller.ButtonH_Disabled = new Bordered(texture, 384, 128 + 64, 127, 15, Margin.Four);

            textures.Scroller.Button.Normal   = new Bordered[4];
            textures.Scroller.Button.Disabled = new Bordered[4];
            textures.Scroller.Button.Hover    = new Bordered[4];
            textures.Scroller.Button.Down     = new Bordered[4];

            textures.Tree.Background = new Bordered(texture, 256, 128, 127, 127, new Margin(16, 16, 16, 16));
            textures.Tree.Plus       = new Single(texture, 448, 96, 15, 15);
            textures.Tree.Minus      = new Single(texture, 464, 96, 15, 15);

            textures.Input.Button.Normal   = new Bordered(texture, 480, 0, 31, 31, Margin.Eight);
            textures.Input.Button.Hovered  = new Bordered(texture, 480, 32, 31, 31, Margin.Eight);
            textures.Input.Button.Disabled = new Bordered(texture, 480, 64, 31, 31, Margin.Eight);
            textures.Input.Button.Pressed  = new Bordered(texture, 480, 96, 31, 31, Margin.Eight);

            for (int i = 0; i < 4; i++)
            {
                textures.Scroller.Button.Normal[i]   = new Bordered(texture, 464 + 0, 208 + i * 16, 15, 15, Margin.Two);
                textures.Scroller.Button.Hover[i]    = new Bordered(texture, 480, 208 + i * 16, 15, 15, Margin.Two);
                textures.Scroller.Button.Down[i]     = new Bordered(texture, 464, 272 + i * 16, 15, 15, Margin.Two);
                textures.Scroller.Button.Disabled[i] = new Bordered(texture, 480 + 48, 272 + i * 16, 15, 15, Margin.Two);
            }

            textures.Input.ListBox.Background       = new Bordered(texture, 256, 256, 63, 127, Margin.Eight);
            textures.Input.ListBox.Hovered          = new Bordered(texture, 320, 320, 31, 31, Margin.Eight);
            textures.Input.ListBox.EvenLine         = new Bordered(texture, 352, 256, 31, 31, Margin.Eight);
            textures.Input.ListBox.OddLine          = new Bordered(texture, 352, 288, 31, 31, Margin.Eight);
            textures.Input.ListBox.EvenLineSelected = new Bordered(texture, 320, 256, 31, 31, Margin.Eight);
            textures.Input.ListBox.OddLineSelected  = new Bordered(texture, 320, 288, 31, 31, Margin.Eight);

            textures.Input.ComboBox.Normal   = new Bordered(texture, 384, 336, 127, 31, new Margin(8, 8, 32, 8));
            textures.Input.ComboBox.Hover    = new Bordered(texture, 384, 336 + 32, 127, 31, new Margin(8, 8, 32, 8));
            textures.Input.ComboBox.Down     = new Bordered(texture, 384, 336 + 64, 127, 31, new Margin(8, 8, 32, 8));
            textures.Input.ComboBox.Disabled = new Bordered(texture, 384, 336 + 96, 127, 31, new Margin(8, 8, 32, 8));

            textures.Input.ComboBox.Button.Normal   = new Single(texture, 496, 272, 15, 15);
            textures.Input.ComboBox.Button.Hover    = new Single(texture, 496, 272 + 16, 15, 15);
            textures.Input.ComboBox.Button.Down     = new Single(texture, 496, 272 + 32, 15, 15);
            textures.Input.ComboBox.Button.Disabled = new Single(texture, 496, 272 + 48, 15, 15);

            textures.Input.UpDown.Up.Normal     = new Single(texture, 384, 112, 7, 7);
            textures.Input.UpDown.Up.Hover      = new Single(texture, 384 + 8, 112, 7, 7);
            textures.Input.UpDown.Up.Down       = new Single(texture, 384 + 16, 112, 7, 7);
            textures.Input.UpDown.Up.Disabled   = new Single(texture, 384 + 24, 112, 7, 7);
            textures.Input.UpDown.Down.Normal   = new Single(texture, 384, 120, 7, 7);
            textures.Input.UpDown.Down.Hover    = new Single(texture, 384 + 8, 120, 7, 7);
            textures.Input.UpDown.Down.Down     = new Single(texture, 384 + 16, 120, 7, 7);
            textures.Input.UpDown.Down.Disabled = new Single(texture, 384 + 24, 120, 7, 7);

            textures.ProgressBar.Back  = new Bordered(texture, 384, 0, 31, 31, Margin.Eight);
            textures.ProgressBar.Front = new Bordered(texture, 384 + 32, 0, 31, 31, Margin.Eight);

            textures.Input.Slider.H.Normal   = new Single(texture, 416, 32, 15, 15);
            textures.Input.Slider.H.Hover    = new Single(texture, 416, 32 + 16, 15, 15);
            textures.Input.Slider.H.Down     = new Single(texture, 416, 32 + 32, 15, 15);
            textures.Input.Slider.H.Disabled = new Single(texture, 416, 32 + 48, 15, 15);

            textures.Input.Slider.V.Normal   = new Single(texture, 416 + 16, 32, 15, 15);
            textures.Input.Slider.V.Hover    = new Single(texture, 416 + 16, 32 + 16, 15, 15);
            textures.Input.Slider.V.Down     = new Single(texture, 416 + 16, 32 + 32, 15, 15);
            textures.Input.Slider.V.Disabled = new Single(texture, 416 + 16, 32 + 48, 15, 15);

            textures.CategoryList.Outer  = new Bordered(texture, 256, 384, 63, 63, Margin.Eight);
            textures.CategoryList.Inner  = new Bordered(texture, 256 + 64, 384, 63, 63, new Margin(8, 21, 8, 8));
            textures.CategoryList.Header = new Bordered(texture, 320, 352, 63, 31, Margin.Eight);
        }
        #endregion

        #region UI elements
        public override void DrawButton(Control.ControlBase control, bool depressed, bool hovered, bool disabled)
        {
            if (disabled)
            {
                textures.Input.Button.Disabled.Draw(Renderer, control.RenderBounds);
                return;
            }
            if (depressed)
            {
                textures.Input.Button.Pressed.Draw(Renderer, control.RenderBounds);
                return;
            }
            if (hovered)
            {
                textures.Input.Button.Hovered.Draw(Renderer, control.RenderBounds);
                return;
            }
            textures.Input.Button.Normal.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawMenuRightArrow(Control.ControlBase control)
        {
            textures.Menu.RightArrow.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawMenuItem(Control.ControlBase control, bool submenuOpen, bool isChecked)
        {
            if (submenuOpen || control.IsHovered)
                textures.Menu.Hover.Draw(Renderer, control.RenderBounds);

            if (isChecked)
                textures.Menu.Check.Draw(Renderer, new Rectangle(control.RenderBounds.X + 4, control.RenderBounds.Y + 3, 15, 15));
        }

        public override void DrawMenuStrip(Control.ControlBase control)
        {
            textures.Menu.Strip.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawMenu(Control.ControlBase control, bool paddingDisabled)
        {
            if (!paddingDisabled)
            {
                textures.Menu.BackgroundWithMargin.Draw(Renderer, control.RenderBounds);
                return;
            }

            textures.Menu.Background.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawShadow(Control.ControlBase control)
        {
            Rectangle r = control.RenderBounds;
            r.X -= 4;
            r.Y -= 4;
            r.Width += 10;
            r.Height += 10;
            textures.Shadow.Draw(Renderer, r);
        }

        public override void DrawRadioButton(Control.ControlBase control, bool selected, bool depressed)
        {
            if (selected)
            {
                if (control.IsDisabled)
                    textures.RadioButton.Disabled.Checked.Draw(Renderer, control.RenderBounds);
                else
                    textures.RadioButton.Active.Checked.Draw(Renderer, control.RenderBounds);
            }
            else
            {
                if (control.IsDisabled)
                    textures.RadioButton.Disabled.Normal.Draw(Renderer, control.RenderBounds);
                else
                    textures.RadioButton.Active.Normal.Draw(Renderer, control.RenderBounds);
            }
        }

        public override void DrawCheckBox(Control.ControlBase control, bool selected, bool depressed)
        {
            if (selected)
            {
                if (control.IsDisabled)
                    textures.CheckBox.Disabled.Checked.Draw(Renderer, control.RenderBounds);
                else
                    textures.CheckBox.Active.Checked.Draw(Renderer, control.RenderBounds);
            }
            else
            {
                if (control.IsDisabled)
                    textures.CheckBox.Disabled.Normal.Draw(Renderer, control.RenderBounds);
                else
                    textures.CheckBox.Active.Normal.Draw(Renderer, control.RenderBounds);
            }
        }

        public override void DrawGroupBox(Control.ControlBase control, int textStart, int textHeight, int textWidth)
        {
            Rectangle rect = control.RenderBounds;

            rect.Y += (int)(textHeight * 0.5f);
            rect.Height -= (int)(textHeight * 0.5f);

            Color m_colDarker = Color.FromArgb(50, 0, 50, 60);
            Color m_colLighter = Color.FromArgb(150, 255, 255, 255);

            Renderer.DrawColor = m_colLighter;

            Renderer.DrawFilledRect(new Rectangle(rect.X + 1, rect.Y + 1, textStart - 3, 1));
            Renderer.DrawFilledRect(new Rectangle(rect.X + 1 + textStart + textWidth, rect.Y + 1, rect.Width - textStart + textWidth - 2, 1));
            Renderer.DrawFilledRect(new Rectangle(rect.X + 1, (rect.Y + rect.Height) - 1, rect.X + rect.Width - 2, 1));

            Renderer.DrawFilledRect(new Rectangle(rect.X + 1, rect.Y + 1, 1, rect.Height));
            Renderer.DrawFilledRect(new Rectangle((rect.X + rect.Width) - 2, rect.Y + 1, 1, rect.Height - 1));

            Renderer.DrawColor = m_colDarker;

            Renderer.DrawFilledRect(new Rectangle(rect.X + 1, rect.Y, textStart - 3, 1));
            Renderer.DrawFilledRect(new Rectangle(rect.X + 1 + textStart + textWidth, rect.Y, rect.Width - textStart - textWidth - 2, 1));
            Renderer.DrawFilledRect(new Rectangle(rect.X + 1, (rect.Y + rect.Height) - 1, rect.X + rect.Width - 2, 1));

            Renderer.DrawFilledRect(new Rectangle(rect.X, rect.Y + 1, 1, rect.Height - 1));
            Renderer.DrawFilledRect(new Rectangle((rect.X + rect.Width) - 1, rect.Y + 1, 1, rect.Height - 1));
        }

        public override void DrawTextBox(Control.ControlBase control)
        {
            if (control.IsDisabled)
            {
                textures.TextBox.Disabled.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (control.HasFocus)
                textures.TextBox.Focus.Draw(Renderer, control.RenderBounds);
            else
                textures.TextBox.Normal.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawTabButton(Control.ControlBase control, bool active, Pos dir)
        {
            if (active)
            {
                drawActiveTabButton(control, dir);
                return;
            }

            if (dir == Pos.Top)
            {
                textures.Tab.Top.Inactive.Draw(Renderer, control.RenderBounds);
                return;
            }
            if (dir == Pos.Left)
            {
                textures.Tab.Left.Inactive.Draw(Renderer, control.RenderBounds);
                return;
            }
            if (dir == Pos.Bottom)
            {
                textures.Tab.Bottom.Inactive.Draw(Renderer, control.RenderBounds);
                return;
            }
            if (dir == Pos.Right)
            {
                textures.Tab.Right.Inactive.Draw(Renderer, control.RenderBounds);
                return;
            }
        }

        private void drawActiveTabButton(Control.ControlBase control, Pos dir)
        {
            if (dir == Pos.Top)
            {
                textures.Tab.Top.Active.Draw(Renderer, control.RenderBounds.Add(new Rectangle(0, 0, 0, 8)));
                return;
            }
            if (dir == Pos.Left)
            {
                textures.Tab.Left.Active.Draw(Renderer, control.RenderBounds.Add(new Rectangle(0, 0, 8, 0)));
                return;
            }
            if (dir == Pos.Bottom)
            {
                textures.Tab.Bottom.Active.Draw(Renderer, control.RenderBounds.Add(new Rectangle(0, -8, 0, 8)));
                return;
            }
            if (dir == Pos.Right)
            {
                textures.Tab.Right.Active.Draw(Renderer, control.RenderBounds.Add(new Rectangle(-8, 0, 8, 0)));
                return;
            }
        }

        public override void DrawTabControl(Control.ControlBase control)
        {
            textures.Tab.Control.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawTabTitleBar(Control.ControlBase control)
        {
            textures.Tab.HeaderBar.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawWindow(Control.ControlBase control, int topHeight, bool inFocus)
        {
            if (inFocus) 
               textures.Window.Normal.Draw(Renderer, control.RenderBounds);
            else 
                textures.Window.Inactive.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawHighlight(Control.ControlBase control)
        {
            Rectangle rect = control.RenderBounds;
            Renderer.DrawColor = Color.FromArgb(255, 255, 100, 255);
            Renderer.DrawFilledRect(rect);
        }

        public override void DrawScrollBar(Control.ControlBase control, bool horizontal, bool depressed)
        {
            if (horizontal)
                textures.Scroller.TrackH.Draw(Renderer, control.RenderBounds);
            else
                textures.Scroller.TrackV.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawScrollBarBar(Control.ControlBase control, bool depressed, bool hovered, bool horizontal)
        {
            if (!horizontal)
            {
                if (control.IsDisabled)
                {
                    textures.Scroller.ButtonV_Disabled.Draw(Renderer, control.RenderBounds);
                    return;
                }

                if (depressed)
                {
                    textures.Scroller.ButtonV_Down.Draw(Renderer, control.RenderBounds);
                    return;
                }

                if (hovered)
                {
                    textures.Scroller.ButtonV_Hover.Draw(Renderer, control.RenderBounds);
                    return;
                }

                textures.Scroller.ButtonV_Normal.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (control.IsDisabled)
            {
                textures.Scroller.ButtonH_Disabled.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (depressed)
            {
                textures.Scroller.ButtonH_Down.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (hovered)
            {
                textures.Scroller.ButtonH_Hover.Draw(Renderer, control.RenderBounds);
                return;
            }

            textures.Scroller.ButtonH_Normal.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawProgressBar(Control.ControlBase control, bool horizontal, float progress)
        {
            Rectangle rect = control.RenderBounds;

            if (horizontal)
            {
                textures.ProgressBar.Back.Draw(Renderer, rect);
                rect.Width = (int) (rect.Width*progress);
                if(rect.Width > 0)
                    textures.ProgressBar.Front.Draw(Renderer, rect);
            }
            else
            {
                textures.ProgressBar.Back.Draw(Renderer, rect);
                rect.Y = (int) (rect.Y + rect.Height*(1 - progress));
                rect.Height = (int)(rect.Height * progress);
                if(rect.Height > 0)
                    textures.ProgressBar.Front.Draw(Renderer, rect);
            }
        }

        public override void DrawListBox(Control.ControlBase control)
        {
            textures.Input.ListBox.Background.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawListBoxLine(Control.ControlBase control, bool selected, bool even)
        {
            if (selected)
            {
                if (even)
                {
                    textures.Input.ListBox.EvenLineSelected.Draw(Renderer, control.RenderBounds);
                    return;
                }
                textures.Input.ListBox.OddLineSelected.Draw(Renderer, control.RenderBounds);
                return;
            }
            
            if (control.IsHovered)
            {
                textures.Input.ListBox.Hovered.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (even)
            {
                textures.Input.ListBox.EvenLine.Draw(Renderer, control.RenderBounds);
                return;
            }

            textures.Input.ListBox.OddLine.Draw(Renderer, control.RenderBounds);
        }

        public void DrawSliderNotchesH(Rectangle rect, int numNotches, float dist)
        {
            if (numNotches == 0) return;

            float iSpacing = rect.Width / (float)numNotches;
            for (int i = 0; i < numNotches + 1; i++)
                Renderer.DrawFilledRect(Util.FloatRect(rect.X + iSpacing * i, rect.Y + dist - 2, 1, 5));
        }

        public void DrawSliderNotchesV(Rectangle rect, int numNotches, float dist)
        {
            if (numNotches == 0) return;

            float iSpacing = rect.Height / (float)numNotches;
            for (int i = 0; i < numNotches + 1; i++)
                Renderer.DrawFilledRect(Util.FloatRect(rect.X + dist - 2, rect.Y + iSpacing * i, 5, 1));
        }

        public override void DrawSlider(Control.ControlBase control, bool horizontal, int numNotches, int barSize)
        {
            Rectangle rect = control.RenderBounds;
            Renderer.DrawColor = Color.FromArgb(100, 0, 0, 0);

            if (horizontal)
            {
                rect.X += (int) (barSize*0.5);
                rect.Width -= barSize;
                rect.Y += (int)(rect.Height * 0.5 - 1);
                rect.Height = 1;
                DrawSliderNotchesH(rect, numNotches, barSize*0.5f);
                Renderer.DrawFilledRect(rect);
                return;
            }

            rect.Y += (int)(barSize * 0.5);
            rect.Height -= barSize;
            rect.X += (int)(rect.Width * 0.5 - 1);
            rect.Width = 1;
            DrawSliderNotchesV(rect, numNotches, barSize * 0.4f);
            Renderer.DrawFilledRect(rect);
        }

        public override void DrawComboBox(Control.ControlBase control, bool down, bool open)
        {
            if (control.IsDisabled)
            {
                textures.Input.ComboBox.Disabled.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (down || open)
            {
                textures.Input.ComboBox.Down.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (control.IsHovered)
            {
                textures.Input.ComboBox.Down.Draw(Renderer, control.RenderBounds);
                return;
            }

            textures.Input.ComboBox.Normal.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawKeyboardHighlight(Control.ControlBase control, Rectangle r, int offset)
        {
            Rectangle rect = r;

            rect.X += offset;
            rect.Y += offset;
            rect.Width -= offset * 2;
            rect.Height -= offset * 2;

            //draw the top and bottom
            bool skip = true;
            for (int i = 0; i < rect.Width * 0.5; i++)
            {
                renderer.DrawColor = Color.Black;
                if (!skip)
                {
                    Renderer.DrawPixel(rect.X + (i * 2), rect.Y);
                    Renderer.DrawPixel(rect.X + (i * 2), rect.Y + rect.Height - 1);
                }
                else
                    skip = false;
            }

            for (int i = 0; i < rect.Height * 0.5; i++)
            {
                Renderer.DrawColor = Color.Black;
                Renderer.DrawPixel(rect.X, rect.Y + i * 2);
                Renderer.DrawPixel(rect.X + rect.Width - 1, rect.Y + i * 2);
            }
        }

        public override void DrawToolTip(Control.ControlBase control)
        {
            textures.Tooltip.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawScrollButton(Control.ControlBase control, Pos direction, bool depressed, bool hovered, bool disabled)
        {
            int i = 0;
            if (direction == Pos.Top) i = 1;
            if (direction == Pos.Right) i = 2;
            if (direction == Pos.Bottom) i = 3;

            if (disabled)
            {
                textures.Scroller.Button.Disabled[i].Draw(Renderer, control.RenderBounds);
                return;
            }

            if (depressed)
            {
                textures.Scroller.Button.Down[i].Draw(Renderer, control.RenderBounds);
                return;
            }

            if (hovered)
            {
                textures.Scroller.Button.Hover[i].Draw(Renderer, control.RenderBounds);
                return;
            }

            textures.Scroller.Button.Normal[i].Draw(Renderer, control.RenderBounds);
        }

        public override void DrawComboBoxArrow(Control.ControlBase control, bool hovered, bool down, bool open, bool disabled)
        {
            if (disabled)
            {
                textures.Input.ComboBox.Button.Disabled.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (down || open)
            {
                textures.Input.ComboBox.Button.Down.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (hovered)
            {
                textures.Input.ComboBox.Button.Hover.Draw(Renderer, control.RenderBounds);
                return;
            }

            textures.Input.ComboBox.Button.Normal.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawNumericUpDownButton(Control.ControlBase control, bool depressed, bool up)
        {
            if (up)
            {
                if (control.IsDisabled)
                {
                    textures.Input.UpDown.Up.Disabled.DrawCenter(Renderer, control.RenderBounds);
                    return;
                }

                if (depressed)
                {
                    textures.Input.UpDown.Up.Down.DrawCenter(Renderer, control.RenderBounds);
                    return;
                }

                if (control.IsHovered)
                {
                    textures.Input.UpDown.Up.Hover.DrawCenter(Renderer, control.RenderBounds);
                    return;
                }

                textures.Input.UpDown.Up.Normal.DrawCenter(Renderer, control.RenderBounds);
                return;
            }

            if (control.IsDisabled)
            {
                textures.Input.UpDown.Down.Disabled.DrawCenter(Renderer, control.RenderBounds);
                return;
            }

            if (depressed)
            {
                textures.Input.UpDown.Down.Down.DrawCenter(Renderer, control.RenderBounds);
                return;
            }

            if (control.IsHovered)
            {
                textures.Input.UpDown.Down.Hover.DrawCenter(Renderer, control.RenderBounds);
                return;
            }

            textures.Input.UpDown.Down.Normal.DrawCenter(Renderer, control.RenderBounds);
        }

        public override void DrawStatusBar(Control.ControlBase control)
        {
            textures.StatusBar.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawTreeButton(Control.ControlBase control, bool open)
        {
            Rectangle rect = control.RenderBounds;

            if (open)
                textures.Tree.Minus.Draw(Renderer, rect);
            else
                textures.Tree.Plus.Draw(Renderer, rect);
        }

        public override void DrawTreeControl(Control.ControlBase control)
        {
            textures.Tree.Background.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawTreeNode(Control.ControlBase ctrl, bool open, bool selected, int labelHeight, int labelWidth, int halfWay, int lastBranch, bool isRoot)
        {
            if (selected)
            {
                textures.Selection.Draw(Renderer, new Rectangle(17, 0, labelWidth + 2, labelHeight - 1));
            }

            base.DrawTreeNode(ctrl, open, selected, labelHeight, labelWidth, halfWay, lastBranch, isRoot);
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
                Renderer.DrawFilledRect(Util.FloatRect(rect.Width * 0.5f, rect.Height * 0.5f, rect.Width * 0.5f, rect.Height * 0.5f));
            }

            Renderer.DrawColor = color;
            Renderer.DrawFilledRect(rect);

            Renderer.DrawColor = Color.Black;
            Renderer.DrawLinedRect(rect);
        }

        public override void DrawModalControl(Control.ControlBase control)
        {
            if (!control.ShouldDrawBackground)
                return;
            Rectangle rect = control.RenderBounds;
            Renderer.DrawColor = Colors.ModalBackground;
            Renderer.DrawFilledRect(rect);
        }

        public override void DrawMenuDivider(Control.ControlBase control)
        {
            Rectangle rect = control.RenderBounds;
            Renderer.DrawColor = Color.FromArgb(100, 0, 0, 0);
            Renderer.DrawFilledRect(rect);
        }

        public override void DrawWindowCloseButton(Control.ControlBase control, bool depressed, bool hovered, bool disabled)
        {

            if (disabled)
            {
                textures.Window.Close_Disabled.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (depressed)
            {
                textures.Window.Close_Down.Draw(Renderer, control.RenderBounds);
                return;
            }

            if (hovered)
            {
                textures.Window.Close_Hover.Draw(Renderer, control.RenderBounds);
                return;
            }

            textures.Window.Close.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawSliderButton(Control.ControlBase control, bool depressed, bool horizontal)
        {
            if (!horizontal)
            {
                if (control.IsDisabled)
                {
                    textures.Input.Slider.V.Disabled.DrawCenter(Renderer, control.RenderBounds);
                    return;
                }
                
                if (depressed)
                {
                    textures.Input.Slider.V.Down.DrawCenter(Renderer, control.RenderBounds);
                    return;
                }
                
                if (control.IsHovered)
                {
                    textures.Input.Slider.V.Hover.DrawCenter(Renderer, control.RenderBounds);
                    return;
                }

                textures.Input.Slider.V.Normal.DrawCenter(Renderer, control.RenderBounds);
                return;
            }

            if (control.IsDisabled)
            {
                textures.Input.Slider.H.Disabled.DrawCenter(Renderer, control.RenderBounds);
                return;
            }

            if (depressed)
            {
                textures.Input.Slider.H.Down.DrawCenter(Renderer, control.RenderBounds);
                return;
            }

            if (control.IsHovered)
            {
                textures.Input.Slider.H.Hover.DrawCenter(Renderer, control.RenderBounds);
                return;
            }

            textures.Input.Slider.H.Normal.DrawCenter(Renderer, control.RenderBounds);
        }

        public override void DrawCategoryHolder(Control.ControlBase control)
        {
            textures.CategoryList.Outer.Draw(Renderer, control.RenderBounds);
        }

        public override void DrawCategoryInner(Control.ControlBase control, bool collapsed)
        {
            if (collapsed)
                textures.CategoryList.Header.Draw(Renderer, control.RenderBounds);
            else
                textures.CategoryList.Inner.Draw(Renderer, control.RenderBounds);
        }
        #endregion
    }
}
