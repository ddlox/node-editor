using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;

namespace SG_Administrator
{
    public class DesignerItemChangedEventArgs : EventArgs
    {
        private DesignerItem di;

        public DesignerItem designeritem
        {
            get { return di; }
        }

        public DesignerItemChangedEventArgs(DesignerItem di)
        {
            this.di = di;
        }
    }

    public partial class DesignerCanvas : Canvas
    {
        public EventHandler selection_change = delegate { };

        private Point? rubberbandSelectionStartPoint = null;
        
        private SelectionService selectionService;
        internal SelectionService SelectionService
        {
            get
            {
                if (selectionService == null)
                    selectionService = new SelectionService(this);

                return selectionService;
            }
        }


        public void set_selection_change(DesignerItem di)
        {            
            selection_change(this, new DesignerItemChangedEventArgs(di));
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Source == this)
            {
                // in case that this click is the start of a 
                // drag operation we cache the start point
                this.rubberbandSelectionStartPoint = new Point?(e.GetPosition(this));

                // if you click directly on the canvas all 
                // selected items are 'de-selected'
                SelectionService.ClearSelection();
                Focus();
                e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // if mouse button is not pressed we have no drag operation, ...
            if (e.LeftButton != MouseButtonState.Pressed)
                this.rubberbandSelectionStartPoint = null;

            // ... but if mouse button is pressed and start
            // point value is set we do have one
            if (this.rubberbandSelectionStartPoint.HasValue)
            {
                // create rubberband adorner
                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    RubberbandAdorner adorner = new RubberbandAdorner(this, rubberbandSelectionStartPoint);
                    if (adorner != null)
                    {
                        adornerLayer.Add(adorner);
                    }
                }
            }
            e.Handled = true;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            DragObject dragObject = e.Data.GetData(typeof(DragObject)) as DragObject;
            if (dragObject != null && !String.IsNullOrEmpty(dragObject.Xaml))
            {                
                Object content = XamlReader.Load(XmlReader.Create(new StringReader(dragObject.Xaml)));
                if (content != null)
                {                    
                    DesignerItem newitem = new DesignerItem();
                    if (dragObject.Xaml.Contains("ToolTip=\"Cyclone Input Source\""))
                        newitem.Type = NetworkModel.homely_type.e_cyclone_input_source;
                    else if (dragObject.Xaml.Contains("ToolTip=\"Network Newfor Source\""))
                        newitem.Type = NetworkModel.homely_type.e_net_nw4_source;
                    else if (dragObject.Xaml.Contains("ToolTip=\"Switcher\""))
                        newitem.Type = NetworkModel.homely_type.e_switcher;
                    else if (dragObject.Xaml.Contains("ToolTip=\"ABSwitcher\""))
                        newitem.Type = NetworkModel.homely_type.e_abswitcher;
                    else if (dragObject.Xaml.Contains("ToolTip=\"Aggregator\""))
                        newitem.Type = NetworkModel.homely_type.e_aggregator;
                    else if (dragObject.Xaml.Contains("ToolTip=\"Splitter\""))
                        newitem.Type = NetworkModel.homely_type.e_splitter;
                    else if (dragObject.Xaml.Contains("ToolTip=\"Router\""))
                        newitem.Type = NetworkModel.homely_type.e_router;
                    else if (dragObject.Xaml.Contains("ToolTip=\"XAP Destination\""))
                        newitem.Type = NetworkModel.homely_type.e_xap_destination;
                    else if (dragObject.Xaml.Contains("ToolTip=\"XAP Source\""))
                        newitem.Type = NetworkModel.homely_type.e_xap_source;
                    else if (dragObject.Xaml.Contains("ToolTip=\"Self-Test Source\""))
                        newitem.Type = NetworkModel.homely_type.e_selftest_source;
                    else if (dragObject.Xaml.Contains("ToolTip=\"Cyclone Destination\""))
                        newitem.Type = NetworkModel.homely_type.e_cyclone_destination;
                    else if (dragObject.Xaml.Contains("ToolTip=\"Smart III Destination\""))
                        newitem.Type = NetworkModel.homely_type.e_smart3_destination;
                    else if (dragObject.Xaml.Contains("ToolTip=\"Smart III Source\""))
                        newitem.Type = NetworkModel.homely_type.e_smart3_source;

                    newitem.Content = content;
                    newitem.Xaml = dragObject.Xaml;
                    newitem.Name = "  ";
                    newitem.DesignerCanvas = this;

                    Point position = e.GetPosition(this);

                    if (dragObject.DesiredSize.HasValue)
                    {
                        Size desiredSize = dragObject.DesiredSize.Value;
                        newitem.Width = desiredSize.Width;
                        newitem.Height = desiredSize.Height;

                        DesignerCanvas.SetLeft(newitem, Math.Max(0, position.X - newitem.Width / 2));
                        DesignerCanvas.SetTop(newitem, Math.Max(0, position.Y - newitem.Height / 2));
                    }
                    else
                    {
                        DesignerCanvas.SetLeft(newitem, Math.Max(0, position.X));
                        DesignerCanvas.SetTop(newitem, Math.Max(0, position.Y));
                    }

                    Canvas.SetZIndex(newitem, this.Children.Count);
                    this.Children.Add(newitem);
                    SetConnectorDecoratorTemplate(newitem);

                    //update selection
                    this.SelectionService.SelectItem(newitem);
                    newitem.Focus();
                }

                e.Handled = true;
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size size = new Size();

            foreach (UIElement element in this.InternalChildren)
            {
                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);
                left = double.IsNaN(left) ? 0 : left;
                top = double.IsNaN(top) ? 0 : top;

                //measure desired size for each child
                element.Measure(constraint);

                Size desiredSize = element.DesiredSize;
                if (!double.IsNaN(desiredSize.Width) && !double.IsNaN(desiredSize.Height))
                {
                    size.Width = Math.Max(size.Width, left + desiredSize.Width);
                    size.Height = Math.Max(size.Height, top + desiredSize.Height);
                }
            }
            // add margin 
            size.Width += 10;
            size.Height += 10;
            return size;
        }

        public void SetConnectorDecoratorTemplate(DesignerItem item)
        {
            if (item.ApplyTemplate() && item.Content is UIElement)
            {
                ControlTemplate template = DesignerItem.GetConnectorDecoratorTemplate(item.Content as UIElement);
                Control decorator = item.Template.FindName("PART_ConnectorDecorator", item) as Control;
                if (decorator != null && template != null)
                    decorator.Template = template;
            }
        }
    }
}
