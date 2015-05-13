// //  (
// //  )\ )                                (         (          (
// // (()/(      (  (      (   (           )\ )      )\         )\ )   (
// //  /(_)) (   )\))(    ))\  )(      (  (()/(    (((_)   (   (()/(  ))\
// // (_))   )\ ((_)()\  /((_)(()\     )\  /(_))   )\___   )\   ((_))/((_)
// // | _ \ ((_)_(()((_)(_))   ((_)   ((_)(_) _|  ((/ __| ((_)  _| |(_))
// // |  _// _ \\ V  V // -_) | '_|  / _ \ |  _|   | (__ / _ \/ _` |/ -_)
// // |_|  \___/ \_/\_/ \___| |_|    \___/ |_|      \___|\___/\__,_|\___|
// //
// //
// // Copyright (c) 2015 Power of Code
using System;
using Newtonsoft.Json;

namespace Gwen.Control.Layout
{
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class StackPanel : ControlBase
    {
        [JsonProperty]
        public bool IsVertical { get; set; }

        public StackPanel(ControlBase parent) : base(parent)
        {
            IsVertical = true;
        }

        protected override void Layout(Gwen.Skin.SkinBase skin)
        {
            if (IsVertical)
            {
                Width = Parent.Width;
                Height = 0;
            }
            else
            {
                Height = Parent.Height;
                Width = 0;
            }

            int maxSize = 0;

            for (int i = 0; i < Children.Count; i++)
            {
                ControlBase cache = Children[i];

                if (IsVertical)
                {
                    maxSize = Math.Max(cache.Width + cache.Margin.Left + cache.Margin.Right, maxSize);
                    cache.X = cache.Margin.Left;
                    cache.Y = Height + cache.Margin.Top;
                    Height += cache.Height + cache.Margin.Top + cache.Margin.Bottom;
                }
                else
                {
                    maxSize = Math.Max(cache.Height + cache.Margin.Top + cache.Margin.Bottom, maxSize);
                    cache.X = Width + cache.Margin.Left;
                    cache.Y = cache.Margin.Top;
                    Width += cache.Width + cache.Margin.Left + cache.Margin.Right;
                }
            }

            if (IsVertical)
                Width = maxSize;
            else
                Height = maxSize;

            base.Layout(skin);
        }
    }
}

