using System;
using System.Linq;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Radio button group.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class RadioButtonGroup : GroupBox
    {
        private LabeledRadioButton selected;

        /// <summary>
        /// Selected radio button.
        /// </summary>
        public LabeledRadioButton Selected { get { return selected; } }

        /// <summary>
        /// Internal name of the selected radio button.
        /// </summary>
        public string SelectedName { get { return selected.Name; } }

        /// <summary>
        /// Text of the selected radio button.
        /// </summary>
        public string SelectedLabel { get { return selected.Text; } }

        /// <summary>
        /// Index of the selected radio button.
        /// </summary>
        public int SelectedIndex { get { return Children.IndexOf(selected); } }

        /// <summary>
        /// Invoked when the selected option has changed.
        /// </summary>
        public event GwenEventHandler<ItemSelectedEventArgs> SelectionChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="RadioButtonGroup"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        /// <param name="label">Label for the outlining GroupBox.</param>
        public RadioButtonGroup(ControlBase parent) : base(parent)
        {
            AutoSizeToContents = true;
            IsTabable = false;
            KeyboardInputEnabled = true;
            Text = String.Empty;
        }

        /// <summary>
        /// Adds a new option.
        /// </summary>
        /// <param name="text">Option text.</param>
        /// <returns>Newly created control.</returns>
        public virtual LabeledRadioButton AddOption(string text)
        {
            return AddOption(text, String.Empty);
        }

        /// <summary>
        /// Adds a new option.
        /// </summary>
        /// <param name="text">Option text.</param>
        /// <param name="optionName">Internal name.</param>
        /// <returns>Newly created control.</returns>
        public virtual LabeledRadioButton AddOption(string text, string optionName)
        {
            LabeledRadioButton lrb = new LabeledRadioButton(this);
            lrb.Name = optionName;
            lrb.Text = text;
            lrb.RadioButton.Checked += onRadioClicked;
            lrb.Dock = Pos.Top;
            lrb.Margin = new Margin(0, 0, 0, 1); // 1 bottom
            lrb.KeyboardInputEnabled = false; // todo: true?
            lrb.IsTabable = true;

            Invalidate();
            return lrb;
        }

        /// <summary>
        /// Handler for the option change.
        /// </summary>
        /// <param name="fromPanel">Event source.</param>
        protected virtual void onRadioClicked(ControlBase fromPanel, EventArgs args)
        {
            RadioButton @checked = fromPanel as RadioButton;
            foreach (LabeledRadioButton rb in Children.OfType<LabeledRadioButton>()) // todo: optimize
            {
                if (rb.RadioButton == @checked)
                    selected = rb;
                else
                    rb.RadioButton.IsChecked = false;
            }

            onChanged(selected);
        }
        /*
        /// <summary>
        /// Sizes to contents.
        /// </summary>
        public override void SizeToContents()
        {
            recurseLayout(Skin); // options are docked so positions are not updated until layout runs
            //base.SizeToContents();
            int width = 0;
            int height = 0;
            foreach (Base child in Children)
            {
                width = Math.Max(child.Width, width);
                height += child.Height;
            }
            SetSize(width, height);
            InvalidateParent();
        }
        */
        protected virtual void onChanged(ControlBase NewTarget)
        {
            if (SelectionChanged != null)
                SelectionChanged.Invoke(this, new ItemSelectedEventArgs(NewTarget));
        }

        /// <summary>
        /// Selects the specified option.
        /// </summary>
        /// <param name="index">Option to select.</param>
        public void SetSelection(int index)
        {
            if (index < 0 || index >= Children.Count)
                return;

            (Children[index] as LabeledRadioButton).RadioButton.Press();
        }
    }
}
