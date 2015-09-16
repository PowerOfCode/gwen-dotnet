using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using Gwen.Input;
using Newtonsoft.Json;

namespace Gwen.Control
{
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class MultilineTextBox : Label
    {
		private readonly ScrollControl scrollControl;

		private bool selectAll;

		private Point cursorPos;
		private Point cursorEnd;

		protected Rectangle caretBounds;

		private float lastInputTime;

		private List<string> textLines = new List<string>();

		private Point startPoint {
			get {
				if (CursorPosition.Y == cursorEnd.Y) {
					return CursorPosition.X < CursorEnd.X ? CursorPosition : CursorEnd;
				} else {
					return CursorPosition.Y < CursorEnd.Y ? CursorPosition : CursorEnd;
				}
			}
		}

		private Point endPoint {
			get {
				if (CursorPosition.Y == cursorEnd.Y) {
					return CursorPosition.X > CursorEnd.X ? CursorPosition : CursorEnd;
				} else {
					return CursorPosition.Y > CursorEnd.Y ? CursorPosition : CursorEnd;
				}
			}
		}

		/// <summary>
		/// Indicates whether the text has active selection.
		/// </summary>
		public bool HasSelection { get { return cursorPos != cursorEnd; } }

		/// <summary>
		/// Invoked when the text has changed.
		/// </summary>
		public event GwenEventHandler<EventArgs> TextChanged;

		/// <summary>
		/// Get a point representing where the cursor physically appears on the screen.
		/// Y is line number, X is character position on that line.
		/// </summary>
		public Point CursorPosition {
			get {
				if (textLines == null || textLines.Count() == 0)
					return new Point(0, 0);

				int Y = cursorPos.Y;
				Y = Math.Max(Y, 0);
				Y = Math.Min(Y, textLines.Count() - 1);

				int X = cursorPos.X; //X may be beyond the last character, but we will want to draw it at the end of line.
				X = Math.Max(X, 0);
				X = Math.Min(X, textLines[Y].Length);

				return new Point(X, Y);
			}
			set {
				cursorPos.X = value.X;
				cursorPos.Y = value.Y;
				refreshCursorBounds();
			}
		}

		/// <summary>
		/// Get a point representing where the endpoint of text selection.
		/// Y is line number, X is character position on that line.
		/// </summary>
		public Point CursorEnd {
			get {
				if (textLines == null || textLines.Count() == 0)
					return new Point(0, 0);

				int Y = cursorEnd.Y;
				Y = Math.Max(Y, 0);
				Y = Math.Min(Y, textLines.Count() - 1);

				int X = cursorEnd.X; //X may be beyond the last character, but we will want to draw it at the end of line.
				X = Math.Max(X, 0);
				X = Math.Min(X, textLines[Y].Length);

				return new Point(X, Y);
			}
			set {
				cursorEnd.X = value.X;
				cursorEnd.Y = value.Y;
				refreshCursorBounds();
			}
		}

		/// <summary>
		/// Indicates whether the control will accept Tab characters as input.
		/// </summary>
		public bool AcceptTabs { get; set; }

		/// <summary>
		/// Returns the number of lines that are in the Multiline Text Box.
		/// </summary>
        public int TotalLines
        {
            get
            {
				return textLines.Count;
            }
        }

		/// <summary>
		/// Gets and sets the text to display to the user. Each line is seperated by
		/// an Environment.NetLine character.
		/// </summary>
		public override string Text {
			get {
				string ret = "";
				for (int i = 0; i < TotalLines; i++){
					ret += textLines[i];
					if (i != TotalLines - 1) {
						ret += Environment.NewLine;
					}
				}
				return ret;
			}
			set {
				//Label (base) calls SetText.
				//SetText is overloaded to dump value into TextLines.
				//We're cool.
				base.Text = value;
			}
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBox"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public MultilineTextBox(ControlBase parent) : base(parent)
        {
            AutoSizeToContents = false;
            SetSize(200, 20);

			MouseInputEnabled = true;
			KeyboardInputEnabled = true;

			Alignment = Pos.Left | Pos.Top;
			TextPadding = new Padding(4, 2, 4, 2);

			cursorPos = new Point(0, 0);
			cursorEnd = new Point(0, 0);
			selectAll = false;

			TextColor = Color.FromArgb(255, 50, 50, 50); // TODO: From Skin

			IsTabable = false;
			AcceptTabs = true;

            scrollControl = new ScrollControl(this);
            scrollControl.Dock = Pos.Fill;
            scrollControl.EnableScroll(true, true);
            scrollControl.AutoHideBars = true;
            scrollControl.Margin = Margin.One;
            innerPanel = scrollControl;
            text.Parent = innerPanel;
            scrollControl.InnerPanel.BoundsChanged += new GwenEventHandler<EventArgs>(scrollChanged);


			textLines.Add(String.Empty);

			// [halfofastaple] TODO Figure out where these numbers come from. See if we can remove the magic numbers.
			//	This should be as simple as 'm_ScrollControl.AutoSizeToContents = true' or 'm_ScrollControl.NoBounds()'
            scrollControl.SetInnerSize(1000, 1000);

			AddAccelerator("Ctrl + C", onCopy);
			AddAccelerator("Ctrl + X", onCut);
			AddAccelerator("Ctrl + V", onPaste);
			AddAccelerator("Ctrl + A", onSelectAll);
        }

		public string GetTextLine(int index) {
			return textLines[index];
		}

		public void SetTextLine(int index, string value) {
			textLines[index] = value;
		}

		/// <summary>
		/// Refreshes the cursor location and selected area when the inner panel scrolls
		/// </summary>
		/// <param name="control">The inner panel the text is embedded in</param>
        private void scrollChanged(ControlBase control, EventArgs args)
        {
            refreshCursorBounds();
        }

		/// <summary>
		/// Handler for text changed event.
		/// </summary>
		protected override void onTextChanged() {
			base.onTextChanged();
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
		protected override bool onChar(char chr) {
			//base.onChar(chr);
			if (chr == '\t' && !AcceptTabs) return false;

			insertText(chr.ToString());
			return true;
		}

		/// <summary>
		/// Inserts text at current cursor position, erasing selection if any.
		/// </summary>
		/// <param name="text">Text to insert.</param>
		protected void insertText(string text) {
			// TODO: Make sure fits (implement maxlength)

			if (HasSelection) {
				EraseSelection();
			}

			string str = textLines[cursorPos.Y];
			str = str.Insert(CursorPosition.X, text);
			textLines[cursorPos.Y] = str;

			cursorPos.X = CursorPosition.X + text.Length;
			cursorEnd = cursorPos;

			Invalidate();
			refreshCursorBounds();
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void render(Skin.SkinBase skin) {
			base.render(skin);

			if (ShouldDrawBackground)
				skin.DrawTextBox(this);

			if (!HasFocus) return;

			int VerticalOffset = 2 - scrollControl.VerticalScroll;
			int VerticalSize = Font.Size + 6;

			// Draw selection.. if selected..
			if (cursorPos != cursorEnd) {
				if (startPoint.Y == endPoint.Y) {
					Point pA = getCharacterPosition(startPoint);
					Point pB = getCharacterPosition(endPoint);

					Rectangle SelectionBounds = new Rectangle();
					SelectionBounds.X = Math.Min(pA.X, pB.X);
					SelectionBounds.Y = pA.Y - VerticalOffset;
					SelectionBounds.Width = Math.Max(pA.X, pB.X) - SelectionBounds.X;
					SelectionBounds.Height = VerticalSize;

					skin.Renderer.DrawColor = Color.FromArgb(200, 50, 170, 255);
					skin.Renderer.DrawFilledRect(SelectionBounds);
				} else {
					/* Start */
					Point pA = getCharacterPosition(startPoint);
					Point pB = getCharacterPosition(new Point(textLines[startPoint.Y].Length, startPoint.Y));

					Rectangle SelectionBounds = new Rectangle();
					SelectionBounds.X = Math.Min(pA.X, pB.X);
					SelectionBounds.Y = pA.Y - VerticalOffset;
					SelectionBounds.Width = Math.Max(pA.X, pB.X) - SelectionBounds.X;
					SelectionBounds.Height = VerticalSize;

					skin.Renderer.DrawColor = Color.FromArgb(200, 50, 170, 255);
					skin.Renderer.DrawFilledRect(SelectionBounds);

					/* Middle */
					for (int i = 1; i < endPoint.Y - startPoint.Y; i++) {
						pA = getCharacterPosition(new Point(0, startPoint.Y + i));
						pB = getCharacterPosition(new Point(textLines[startPoint.Y + i].Length, startPoint.Y + i));

						SelectionBounds = new Rectangle();
						SelectionBounds.X = Math.Min(pA.X, pB.X);
						SelectionBounds.Y = pA.Y - VerticalOffset;
						SelectionBounds.Width = Math.Max(pA.X, pB.X) - SelectionBounds.X;
						SelectionBounds.Height = VerticalSize;

						skin.Renderer.DrawColor = Color.FromArgb(200, 50, 170, 255);
						skin.Renderer.DrawFilledRect(SelectionBounds);
					}

					/* End */
					pA = getCharacterPosition(new Point(0, endPoint.Y));
					pB = getCharacterPosition(endPoint);

					SelectionBounds = new Rectangle();
					SelectionBounds.X = Math.Min(pA.X, pB.X);
					SelectionBounds.Y = pA.Y - VerticalOffset;
					SelectionBounds.Width = Math.Max(pA.X, pB.X) - SelectionBounds.X;
					SelectionBounds.Height = VerticalSize;

					skin.Renderer.DrawColor = Color.FromArgb(200, 50, 170, 255);
					skin.Renderer.DrawFilledRect(SelectionBounds);
				}
			}

			// Draw caret
			float time = Platform.Neutral.GetTimeInSeconds() - lastInputTime;

			if ((time % 1.0f) <= 0.5f) {
				skin.Renderer.DrawColor = Color.Black;
				skin.Renderer.DrawFilledRect(caretBounds);
			}
		}

		protected void refreshCursorBounds() {
			lastInputTime = Platform.Neutral.GetTimeInSeconds();

			makeCaretVisible();

			Point pA = getCharacterPosition(CursorPosition);
			Point pB = getCharacterPosition(cursorEnd);

			//m_SelectionBounds.X = Math.Min(pA.X, pB.X);
			//m_SelectionBounds.Y = TextY - 1;
			//m_SelectionBounds.Width = Math.Max(pA.X, pB.X) - m_SelectionBounds.X;
			//m_SelectionBounds.Height = TextHeight + 2;

			caretBounds.X = pA.X;
			caretBounds.Y = (pA.Y + 1);

			caretBounds.Y += scrollControl.VerticalScroll;

			caretBounds.Width = 1;
			caretBounds.Height = Font.Size + 2;

			Redraw();
		}

		/// <summary>
		/// Handler for Paste event.
		/// </summary>
		/// <param name="from">Source control.</param>
		protected override void onPaste(ControlBase from, EventArgs args) {
			base.onPaste(from, args);
			insertText(Platform.Neutral.GetClipboardText());
		}

		/// <summary>
		/// Handler for Copy event.
		/// </summary>
		/// <param name="from">Source control.</param>
		protected override void onCopy(ControlBase from, EventArgs args) {
			if (!HasSelection) return;
			base.onCopy(from, args);

			Platform.Neutral.SetClipboardText(GetSelection());
		}

		/// <summary>
		/// Handler for Cut event.
		/// </summary>
		/// <param name="from">Source control.</param>
		protected override void onCut(ControlBase from, EventArgs args) {
			if (!HasSelection) return;
			base.onCut(from, args);

			Platform.Neutral.SetClipboardText(GetSelection());
			EraseSelection();
		}


		/// <summary>
		/// Handler for Select All event.
		/// </summary>
		/// <param name="from">Source control.</param>
		protected override void onSelectAll(ControlBase from, EventArgs args) {
			//base.onSelectAll(from);
			cursorEnd = new Point(0, 0);
			cursorPos = new Point(textLines.Last().Length, textLines.Count());

			refreshCursorBounds();
		}

		/// <summary>
		/// Handler invoked on mouse double click (left) event.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		protected override void onMouseDoubleClickedLeft(int x, int y) {
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
		protected override bool onKeyReturn(bool down) {
			if (down) return true;

			//Split current string, putting the rhs on a new line
			string CurrentLine = textLines[cursorPos.Y];
			string lhs = CurrentLine.Substring(0, CursorPosition.X);
			string rhs = CurrentLine.Substring(CursorPosition.X);

			textLines[cursorPos.Y] = lhs;
			textLines.Insert(cursorPos.Y + 1, rhs);

			onKeyDown(true);
			onKeyHome(true);

			if (cursorPos.Y == TotalLines - 1) {
				scrollControl.ScrollToBottom();
			}

			Invalidate();
			refreshCursorBounds();

			return true;
		}

		/// <summary>
		/// Handler for Backspace keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool onKeyBackspace(bool down) {
			if (!down) return true;

			if (HasSelection) {
				EraseSelection();
				return true;
			}

			if (cursorPos.X == 0) {
				if (cursorPos.Y == 0) {
					return true; //Nothing left to delete
				} else {
					string lhs = textLines[cursorPos.Y - 1];
					string rhs = textLines[cursorPos.Y];
					textLines.RemoveAt(cursorPos.Y);
					onKeyUp(true);
					onKeyEnd(true);
					textLines[cursorPos.Y] = lhs + rhs;
				}
			} else {
				string CurrentLine = textLines[cursorPos.Y];
				string lhs = CurrentLine.Substring(0, CursorPosition.X - 1);
				string rhs = CurrentLine.Substring(CursorPosition.X);
				textLines[cursorPos.Y] = lhs + rhs;
				onKeyLeft(true);
			}

			Invalidate();
			refreshCursorBounds();

			return true;
		}

		/// <summary>
		/// Handler for Delete keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool onKeyDelete(bool down) {
			if (!down) return true;

			if (HasSelection) {
				EraseSelection();
				return true;
			}

			if (cursorPos.X == textLines[cursorPos.Y].Length) {
				if (cursorPos.Y == textLines.Count - 1) {
					return true; //Nothing left to delete
				} else {
					string lhs = textLines[cursorPos.Y];
					string rhs = textLines[cursorPos.Y + 1];
					textLines.RemoveAt(cursorPos.Y + 1);
					onKeyEnd(true);
					textLines[cursorPos.Y] = lhs + rhs;
				}
			} else {
				string CurrentLine = textLines[cursorPos.Y];
				string lhs = CurrentLine.Substring(0, CursorPosition.X);
				string rhs = CurrentLine.Substring(CursorPosition.X + 1);
				textLines[cursorPos.Y] = lhs + rhs;
			}

			Invalidate();
			refreshCursorBounds();

			return true;
		}

		/// <summary>
		/// Handler for Up Arrow keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool onKeyUp(bool down) {
			if (!down) return true;

			if (cursorPos.Y > 0) {
				cursorPos.Y -= 1;
			}

			if (!Input.InputHandler.IsShiftDown) {
				cursorEnd = cursorPos;
			}

			Invalidate();
			refreshCursorBounds();

			return true;
		}

		/// <summary>
		/// Handler for Down Arrow keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool onKeyDown(bool down) {
			if (!down) return true;

			if (cursorPos.Y < TotalLines - 1) {
				cursorPos.Y += 1;
			}

			if (!Input.InputHandler.IsShiftDown) {
				cursorEnd = cursorPos;
			}

			Invalidate();
			refreshCursorBounds();

			return true;
		}

