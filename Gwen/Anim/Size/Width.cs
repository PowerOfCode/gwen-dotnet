using System;

namespace Gwen.Anim.Size
{
    class Width : TimedAnimation
    {
        private int startSize;
        private int delta;
        private bool hide;

        public Width(int startSize, int endSize, float length, bool hide = false, float delay = 0.0f, float ease = 1.0f)
            : base(length, delay, ease)
        {
            this.startSize = startSize;
            delta = endSize - startSize;
            this.hide = hide;
        }

        protected override void onStart()
        {
            base.onStart();
            control.Width = startSize;
        }

        protected override void run(float delta)
        {
            base.run(delta);
            control.Width = (int)Math.Round(startSize + (delta * delta));
        }

        protected override void onFinish()
        {
            base.onFinish();
            control.Width = startSize + delta;
            control.IsHidden = hide;
        }
    }
}
