using Gwen.Control;
using Newtonsoft.Json;

namespace Gwen.ControlInternal
{
    /// <summary>
    /// Properties node.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class PropertyTreeNode : TreeNode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTreeNode"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public PropertyTreeNode(ControlBase parent)
            : base(parent)
        {
            title.TextColorOverride = Skin.Colors.Properties.Title;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            skin.DrawPropertyTreeNode(this, innerPanel.X, innerPanel.Y);
        }
    }
}