		/// <summary>
		/// Handler for Left Arrow keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool onKeyLeft(bool down) {
			if (!down) return true;

			if (cursorPos.X > 0) {
				cursorPos.X = Math.Min(cursorPos.X - 1, textLines[cursorPos.Y].Length);
			} else {
				if (cursorPos.Y > 0) {
					onKeyUp(down);
					onKeyEnd(down);
				}
			}

			if (!Input.InputHandler.IsShiftDown) {
				cursorEnd = cursorPos;
			}

			Invalidate();
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
		protected override bool onKeyRight(bool down) {
			if (!down) return true;

			if (cursorPos.X < textLines[cursorPos.Y].Length) {
				cursorPos.X = Math.Min(cursorPos.X + 1, textLines[cursorPos.Y].Length);
			} else {
				if (cursorPos.Y < textLines.Count - 1) {
					onKeyDown(down);
					onKeyHome(down);
				}
			}

			if (!Input.InputHandler.IsShiftDown) {
				cursorEnd = cursorPos;
			}

			Invalidate();
			refreshCursorBounds();

			return true;
		}

		/// <summary>
		/// Handler for Home Key keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool onKeyHome(bool down) {
			if (!down) return true;

			cursorPos.X = 0;

			if (!Input.InputHandler.IsShiftDown) {
				cursorEnd = cursorPos;
			}

