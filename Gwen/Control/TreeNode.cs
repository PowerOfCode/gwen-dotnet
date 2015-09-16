using System;
using System.Linq;
using Gwen.ControlInternal;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Tree control node.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class TreeNode : ControlBase
    {
        public const int TreeIndentation = 14;

        protected TreeControl treeControl;
        protected Button toggleButton;
        protected Button title;
        private bool root;
        private bool selected;
        private bool selectable;

        /// <summary>
        /// Indicates whether this is a root node.
        /// </summary>
        public bool IsRoot { get { return root; } set { root = value; } }

        /// <summary>
        /// Parent tree control.
        /// </summary>
        public TreeControl TreeControl { get { return treeControl; } set { treeControl = value; } }

        /// <summary>
        /// Determines whether the node is selectable.
        /// </summary>
        public bool IsSelectable { get { return selectable; } set { selectable = value; } }

        /// <summary>
        /// Indicates whether the node is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return selected; }
            set
            {
                if (!IsSelectable)
                    return;
                if (IsSelected == value)
                    return;

                selected = value;

                if (title != null)
                    title.ToggleState = value;

                if (SelectionChanged != null)
					SelectionChanged.Invoke(this, EventArgs.Empty);

                // propagate to root parent (tree)
                if (treeControl != null && treeControl.SelectionChanged != null)
					treeControl.SelectionChanged.Invoke(this, EventArgs.Empty);

                if (value)
                {
                    if (Selected != null)
						Selected.Invoke(this, EventArgs.Empty);

                    if (treeControl != null && treeControl.Selected != null)
						treeControl.Selected.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    if (Unselected != null)
						Unselected.Invoke(this, EventArgs.Empty);

                    if (treeControl != null && treeControl.Unselected != null)
						treeControl.Unselected.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Node's label.
        /// </summary>
        public string Text { get { return title.Text; } set { title.Text = value; } }

        /// <summary>
        /// Invoked when the node label has been pressed.
        /// </summary>
		public event GwenEventHandler<EventArgs> LabelPressed;

        /// <summary>
        /// Invoked when the node's selected state has changed.
        /// </summary>
        public event GwenEventHandler<EventArgs> SelectionChanged;

        /// <summary>
        /// Invoked when the node has been selected.
        /// </summary>
		public event GwenEventHandler<EventArgs> Selected;

        /// <summary>
        /// Invoked when the node has been unselected.
        /// </summary>
		public event GwenEventHandler<EventArgs> Unselected;

        /// <summary>
        /// Invoked when the node has been expanded.
        /// </summary>
		public event GwenEventHandler<EventArgs> Expanded;

        /// <summary>
        /// Invoked when the node has been collapsed.
        /// </summary>
		public event GwenEventHandler<EventArgs> Collapsed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeNode"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public TreeNode(ControlBase parent)
            : base(parent)
        {
            toggleButton = new TreeToggleButton(this);
            toggleButton.SetBounds(0, 0, 15, 15);
            toggleButton.Toggled += onToggleButtonPress;

            title = new TreeNodeLabel(this);
            title.Dock = Pos.Top;
            title.Margin = new Margin(16, 0, 0, 0);
            title.DoubleClicked += onDoubleClickName;
            title.Clicked += onClickName;

            innerPanel = new ControlBase(this);
            innerPanel.Dock = Pos.Top;
            innerPanel.Height = 100;
            innerPanel.Margin = new Margin(TreeIndentation, 1, 0, 0);
            innerPanel.Hide();

			root = parent is TreeControl;
            selected = false;
            selectable = true;

			Dock = Pos.Top;
        }

        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void render(Skin.SkinBase skin)
        {
            int bottom = 0;
            if (innerPanel.Children.Count > 0)
            {
                bottom = innerPanel.Children.Last().Y + innerPanel.Y;
            }

            skin.DrawTreeNode(this, innerPanel.IsVisible, IsSelected, title.Height, title.TextRight,
                (int)(toggleButton.Y + toggleButton.Height * 0.5f), bottom, treeControl == Parent); // IsRoot

            //[halfofastaple] HACK - The treenodes are taking two passes until their height is set correctly,
            //  this means that the height is being read incorrectly by the parent, causing
            //  the TreeNode bug where nodes get hidden when expanding and collapsing.
            //  The hack is to constantly invalide TreeNodes, which isn't bad, but there is
            //  definitely a better solution (possibly: Make it set the height from childmost
            //  first and work it's way up?) that invalidates and draws properly in 1 loop.
            this.Invalidate();
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            if (toggleButton != null)
            {
                if (title != null)
                {
                    toggleButton.SetPosition(0, (title.Height - toggleButton.Height)*0.5f);
                }

                if (innerPanel.Children.Count == 0)
                {
                    toggleButton.Hide();
                    toggleButton.ToggleState = false;
                    innerPanel.Hide();
                }
                else
                {
                    toggleButton.Show();
                    innerPanel.SizeToChildren(false, true);
                }
            }

            base.layout(skin);
        }

        /// <summary>
        /// Function invoked after layout.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void postLayout(Skin.SkinBase skin)
        {
            if (SizeToChildren(false, true))
            {
                InvalidateParent();
            }
        }

        /// <summary>
        /// Adds a new child node.
        /// </summary>
        /// <param name="label">Node's label.</param>
        /// <returns>Newly created control.</returns>
        public TreeNode AddNode(string label)
        {
            TreeNode node = new TreeNode(this);
            node.Text = label;

            return node;
        }

        /// <summary>
        /// Opens the node.
        /// </summary>
        public void Open()
        {
            innerPanel.Show();
            if (toggleButton != null)
                toggleButton.ToggleState = true;

            if (Expanded != null)
				Expanded.Invoke(this, EventArgs.Empty);
            if (treeControl != null && treeControl.Expanded != null)
				treeControl.Expanded.Invoke(this, EventArgs.Empty);

            Invalidate();
        }

        /// <summary>
        /// Closes the node.
        /// </summary>
        public void Close()
        {
            innerPanel.Hide();
            if (toggleButton != null)
                toggleButton.ToggleState = false;

            if (Collapsed != null)
				Collapsed.Invoke(this, EventArgs.Empty);
            if (treeControl != null && treeControl.Collapsed != null)
				treeControl.Collapsed.Invoke(this, EventArgs.Empty);

            Invalidate();
        }

        /// <summary>
        /// Opens the node and all child nodes.
        /// </summary>
        public void ExpandAll()
        {
            Open();
            foreach (ControlBase child in Children)
            {
                TreeNode node = child as TreeNode;
                if (node == null)
                    continue;
                node.ExpandAll();
            }
        }

        /// <summary>
        /// Clears the selection on the node and all child nodes.
        /// </summary>
        public void UnselectAll()
        {
            IsSelected = false;
            if (title != null)
                title.ToggleState = false;

            foreach (ControlBase child in Children)
            {
                TreeNode node = child as TreeNode;
                if (node == null)
                    continue;
                node.UnselectAll();
            }
        }

        /// <summary>
        /// Handler for the toggle button.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected virtual void onToggleButtonPress(ControlBase control, EventArgs args)
        {
            if (toggleButton.ToggleState)
            {
                Open();
            }
            else
            {
                Close();
            }
        }

        /// <summary>
        /// Handler for label double click.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected virtual void onDoubleClickName(ControlBase control, EventArgs args)
        {
            if (!toggleButton.IsVisible)
                return;
            toggleButton.Toggle();
        }

        /// <summary>
        /// Handler for label click.
        /// </summary>
        /// <param name="control">Event source.</param>
		protected virtual void onClickName(ControlBase control, EventArgs args)
        {
            if (LabelPressed != null)
                LabelPressed.Invoke(this, EventArgs.Empty);
            IsSelected = !IsSelected;
        }

        public void SetImage(string textureName)
        {
            title.SetImage(textureName);
        }

		protected override void onChildAdded(ControlBase child) {
			TreeNode node = child as TreeNode;
			if (node != null) {
				node.TreeControl = treeControl;

				if (treeControl != null) {
					treeControl.OnNodeAdded(node);
				}
			}

			base.onChildAdded(child);
		}

		public override event GwenEventHandler<ClickedEventArgs> Clicked
        {
            add {
                title.Clicked += delegate(ControlBase sender, ClickedEventArgs args) { value(this, args); };
            }
            remove {
				title.Clicked -= delegate(ControlBase sender, ClickedEventArgs args) { value(this, args); };
            }
        }

		public override event GwenEventHandler<ClickedEventArgs> DoubleClicked
        {
            add {
				if (value != null) {
					title.DoubleClicked += delegate(ControlBase sender, ClickedEventArgs args) { value(this, args); };
				}
            }
            remove {
				title.DoubleClicked -= delegate(ControlBase sender, ClickedEventArgs args) { value(this, args); };
            }
        }

		public override event GwenEventHandler<ClickedEventArgs> RightClicked {
			add {
				title.RightClicked += delegate(ControlBase sender, ClickedEventArgs args) { value(this, args); };
			}
			remove {
				title.RightClicked -= delegate(ControlBase sender, ClickedEventArgs args) { value(this, args); };
			}
		}

		public override event GwenEventHandler<ClickedEventArgs> DoubleRightClicked {
			add {
				if (value != null) {
					title.DoubleRightClicked += delegate(ControlBase sender, ClickedEventArgs args) { value(this, args); };
				}
			}
			remove {
				title.DoubleRightClicked -= delegate(ControlBase sender, ClickedEventArgs args) { value(this, args); };
			}
		}

		public IEnumerable<TreeNode> SelectedChildren
		{
			get {
				List<TreeNode> Trees = new List<TreeNode>();

				foreach (ControlBase child in Children) {
					TreeNode node = child as TreeNode;
					if (node == null)
						continue;
					Trees.AddRange(node.SelectedChildren);
				}

				if (this.IsSelected) {
					Trees.Add(this);
				}

				return Trees;
			}
		}
    }
}
