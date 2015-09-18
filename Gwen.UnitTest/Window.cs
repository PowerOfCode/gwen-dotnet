using System;
using Gwen.Control;

namespace Gwen.UnitTest
{
    public class Window : GUnit
    {
        private int m_WindowCount;
        private readonly Random rand;

        public Window(ControlBase parent)
            : base(parent)
        {
            rand = new Random();

            Control.Button button1 = new Control.Button(this);
            button1.SetText("Open a Window");
            button1.Clicked += OpenWindow;

            Control.Button button2 = new Control.Button(this);
            button2.SetText("Open a MessageBox");
            button2.Clicked += OpenMsgbox;
            Align.PlaceRightBottom(button2, button1, 10);

            m_WindowCount = 1;
        }

		void OpenWindow(ControlBase control, EventArgs args)
        {
            Control.WindowControl window = new Control.WindowControl(GetCanvas());
            window.Title = String.Format("Window {0}", m_WindowCount);
            window.SetSize(rand.Next(200, 400), rand.Next(200, 400));
            window.SetPosition(rand.Next(700), rand.Next(400));

            Control.Button b1 = new Control.Button(window);
            b1.Text = "Test";
            var size = window.DrawAreaSize;
            b1.SetPosition(size.X - b1.Width, size.Y - b1.Height);
            window.Resized += (s, e) => {
                var size2 = window.DrawAreaSize;
                b1.SetPosition(size2.X - b1.Width, size2.Y - b1.Height);
            };

            m_WindowCount++;
        }

		void OpenMsgbox(ControlBase control, EventArgs args)
        {
            MessageBox window = new MessageBox(GetCanvas(), String.Format("Window {0}   MessageBox window = new MessageBox(GetCanvas(), String.Format(  MessageBox window = new MessageBox(GetCanvas(), String.Format(", m_WindowCount));
            window.SetPosition(rand.Next(700), rand.Next(400));

            m_WindowCount++;
        }
    }
}
