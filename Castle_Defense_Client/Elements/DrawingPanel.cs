using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Castle_Defense_Client.Elements
{
    public partial class DrawingPanel : FrameworkElement
    {
        private VisualCollection visuals;

        public DrawingPanel()
        {
            visuals = new VisualCollection(this);
        }

        public void AddVisual(DrawingVisual visual)
        {
            visuals.Add(visual);
        }

        public void Clear()
        {
            visuals.Clear();
        }

        protected override int VisualChildrenCount => visuals.Count;

        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }
    }
}
