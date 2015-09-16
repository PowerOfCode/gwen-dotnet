using System;
using System.Windows.Forms;
using Gwen.ControlInternal;
using Newtonsoft.Json;

namespace Gwen.Control
{
    /// <summary>
    /// Splitter control.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    [JsonConverter(typeof(Serialization.GwenConverter))]
    public class CrossSplitter : ControlBase
    {
        private readonly SplitterBar vSplitter;
        private readonly SplitterBar hSplitter;
        private readonly SplitterBar cSplitter;

        private readonly ControlBase[] sections;

        private float hVal; // 0-1
        private float vVal; // 0-1
        private int barSize; // pixels

        private int zoomedSection; // 0-3

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
        public CrossSplitter(ControlBase parent)
            : base(parent)
        {
            sections = new ControlBase[4];

            vSplitter = new SplitterBar(this);
            vSplitter.SetPosition(0, 128);
            vSplitter.Dragged += onVerticalMoved;
            vSplitter.Cursor = Cursors.SizeNS;

            hSplitter = new SplitterBar(this);
            hSplitter.SetPosition(128, 0);
            hSplitter.Dragged += onHorizontalMoved;
            hSplitter.Cursor = Cursors.SizeWE;

            cSplitter = new SplitterBar(this);
            cSplitter.SetPosition(128, 128);
            cSplitter.Dragged += onCenterMoved;
            cSplitter.Cursor = Cursors.SizeAll;

            hVal = 0.5f;
            vVal = 0.5f;

            SetPanel(0, null);
            SetPanel(1, null);
            SetPanel(2, null);
            SetPanel(3, null);

            SplitterSize = 5;
            SplittersVisible = false;

            zoomedSection = -1;
        }

        /// <summary>
        /// Centers the panels so that they take even amount of space.
        /// </summary>
        public void CenterPanels()
        {
            hVal = 0.5f;
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
            get { return cSplitter.ShouldDrawBackground; }
            set
            {
                cSplitter.ShouldDrawBackground = value;
                vSplitter.ShouldDrawBackground = value;
                hSplitter.ShouldDrawBackground = value;
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

        private void updateHSplitter()
        {
            hSplitter.MoveTo( ( Width - hSplitter.Width ) * ( hVal ), hSplitter.Y );
        }

        private void updateCSplitter()
        {
            cSplitter.MoveTo((Width - cSplitter.Width) * (hVal), (Height - cSplitter.Height) * (vVal));
        }

        protected void onCenterMoved(ControlBase control, EventArgs args)
        {
            calculateValueCenter();
            Invalidate();
        }

        protected void onVerticalMoved(ControlBase control, EventArgs args)
        {
            vVal = calculateValueVertical();
            Invalidate();
        }

        protected void onHorizontalMoved(ControlBase control, EventArgs args)
        {
            hVal = calculateValueHorizontal();
            Invalidate();
        }

        private void calculateValueCenter()
        {
            hVal = cSplitter.X / (float)(Width - cSplitter.Width);
            vVal = cSplitter.Y / (float)(Height - cSplitter.Height);
        }

        private float calculateValueVertical()
        {
            return vSplitter.Y / (float)(Height - vSplitter.Height);
        }

        private float calculateValueHorizontal()
        {
            return hSplitter.X / (float)(Width - hSplitter.Width);
        }

        /// <summary>
        /// Lays out the control's interior according to alignment, padding, dock etc.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void layout(Skin.SkinBase skin)
        {
            vSplitter.SetSize(Width, barSize);
            hSplitter.SetSize(barSize, Height);
            cSplitter.SetSize(barSize, barSize);

            updateVSplitter();
            updateHSplitter();
            updateCSplitter();

            if (zoomedSection == -1)
            {
                if (sections[0] != null)
                    sections[0].SetBounds(0, 0, hSplitter.X, vSplitter.Y);

                if (sections[1] != null)
                    sections[1].SetBounds(hSplitter.X + barSize, 0, Width - (hSplitter.X + barSize), vSplitter.Y);

                if (sections[2] != null)
                    sections[2].SetBounds(0, vSplitter.Y + barSize, hSplitter.X, Height - (vSplitter.Y + barSize));

                if (sections[3] != null)
                    sections[3].SetBounds(hSplitter.X + barSize, vSplitter.Y + barSize, Width - (hSplitter.X + barSize), Height - (vSplitter.Y + barSize));
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
                for (int i = 0; i < 4; i++)
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

            for (int i = 0; i < 4; i++)
            {
                if (sections[i] != null)
                    sections[i].IsHidden = false;
            }

            Invalidate();
            onZoomChanged();
        }
    }
}
