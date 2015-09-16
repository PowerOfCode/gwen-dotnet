using System;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Multiline label with text chunks having different color/font.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class RichLabel : ControlBase
    {
        protected struct TextBlock
        {
            public BlockType Type;
            public string Text;
            public Color Color;
            public Font Font;
        }

        protected enum BlockType
        {
            Text,
            NewLine
        }

        private bool needsRebuild;
        private readonly List<TextBlock> textBlocks;
        private readonly string[] newline;

        /// <summary>
        /// Initializes a new instance of the <see cref="RichLabel"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public RichLabel(ControlBase parent)
            : base(parent)
        {
            newline = new string[] { Environment.NewLine };
            textBlocks = new List<TextBlock>();
        }

        /// <summary>
        /// Adds a line break to the control.
        /// </summary>
        public void AddLineBreak()
        {
            TextBlock block = new TextBlock { Type = BlockType.NewLine };
            textBlocks.Add(block);
        }

        /// <summary>
        /// Adds text to the control.
        /// </summary>
        /// <param name="text">Text to add.</param>
        /// <param name="color">Text color.</param>
        /// <param name="font">Font to use.</param>
        public void AddText(string text, Color color, Font font = null)
        {
            if (String.IsNullOrEmpty(text))
                return;

            var lines = text.Split(newline, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; i++)
            {
                if (i > 0)
                    AddLineBreak();

                TextBlock block = new TextBlock { Type = BlockType.Text, Text = lines[i], Color = color, Font = font };

                textBlocks.Add(block);
                needsRebuild = true;
                Invalidate();
            }
        }

        /// <summary>
        /// Resizes the control to fit its children.
        /// </summary>
        /// <param name="width">Determines whether to change control's width.</param>
        /// <param name="height">Determines whether to change control's height.</param>
        /// <returns>
        /// True if bounds changed.
        /// </returns>
        public override bool SizeToChildren(bool width = true, bool height = true)
        {
            rebuild();
            return base.SizeToChildren(width, height);
        }

        protected void splitLabel(string text, Font font, TextBlock block, ref int x, ref int y, ref int lineHeight)
        {
            var spaced = Util.SplitAndKeep(text, " ");
            if (spaced.Length == 0)
                return;

            int spaceLeft = Width - x;
            string leftOver;

            // Does the whole word fit in?
            Point stringSize = Skin.Renderer.MeasureText(font, text);
            if (spaceLeft > stringSize.X)
            {
                createLabel(text, block, ref x, ref y, ref lineHeight, true);
                return;
            }

            // If the first word is bigger than the line, just give up.
            Point wordSize = Skin.Renderer.MeasureText(font, spaced[0]);
            if (wordSize.X >= spaceLeft)
            {
                createLabel(spaced[0], block, ref x, ref y, ref lineHeight, true);
                if (spaced[0].Length >= text.Length)
                    return;

                leftOver = text.Substring(spaced[0].Length + 1);
                splitLabel(leftOver, font, block, ref x, ref y, ref lineHeight);
                return;
            }

            string newString = String.Empty;
            for (int i = 0; i < spaced.Length; i++)
            {
                wordSize = Skin.Renderer.MeasureText(font, newString + spaced[i]);
                if (wordSize.X > spaceLeft)
                {
                    createLabel(newString, block, ref x, ref y, ref lineHeight, true);
                    x = 0;
                    y += lineHeight;
                    break;
                }

                newString += spaced[i];
            }

            int newstr_len = newString.Length;
            if (newstr_len < text.Length)
            {
                leftOver = text.Substring(newstr_len + 1);
                splitLabel(leftOver, font, block, ref x, ref y, ref lineHeight);
            }
        }

        protected void createLabel(string text, TextBlock block, ref int x, ref int y, ref int lineHeight, bool noSplit)
        {
            // Use default font or is one set?
            Font font = Skin.DefaultFont;
            if (block.Font != null)
                font = block.Font;

            // This string is too long for us, split it up.
            Point p = Skin.Renderer.MeasureText(font, text);

            if (lineHeight == -1)
            {
                lineHeight = p.Y;
            }

            if (!noSplit)
            {
                if (x + p.X > Width)
                {
                    splitLabel(text, font, block, ref x, ref y, ref lineHeight);
                    return;
                }
            }

            // Wrap
            if (x + p.X >= Width)
            {
                createNewline(ref x, ref y, lineHeight);
            }

            Label label = new Label(this);
            label.SetText(x == 0 ? text.TrimStart(' ') : text);
            label.TextColorOverride = block.Color;
            label.Font = font;
            label.SizeToContents();
            label.SetPosition(x, y);

            //lineheight = (lineheight + pLabel.Height()) / 2;

            x += label.Width;

            if (x >= Width)
            {
                createNewline(ref x, ref y, lineHeight);
            }
        }

        protected void createNewline(ref int x, ref int y, int lineHeight)
        {
            x = 0;
            y += lineHeight;
        }

        protected void rebuild()
        {
            DeleteAllChildren();

            int x = 0;
            int y = 0;
            int lineHeight = -1;


            foreach (var block in textBlocks)
            {
                if (block.Type == BlockType.NewLine)
                {
                    createNewline(ref x, ref y, lineHeight);
                    continue;
                }

                if (block.Type == BlockType.Text)
                {
                    createLabel(block.Text, block, ref x, ref y, ref lineHeight, false);
                    continue;
                }
            }

            needsRebuild = false;
        }

        /// <summary>
        /// Handler invoked when control's bounds change.
        /// </summary>
        /// <param name="oldBounds">Old bounds.</param>
        protected override void onBoundsChanged(Rectangle oldBounds)
        {
            base.onBoundsChanged(oldBounds);
            rebuild();
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            base.layout(skin);
            if (needsRebuild)
                rebuild();

            // align bottoms. this is still not ideal, need to take font metrics into account.
            ControlBase prev = null;
            foreach (ControlBase child in Children)
            {
                if (prev != null && child.Y == prev.Y)
                {
                    Align.PlaceRightBottom(child, prev);
                }
                prev = child;
            }
        }
    }
}