			Invalidate();
			refreshCursorBounds();

			return true;
		}

		/// <summary>
		/// Handler for End Key keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool onKeyEnd(bool down) {
			if (!down) return true;

			cursorPos.X = textLines[cursorPos.Y].Length;

			if (!Input.InputHandler.IsShiftDown) {
				cursorEnd = cursorPos;
			}

			Invalidate();
			refreshCursorBounds();

			return true;
		}

		/// <summary>
		/// Handler for Tab Key keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool onKeyTab(bool down) {
			if (!AcceptTabs) return base.onKeyTab(down);
			if (!down) return false;

			onChar('\t');
			return true;
		}

		/// <summary>
		/// Returns currently selected text.
		/// </summary>
		/// <returns>Current selection.</returns>
		public string GetSelection() {
			if (!HasSelection) return String.Empty;

			string str = String.Empty;

			if (startPoint.Y == endPoint.Y) {
				int start = startPoint.X;
				int end = endPoint.X;

				str = textLines[cursorPos.Y];
				str = str.Substring(start, end - start);
			} else {
				str = String.Empty;
				str += textLines[startPoint.Y].Substring(startPoint.X); //Copy start
				for (int i = 1; i < endPoint.Y - startPoint.Y; i++) {
					str += textLines[startPoint.Y + i]; //Copy middle
				}
				str += textLines[endPoint.Y].Substring(0, endPoint.X); //Copy end
			}

			return str;
		}

		//[halfofastaple] TODO Implement this and use it. The end user can work around not having it, but it is terribly convenient.
		//	See the delete key handler for help. Eventually, the delete key should use this.
		///// <summary>
		///// Deletes text.
		///// </summary>
		///// <param name="startPos">Starting cursor position.</param>
		///// <param name="length">Length in characters.</param>
		//public void DeleteText(Point StartPos, int length) {
		//    /* Single Line Delete */
		//    if (StartPos.X + length <= m_TextLines[StartPos.Y].Length) {
		//        string str = m_TextLines[StartPos.Y];
		//        str = str.Remove(StartPos.X, length);
		//        m_TextLines[StartPos.Y] = str;

		//        if (CursorPosition.X > StartPos.X) {
		//            m_CursorPos.X = CursorPosition.X - length;
		//        }

		//        m_CursorEnd = m_CursorPos;
		//    /* Multiline Delete */
		//    } else {
				
		//    }
		//}

		/// <summary>
		/// Deletes selected text.
		/// </summary>
		public void EraseSelection() {
			if (startPoint.Y == endPoint.Y) {
				int start = startPoint.X;
				int end = endPoint.X;

				textLines[startPoint.Y] = textLines[startPoint.Y].Remove(start, end - start);
			} else {
				/* Remove Start */
				if (startPoint.X < textLines[startPoint.Y].Length) {
					textLines[startPoint.Y] = textLines[startPoint.Y].Remove(startPoint.X);
				}

				/* Remove Middle */
				for (int i = 1; i < endPoint.Y - startPoint.Y; i++) {
					textLines.RemoveAt(startPoint.Y + 1);
				}

				/* Remove End */
				if (endPoint.X < textLines[startPoint.Y + 1].Length) {
					textLines[startPoint.Y] += textLines[startPoint.Y + 1].Substring(endPoint.X);
				}
				textLines.RemoveAt(startPoint.Y + 1);
			}

			// Move the cursor to the start of the selection,
			// since the end is probably outside of the string now.
			cursorPos = startPoint;
			cursorEnd = startPoint;

			Invalidate();
			refreshCursorBounds();
		}

		/// <summary>
		/// Handler invoked on mouse click (left) event.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="down">If set to <c>true</c> mouse button is down.</param>
		protected override void onMouseClickedLeft(int x, int y, bool down) {
			base.onMouseClickedLeft(x, y, down);
			if (selectAll) {
				onSelectAll(this, EventArgs.Empty);
				//m_SelectAll = false;
				return;
			}

			Point coords = getClosestCharacter(x, y);

			if (down) {
				CursorPosition = coords;

				if (!Input.InputHandler.IsShiftDown)
					CursorEnd = coords;

				InputHandler.MouseFocus = this;
			} else {
				if (InputHandler.MouseFocus == this) {
					CursorPosition = coords;
					InputHandler.MouseFocus = null;
				}
			}

			Invalidate();
			refreshCursorBounds();
		}

		/// <summary>
		/// Returns index of the character closest to specified point (in canvas coordinates).
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		protected override Point getClosestCharacter(int px, int py) {
			Point p = CanvasPosToLocal(new Point(px, py));
			double distance = Double.MaxValue;
			Point Best = new Point(0, 0);
			string sub = String.Empty;

			/* Find the appropriate Y row (always pick whichever y the mouse currently is on) */
			for (int y = 0; y < textLines.Count(); y++) {
				sub += textLines[y] + Environment.NewLine;
				Point cp = Skin.Renderer.MeasureText(Font, sub);

				double YDist = Math.Abs(cp.Y - p.Y);
				if (YDist < distance) {
					distance = YDist;
					Best.Y = y;
				}
			}

			/* Find the best X row, closest char */
			sub = String.Empty;
			distance = Double.MaxValue;
			for (int x = 0; x <= textLines[Best.Y].Count(); x++) {
				if (x < textLines[Best.Y].Count()) {
					sub += textLines[Best.Y][x];
				} else {
					sub += " ";
				}

				Point cp = Skin.Renderer.MeasureText(Font, sub);

				double XDiff = Math.Abs(cp.X - p.X);

				if (XDiff < distance){
					distance = XDiff;
					Best.X = x;
				}
			}

			return Best;
		}

		/// <summary>
		/// Handler invoked on mouse moved event.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="dx">X change.</param>
		/// <param name="dy">Y change.</param>
		protected override void onMouseMoved(int x, int y, int dx, int dy) {
			base.onMouseMoved(x, y, dx, dy);
			if (InputHandler.MouseFocus != this) return;

			Point c = getClosestCharacter(x, y);

			CursorPosition = c;

			Invalidate();
			refreshCursorBounds();
		}

		protected virtual void makeCaretVisible() {
			int caretPos = getCharacterPosition(CursorPosition).X - TextX;

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
        /// Handler invoked when control children's bounds change.
        /// </summary>
        /// <param name="oldChildBounds"></param>
        /// <param name="child"></param>
        protected override void onChildBoundsChanged(System.Drawing.Rectangle oldChildBounds, ControlBase child)
        {
            if (scrollControl != null)
            {
                scrollControl.UpdateScrollBars();
            }
        }

		/// <summary>
		/// Sets the label text.
		/// </summary>
		/// <param name="str">Text to set.</param>
		/// <param name="doEvents">Determines whether to invoke "text changed" event.</param>
		public override void SetText(string str, bool doEvents = true) {
			string EasySplit = str.Replace("\r\n", "\n").Replace("\r", "\n");
			string[] Lines = EasySplit.Split('\n');

			textLines = new List<string>(Lines);

			Invalidate();
			refreshCursorBounds();
		}

		/// <summary>
		/// Invalidates the control.
		/// </summary>
		/// <remarks>
		/// Causes layout, repaint, invalidates cached texture.
		/// </remarks>
		public override void Invalidate() {
			if (text != null) {
				text.String = Text;
			}
			if (AutoSizeToContents)
				SizeToContents();

			base.Invalidate();
			InvalidateParent();
			onTextChanged();
		}

		private Point getCharacterPosition(Point CursorPosition) {
			if (textLines.Count == 0) {
				return new Point(0, 0);
			}
			string CurrLine = textLines[CursorPosition.Y].Substring(0, Math.Min(CursorPosition.X, textLines[CursorPosition.Y].Length));

			string sub = "";
			for (int i = 0; i < CursorPosition.Y; i++) {
				sub += textLines[i] + "\n";
			}

			Point p = new Point(Skin.Renderer.MeasureText(Font, CurrLine).X, Skin.Renderer.MeasureText(Font, sub).Y);

			return new Point(p.X + text.X, p.Y + text.Y + TextPadding.Top);
		}

		protected override bool onMouseWheeled(int delta) {
			return scrollControl.InputMouseWheeled(delta);
		}
    }
}