using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SG_Administrator
{
    public class route_form : Control
    {
        private Rectangle m_routeform = null;
        private Label m_title = null;
        private DesignerItem m_parent = null;
        private Line m_line = null;
        private int routeheight = 20;

        public route_form(DesignerItem parent, int numroutes)
        {            
            m_parent = parent;

            m_routeform = new Rectangle();
            m_routeform.Width = 52;
            m_routeform.Height = 20 + 1 + numroutes * routeheight; // title height + line height + routes * 20 (each route is cca 20 in height)
            m_routeform.Fill = Brushes.DarkBlue;
            m_routeform.Stroke = Brushes.Black;
            m_routeform.StrokeThickness = 1;
            m_routeform.RadiusX = 10;
            m_routeform.RadiusY = 10;
            m_routeform.Opacity = 0.25;
            DesignerCanvas.SetLeft(m_routeform, 4);
            DesignerCanvas.SetTop(m_routeform, 4);
            DesignerCanvas.SetZIndex(m_routeform, m_parent.DesignerItemCanvas.Children.Count);
            m_parent.DesignerItemCanvas.Children.Add(m_routeform);

            m_title = new Label();
            m_title.Content = "Routes";
            m_title.Foreground = Brushes.White;
            DesignerCanvas.SetLeft(m_title, 5);
            DesignerCanvas.SetTop(m_title, 0);
            DesignerCanvas.SetZIndex(m_title, m_parent.DesignerItemCanvas.Children.Count);
            m_parent.DesignerItemCanvas.Children.Add(m_title);

            m_line = new Line();
            m_line.Stroke = Brushes.White;
            m_line.X1 = 10;
            m_line.Y1 = 25;
            m_line.X2 = m_routeform.Width;
            m_line.Y2 = 25;
            m_line.Opacity = 1;
            m_line.StrokeDashArray.Add(2);
            m_line.StrokeDashArray.Add(4);
            m_line.StrokeThickness = 1;
            DesignerCanvas.SetZIndex(m_line, m_parent.DesignerItemCanvas.Children.Count);
            m_parent.DesignerItemCanvas.Children.Add(m_line);
        }

        public double get_edit_vpos()
        {
            return m_routeform.Height - routeheight - 5;
        }

        public void remove_from_canvas()
        {
            m_parent.DesignerItemCanvas.Children.Remove(m_routeform);
            m_parent.DesignerItemCanvas.Children.Remove(m_title);
            m_parent.DesignerItemCanvas.Children.Remove(m_line);
        }

        public void increase_rect_height()
        {
            m_routeform.Height += routeheight;
        }
    }
}
