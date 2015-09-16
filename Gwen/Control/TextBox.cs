using System;
using System.Drawing;
using Gwen.Input;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Text box (editable).
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class TextBox : Label
    {
        private bool selectAll;

        private int cursorPos;
        private int cursorEnd;

        protected Rectangle selectionBounds;
        protected Rectangle caretBounds;

        protected float lastInputTime;

        protected override bool accelOnlyFocus { get { return true; } }
        protected override bool needsInputChars { get { return true; } }

        /// <summary>
        /// Determines whether text should be selected when the control is focused.
        /// </summary>
        public bool SelectAllOnFocus { get { return selectAll; } set { selectAll = value; if (value) onSelectAll(this, EventArgs.Empty); } }

        /// <summary>
        /// Indicates whether the text has active selection.
        /// </summary>
        public virtual bool HasSelection { get { return cursorPos != cursorEnd; } }

        /// <summary>
        /// Invoked when the text has changed.
        /// </summary>
		public event GwenEventHandler<EventArgs> TextChanged;

        /// <summary>
        /// Invoked when the submit key has been pressed.
        /// </summary>
		public event GwenEventHandler<EventArgs> SubmitPressed;

        /// <summary>
        /// Current cursor position (character index).
        /// </summary>
        public int CursorPos
        {
            get { return cursorPos; }
            set
            {
                if (cursorPos == value) return;

                cursorPos = value;
                refreshCursorBounds();
            }
        }

        public int CursorEnd
        {
            get { return cursorEnd; }
            set
            {
                if (cursorEnd == value) return;

                cursorEnd = value;
                refreshCursorBounds();
            }
        }

        /// <summary>
        /// Determines whether the control can insert text at a given cursor position.
        /// </summary>
        /// <param name="text">Text to check.</param>
        /// <param name="position">Cursor position.</param>
        /// <returns>True if allowed.</returns>
        protected virtual bool isTextAllowed(string text, int position)
        {
            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBox"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public TextBox(ControlBase parent)
            : base(parent)
        {
			AutoSizeToContents = false;
            SetSize(200, 20);

            MouseInputEnabled = true;
            KeyboardInputEnabled = true;

            Alignment = Pos.Left | Pos.CenterV;
            TextPadding = new Padding(4, 2, 4, 2);

            cursorPos = 0;
            cursorEnd = 0;
            selectAll = false;

            TextColor = Color.FromArgb(255, 50, 50, 50); // TODO: From Skin

            IsTabable = true;

            AddAccelerator("Ctrl + C", onCopy);
            AddAccelerator("Ctrl + X", onCut);
            AddAccelerator("Ctrl + V", onPaste);
            AddAccelerator("Ctrl + A", onSelectAll);
        }

        /// <summary>
        /// Renders the focus overlay.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void renderFocus(Skin.SkinBase skin)
        {
            // nothing
        }

        /// <summary>
        /// Handler for text changed event.
        /// </summary>
        protected override void onTextChanged()
        {
            base.onTextChanged();

            if (cursorPos > TextLength) cursorPos = TextLength;
            if (cursorEnd > TextLength) cursorEnd = TextLength;

            if (TextChanged != null)
                TextChanged.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handler for character input event.
        /// </summary>
        /// <param name="chr">Character typed.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onChar(char chr)
        {
            base.onChar(chr);

            if (chr == '\t') return false;

            insertText(chr.ToString());
            return true;
        }

        /// <summary>
        /// Inserts text at current cursor position, erasing selection if any.
        /// </summary>
        /// <param name="text">Text to insert.</param>
        protected virtual void insertText(string text)
        {
            // TODO: Make sure fits (implement maxlength)

            if (HasSelection)
            {
                EraseSelection();
            }

            if (cursorPos > TextLength)
                cursorPos = TextLength;

            if (!isTextAllowed(text, cursorPos))
                return;

            string str = Text;
            str = str.Insert(cursorPos, text);
            SetText(str);

            cursorPos += text.Length;
            cursorEnd = cursorPos;

            refreshCursorBounds();
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            base.render(skin);

            if (ShouldDrawBackground)
                skin.DrawTextBox(this);

            if (!HasFocus) return;

            // Draw selection.. if selected..
            if (cursorPos != cursorEnd)
            {
                skin.Renderer.DrawColor = Color.FromArgb(200, 50, 170, 255);
                skin.Renderer.DrawFilledRect(selectionBounds);
            }

            // Draw caret
            float time = Platform.Neutral.GetTimeInSeconds() - lastInputTime;

            if ((time % 1.0f) <= 0.5f)
            {
                skin.Renderer.DrawColor = Color.Black;
                skin.Renderer.DrawFilledRect(caretBounds);
            }
        }

        protected virtual void refreshCursorBounds()
        {
            lastInputTime = Platform.Neutral.GetTimeInSeconds();

            makeCaretVisible();

            Point pA = GetCharacterPosition(cursorPos);
            Point pB = GetCharacterPosition(cursorEnd);

            selectionBounds.X = Math.Min(pA.X, pB.X);
            selectionBounds.Y = TextY - 1;
            selectionBounds.Width = Math.Max(pA.X, pB.X) - selectionBounds.X;
            selectionBounds.Height = TextHeight + 2;

            caretBounds.X = pA.X;
            caretBounds.Y = TextY - 1;
            caretBounds.Width = 1;
            caretBounds.Height = TextHeight + 2;

            Redraw();
        }

        /// <summary>
        /// Handler for Paste event.
        /// </summary>
        /// <param name="from">Source control.</param>
        protected override void onPaste(ControlBase from, EventArgs args)
        {
            base.onPaste(from, args);
            insertText(Platform.Neutral.GetClipboardText());
        }

        /// <summary>
        /// Handler for Copy event.
        /// </summary>
        /// <param name="from">Source control.</param>
        protected override void onCopy(ControlBase from, EventArgs args)
        {
            if (!HasSelection) return;
            base.onCopy(from, args);

            Platform.Neutral.SetClipboardText(GetSelection());
        }

        /// <summary>
        /// Handler for Cut event.
        /// </summary>
        /// <param name="from">Source control.</param>
        protected override void onCut(ControlBase from, EventArgs args)
        {
            if (!HasSelection) return;
            base.onCut(from, args);

            Platform.Neutral.SetClipboardText(GetSelection());
            EraseSelection();
        }

        /// <summary>
        /// Handler for Select All event.
        /// </summary>
        /// <param name="from">Source control.</param>
        protected override void onSelectAll(ControlBase from, EventArgs args)
        {
            //base.onSelectAll(from);
            cursorEnd = 0;
            cursorPos = TextLength;

            refreshCursorBounds();
        }

        /// <summary>
        /// Handler invoked on mouse double click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        protected override void onMouseDoubleClickedLeft(int x, int y)
        {
            //base.onMouseDoubleClickedLeft(x, y);
			onSelectAll(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handler for Return keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onKeyReturn(bool down)
        {
            base.onKeyReturn(down);
            if (down) return true;

            onReturn();

            // Try to move to the next control, as if tab had been pressed
            onKeyTab(true);

            // If we still have focus, blur it.
            if (HasFocus)
            {
                Blur();
            }

            return true;
        }

        /// <summary>
        /// Handler for Backspace keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onKeyBackspace(bool down)
        {
            base.onKeyBackspace(down);

            if (!down) return true;
            if (HasSelection)
            {
                EraseSelection();
                return true;
            }

            if (cursorPos == 0) return true;

            DeleteText(cursorPos - 1, 1);

            return true;
        }

        /// <summary>
        /// Handler for Delete keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onKeyDelete(bool down)
        {
            base.onKeyDelete(down);
            if (!down) return true;
            if (HasSelection)
            {
                EraseSelection();
                return true;
            }

            if (cursorPos >= TextLength) return true;

            DeleteText(cursorPos, 1);

            return true;
        }

        /// <summary>
        /// Handler for Left Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onKeyLeft(bool down)
        {
            base.onKeyLeft(down);
            if (!down) return true;

            if (cursorPos > 0)
                cursorPos--;

            if (!Input.InputHandler.IsShiftDown)
            {
                cursorEnd = cursorPos;
            }

            refreshCursorBounds();
            return true;
        }

        /// <summary>
        /// Handler for Right Arrow keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onKeyRight(bool down)
        {
            base.onKeyRight(down);
            if (!down) return true;

            if (cursorPos < TextLength)
                cursorPos++;

            if (!Input.InputHandler.IsShiftDown)
            {
                cursorEnd = cursorPos;
            }

            refreshCursorBounds();
            return true;
        }

        /// <summary>
        /// Handler for Home keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onKeyHome(bool down)
        {
            base.onKeyHome(down);
            if (!down) return true;
            cursorPos = 0;

            if (!Input.InputHandler.IsShiftDown)
            {
                cursorEnd = cursorPos;
            }

            refreshCursorBounds();
            return true;
        }

        /// <summary>
        /// Handler for End keyboard event.
        /// </summary>
        /// <param name="down">Indicates whether the key was pressed or released.</param>
        /// <returns>
        /// True if handled.
        /// </returns>
        protected override bool onKeyEnd(bool down)
        {
            base.onKeyEnd(down);
            cursorPos = TextLength;

            if (!Input.InputHandler.IsShiftDown)
            {
                cursorEnd = cursorPos;
            }

            refreshCursorBounds();
            return true;
        }

        /// <summary>
        /// Returns currently selected text.
        /// </summary>
        /// <returns>Current selection.</returns>
        public string GetSelection()
        {
            if (!HasSelection) return String.Empty;

            int start = Math.Min(cursorPos, cursorEnd);
            int end = Math.Max(cursorPos, cursorEnd);

            string str = Text;
            return str.Substring(start, end - start);
        }

        /// <summary>
        /// Deletes text.
        /// </summary>
        /// <param name="startPos">Starting cursor position.</param>
        /// <param name="length">Length in characters.</param>
        public virtual void DeleteText(int startPos, int length)
        {
            string str = Text;
            str = str.Remove(startPos, length);
            SetText(str);

            if (cursorPos > startPos)
            {
                CursorPos = cursorPos - length;
            }

            CursorEnd = cursorPos;
        }

        /// <summary>
        /// Deletes selected text.
        /// </summary>
        public virtual void EraseSelection()
        {
            int start = Math.Min(cursorPos, cursorEnd);
            int end = Math.Max(cursorPos, cursorEnd);

            DeleteText(start, end - start);

            // Move the cursor to the start of the selection,
            // since the end is probably outside of the string now.
            cursorPos = start;
            cursorEnd = start;
        }

        /// <summary>
        /// Handler invoked on mouse click (left) event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="down">If set to <c>true</c> mouse button is down.</param>
        protected override void onMouseClickedLeft(int x, int y, bool down)
        {
            base.onMouseClickedLeft(x, y, down);
            if (selectAll)
            {
                onSelectAll(this, EventArgs.Empty);
                //m_SelectAll = false;
                return;
            }

            int c = getClosestCharacter(x, y).X;

            if (down)
            {
                CursorPos = c;

                if (!Input.InputHandler.IsShiftDown)
                    CursorEnd = c;

                InputHandler.MouseFocus = this;
            }
            else
            {
                if (InputHandler.MouseFocus == this)
                {
                    CursorPos = c;
                    InputHandler.MouseFocus = null;
                }
            }
        }

        /// <summary>
        /// Handler invoked on mouse moved event.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="dx">X change.</param>
        /// <param name="dy">Y change.</param>
        protected override void onMouseMoved(int x, int y, int dx, int dy)
        {
            base.onMouseMoved(x, y, dx, dy);
            if (InputHandler.MouseFocus != this) return;

            int c = getClosestCharacter(x, y).X;

            CursorPos = c;
        }

        protected virtual void makeCaretVisible()
        {
            int caretPos = GetCharacterPosition(cursorPos).X - TextX;

            // If the caret is already in a semi-good position, leave it.
            {
                int realCaretPos = caretPos + TextX;
                if (realCaretPos > Width * 0.1f && realCaretPos < Width * 0.9f)
                    return;
            }

            // The ideal position is for the caret to be right in the middle
            int idealx = (int)(-caretPos + Width * 0.5f);

            // Don't show too much whitespace to the right
            if (idealx + TextWidth < Width - TextPadding.Right)
                idealx = -TextWidth + (Width - TextPadding.Right);

            // Or the left
            if (idealx > TextPadding.Left)
                idealx = TextPadding.Left;

            setTextPosition(idealx, TextY);
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            base.layout(skin);

            refreshCursorBounds();
        }

        /// <summary>
        /// Handler for the return key.
        /// </summary>
        protected virtual void onReturn()
        {
            if (SubmitPressed != null)
				SubmitPressed.Invoke(this, EventArgs.Empty);
        }
    }
}
