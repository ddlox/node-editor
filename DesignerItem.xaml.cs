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
using SG_Administrator.Controls;
using System.Windows.Markup;

namespace SG_Administrator
{
    //These attributes identify the types of the named parts that are used for templating
    [TemplatePart(Name = "PART_DragThumb", Type = typeof(DragThumb))]
    [TemplatePart(Name = "PART_ResizeDecorator", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ConnectorDecorator", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_DI_Hosted_Canvas", Type = typeof(Canvas))]
  //  [ContentProperty("Children")]
    public partial class DesignerItem : ContentControl, ISelectable, IGroupable
    {
       /* public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
           "Children",
           typeof(UIElementCollection),
           typeof(DesignerItem),
           new PropertyMetadata());

        public UIElementCollection Children
        {
            get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
            private set { SetValue(ChildrenProperty, value); }
        }
        */

        private HashSet<Ellipse> routewidgetselection = new HashSet<Ellipse>();
        public route_design routedesign = null;
        public route_form routeform = null;
        private int numroutes = 0;

        private NetworkModel.homely_type ht;
        public NetworkModel.homely_type Type
        {
            get { return ht; }
            set 
            { 
                ht = value; 

                routedesign = null;
                if (ht == NetworkModel.homely_type.e_router)         // If this DesignerItem is a router then we need to be ready to accomodate XAP routes.
                    routedesign = new route_design();   // So we'll need a Route Design object to achieve this.
            }
        }

        private Canvas dic;
        public Canvas DesignerItemCanvas
        {
            get { return dic; }
            set { dic = value; }
        }

        private DesignerCanvas dc;
        public DesignerCanvas DesignerCanvas
        {
            get { return dc; }
            set { dc = value; }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string xaml;
        public string Xaml
        {
            get { return xaml; }
            set { xaml = value; }
        }
        
        private Guid id;
        public Guid ID
        {
            get { return id; }
        }

        public Guid ParentID
        {
            get { return (Guid)GetValue(ParentIDProperty); }
            set { SetValue(ParentIDProperty, value); }
        }
        public static readonly DependencyProperty ParentIDProperty = DependencyProperty.Register("ParentID", typeof(Guid), typeof(DesignerItem));        
        
        public bool IsGroup
        {
            get { return (bool)GetValue(IsGroupProperty); }
            set { SetValue(IsGroupProperty, value); }
        }

        public static readonly DependencyProperty IsGroupProperty =
            DependencyProperty.Register("IsGroup", typeof(bool), typeof(DesignerItem));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        public static readonly DependencyProperty IsSelectedProperty =
          DependencyProperty.Register("IsSelected", typeof(bool), typeof(DesignerItem), new FrameworkPropertyMetadata(false));

        // can be used to replace the default template for the DragThumb
        public static readonly DependencyProperty DragThumbTemplateProperty =
            DependencyProperty.RegisterAttached("DragThumbTemplate", typeof(ControlTemplate), typeof(DesignerItem));

        public static ControlTemplate GetDragThumbTemplate(UIElement element)
        {
            return (ControlTemplate)element.GetValue(DragThumbTemplateProperty);
        }

        public static void SetDragThumbTemplate(UIElement element, ControlTemplate value)
        {
            element.SetValue(DragThumbTemplateProperty, value);
        }

        // can be used to replace the default template for the ConnectorDecorator
        public static readonly DependencyProperty ConnectorDecoratorTemplateProperty =
            DependencyProperty.RegisterAttached("ConnectorDecoratorTemplate", typeof(ControlTemplate), typeof(DesignerItem));

        public static ControlTemplate GetConnectorDecoratorTemplate(UIElement element)
        {
            return (ControlTemplate)element.GetValue(ConnectorDecoratorTemplateProperty);
        }

        public static void SetConnectorDecoratorTemplate(UIElement element, ControlTemplate value)
        {
            element.SetValue(ConnectorDecoratorTemplateProperty, value);
        }

        // while drag connection procedure is ongoing and the mouse moves over 
        // this item this value is true; if true the ConnectorDecorator is triggered
        // to be visible, see template
        public bool IsDragConnectionOver
        {
            get { return (bool)GetValue(IsDragConnectionOverProperty); }
            set { SetValue(IsDragConnectionOverProperty, value); }
        }
        public static readonly DependencyProperty IsDragConnectionOverProperty =
            DependencyProperty.Register("IsDragConnectionOver", typeof(bool), typeof(DesignerItem), new FrameworkPropertyMetadata(false));

        //static DesignerItem()
        /*public DesignerItem()
        {
            // set the key to reference the style for this control
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DesignerItem), new FrameworkPropertyMetadata(typeof(DesignerItem)));
        }*/

        public DesignerItem(Guid id)
        {
            InitializeComponent();

            this.Type = NetworkModel.homely_type.e_dunno;
            this.id = id;
            this.Loaded += DesignerItem_Loaded;
        }

        public DesignerItem() :
            this(Guid.NewGuid())
        {
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            DesignerCanvas designer = VisualTreeHelper.GetParent(this) as DesignerCanvas;

            // update selection
            if (designer != null)
            {
                if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None)
                {
                    if (this.IsSelected)
                    {
                        designer.SelectionService.RemoveFromSelection(this);
                    }
                    else
                    {
                        designer.SelectionService.AddToSelection(this);
                        designer.set_selection_change(this);
                    }
                }
                else if (!this.IsSelected)
                {
                    designer.SelectionService.SelectItem(this);
                    designer.set_selection_change(this);
                }
                Focus();
            }

            e.Handled = false;
        }

