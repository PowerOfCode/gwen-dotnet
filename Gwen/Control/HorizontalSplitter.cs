using System;
using System.Windows.Forms;
using Gwen.ControlInternal;
using Newtonsoft.Json;

namespace Gwen.Control
{
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class HorizontalSplitter : ControlBase
    {
        private readonly SplitterBar vSplitter;
        private readonly ControlBase[] sections;

        private float vVal; // 0-1
        private int barSize; // pixels
        private int zoomedSection; // 0-1

        /// <summary>
        /// Invoked when one of the panels has been zoomed (maximized).
        /// </summary>
        public event GwenEventHandler<EventArgs> PanelZoomed;

        /// <summary>
        /// Invoked when one of the panels has been unzoomed (restored).
        /// </summary>
		public event GwenEventHandler<EventArgs> PanelUnZoomed;

        /// <summary>
        /// Invoked when the zoomed panel has been changed.
        /// </summary>
		public event GwenEventHandler<EventArgs> ZoomChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossSplitter"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public HorizontalSplitter(ControlBase parent)
            : base(parent)
        {
            sections = new ControlBase[2];

            vSplitter = new SplitterBar(this);
            vSplitter.SetPosition(0, 128);
            vSplitter.Dragged += onVerticalMoved;
            vSplitter.Cursor = Cursors.SizeNS;

            vVal = 0.5f;

            SetPanel(0, null);
            SetPanel(1, null);

            SplitterSize = 5;
            SplittersVisible = false;

            zoomedSection = -1;
        }

        /// <summary>
        /// Centers the panels so that they take even amount of space.
        /// </summary>
        public void CenterPanels()
        {
            vVal = 0.5f;
            Invalidate();
        }

        /// <summary>
        /// Indicates whether any of the panels is zoomed.
        /// </summary>
        public bool IsZoomed { get { return zoomedSection != -1; } }

        /// <summary>
        /// Gets or sets a value indicating whether splitters should be visible.
        /// </summary>
        public bool SplittersVisible
        {
            get { return vSplitter.ShouldDrawBackground; }
            set
            {
                vSplitter.ShouldDrawBackground = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the splitter.
        /// </summary>
        public int SplitterSize { get { return barSize; } set { barSize = value; } }

        private void updateVSplitter()
        {
            vSplitter.MoveTo(vSplitter.X, (Height - vSplitter.Height) * (vVal));
        }

        protected void onVerticalMoved(ControlBase control, EventArgs args)
        {
            vVal = calculateValueVertical();
            Invalidate();
        }

        private float calculateValueVertical()
        {
            return vSplitter.Y / (float)(Height - vSplitter.Height);
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            vSplitter.SetSize(Width, barSize);

            updateVSplitter();

            if (zoomedSection == -1)
            {
                if (sections[0] != null)
                    sections[0].SetBounds(0, 0, Width, vSplitter.Y);

                if (sections[1] != null)
                    sections[1].SetBounds(0, vSplitter.Y + barSize, Width, Height - (vSplitter.Y + barSize));
            }
            else
            {
                //This should probably use Fill docking instead
                sections[zoomedSection].SetBounds(0, 0, Width, Height);
            }
        }

        /// <summary>
        /// Assigns a control to the specific inner section.
        /// </summary>
        /// <param name="index">Section index (0-3).</param>
        /// <param name="panel">Control to assign.</param>
        public void SetPanel(int index, ControlBase panel)
        {
            sections[index] = panel;

            if (panel != null)
            {
                panel.Dock = Pos.None;
                panel.Parent = this;
            }

            Invalidate();
        }

        /// <summary>
        /// Gets the specific inner section.
        /// </summary>
        /// <param name="index">Section index (0-3).</param>
        /// <returns>Specified section.</returns>
        public ControlBase GetPanel(int index)
        {
            return sections[index];
        }

        /// <summary>
        /// Internal handler for the zoom changed event.
        /// </summary>
        protected void onZoomChanged()
        {
            if (ZoomChanged != null)
				ZoomChanged.Invoke(this, EventArgs.Empty);

            if (zoomedSection == -1)
            {
                if (PanelUnZoomed != null)
					PanelUnZoomed.Invoke(this, EventArgs.Empty);
            }
            else
            {
                if (PanelZoomed != null)
					PanelZoomed.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Maximizes the specified panel so it fills the entire control.
        /// </summary>
        /// <param name="section">Panel index (0-3).</param>
        public void Zoom(int section)
        {
            UnZoom();

            if (sections[section] != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (i != section && sections[i] != null)
                        sections[i].IsHidden = true;
                }
                zoomedSection = section;

                Invalidate();
            }
            onZoomChanged();
        }

        /// <summary>
        /// Restores the control so all panels are visible.
        /// </summary>
        public void UnZoom()
        {
            zoomedSection = -1;

            for (int i = 0; i < 2; i++)
            {
                if (sections[i] != null)
                    sections[i].IsHidden = false;
            }

            Invalidate();
            onZoomChanged();
        }
    }
}
