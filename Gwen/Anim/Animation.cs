using System;
using System.Collections.Generic;
using Gwen.Control;

namespace Gwen.Anim
{
    public class Animation
    {
        protected ControlBase control;

        //private static List<Animation> g_AnimationsListed = new List<Animation>(); // unused
        private static readonly Dictionary<ControlBase, List<Animation>> animations = new Dictionary<ControlBase, List<Animation>>();

        protected virtual void think()
        {
            
        }

        public virtual bool Finished
        {
            get { throw new InvalidOperationException("Pure virtual function call"); }
        }

        public static void Add(ControlBase control, Animation animation)
        {
            animation.control = control;
            if (!animations.ContainsKey(control))
                animations[control] = new List<Animation>();
            animations[control].Add(animation);
        }

        public static void Cancel(ControlBase control)
        {
            if (animations.ContainsKey(control))
            {
                animations[control].Clear();
                animations.Remove(control);
            }
        }

        internal static void GlobalThink()
        {
            foreach (KeyValuePair<ControlBase, List<Animation>> pair in animations)
            {
                var valCopy = pair.Value.FindAll(x =>true); // list copy so foreach won't break when we remove elements
                foreach (Animation animation in valCopy)
                {
                    animation.think();
                    if (animation.Finished)
                    {
                        pair.Value.Remove(animation);
                    }
                }
            }
        }
    }
}