        void DesignerItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (base.Template != null)
            {
                ContentPresenter contentPresenter =
                    this.Template.FindName("PART_ContentPresenter", this) as ContentPresenter;
                if (contentPresenter != null)
                {
                    UIElement contentVisual = VisualTreeHelper.GetChild(contentPresenter, 0) as UIElement;
                    if (contentVisual != null)
                    {
                        DragThumb thumb = this.Template.FindName("PART_DragThumb", this) as DragThumb;
                        if (thumb != null)
                        {
                            ControlTemplate template =
                                DesignerItem.GetDragThumbTemplate(contentVisual) as ControlTemplate;
                            if (template != null)
                                thumb.Template = template;
                        }
                    }
                }

                set_designeritem_canvas();
            }
        }

        public void set_designeritem_canvas()
        {
            DesignerItemCanvas = this.Template.FindName("PART_DI_Hosted_Canvas", this) as Canvas;
        }

        public Connector ConnectorLeft
        {
            get
            {
                var connectorDecorator = this.Template.FindName("PART_ConnectorDecorator", this) as Control;
                connectorDecorator.ApplyTemplate();
                return connectorDecorator.Template.FindName("Left", connectorDecorator) as Connector;
            }
        }

        public Connector ConnectorRight
        {
            get
            {
                var connectorDecorator = this.Template.FindName("PART_ConnectorDecorator", this) as Control;
                connectorDecorator.ApplyTemplate();
                return connectorDecorator.Template.FindName("Right", connectorDecorator) as Connector;
            }
        }

        public Connector ConnectorTop
        {
            get
            {
                var connectorDecorator = this.Template.FindName("PART_ConnectorDecorator", this) as Control;
                connectorDecorator.ApplyTemplate();
                return connectorDecorator.Template.FindName("Top", connectorDecorator) as Connector;
            }
        }

        public Connector ConnectorBottom
        {
            get
            {
                var connectorDecorator = this.Template.FindName("PART_ConnectorDecorator", this) as Control;
                connectorDecorator.ApplyTemplate();
                return connectorDecorator.Template.FindName("Bottom", connectorDecorator) as Connector;
            }
        }

        public void clear_routewidget_selection()
        {
            foreach (Ellipse ell in routewidgetselection)
                ell.Fill = Brushes.White; // or null for transparent
            routewidgetselection.Clear();
        }

        public void add_routewidget_selection(Ellipse el)
        {
            routewidgetselection.Add(el);
        }

        public bool contains_routewidget(Ellipse el)
        {
            return routewidgetselection.Contains(el);
        }

        public void create_route_form()
        {
            ++numroutes;
            routeform = new route_form(this, numroutes);
        }

        public void set_route_vpos()
        {
            routedesign.set_route_vpos((int)routeform.get_edit_vpos()); 
        }
    }
}
