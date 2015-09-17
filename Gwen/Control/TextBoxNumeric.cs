﻿using System;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Numeric text box - accepts only float numbers.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class TextBoxNumeric : TextBox
    {
        /// <summary>
        /// Current numeric value.
        /// </summary>
        protected float value;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBoxNumeric"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public TextBoxNumeric(ControlBase parent) : base(parent)
        {
			AutoSizeToContents = false;
            SetText("0", false);
        }

        protected virtual bool isTextAllowed(string str)
        {
            if (str == "" || str == "-")
                return true; // annoying if single - is not allowed
            float d;
            return float.TryParse(str, out d);
        }

        /// <summary>
        /// Determines whether the control can insert text at a given cursor position.
        /// </summary>
        /// <param name="text">Text to check.</param>
        /// <param name="position">Cursor position.</param>
        /// <returns>True if allowed.</returns>
        protected override bool isTextAllowed(string text, int position)
        {
            string newText = Text.Insert(position, text);
            return isTextAllowed(newText);
        }

        /// <summary>
        /// Current numerical value.
        /// </summary>
        public virtual float Value
        {
            get { return value; }
            set
            {
                this.value = value;
                Text = value.ToString();
            }
        }

        // text -> value
        /// <summary>
        /// Handler for text changed event.
        /// </summary>
        protected override void onTextChanged()
        {
            if (String.IsNullOrEmpty(Text) || Text == "-")
            {
                value = 0;
                //SetText("0");
            }
            else
                value = float.Parse(Text);
            base.onTextChanged();
        }

        /// <summary>
        /// Sets the control text.
        /// </summary>
        /// <param name="str">Text to set.</param>
        /// <param name="doEvents">Determines whether to invoke "text changed" event.</param>
        public override void SetText(string str, bool doEvents = true)
        {
            if (isTextAllowed(str))
                base.SetText(str, doEvents);
        }
    }
}
