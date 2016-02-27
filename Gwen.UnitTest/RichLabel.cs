using System;
using System.Drawing;
using Gwen.Control;

namespace Gwen.UnitTest
{
    public class RichLabel : GUnit
    {
        private Font f1, f2, f3, f4, f5, f6;

        public RichLabel(ControlBase parent) : base(parent)
        {
            Control.RichLabel label = new Control.RichLabel(this);
            label.SetBounds(10, 10, 600, 400);

            f1 = new Font(Skin.Renderer, "Arial", 15);
            label.AddText("This test uses Arial 15, Red. Padding.\n", Color.Red, f1);

            f2 = new Font(Skin.Renderer, "Times New Roman", 20, FontStyle.Bold);
            label.AddText("This text uses Times New Roman Bold 20, Green. Padding.\n", Color.Green, f2);

            f3 = new Font(Skin.Renderer, "Courier New", 15, FontStyle.Italic);
            label.AddText("This text uses Courier New Italic 15, Blue. Padding.\n", Color.Blue, f3);

            f4 = new Font(Skin.Renderer, "Arial", 15, FontStyle.Underline);
            label.AddText("This text uses Arial Underline 15, Red. Padding.\n", Color.Red, f4);

            f5 = new Font(Skin.Renderer, "Times New Roman", 15, FontStyle.Strikeout);
            label.AddText("This text uses Times New Roman Strikeout 15, Green. Padding.\n", Color.Green, f5);

            f6 = new Font(Skin.Renderer, "Courier New", 15, FontStyle.Italic | FontStyle.Bold | FontStyle.Underline | FontStyle.Strikeout);
            label.AddText("This text uses Courier New <All Styles> 15, Blue. Padding.\n", Color.Blue, f6);
        }

        public override void Dispose()
        {
            f1.Dispose();
            f2.Dispose();
            f3.Dispose();
            f4.Dispose();
            f5.Dispose();
            f6.Dispose();
            base.Dispose();
        }
    }
}
