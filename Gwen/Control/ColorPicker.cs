using System;
using System.Drawing;
using Gwen.ControlInternal;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// RGBA color picker.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class ColorPicker : ControlBase, IColorPicker
    {
        private Color color;

        /// <summary>
        /// Selected color.
        /// </summary>
        public Color SelectedColor { get { return color; } set { color = value; updateControls(); } }

        /// <summary>
        /// Red value of the selected color.
        /// </summary>
        public int R { get { return color.R; } set { color = Color.FromArgb(color.A, value, color.G, color.B); } }

        /// <summary>
        /// Green value of the selected color.
        /// </summary>
        public int G { get { return color.G; } set { color = Color.FromArgb(color.A, color.R, value, color.B); } }

        /// <summary>
        /// Blue value of the selected color.
        /// </summary>
        public int B { get { return color.B; } set { color = Color.FromArgb(color.A, color.R, color.G, value); } }

        /// <summary>
        /// Alpha value of the selected color.
        /// </summary>
        public int A { get { return color.A; } set { color = Color.FromArgb(value, color.R, color.G, color.B); } }

        /// <summary>
        /// Invoked when the selected color has been changed.
        /// </summary>
		public event GwenEventHandler<EventArgs> ColorChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorPicker"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ColorPicker(ControlBase parent)
            : base(parent)
        {
            MouseInputEnabled = true;

            SetSize(256, 150);
            createControls();
            SelectedColor = Color.FromArgb(255, 50, 60, 70);
        }

        private void createColorControl(string name, int y)
        {
            const int colorSize = 12;

            GroupBox colorGroup = new GroupBox(this);
            colorGroup.SetPosition(10, y);
            colorGroup.SetText(name);
            colorGroup.SetSize(160, 35);
            colorGroup.Name = name + "groupbox";

            ColorDisplay disp = new ColorDisplay(colorGroup);
            disp.Name = name;
            disp.SetBounds(0, 10, colorSize, colorSize);

            TextBoxNumeric numeric = new TextBoxNumeric(colorGroup);
            numeric.Name = name + "Box";
            numeric.SetPosition(105, 7);
            numeric.SetSize(26, 16);
            numeric.SelectAllOnFocus = true;
            numeric.TextChanged += numericTyped;

            HorizontalSlider slider = new HorizontalSlider(colorGroup);
            slider.SetPosition(colorSize + 5, 10);
            slider.SetRange(0, 255);
            slider.SetSize(80, colorSize);
            slider.Name = name + "Slider";
            slider.ValueChanged += slidersMoved;
        }

		private void numericTyped(ControlBase control, EventArgs args)
        {
            TextBoxNumeric box = control as TextBoxNumeric;
            if (null == box)
                return;

            if (box.Text == string.Empty)
                return;

            int textValue = (int)box.Value;
            if (textValue < 0) textValue = 0;
            if (textValue > 255) textValue = 255;

            if (box.Name.Contains("Red"))
                R = textValue;

            if (box.Name.Contains("Green"))
                G = textValue;

            if (box.Name.Contains("Blue"))
                B = textValue;

            if (box.Name.Contains("Alpha"))
                A = textValue;

            updateControls();
        }

        private void createControls()
        {
            const int startY = 5;
            const int height = 35;

            createColorControl("Red", startY);
            createColorControl("Green", startY + height);
            createColorControl("Blue", startY + height * 2);
            createColorControl("Alpha", startY + height * 3);

            GroupBox finalGroup = new GroupBox(this);
            finalGroup.SetPosition(180, 40);
            finalGroup.SetSize(60, 60);
            finalGroup.SetText("Result");
            finalGroup.Name = "ResultGroupBox";

            ColorDisplay disp = new ColorDisplay(finalGroup);
            disp.Name = "Result";
            disp.SetBounds(0, 10, 32, 32);
            //disp.DrawCheckers = true;

            //UpdateControls();
        }

        private void updateColorControls(string name, Color col, int sliderVal)
        {
            ColorDisplay disp = FindChildByName(name, true) as ColorDisplay;
            disp.Color = col;

            HorizontalSlider slider = FindChildByName(name + "Slider", true) as HorizontalSlider;
            slider.Value = sliderVal;

            TextBoxNumeric box = FindChildByName(name + "Box", true) as TextBoxNumeric;
            box.Value = sliderVal;
        }

        private void updateControls()
        {	//This is a little weird, but whatever for now
            updateColorControls("Red", Color.FromArgb(255, SelectedColor.R, 0, 0), SelectedColor.R);
            updateColorControls("Green", Color.FromArgb(255, 0, SelectedColor.G, 0), SelectedColor.G);
            updateColorControls("Blue", Color.FromArgb(255, 0, 0, SelectedColor.B), SelectedColor.B);
            updateColorControls("Alpha", Color.FromArgb(SelectedColor.A, 255, 255, 255), SelectedColor.A);

            ColorDisplay disp = FindChildByName("Result", true) as ColorDisplay;
            disp.Color = SelectedColor;

            if (ColorChanged != null)
                ColorChanged.Invoke(this, EventArgs.Empty);
        }

        private void slidersMoved(ControlBase control, EventArgs args)
        {
            /*
            HorizontalSlider* redSlider		= gwen_cast<HorizontalSlider>(	FindChildByName( "RedSlider",   true ) );
            HorizontalSlider* greenSlider	= gwen_cast<HorizontalSlider>(	FindChildByName( "GreenSlider", true ) );
            HorizontalSlider* blueSlider	= gwen_cast<HorizontalSlider>(	FindChildByName( "BlueSlider",  true ) );
            HorizontalSlider* alphaSlider	= gwen_cast<HorizontalSlider>(	FindChildByName( "AlphaSlider", true ) );
            */

            HorizontalSlider slider = control as HorizontalSlider;
            if (slider != null)
                setColorByName(getColorFromName(slider.Name), (int)slider.Value);

            updateControls();
            //SetColor( Gwen::Color( redSlider->GetValue(), greenSlider->GetValue(), blueSlider->GetValue(), alphaSlider->GetValue() ) );
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            base.layout(skin);

            SizeToChildren(false, true);
            SetSize(Width, Height + 5);

            GroupBox groupBox = FindChildByName("ResultGroupBox", true) as GroupBox;
            if (groupBox != null)
                groupBox.SetPosition(groupBox.X, Height * 0.5f - groupBox.Height * 0.5f);

            //UpdateControls(); // this spams events continuously every tick
        }

        private int getColorByName(string colorName)
        {
            if (colorName == "Red")
                return SelectedColor.R;
            if (colorName == "Green")
                return SelectedColor.G;
            if (colorName == "Blue")
                return SelectedColor.B;
            if (colorName == "Alpha")
                return SelectedColor.A;
            return 0;
        }

        private static string getColorFromName(string name)
        {
            if (name.Contains("Red"))
                return "Red";
            if (name.Contains("Green"))
                return "Green";
            if (name.Contains("Blue"))
                return "Blue";
            if (name.Contains("Alpha"))
                return "Alpha";
            return String.Empty;
        }

        private void setColorByName(string colorName, int colorValue)
        {
            if (colorName == "Red")
                R = colorValue;
            else if (colorName == "Green")
                G = colorValue;
            else if (colorName == "Blue")
                B = colorValue;
            else if (colorName == "Alpha")
                A = colorValue;
        }

        /// <summary>
        /// Determines whether the Alpha control is visible.
        /// </summary>
        public bool AlphaVisible
        {
            get
            {
                GroupBox gb = FindChildByName("Alphagroupbox", true) as GroupBox;
                return !gb.IsHidden;
            }
            set
            {
                GroupBox gb = FindChildByName("Alphagroupbox", true) as GroupBox;
                gb.IsHidden = !value;
                Invalidate();
            }
        }
    }
}
