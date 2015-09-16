using System;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Text box with masked text.
    /// </summary>
    /// <remarks>
    /// This class doesn't prevent programatic access to the text in any way.
    /// </remarks>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class TextBoxPassword : TextBox
    {
        private string mask;
        private char maskCharacter;

        /// <summary>
        /// Character used in place of actual characters for display.
        /// </summary>
        public char MaskCharacter { get { return maskCharacter; } set { maskCharacter = value; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBoxPassword"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public TextBoxPassword(ControlBase parent)
            : base(parent)
        {
            maskCharacter = '*';
        }

        /// <summary>
        /// Handler for text changed event.
        /// </summary>
        protected override void onTextChanged()
        {
            mask = new String(MaskCharacter, Text.Length);
            TextOverride = mask;
            base.onTextChanged();
        }
    }
}
