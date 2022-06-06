using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;
using NetworkUI;
using NetworkModel;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.ComponentModel;
using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceProcess;
using System.Windows.Media.Imaging;

namespace SG_Administrator
{
    public partial class MainWindow : Window
    {
        private NodeViewModel m_current_sel_node = null;
        private ConnectorViewModel m_current_cvm = null;
        private Ellipse m_current_cvm_ellipse = null;
        private bool m_dirty = false;
        private string m_current_sga_filename = "";
        private string m_active_cfg_to_load = "";
        private ServiceController m_service = null;        

        public MainWindow()
        {
            InitializeComponent();

            MyDetails.PropertyChanged += detailsedit_property_handler;
            
            new_cfg();

            m_service = new ServiceController("SubtitleGateway");
        }

        /// <summary>
        /// Convenient accessor for the view-model.
        /// </summary>
        public MainWindowViewModel ViewModel
        {
            get
            {
                return (MainWindowViewModel)DataContext;
            }
        }

        private void networkControl_ConnectionDragStarted(object sender, ConnectionDragStartedEventArgs e)
        {
            var draggedOutConnector = (ConnectorViewModel)e.ConnectorDraggedOut;
            var curDragPoint = Mouse.GetPosition(networkControl);

            //
            // Delegate the real work to the view model.
            //
            var connection = this.ViewModel.ConnectionDragStarted(draggedOutConnector, curDragPoint);

            //
            // Must return the view-model object that represents the connection via the event args.
            // This is so that NetworkView can keep track of the object while it is being dragged.
            //
            e.Connection = connection;
        }

        /// <summary>
        /// Event raised, to query for feedback, while the user is dragging a connection.
        /// </summary>
        private void networkControl_QueryConnectionFeedback(object sender, QueryConnectionFeedbackEventArgs e)
        {
            var draggedOutConnector = (ConnectorViewModel)e.ConnectorDraggedOut;
            var draggedOverConnector = (ConnectorViewModel)e.DraggedOverConnector;
            object feedbackIndicator = null;
            bool connectionOk = true;

            this.ViewModel.QueryConnnectionFeedback(draggedOutConnector, draggedOverConnector, out feedbackIndicator, out connectionOk);

            //
            // Return the feedback object to NetworkView.
            // The object combined with the data-template for it will be used to create a 'feedback icon' to
            // display (in an adorner) to the user.
            //
            e.FeedbackIndicator = feedbackIndicator;

            //
            // Let NetworkView know if the connection is ok or not ok.
            //
            e.ConnectionOk = connectionOk;
        }

        /// <summary>
        /// Event raised while the user is dragging a connection.
        /// </summary>
        private void networkControl_ConnectionDragging(object sender, ConnectionDraggingEventArgs e)
        {
            Point curDragPoint = Mouse.GetPosition(networkControl);
            var connection = (ConnectionViewModel)e.Connection;
            this.ViewModel.ConnectionDragging(curDragPoint, connection);
        }

        /// <summary>
        /// Event raised when the user has finished dragging out a connection.
        /// </summary>
        private void networkControl_ConnectionDragCompleted(object sender, ConnectionDragCompletedEventArgs e)
        {
            var connectorDraggedOut = (ConnectorViewModel)e.ConnectorDraggedOut;
            var connectorDraggedOver = (ConnectorViewModel)e.ConnectorDraggedOver;
            var newConnection = (ConnectionViewModel)e.Connection;
            if (this.ViewModel.ConnectionDragCompleted(newConnection, connectorDraggedOut, connectorDraggedOver))
                set_dirty_flag(true);
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
                    //DesignerItem newitem = new DesignerItem();
                    homely_type type = homely_type.e_dunno;
                    int numinputs = 0;
                    int numoutputs = 0;
                    if (dragObject.Xaml.Contains("ToolTip=\"Cyclone Input Source\""))
                    {
                        type = homely_type.e_cyclone_input_source;
                        numinputs = 0;
                        numoutputs = 1;
                    }
                    else if (dragObject.Xaml.Contains("ToolTip=\"Network Newfor Source\""))
                    {
                        type = homely_type.e_net_nw4_source;
                        numinputs = 0;
                        numoutputs = 1;
                    }
                    else if (dragObject.Xaml.Contains("ToolTip=\"Switcher\""))
                    {
                        type = homely_type.e_switcher;
                        numinputs = 2;
                        numoutputs = 1;
                    }
                    else if (dragObject.Xaml.Contains("ToolTip=\"ABSwitcher\""))
                    {
                        type = homely_type.e_abswitcher;
                        numinputs = 2;
                        numoutputs = 1;
                    }
                    else if (dragObject.Xaml.Contains("ToolTip=\"Aggregator\""))
                    {
                        type = homely_type.e_aggregator;
                        numinputs = 4;
                        numoutputs = 1;
                    }
                    else if (dragObject.Xaml.Contains("ToolTip=\"Splitter\""))
                    {
                        type = homely_type.e_splitter;
                        numinputs = 1;
                        numoutputs = 2;
                    }
                    else if (dragObject.Xaml.Contains("ToolTip=\"Router\""))
                    {
                        type = homely_type.e_router;
                        numinputs = 1;
                        numoutputs = 1;
                    }
                    else if (dragObject.Xaml.Contains("ToolTip=\"XAP Destination\""))
                    {
                        type = homely_type.e_xap_destination;
                        numinputs = 1;
                        numoutputs = 0;
                    }
                    else if (dragObject.Xaml.Contains("ToolTip=\"XAP Source\""))
                    {
                        type = homely_type.e_xap_source;
                        numinputs = 0;
                        numoutputs = 1;
                    }
                    else if (dragObject.Xaml.Contains("ToolTip=\"Self-Test Source\""))
                    {
                        type = homely_type.e_selftest_source;
                        numinputs = 0;
                        numoutputs = 1;
                    }
                    else if (dragObject.Xaml.Contains("ToolTip=\"Cyclone Destination\""))
                    {
                        type = homely_type.e_cyclone_destination;
                        numinputs = 1;
                        numoutputs = 0;
                    }
                    else if (dragObject.Xaml.Contains("ToolTip=\"Smart III Destination\""))
                    {
                        type = homely_type.e_smart3_destination;
                        numinputs = 1;
                        numoutputs = 0;
                    }
                    else if (dragObject.Xaml.Contains("ToolTip=\"Smart III Source\""))
                    {
                        type = homely_type.e_smart3_source;
                        numinputs = 0;
                        numoutputs = 1;
                    }

                    Point p = e.GetPosition(this);
                    CreateNode(type, p, numinputs, numoutputs);
                    set_dirty_flag(true);
                }

                e.Handled = true;
            }
        }

        /// <summary>
        /// Event raised when the Window has loaded.
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            /*
            // Display help text for the app.
            HelpTextWindow helpTextWindow = new HelpTextWindow();
            helpTextWindow.Left = this.Left + this.Width + 5;
            helpTextWindow.Top = this.Top;
            helpTextWindow.Owner = this;
            helpTextWindow.Show();

            OverviewWindow overviewWindow = new OverviewWindow();
            overviewWindow.Left = this.Left;
            overviewWindow.Top = this.Top + this.Height + 5;
            overviewWindow.Owner = this;
            overviewWindow.DataContext = this.ViewModel; // Pass the view model onto the overview window.
            overviewWindow.Show();
            */

            check_service_status();
        }

        /// <summary>
        /// Event raised to delete the selected node.
        /// </summary>
        private void DeleteSelectedNodes_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.ViewModel.DeleteSelectedNodes();
            set_dirty_flag(true);
        }

        /// <summary>
        /// Event raised to create a new node.
        /// </summary>
        private void CreateNode_Executed(object sender, ExecutedRoutedEventArgs e)
        {
        }

        /// <summary>
        /// Event raised to delete a node.
        /// </summary>
        private void DeleteNode_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NodeViewModel node = (NodeViewModel)e.Parameter;
            node.PropertyChanged -= node_property_handler;
            if (m_current_sel_node == node)
            {
                m_current_sel_node = null;
                MyDetails.set_current_item(m_current_sel_node);
            }
            this.ViewModel.DeleteNode(node);
            set_dirty_flag(true);
        }

        /// <summary>
        /// Event raised to delete a connection.
        /// </summary>
        private void DeleteConnection_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var connection = (ConnectionViewModel)e.Parameter;
            this.ViewModel.DeleteConnection(connection);
            set_dirty_flag(true);
        }

        /// <summary>
        /// Creates a new node in the network at the current mouse location.
        /// </summary>
        private void CreateNode(homely_type type, Point p, int numinputs, int numoutputs)
        {
            //var newNodePosition = Mouse.GetPosition(networkControl);
            var current_sel_node = this.ViewModel.CreateNode("New Node", type, p, numinputs, numoutputs, true); //newNodePosition, true);
            current_sel_node.PropertyChanged += node_property_handler;
        }

        /// <summary>
        /// Event raised when the size of a node has changed.
        /// </summary>
        private void Node_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //
            // The size of a node, as determined in the UI by the node's data-template,
            // has changed.  Push the size of the node through to the view-model.
            //
            var element = (FrameworkElement)sender;
            var node = (NodeViewModel)element.DataContext;
            node.Size = new Size(element.ActualWidth, element.ActualHeight);
        }

        private void AddInputConnector_Executed(object sender, ExecutedRoutedEventArgs e)
        {            
            if (this.ViewModel.has_selected_node())            
            {
                NodeViewModel node = this.ViewModel.get_first_selected_node();

                if (node != null)
                {
                    if (node.HomelyType == homely_type.e_selftest_source)
                    {
                        MessageBox.Show("Input connector not added:\nA Self-Test Source does not require any input connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_cyclone_input_source)
                    {
                        MessageBox.Show("Input connector not added:\nA Cyclone Input Source does not require any input connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_net_nw4_source)
                    {
                        MessageBox.Show("Input connector not added:\nA Network Newfor Source does not require any input connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_xap_destination)
                    {
                        MessageBox.Show("Input connector not added:\nA XAP Destination does not require more than 1 input connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_cyclone_destination)
                    {
                        MessageBox.Show("Input connector not added:\nA Cyclone Destination does not require more than 1 input connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_smart3_destination)
                    {
                        MessageBox.Show("Input connector not added:\nA Smart III Destination does not require more than 1 input connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_splitter)
                    {
                        MessageBox.Show("Input connector not added:\nA XAP Splitter does not require more than 1 input connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_xap_source)
                    {
                        MessageBox.Show("Input connector not added:\nA XAP Source does not require any input connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_abswitcher)
                    {
                        MessageBox.Show("Input connector not added:\nA XAP AB Switcher requires exactly 2 input connectors.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_smart3_source)
                    {
                        MessageBox.Show("Input connector not added:\nA Smart III Source does not require any input connector.", "Connector");
                        return;
                    }


                    node.InputConnectors.Add(new ConnectorViewModel(get_new_connectorname(node, "In")));
                    if (node.HomelyType == homely_type.e_router)
                        node.OutputConnectors.Add(new ConnectorViewModel(get_new_connectorname(node, "Out")));
                    set_dirty_flag(true);
                }
            }
            else
            {
                m_current_sel_node = null;
                MessageBox.Show("No node selected.", "Connector");
            }
        }

        private string get_new_connectorname(NodeViewModel node, string ss)
        {
            Utils.ImpObservableCollection<ConnectorViewModel> coll = null;

            if (ss == "In")
                coll = node.InputConnectors;
            else if (ss == "Out")
                coll = node.OutputConnectors;
            else
                return "";
            
            int in_num = coll.Count + 1;
            string newname = ss + in_num.ToString();
            bool namefound = false;
            do
            {
                namefound = false;
                foreach (ConnectorViewModel cvm in coll)
                {
                    if (cvm.Name == newname)
                    {
                        namefound = true;
                        ++in_num;
                        newname = ss + in_num.ToString();
                        break;
                    }
                }
            }
            while (namefound);

            return newname;
        }

        private void RemoveInputConnector_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.ViewModel.has_selected_node())            
            {
                NodeViewModel node = this.ViewModel.get_first_selected_node();

                if (node != null)
                {
                    if (node.HomelyType == homely_type.e_selftest_source)
                    {
                        MessageBox.Show("Input connector not removed:\nA Self-Test Source does not carry any input connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_cyclone_input_source)
                    {
                        MessageBox.Show("Input connector not removed:\nA Cyclone Input Source does not carry any input connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_net_nw4_source)
                    {
                        MessageBox.Show("Input connector not removed:\nA Network Newfor Source does not carry any input connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_xap_destination)
                    {
                        MessageBox.Show("Input connector not removed:\nA XAP Destination requires 1 input connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_cyclone_destination)
                    {
                        MessageBox.Show("Input connector not removed:\nA Cyclone Destination requires 1 input connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_smart3_destination)
                    {
                        MessageBox.Show("Input connector not removed:\nA Smart III Destination requires 1 input connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_switcher)
                    {
                        if (node.InputConnectors.Count == 2)
                        {
                            MessageBox.Show("Input connector not removed:\nA XAP Switcher requires 2 or more input connectors.", "Connector");
                            return;
                        }
                    }

                    if (node.HomelyType == homely_type.e_abswitcher)
                    {
                        if (node.InputConnectors.Count == 2)
                        {
                            MessageBox.Show("Input connector not removed:\nA XAP AB Switcher requires exactly 2 input connectors.", "Connector");
                            return;
                        }
                    }

                    if (node.HomelyType == homely_type.e_aggregator)
                    {
                        if (node.InputConnectors.Count == 4)
                        {
                            MessageBox.Show("Input connector not removed:\nA XAP Aggregator requires 4 or more input connectors.", "Connector");
                            return;
                        }
                    }

                    if (node.HomelyType == homely_type.e_splitter)
                    {
                        if (node.InputConnectors.Count == 1)
                        {
                            MessageBox.Show("Input connector not removed:\nA XAP Splitter requires 1 input connector.", "Connector");
                            return;
                        }
                    }

                    if (node.HomelyType == homely_type.e_router)
                    {
                        if (node.InputConnectors.Count == 1)
                        {
                            MessageBox.Show("Input connector not removed:\nA XAP Router requires 1 or more Input-Output routes.", "Connector");
                            return;
                        }
                    }

                    if (node.HomelyType == homely_type.e_xap_source)
                    {
                        MessageBox.Show("Input connector not removed:\nA XAP Source does not carry any input connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_smart3_source)
                    {
                        MessageBox.Show("Input connector not removed:\nA Smart III Source does not carry any input connector.", "Connector");
                        return;
                    }

                    foreach (ConnectorViewModel cvm in node.InputConnectors)
                    {
                        if (cvm == m_current_cvm)
                        {
                            clear_current_cvm();
                            int idx = node.InputConnectors.IndexOf(cvm); 
                            Utils.ImpObservableCollection<ConnectionViewModel> all_conns = cvm.AttachedConnections;
                            do
                            {
                                foreach (var ao in all_conns)
                                {
                                    this.ViewModel.Network.Connections.Remove(ao);
                                    break; // deletion has to happen 1 by 1, to give chance to network to update itself, hence the loop.
                                }
                                all_conns = cvm.AttachedConnections;
                            } while (all_conns.Count > 0);
                            node.InputConnectors.Remove(cvm);
                            if (node.HomelyType == homely_type.e_router)
                            {
                                ConnectorViewModel deleteme = node.OutputConnectors[idx];
                                all_conns = deleteme.AttachedConnections;
                                do
                                {
                                    foreach (var ao in all_conns)
                                    {
                                        this.ViewModel.Network.Connections.Remove(ao);
                                        break; // deletion has to happen 1 by 1, to give chance to network to update itself, hence the loop.
                                    }
                                    all_conns = deleteme.AttachedConnections;
                                } while (all_conns.Count > 0);
                                node.OutputConnectors.Remove(deleteme); //.RemoveAt(idx);
                            }

                            set_dirty_flag(true);
                            return;
                        }
                    }

                    MessageBox.Show("Input connector not removed:\nNone selected.", "Connector");
                }
            }
            else
            {
                m_current_sel_node = null;
                MessageBox.Show("No node selected.", "Connector");
            }
        }

        private void AddOutputConnector_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.ViewModel.has_selected_node())
            {
                NodeViewModel node = this.ViewModel.get_first_selected_node();

                if (node != null)
                {
                    if (node.HomelyType == homely_type.e_selftest_source)
                    {
                        MessageBox.Show("Output connector not added:\nA Self-Test Source does not require more than 1 output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_cyclone_input_source)
                    {
                        MessageBox.Show("Output connector not added:\nA Cyclone Input Source does not require more than 1 output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_net_nw4_source)
                    {
                        MessageBox.Show("Output connector not added:\nA Network Newfor Source does not require more than 1 output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_xap_destination)
                    {
                        MessageBox.Show("Output connector not added:\nA XAP Destination does not require any output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_cyclone_destination)
                    {
                        MessageBox.Show("Output connector not added:\nA Cyclone Destination does not require any output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_smart3_destination)
                    {
                        MessageBox.Show("Output connector not added:\nA Smart III Destination does not require any output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_switcher)
                    {
                        MessageBox.Show("Output connector not added:\nA XAP Switcher does not require more than 1 output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_abswitcher)
                    {
                        MessageBox.Show("Output connector not added:\nA XAP ABSwitcher requires exactly 1 output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_aggregator)
                    {
                        MessageBox.Show("Output connector not added:\nA XAP Aggregator does not require more than 1 output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_xap_source)
                    {
                        MessageBox.Show("Output connector not added:\nA XAP Source does not require more than 1 output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_smart3_source)
                    {
                        MessageBox.Show("Output connector not added:\nA Smart III Source does not require more than 1 output connector.", "Connector");
                        return;
                    }

                    node.OutputConnectors.Add(new ConnectorViewModel(get_new_connectorname(node, "Out")));
                    if (node.HomelyType == homely_type.e_router)
                        node.InputConnectors.Add(new ConnectorViewModel(get_new_connectorname(node, "In")));

                    set_dirty_flag(true);
                }
            }
            else
            {
                m_current_sel_node = null;
                MessageBox.Show("No node selected.", "Connector");
            }
        }

        private void RemoveOutputConnector_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.ViewModel.has_selected_node())
            {
                NodeViewModel node = this.ViewModel.get_first_selected_node();

                if (node != null)
                {
                    if (node.HomelyType == homely_type.e_selftest_source)
                    {
                        MessageBox.Show("Output connector not removed:\nA Self-Test Source requires 1 output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_cyclone_input_source)
                    {
                        MessageBox.Show("Output connector not removed:\nA Cyclone Input Source requires 1 output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_net_nw4_source)
                    {
                        MessageBox.Show("Output connector not removed:\nA Network Newfor Source requires 1 output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_xap_destination)
                    {
                        MessageBox.Show("Output connector not removed:\nA XAP Destination does not carry any output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_cyclone_destination)
                    {
                        MessageBox.Show("Output connector not removed:\nA Cyclone Destination does not carry any output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_smart3_destination)
                    {
                        MessageBox.Show("Output connector not removed:\nA Smart III Destination does not carry any output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_switcher)
                    {
                        MessageBox.Show("Output connector not removed:\nA XAP Switcher requires 1 output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_abswitcher)
                    {
                        MessageBox.Show("Output connector not removed:\nA XAP ABSwitcher requires exactly 1 output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_aggregator)
                    {
                        MessageBox.Show("Output connector not removed:\nA XAP Aggregator requires 1 output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_splitter)
                    {
                        if (node.OutputConnectors.Count == 2)
                        {
                            MessageBox.Show("Output connector not removed:\nA XAP Splitter requires 2 or more output connectors.", "Connector");
                            return;
                        }
                    }

                    if (node.HomelyType == homely_type.e_router)
                    {
                        if (node.OutputConnectors.Count == 1)
                        {
                            MessageBox.Show("Output connector not removed:\nA XAP Router requires 1 or more Input-Output routes.", "Connector");
                            return;
                        }
                    }

                    if (node.HomelyType == homely_type.e_xap_source)
                    {
                        MessageBox.Show("Output connector not removed:\nA XAP Source requires 1 output connector.", "Connector");
                        return;
                    }

                    if (node.HomelyType == homely_type.e_smart3_source)
                    {
                        MessageBox.Show("Output connector not removed:\nA Smart III Source requires 1 output connector.", "Connector");
                        return;
                    }

                    foreach (ConnectorViewModel cvm in node.OutputConnectors)
                    {
                        if (cvm == m_current_cvm)
                        {
                            clear_current_cvm();
                            int idx = node.OutputConnectors.IndexOf(cvm);
                            Utils.ImpObservableCollection<ConnectionViewModel> all_conns = cvm.AttachedConnections;
                            do
                            {
                                foreach (var ao in all_conns)
                                {
                                    this.ViewModel.Network.Connections.Remove(ao);
                                    break; // deletion has to happen 1 by 1, to give chance to network to update itself, hence the loop.
                                }
                                all_conns = cvm.AttachedConnections;
                            } while (all_conns.Count > 0);
                            node.OutputConnectors.Remove(cvm);
                            if (node.HomelyType == homely_type.e_router)
                            {
                                ConnectorViewModel deleteme = node.InputConnectors[idx];
                                all_conns = deleteme.AttachedConnections;
                                do
                                {
                                    foreach (var ao in all_conns)
                                    {
                                        this.ViewModel.Network.Connections.Remove(ao);
                                        break; // deletion has to happen 1 by 1, to give chance to network to update itself, hence the loop.
                                    }
                                    all_conns = deleteme.AttachedConnections;
                                } while (all_conns.Count > 0);
                                node.InputConnectors.Remove(deleteme); //.RemoveAt(idx);
                            }

                            set_dirty_flag(true);
                            return;
                        }
                    }

                    MessageBox.Show("Output connector not removed:\nNone selected.", "Connector");
                }
            }
            else
            {
                m_current_sel_node = null;
                MessageBox.Show("No node selected.", "Connector");
            }
        }

        private void node_property_handler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                m_current_sel_node = null;
                NodeViewModel node = (NodeViewModel)sender;
                m_current_sel_node = node;

                MyDetails.set_current_item(m_current_sel_node);
            }
        }

        bool SaveFile(XElement xElement)
        {
            bool cfg_saved = false;
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "Subtitle Gateway Configuration Files (*.sga)|*.sga"; // Not allowed: |All Files (*.*)|*.*
            if (saveFile.ShowDialog() == true)
            {
                try
                {
                    string tempfile = System.IO.Path.GetTempFileName();
                    xElement.Save(tempfile);
                    string filecontent = File.ReadAllText(tempfile);
                    squeeze_it.compressstringtofile(saveFile.FileName, filecontent);

                    m_current_sga_filename = saveFile.FileName;
                    set_dirty_flag(false);
                    
                    cfg_saved = true;
                }
                catch (Exception ex)
                {
                    // Are you in Debug mode? if so, then make sure that in app.manifest, this is set:
                    // <requestedExecutionLevel level="highestAvailable" uiAccess="false" /> (asInvoker is not enough to use the Save functions)
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); // MessageBox.Show(ex.StackTrace, 
                }
            }

            return cfg_saved;
        }

        bool SaveAsActiveConfig(XElement xElement, string filename)
        {
            bool cfg_saved = false;
            try
            {
                string tempfile = System.IO.Path.GetTempFileName();
                xElement.Save(tempfile);
                string filecontent = File.ReadAllText(tempfile);
                squeeze_it.compressstringtofile(filename, filecontent);

                m_current_sga_filename = filename;
                set_dirty_flag(false);

                cfg_saved = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); // MessageBox.Show(ex.StackTrace, 
            }

            return cfg_saved;
        }

        private XElement serialise_connectors(string name, Utils.ImpObservableCollection<ConnectorViewModel> connectors)
        {
            XElement ee = new XElement(name);
            string s = string.Empty;
            foreach (ConnectorViewModel cvm in connectors)
            {
                s += cvm.Name;
                s += ", ";
            }
            s = s.Remove(s.LastIndexOf(','));

            if (name == "xap_input")
                ee.Add(new XAttribute("from", s));
            else if (name == "xap_output")
                ee.Add(new XAttribute("to", s));

            return ee;
        }

        private bool serialise_connections(IEnumerable<ConnectionViewModel> connections, XElement root)
        {
            // save pipeline nodes connections 
            XElement e = new XElement("pipeline");
            e.Add(new XAttribute("name", "Main1"));
            e.Add(new XAttribute("role", "active"));
            {
                XElement ee = new XElement("connections");
                foreach (ConnectionViewModel cvm in connections)
                {
                    if (cvm.SourceConnector.IsConnected && cvm.SourceConnector.IsConnectionAttached &&
                        cvm.DestConnector.IsConnected && cvm.DestConnector.IsConnectionAttached)
                    {
                        string src = cvm.SourceConnector.ParentNode.Name + "." + cvm.SourceConnector.Name;
                        string dst = cvm.DestConnector.ParentNode.Name + "." + cvm.DestConnector.Name;

                        if (src != null && src != "" && dst != null && dst != "")
                        {
                            XElement eee = new XElement("connect");
                            eee.Add(new XAttribute("source", src));
                            eee.Add(new XAttribute("destination", dst));
                            ee.Add(eee);
                        }
                    }
                }
                e.Add(ee);
            }
            root.Add(e);

            return true;
        }

        XElement serialise_coords(NodeViewModel nvm)
        {
            XElement e = new XElement("screenpos");
            e.Add(new XAttribute("x", nvm.X));
            e.Add(new XAttribute("y", nvm.Y));
            e.Add(new XAttribute("z", nvm.ZIndex));
            return e;
        }

        private bool serialise_nodes(IEnumerable<NodeViewModel> nodes, XElement root)
        {
            //<!-- ********************************* README ******************************* 
            //     All names are arbitrary and unique.
            //     Each splitter accepts only 1 XAP input.
            //     Each switcher only supports 1 XAP output.
            //     Each ABSwitcher only supports 1 XAP output.
            //     Each Cyclone Input Source only supports 1 XAP output.
            //     Each aggregrator only supports 1 XAP output.
            //     Floating or unconnected components are ignored.				
            //     Currently, router connections are 1-to-1 connections, this means 2 or more inputs cannot go to the same output.
            //     These constraints are checked by the main application before connecting all parts
            //     ********************************* END OF README ******************************* -->

            // let's check that all names are unique
            {
                Hashtable names = new Hashtable();
                foreach (NodeViewModel nvm in nodes)
                {
                    if (nvm.Name == null)
                    {
                        MessageBox.Show("A node exists with an undefined name:\nNodes must have unique and arbitrary names.\n\nConfiguration not saved, please try again.", "Configuration");
                        return false;
                    }

                    nvm.Name.Trim(); // is this ok? Or does it need to be propagated back to the UI?

                    if (nvm.Name == "")
                    {
                        MessageBox.Show("A node exists with an empty name:\nNodes must have unique and arbitrary names.\n\nConfiguration not saved, please try again.", "Configuration");
                        return false;
                    }

                    if (names.Contains(nvm.Name))
                    {
                        MessageBox.Show("The name '" + nvm.Name + "' is used more than once:\nAll names must be unique and arbitrary.\n\nConfiguration not saved, please try again.", "Configuration");
                        return false;
                    }


                    names.Add(nvm.Name, true);
                }
            }

            if (!save_cyclone_input_sources(nodes, root))
                return false;

            if (!save_nw4_sources(nodes, root))
                return false;

            if (!save_selftest_sources(nodes, root))
                return false;

            if (!save_aggregators(nodes, root))
                return false;

            if (!save_routers(nodes, root))
                return false;

            if (!save_splitters(nodes, root))
                return false;

            if (!save_switchers(nodes, root))
                return false;

            if (!save_abswitchers(nodes, root)) 
                return false;

            if (!save_xap_destinations_list(nodes, root))
                return false;

            if (!save_xap_sources_list(nodes, root))
                return false;

            if (!save_cyclone_destinations_list(nodes, root))
                return false;
            
            if (!save_smart3_destinations_list(nodes, root))
                return false;

            if (!save_smart3_sources_list(nodes, root))
                return false;

            return true;
        }

        private bool save_cyclone_input_sources(IEnumerable<NodeViewModel> nodes, XElement root)
        {
            foreach (NodeViewModel nvm in nodes)
            {
                switch (nvm.HomelyType)
                {
                    case NetworkModel.homely_type.e_cyclone_input_source:
                        {
                            CycloneDetailsViewModel c = nvm as CycloneDetailsViewModel;
                            if (c.ServerPort == null)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has no Server Port defined.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }

                            if (c.SvcID == null)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has no Service ID defined.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }

                            try
                            {
                                Convert.ToInt32(c.ServerPort);
                            }
                            catch (System.FormatException)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has invalid Server Port.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }

                            try
                            {
                                Convert.ToInt32(c.SvcID);
                            }
                            catch (System.FormatException)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has invalid Service ID.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }

                            XElement e = new XElement("cyclone_input_source");
                            e.Add(new XAttribute("name", nvm.Name));
                            {
                                XElement ee = new XElement("connection");
                                ee.Add(new XAttribute("serverport", c.ServerPort));
                                ee.Add(new XAttribute("svcid", c.SvcID));
                                e.Add(ee);
                            }
                            e.Add(serialise_connectors("xap_output", c.OutputConnectors));
                            e.Add(serialise_coords(c as NodeViewModel));
                            root.Add(e);
                        }
                        break;
                }
            }
            return true;
        }

        private bool save_nw4_sources(IEnumerable<NodeViewModel> nodes, XElement root)
        {
            foreach (NodeViewModel nvm in nodes)
            {
                switch (nvm.HomelyType)
                {
                    case NetworkModel.homely_type.e_net_nw4_source:
                        {
                            NetNW4DetailsViewModel c = nvm as NetNW4DetailsViewModel;
                            if (c.ServerPort == null)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has no Server Port defined.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }

                            if (c.SvcID == null)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has no Service ID defined.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }

                            try
                            {
                                Convert.ToInt32(c.ServerPort);
                            }
                            catch (System.FormatException)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has invalid Server Port.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }

                            try
                            {
                                Convert.ToInt32(c.SvcID);
                            }
                            catch (System.FormatException)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has invalid Service ID.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }

                            if (c.SendAck == null)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has undefined SendAck.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }

                           XElement e = new XElement("net_nw4_source");
                            e.Add(new XAttribute("name", nvm.Name));
                            {
                                XElement ee = new XElement("connection");
                                ee.Add(new XAttribute("serverport", c.ServerPort));
                                ee.Add(new XAttribute("svcid", c.SvcID));
                                ee.Add(new XAttribute("send_ack", c.SendAck ? "true" : "false"));
                                e.Add(ee);
                            }
                            e.Add(serialise_connectors("xap_output", c.OutputConnectors));
                            e.Add(serialise_coords(c as NodeViewModel));
                            root.Add(e);
                        }
                        break;
                }
            }
            return true;
        }

        private bool save_selftest_sources(IEnumerable<NodeViewModel> nodes, XElement root)
        {
            foreach (NodeViewModel nvm in nodes)
            {
                switch (nvm.HomelyType)
                {
                    case NetworkModel.homely_type.e_selftest_source:
                        {
                            SelfTestSourceDetailsViewModel c = nvm as SelfTestSourceDetailsViewModel;
                            if (c.SvcID == null)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has no Service ID defined.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }

                            if ((c.TxtFgCol != null) && (c.RowTimeout != null) && (c.XAPType != null))
                            {
                                try
                                {
                                    Convert.ToInt32(c.SvcID);
                                }
                                catch (System.FormatException)
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has invalid Service ID.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }

                                try
                                {
                                    Convert.ToInt32(c.RowTimeout);
                                }
                                catch (System.FormatException)
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has invalid Row Timeout.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }

                                XElement e = new XElement("selftest_source");
                                e.Add(new XAttribute("name", nvm.Name));
                                {
                                    XElement ee = new XElement("testdata");
                                    ee.Add(new XAttribute("txt_fgcolour", c.TxtFgCol));
                                    ee.Add(new XAttribute("xap_type", c.XAPType));
                                    ee.Add(new XAttribute("rowtimeout", c.RowTimeout));
                                    ee.Add(new XAttribute("svcid", c.SvcID));
                                    e.Add(ee);
                                }
                                e.Add(serialise_connectors("xap_output", c.OutputConnectors));
                                e.Add(serialise_coords(c as NodeViewModel));
                                root.Add(e);
                            }
                            else
                            {
                                MessageBox.Show("'" + nvm.Name + "' has invalid either Foreground Colour or Row Timeout or XAP Type.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }
                        }
                        break;
                }
            }
            return true;
        }

        private bool save_aggregators(IEnumerable<NodeViewModel> nodes, XElement root)
        {
            foreach (NodeViewModel nvm in nodes)
            {
                switch (nvm.HomelyType)
                {
                    case NetworkModel.homely_type.e_aggregator: // each aggregrator only supports multi_in and 1 XAP output.
                        {
                            AggregatorDetailsViewModel c = nvm as AggregatorDetailsViewModel;

                            XElement e = new XElement("aggregator");
                            e.Add(new XAttribute("name", nvm.Name));
                            e.Add(serialise_connectors("xap_input", c.InputConnectors));
                            e.Add(serialise_connectors("xap_output", c.OutputConnectors));
                            e.Add(serialise_coords(c as NodeViewModel));
                            root.Add(e);
                        }
                        break;
                }
            }
            return true;
        }

        private bool save_routers(IEnumerable<NodeViewModel> nodes, XElement root)
        {
            foreach (NodeViewModel nvm in nodes)
            {
                switch (nvm.HomelyType)
                {
                    case NetworkModel.homely_type.e_router: // multi_in and multi_out
                        {
                            RouterDetailsViewModel c = nvm as RouterDetailsViewModel;
                            XElement e = new XElement("router");
                            e.Add(new XAttribute("name", nvm.Name));
                            e.Add(serialise_connectors("xap_input", c.InputConnectors));
                            e.Add(serialise_connectors("xap_output", c.OutputConnectors));
                            e.Add(serialise_coords(c as NodeViewModel));
                            root.Add(e);
                        }
                        break;
                }
            }
            return true;
        }

        private bool save_splitters(IEnumerable<NodeViewModel> nodes, XElement root)
        {
            foreach (NodeViewModel nvm in nodes)
            {
                switch (nvm.HomelyType)
                {
                    case NetworkModel.homely_type.e_splitter: // each splitter accepts only 1 XAP input and multi_out
                        {
                            SplitterDetailsViewModel c = nvm as SplitterDetailsViewModel;
                            XElement e = new XElement("splitter");
                            e.Add(new XAttribute("name", nvm.Name));
                            e.Add(serialise_connectors("xap_input", c.InputConnectors));
                            e.Add(serialise_connectors("xap_output", c.OutputConnectors));
                            e.Add(serialise_coords(c as NodeViewModel));
                            root.Add(e);
                        }
                        break;
                }
            }
            return true;
        }

        private bool save_switchers(IEnumerable<NodeViewModel> nodes, XElement root)
        {
            foreach (NodeViewModel nvm in nodes)
            {
                switch (nvm.HomelyType)
                {
                    case NetworkModel.homely_type.e_switcher: // each switcher only supports multi_in and 1 XAP output.
                        {
                            SwitcherDetailsViewModel c = nvm as SwitcherDetailsViewModel;
                            if (c.DiscrepancyTime == null)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has no Discrepancy Time defined.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }

                            try
                            {
                                Convert.ToInt32(c.DiscrepancyTime);
                            }
                            catch (System.FormatException)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has invalid Discrepancy Time.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }

                            XElement e = new XElement("switcher");
                            e.Add(new XAttribute("name", nvm.Name));
                            {
                                XElement ee = new XElement("input_selector");
                                ee.Add(new XAttribute("discrepancy_time", c.DiscrepancyTime));
                                e.Add(ee);
                            }
                            e.Add(serialise_connectors("xap_input", c.InputConnectors));
                            e.Add(serialise_connectors("xap_output", c.OutputConnectors));
                            e.Add(serialise_coords(c as NodeViewModel));
                            root.Add(e);
                        }
                        break;
                }
            }
            return true;
        }

        private bool save_abswitchers(IEnumerable<NodeViewModel> nodes, XElement root)
        {
            foreach (NodeViewModel nvm in nodes)
            {
                switch (nvm.HomelyType)
                {
                    case NetworkModel.homely_type.e_abswitcher:// each abswitcher only supports 2 in and 1 XAP output.
                        {
                            ABSwitcherDetailsViewModel c = nvm as ABSwitcherDetailsViewModel;
                            if (c.DiscrepancyTime == null)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has no Discrepancy Time defined.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }

                            try
                            {
                                int i = Convert.ToInt32(c.DiscrepancyTime);
                                if (i < 0 || i > 10000)
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has invalid Discrepancy Time, should be between 0 and 10000.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }
                            }
                            catch (System.FormatException)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has invalid Discrepancy Time.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }

                            if (c.MasterReceiveTimeout == null)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has no Master Receive Timeout defined.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }

                            try
                            {
                                int i = Convert.ToInt32(c.MasterReceiveTimeout);
                                if (i < 0 || i > 10000)
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has invalid Master Receive Timeout, should be between 0 and 10000.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }
                            }
                            catch (System.FormatException)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has invalid Master Receive Timeout.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }

                            XElement e = new XElement("abswitcher");
                            e.Add(new XAttribute("name", nvm.Name));
                            {
                                XElement ee = new XElement("input_selector");
                                ee.Add(new XAttribute("discrepancy_time", c.DiscrepancyTime));
                                ee.Add(new XAttribute("master_receive_timeout", c.MasterReceiveTimeout));
                                e.Add(ee);
                            }
                            e.Add(serialise_connectors("xap_input", c.InputConnectors));
                            e.Add(serialise_connectors("xap_output", c.OutputConnectors));
                            e.Add(serialise_coords(c as NodeViewModel));
                            root.Add(e);
                        }
                        break;
                }
            }            
            return true;
        }

        private bool save_xap_destinations_list(IEnumerable<NodeViewModel> nodes, XElement root)
        {
            // save xap destinations
            XElement ne = new XElement("xap_destinations");
            foreach (NodeViewModel nvm in nodes)
            {
                switch (nvm.HomelyType)
                {
                    case NetworkModel.homely_type.e_xap_destination:
                        {
                            XAPDestinationDetailsViewModel c = nvm as XAPDestinationDetailsViewModel;
                            if (c.IsXAPClient)
                            {
                                if ((c.Host == null) || (c.Port == null))
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has incomplete XAP destination host & port details.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }

                                try
                                {
                                    Convert.ToInt32(c.Port);
                                }
                                catch (System.FormatException)
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has invalid Port.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }

                                if ((c.SvcID != null) && (c.SvcID != ""))
                                {
                                    try
                                    {
                                        Convert.ToInt32(c.SvcID);
                                    }
                                    catch (System.FormatException)
                                    {
                                        MessageBox.Show("'" + nvm.Name + "' has invalid Service ID.\nConfiguration not saved, please try again.", "Configuration");
                                        return false;
                                    }
                                }
                                else
                                {
                                    c.SvcID = ""; // Empty string means ALL service IDs are allowed
                                }

                                {
                                    XElement ee = new XElement("destination");
                                    ee.Add(new XAttribute("name", c.Name));
                                    ee.Add(new XAttribute("host", c.Host));
                                    ee.Add(new XAttribute("port", c.Port));
                                    ee.Add(new XAttribute("svcid", c.SvcID));
                                    ee.Add(new XAttribute("act", "client"));
                                    ee.Add(serialise_connectors("xap_input", c.InputConnectors));
                                    ee.Add(serialise_coords(c as NodeViewModel));
                                    ne.Add(ee);
                                }
                            }
                            else // save XAP server
                            {
                                if (c.Port == null)
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has incomplete XAP destination port detail.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }

                                try
                                {
                                    Convert.ToInt32(c.Port);
                                }
                                catch (System.FormatException)
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has invalid Port.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }

                                if ((c.SvcID != null) && (c.SvcID != ""))
                                {
                                    try
                                    {
                                        Convert.ToInt32(c.SvcID);
                                    }
                                    catch (System.FormatException)
                                    {
                                        MessageBox.Show("'" + nvm.Name + "' has invalid Service ID.\nConfiguration not saved, please try again.", "Configuration");
                                        return false;
                                    }
                                }
                                else
                                {
                                    c.SvcID = ""; // Empty string means ALL service IDs are allowed
                                }

                                {
                                    XElement ee = new XElement("destination");
                                    ee.Add(new XAttribute("name", c.Name));
                                    ee.Add(new XAttribute("host", "")); // no host needed for XAP server setup
                                    ee.Add(new XAttribute("port", c.Port));
                                    ee.Add(new XAttribute("svcid", c.SvcID));
                                    ee.Add(new XAttribute("act", "server"));
                                    ee.Add(serialise_connectors("xap_input", c.InputConnectors));
                                    ee.Add(serialise_coords(c as NodeViewModel));
                                    ne.Add(ee);
                                }
                            }
                        }
                        break;
                }
            }
            root.Add(ne);
            return true;
        }

        private bool save_xap_sources_list(IEnumerable<NodeViewModel> nodes, XElement root)
        {
            // save xap sources
            XElement ne = new XElement("xap_sources");
            foreach (NodeViewModel nvm in nodes)
            {
                switch (nvm.HomelyType)
                {
                    case NetworkModel.homely_type.e_xap_source:
                        {
                            XAPSourceDetailsViewModel c = nvm as XAPSourceDetailsViewModel;
                            if (c.IsXAPClient)
                            {
                                if ((c.Host == null) || (c.Port == null))
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has incomplete XAP source host & port details.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }

                                try
                                {
                                    Convert.ToInt32(c.Port);
                                }
                                catch (System.FormatException)
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has invalid Port.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }

                                if ((c.SvcID != null) && (c.SvcID != ""))
                                {
                                    try
                                    {
                                        Convert.ToInt32(c.SvcID);
                                    }
                                    catch (System.FormatException)
                                    {
                                        MessageBox.Show("'" + nvm.Name + "' has invalid Service ID.\nConfiguration not saved, please try again.", "Configuration");
                                        return false;
                                    }
                                }
                                else
                                {
                                    c.SvcID = ""; // Empty string means ALL service IDs are allowed
                                }

                                {
                                    XElement ee = new XElement("source");
                                    ee.Add(new XAttribute("name", c.Name));
                                    ee.Add(new XAttribute("host", c.Host));
                                    ee.Add(new XAttribute("port", c.Port));
                                    ee.Add(new XAttribute("svcid", c.SvcID));
                                    ee.Add(new XAttribute("act", "client"));
                                    ee.Add(new XAttribute("sendendliveondisconnect", c.SendEndLiveOnDisconnect));
                                    ee.Add(serialise_connectors("xap_output", c.OutputConnectors));
                                    ee.Add(serialise_coords(c as NodeViewModel));
                                    ne.Add(ee);
                                }
                            }
                            else // save XAP server
                            {
                                if (c.Port == null)
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has incomplete XAP source port detail.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }

                                try
                                {
                                    Convert.ToInt32(c.Port);
                                }
                                catch (System.FormatException)
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has invalid Port.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }

                                if ((c.SvcID != null) && (c.SvcID != ""))
                                {
                                    try
                                    {
                                        Convert.ToInt32(c.SvcID);
                                    }
                                    catch (System.FormatException)
                                    {
                                        MessageBox.Show("'" + nvm.Name + "' has invalid Service ID.\nConfiguration not saved, please try again.", "Configuration");
                                        return false;
                                    }
                                }
                                else
                                {
                                    c.SvcID = ""; // Empty string means ALL service IDs are allowed
                                }

                                {
                                    XElement ee = new XElement("source");
                                    ee.Add(new XAttribute("name", c.Name));
                                    ee.Add(new XAttribute("host", "")); // no host needed for XAP server setup
                                    ee.Add(new XAttribute("port", c.Port));
                                    ee.Add(new XAttribute("svcid", c.SvcID));
                                    ee.Add(new XAttribute("act", "server"));
                                    ee.Add(new XAttribute("sendendliveondisconnect", c.SendEndLiveOnDisconnect));
                                    ee.Add(serialise_connectors("xap_output", c.OutputConnectors));
                                    ee.Add(serialise_coords(c as NodeViewModel));
                                    ne.Add(ee);
                                }
                            }
                        }
                        break;
                }
            }
            root.Add(ne);
            return true;
        }

        private bool save_cyclone_destinations_list(IEnumerable<NodeViewModel> nodes, XElement root)
        {
            XElement ne = new XElement("cyclone_destinations");
            foreach (NodeViewModel nvm in nodes)
            {
                switch (nvm.HomelyType)
                {
                    case NetworkModel.homely_type.e_cyclone_destination:
                        {
                            CycloneDestinationDetailsViewModel c = nvm as CycloneDestinationDetailsViewModel;
                            if (c.IsCycloneClient)
                            {
                                if ((c.Host == null) || (c.Port == null))
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has incomplete Cyclone destination host & port details.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }

                                try
                                {
                                    Convert.ToInt32(c.Port);
                                }
                                catch (System.FormatException)
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has invalid Port.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }

                                {
                                    XElement ee = new XElement("destination");
                                    ee.Add(new XAttribute("name", c.Name));
                                    ee.Add(new XAttribute("host", c.Host));
                                    ee.Add(new XAttribute("port", c.Port));
                                    ee.Add(new XAttribute("act", "client"));
                                    ee.Add(new XAttribute("default_lang", c.DefaultLang));
                                    ee.Add(new XAttribute("svcid", c.SvcID == null ? "" : c.SvcID));
                                    ee.Add(serialise_connectors("xap_input", c.InputConnectors));
                                    ee.Add(serialise_coords(c as NodeViewModel));
                                    ne.Add(ee);
                                }
                            }
                            else // save Cyclone server (Currently Not supported)
                            {
                                if (c.Port == null)
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has incomplete Cyclone destination port detail.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }

                                try
                                {
                                    Convert.ToInt32(c.Port);
                                }
                                catch (System.FormatException)
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has invalid Port.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }

                                {
                                    XElement ee = new XElement("destination");
                                    ee.Add(new XAttribute("name", c.Name));
                                    ee.Add(new XAttribute("host", "")); // no host needed for Cyclone server setup
                                    ee.Add(new XAttribute("port", c.Port));
                                    ee.Add(new XAttribute("act", "server"));
                                    ee.Add(serialise_connectors("xap_input", c.InputConnectors));
                                    ee.Add(serialise_coords(c as NodeViewModel));
                                    ne.Add(ee);
                                }
                            }
                        }
                        break;
                }
            }
            root.Add(ne);
            return true;
        }

        private bool save_smart3_destinations_list(IEnumerable<NodeViewModel> nodes, XElement root)
        {
            XElement ne = new XElement("smart3_destinations");
            foreach (NodeViewModel nvm in nodes)
            {
                switch (nvm.HomelyType)
                {
                    case NetworkModel.homely_type.e_smart3_destination:
                        {
                            Smart3DestinationDetailsViewModel c = nvm as Smart3DestinationDetailsViewModel;
                            if (c.IsTCPClient)
                            {
                                if (c.Host == null || c.TCPPort == null)
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has incomplete destination host / port details.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }

                                try
                                {
                                    Convert.ToInt32(c.TCPPort);
                                }
                                catch (System.FormatException)
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has invalid Port.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }

                                {
                                    XElement ee = new XElement("destination");
                                    ee.Add(new XAttribute("name", c.Name));
                                    ee.Add(new XAttribute("host", c.Host));
                                    ee.Add(new XAttribute("tcp_port", c.TCPPort));
                                    ee.Add(new XAttribute("transport", "tcp"));
                                    ee.Add(new XAttribute("serial_port", ""));
                                    ee.Add(new XAttribute("baud", "")); // serial parameter only
                                    ee.Add(new XAttribute("parity", "")); // serial parameter only
                                    ee.Add(new XAttribute("data", "")); // serial parameter only
                                    ee.Add(new XAttribute("stop", "")); // serial parameter only
                                    ee.Add(new XAttribute("default_mode", c.EncoderDefaultMode));
                                    ee.Add(new XAttribute("real_time_rows", c.EncoderRealTimeRows));
                                    ee.Add(new XAttribute("real_time_base_row", c.EncoderRealTimeBaseRow));
                                    ee.Add(new XAttribute("user_defined_mode", c.EncoderUserDefinedMode));
                                    ee.Add(new XAttribute("remove_top_bit", c.RemoveTopBit));
                                    ee.Add(serialise_connectors("xap_input", c.InputConnectors));
                                    ee.Add(serialise_coords(c as NodeViewModel));
                                    ne.Add(ee);
                                }
                            }
                            else // save Smart III serial
                            {
                                if (c.SerialPort == null || c.BaudRate == null || c.Parity == null || c.DataBits == null || c.StopBits == null)
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has incomplete serial port details.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }

                                try
                                {
                                    Convert.ToInt32(c.SerialPort);
                                }
                                catch (System.FormatException)
                                {
                                    MessageBox.Show("'" + nvm.Name + "' has invalid Port.\nConfiguration not saved, please try again.", "Configuration");
                                    return false;
                                }

                                {
                                    XElement ee = new XElement("destination");
                                    ee.Add(new XAttribute("name", c.Name));
                                    ee.Add(new XAttribute("host", "")); // no host required for serial comms
                                    ee.Add(new XAttribute("tcp_port", "")); // no tcp port required for serial comms
                                    ee.Add(new XAttribute("transport", "serial"));
                                    ee.Add(new XAttribute("serial_port", c.SerialPort));
                                    ee.Add(new XAttribute("baud", c.BaudRate));
                                    ee.Add(new XAttribute("parity", c.Parity));
                                    ee.Add(new XAttribute("data", c.DataBits));
                                    ee.Add(new XAttribute("stop", c.StopBits));
                                    ee.Add(new XAttribute("default_mode", c.EncoderDefaultMode));
                                    ee.Add(new XAttribute("real_time_rows", c.EncoderRealTimeRows));
                                    ee.Add(new XAttribute("real_time_base_row", c.EncoderRealTimeBaseRow));
                                    ee.Add(new XAttribute("user_defined_mode", c.EncoderUserDefinedMode));
                                    ee.Add(new XAttribute("remove_top_bit", c.RemoveTopBit));
                                    ee.Add(serialise_connectors("xap_input", c.InputConnectors));
                                    ee.Add(serialise_coords(c as NodeViewModel));
                                    ne.Add(ee);
                                }
                            }
                        }
                        break;
                }
            }
            root.Add(ne);
            return true;
        }

        private bool save_smart3_sources_list(IEnumerable<NodeViewModel> nodes, XElement root)
        {
            XElement ne = new XElement("smart3_sources");
            foreach (NodeViewModel nvm in nodes)
            {
                switch (nvm.HomelyType)
                {
                    case NetworkModel.homely_type.e_smart3_source:
                        {
                            Smart3SourceDetailsViewModel c = nvm as Smart3SourceDetailsViewModel;
                            if (c.CDP_FrameRate == null || c.SvcID == null || c.ServerPort == null)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has one or more incomplete details:\nPlease check CDP Frame rate, Svc ID or Server Port.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }

                            try
                            {
                                Convert.ToInt32(c.ServerPort);
                            }
                            catch (System.FormatException)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has invalid Server Port.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }

                            try
                            {
                                Convert.ToInt32(c.SvcID);
                            }
                            catch (System.FormatException)
                            {
                                MessageBox.Show("'" + nvm.Name + "' has invalid Service ID.\nConfiguration not saved, please try again.", "Configuration");
                                return false;
                            }
                                
                            XElement ee = new XElement("source");
                            ee.Add(new XAttribute("name", c.Name));
                            ee.Add(new XAttribute("serverport", c.ServerPort)); 
                            ee.Add(new XAttribute("svcid", c.SvcID));
                            ee.Add(new XAttribute("cdp_framerate", c.CDP_FrameRate));
                            ee.Add(serialise_connectors("xap_output", c.OutputConnectors));
                            ee.Add(serialise_coords(c as NodeViewModel));
                            ne.Add(ee);                            
                        }
                        break;
                }
            }
            root.Add(ne);
            return true;
        }

        // mouse clicked on a connectoritem's ellipse
        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            clear_current_cvm();

            m_current_cvm_ellipse = sender as Ellipse;
            m_current_cvm_ellipse.StrokeThickness *= 2;
            m_current_cvm_ellipse.Stroke = System.Windows.Media.Brushes.Red;

            m_current_cvm = m_current_cvm_ellipse.DataContext as ConnectorViewModel;
            m_current_sel_node = m_current_cvm.ParentNode;                       
        }

        private void clear_current_cvm()
        {
            if (m_current_cvm_ellipse != null)
            {
                m_current_cvm_ellipse.Stroke = System.Windows.Media.Brushes.Black;
                m_current_cvm_ellipse.StrokeThickness /= 2;
            }
            m_current_cvm_ellipse = null;
            m_current_cvm = null;
        }

        private void btn_new_cfg_Click(object sender, RoutedEventArgs e)
        {
            bool can_create_new = save_if_modified();

            if (can_create_new)
                new_cfg();
        }

        private void btn_open_cfg_Click(object sender, RoutedEventArgs e)
        {
            save_if_modified();
            open_cfg(false);
        }

        private void btn_save_cfg_Click(object sender, RoutedEventArgs e)
        {
            save_cfg();
        }

        private bool save_cfg()
        {
            if (this.ViewModel.Network.Nodes.Count > 0)
            {
                IEnumerable<NodeViewModel> nodes = this.ViewModel.Network.Nodes; // OfType<NodeViewModel>();
                IEnumerable<ConnectionViewModel> connections = this.ViewModel.Network.Connections; // this.Children.OfType<Connection>();

                XElement root = new XElement("config");
                root.Add(new XAttribute("admin_version", partname_adminversion.Content)); // store the version of this administrator

                bool cfg_saved = false;
                if (serialise_nodes(nodes, root))
                    if (serialise_connections(connections, root))
                        cfg_saved = SaveFile(root);
                return cfg_saved;
            }
            else
            {
                MessageBox.Show("Configuration not saved: Empty file", "Warning");
            }
            return false;
        }

        private XElement load_serialised_datafromfile(bool is_active_load, out string loaded_filename)
        {
            loaded_filename = "";
            if (!is_active_load)
            {
                OpenFileDialog openFile = new OpenFileDialog();
                openFile.Filter = "Subtitle Gateway Configuration Files (*.sga)|*.sga";

                if (openFile.ShowDialog() == true)
                {
                    try
                    {
                        byte[] file = File.ReadAllBytes(openFile.FileName);
                        byte[] decompressed = squeeze_it.decompress(file);
                        string tempfile = System.IO.Path.GetTempFileName();
                        File.WriteAllText(tempfile, System.Text.ASCIIEncoding.ASCII.GetString(decompressed), System.Text.ASCIIEncoding.ASCII);
                        XElement root = XElement.Load(tempfile);
                        if (root != null)
                            loaded_filename = openFile.FileName;
                        return root;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); // MessageBox.Show(ex.StackTrace, 
                    }
                }
            }
            else
            {
                try
                {
                    byte[] file = File.ReadAllBytes(m_active_cfg_to_load);
                    byte[] decompressed = squeeze_it.decompress(file);
                    string tempfile = System.IO.Path.GetTempFileName();
                    File.WriteAllText(tempfile, System.Text.ASCIIEncoding.ASCII.GetString(decompressed), System.Text.ASCIIEncoding.ASCII);
                    XElement root = XElement.Load(tempfile);
                    if (root != null)
                        loaded_filename = m_active_cfg_to_load;
                    return root;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); // MessageBox.Show(ex.StackTrace, 
                }
            }
            return null;
        }

        private void LoadNode(homely_type ht, XElement e)
        {
            NodeViewModel node = null;
            
            switch (ht)
            {
                case homely_type.e_aggregator:
                    {
                        string name = e.Attribute("name").Value;
                        node = new AggregatorDetailsViewModel(name);
                    }
                    break;

                case homely_type.e_cyclone_input_source:
                    {
                        string name = e.Attribute("name").Value;
                        node = new CycloneDetailsViewModel(name);
                        CycloneDetailsViewModel c = node as CycloneDetailsViewModel;
                        c.ServerPort = e.Element("connection").Attribute("serverport").Value;
                        c.SvcID = e.Element("connection").Attribute("svcid").Value;
                    }
                    break;

                case homely_type.e_net_nw4_source:
                    {
                        string name = e.Attribute("name").Value;
                        node = new NetNW4DetailsViewModel(name);
                        NetNW4DetailsViewModel c = node as NetNW4DetailsViewModel;
                        c.ServerPort = e.Element("connection").Attribute("serverport").Value;
                        c.SvcID = e.Element("connection").Attribute("svcid").Value;
                        c.SendAck = e.Element("connection").Attribute("send_ack").Value == "true";
                    }
                    break;

                case homely_type.e_selftest_source:
                    {
                        string name = e.Attribute("name").Value;
                        node = new SelfTestSourceDetailsViewModel(name);
                        SelfTestSourceDetailsViewModel c = node as SelfTestSourceDetailsViewModel;
                        c.RowTimeout = e.Element("testdata").Attribute("rowtimeout").Value;
                        c.TxtFgCol = e.Element("testdata").Attribute("txt_fgcolour").Value;
                        c.SvcID = e.Element("testdata").Attribute("svcid").Value;
                        c.XAPType = e.Element("testdata").Attribute("xap_type").Value;
                    }
                    break;

                case homely_type.e_router:
                    {
                        string name = e.Attribute("name").Value;
                        node = new RouterDetailsViewModel(name);
                    }
                    break;

                case homely_type.e_splitter:
                    {
                        string name = e.Attribute("name").Value;
                        node = new SplitterDetailsViewModel(name);
                    }
                    break;

                case homely_type.e_switcher:
                    {
                        string name = e.Attribute("name").Value;
                        node = new SwitcherDetailsViewModel(name);
                        SwitcherDetailsViewModel c = node as SwitcherDetailsViewModel;
                        c.DiscrepancyTime = e.Element("input_selector").Attribute("discrepancy_time").Value;
                    }
                    break;

                case homely_type.e_abswitcher:
                    {
                        string name = e.Attribute("name").Value;
                        node = new ABSwitcherDetailsViewModel(name);
                        ABSwitcherDetailsViewModel c = node as ABSwitcherDetailsViewModel;
                        c.DiscrepancyTime = e.Element("input_selector").Attribute("discrepancy_time").Value;
                        c.MasterReceiveTimeout = e.Element("input_selector").Attribute("master_receive_timeout").Value;
                    }
                    break;

                case homely_type.e_xap_destination:
                    {
                        string name = e.Attribute("name").Value;
                        node = new XAPDestinationDetailsViewModel(name);
                        XAPDestinationDetailsViewModel c = node as XAPDestinationDetailsViewModel;
                        c.Host = e.Attribute("host").Value;
                        c.Port = e.Attribute("port").Value;
                        c.SvcID = e.Attribute("svcid").Value;
                        c.IsXAPClient = e.Attribute("act").Value == "client";
                    }
                    break;

                case homely_type.e_cyclone_destination:
                    {
                        string name = e.Attribute("name").Value;
                        node = new CycloneDestinationDetailsViewModel(name);
                        CycloneDestinationDetailsViewModel c = node as CycloneDestinationDetailsViewModel;
                        c.Host = e.Attribute("host").Value;
                        c.Port = e.Attribute("port").Value;
                        c.IsCycloneClient = e.Attribute("act").Value == "client";
                        c.DefaultLang = e.Attribute("default_lang").Value;
                        if (e.Attribute("svcid") != null)
                        {
                            c.SvcID = e.Attribute("svcid").Value;
                        }
                        else
                        {
                            // MessageBox("A word of warning: Your config file is found to be old: Your cyclone destination XXX lacks service Id entry.")
                            // MessageBox("You need to re-save it. Without a Service ID entry, Sub. Gateway will not filter out (or ignore) any services sent to a Cyclone destination.")
                            // MessageBox("This is not good because Cyclone server will not be able to handle it properly. ")
                            c.SvcID = ""; // use of empty string means all service IDs are allowed through.
                        }
                    }
                    break;

                case homely_type.e_smart3_destination:
                    {
                        string name = e.Attribute("name").Value;
                        node = new Smart3DestinationDetailsViewModel(name);
                        Smart3DestinationDetailsViewModel c = node as Smart3DestinationDetailsViewModel;
                        c.Host = e.Attribute("host").Value;
                        c.TCPPort = e.Attribute("tcp_port").Value;
                        c.IsTCPClient = e.Attribute("transport").Value == "tcp";
                        c.SerialPort = e.Attribute("serial_port").Value;
                        c.BaudRate = e.Attribute("baud").Value;
                        c.Parity = e.Attribute("parity").Value;
                        c.DataBits = e.Attribute("data").Value;
                        c.StopBits = e.Attribute("stop").Value;
                        c.EncoderDefaultMode = e.Attribute("default_mode").Value;
                        c.EncoderRealTimeRows = e.Attribute("real_time_rows").Value;
                        c.EncoderRealTimeBaseRow = e.Attribute("real_time_base_row").Value;
                        c.EncoderUserDefinedMode = e.Attribute("user_defined_mode").Value;
                        if (e.Attribute("remove_top_bit") != null)
                            c.RemoveTopBit = e.Attribute("remove_top_bit").Value == "true";                     
                    }
                    break;

                case homely_type.e_xap_source:
                    {
                        string name = e.Attribute("name").Value;
                        node = new XAPSourceDetailsViewModel(name);
                        XAPSourceDetailsViewModel c = node as XAPSourceDetailsViewModel;
                        c.Host = e.Attribute("host").Value;
                        c.Port = e.Attribute("port").Value;
                        c.SvcID = e.Attribute("svcid").Value;
                        c.IsXAPClient = e.Attribute("act").Value == "client";
                        if (e.Attribute("sendendliveondisconnect") != null)
                            c.SendEndLiveOnDisconnect = e.Attribute("sendendliveondisconnect").Value == "true";
                    }
                    break;

                case homely_type.e_smart3_source:
                    {
                        string name = e.Attribute("name").Value;
                        node = new Smart3SourceDetailsViewModel(name);
                        Smart3SourceDetailsViewModel c = node as Smart3SourceDetailsViewModel;
                        c.ServerPort = e.Attribute("serverport").Value;
                        c.SvcID = e.Attribute("svcid").Value;
                        c.CDP_FrameRate = e.Attribute("cdp_framerate").Value;
                    }
                    break;
            }

            XElement ee = e.Element("xap_input");
            if (ee != null)
            {
                string s = ee.Attribute("from").Value;
                string[] inputs = s.Split(", ".ToCharArray());

                foreach (var i in inputs)
                    if (i != "")
                        node.InputConnectors.Add(new ConnectorViewModel(i));
            }

            ee = e.Element("xap_output");
            if (ee != null)
            {
                string s = ee.Attribute("to").Value;
                string[] outputs = s.Split(", ".ToCharArray());

                foreach (var o in outputs)
                    if (o != "")
                        node.OutputConnectors.Add(new ConnectorViewModel(o));
            }

            node.X = Double.Parse(e.Element("screenpos").Attribute("x").Value, CultureInfo.InvariantCulture);
            node.Y = Double.Parse(e.Element("screenpos").Attribute("y").Value, CultureInfo.InvariantCulture);
            node.ZIndex = Int32.Parse(e.Element("screenpos").Attribute("z").Value);

            // add selection event handler to the node 
            node.PropertyChanged += node_property_handler;
            this.ViewModel.Network.Nodes.Add(node);            
        }

        private void LoadConnection(XElement e)
        {
            string src = e.Attribute("source").Value;
            string dst = e.Attribute("destination").Value;

            // get nodes 
            NodeViewModel node1 = this.ViewModel.get_node_by_name(src);
            NodeViewModel node2 = this.ViewModel.get_node_by_name(dst);

            // create a connection between the nodes.
            if ((node1 != null) && (node2 != null))
            {
                ConnectionViewModel connection = new ConnectionViewModel();
                connection.SourceConnector = node1.get_outputconnector_by_name(src);
                connection.DestConnector = node2.get_inputconnector_by_name(dst);

                if ((connection.SourceConnector != null) && (connection.DestConnector != null))
                    // add the connection to the view-model.
                    this.ViewModel.Network.Connections.Add(connection);
            }
        }

        private void open_cfg(bool is_active_load)
        {
            string loaded_filename = "";

            XElement root = load_serialised_datafromfile(is_active_load, out loaded_filename);
            if (root == null)
                return;

            // first we need to check if the loaded file can be imported by this administrator. A new file cannot be imported by an old Administrator otherwise
            // there may be fields in the new file which can't be displayed in the old Administrator and thus cause misleading configs.
            {
                string tmp = partname_adminversion.Content.ToString();
                float this_admin_version = float.Parse(tmp.Substring(tmp.IndexOf('v') + 1), CultureInfo.InvariantCulture.NumberFormat);
                tmp = root.Attribute("admin_version").Value;
                float file_admin_version = float.Parse(tmp.Substring(tmp.IndexOf('v') + 1), CultureInfo.InvariantCulture.NumberFormat);

                if (file_admin_version > this_admin_version) 
                {
                    if (this_admin_version != 0.0)
                    {
                        MessageBox.Show("The file that you are opening is newer than this Administrator.\nTo use this file, update to Administrator v" + file_admin_version.ToString() + " or later.");
                        return;
                    }
                    else
                    {
                        if (MessageBox.Show("The file that you are opening is newer than this Administrator or was created with a versioned Administrator.\nIf you continue, this Administrator may stop working properly or crash.\nWould you like to continue?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.No)
                            return;
                    }
                }
            }

            new_cfg();

            // get the admin version which created this file and display it
            string version = root.Attribute("admin_version").Value;
            partname_creatorversion.Content = "c" + version.Substring(version.IndexOf('v') + 1);

            // Load them in the reversed order that they were saved in order to preserve z-indexing... who knows..
            IEnumerable<XElement>
            coll = root.Elements("smart3_sources").Elements("source");
            foreach (XElement e in coll)
                LoadNode(homely_type.e_smart3_source, e);

            coll = root.Elements("smart3_destinations").Elements("destination");
            foreach (XElement e in coll)
                LoadNode(homely_type.e_smart3_destination, e);

            coll = root.Elements("cyclone_destinations").Elements("destination");
            foreach (XElement e in coll)
                LoadNode(homely_type.e_cyclone_destination, e);

            coll = root.Elements("xap_sources").Elements("source");
            foreach (XElement e in coll)
                LoadNode(homely_type.e_xap_source, e);

            coll = root.Elements("xap_destinations").Elements("destination");
            foreach (XElement e in coll)
                LoadNode(homely_type.e_xap_destination, e);

            coll = root.Elements("router");
            foreach (XElement e in coll)
                LoadNode(homely_type.e_router, e);

            coll = root.Elements("aggregator");
            foreach (XElement e in coll)
                LoadNode(homely_type.e_aggregator, e);

            coll = root.Elements("splitter");
            foreach (XElement e in coll)
                LoadNode(homely_type.e_splitter, e);

            coll = root.Elements("switcher");
            foreach (XElement e in coll)
                LoadNode(homely_type.e_switcher, e);

            coll = root.Elements("abswitcher");
            foreach (XElement e in coll)
                LoadNode(homely_type.e_abswitcher, e);

            coll = root.Elements("cyclone_input_source");
            foreach (XElement e in coll)
                LoadNode(homely_type.e_cyclone_input_source, e);

            coll = root.Elements("net_nw4_source");
            foreach (XElement e in coll)
                LoadNode(homely_type.e_net_nw4_source, e);

            coll = root.Elements("selftest_source");
            foreach (XElement e in coll)
                LoadNode(homely_type.e_selftest_source, e);

            coll = root.Elements("pipeline").Elements("connections").Elements("connect");
            foreach (XElement e in coll)
                LoadConnection(e);

            m_current_sga_filename = loaded_filename;
            update_pipeline_name();

            this.InvalidateVisual();
        }

        private void new_cfg()
        {
            // clear network view & stuff
            this.ViewModel.Network.Connections.Clear();
            this.ViewModel.Network.Nodes.Clear();
            // reset creator's version
            partname_creatorversion.Content = "c-.--"; 
            // collapse edit
            MyDetails.clear_edits();
            // reset filename
            m_current_sga_filename = "";
            // reset dirt
            set_dirty_flag(false);
        }

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            bool can_create_new = save_if_modified();

            if (can_create_new)
                new_cfg();
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            save_if_modified();
            open_cfg(false);
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            save_cfg();
        }

        public void set_dirty_flag(bool val)
        {
            m_dirty = val;

            update_pipeline_name();
        }

        private void update_pipeline_name()
        {
            string name = "Pipeline";
            
            if (m_dirty)
                name += "*";
            name += ": ";

            if (m_current_sga_filename != "")
            {
                name += System.IO.Path.GetFileNameWithoutExtension(m_current_sga_filename);
                partname_pipeline.ToolTip = m_current_sga_filename; // To show the tooltip on the UI, move the mouse away from the Pipeline groupbox and move back in. Simple.
            }
            else
            {
                name += "Untitled";
                partname_pipeline.ToolTip = name;
            }

            partname_pipeline.Header = name.Replace("_", "__"); // In WPF, underscore usually denotes a mnemonic. 
                                                                // So I escape the underscore (by preceding it with an underscore (e.g. "abcde__f")).
                                                                // This is fine for display purposes.            
        }

        private void detailsedit_property_handler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "edit")
            {
                set_dirty_flag(true);
            }
        }

        private bool save_if_modified()
        {
            bool cfg_saved = true;
            if (m_dirty) // is the document dirty ?
            {
                if (this.ViewModel.Network.Nodes.Count > 0) // But actually, is there anything to save? 
                                                            // (Note: the doc could be dirty, but the user may have deleted all nodes in which case there wouldnt be anything to save)
                {
                    if (MessageBox.Show("Save changes to file?", "Warning", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        cfg_saved = save_cfg();
                }
            }
            return cfg_saved;
        }

        private void btn_startservice_cfg_Click(object sender, RoutedEventArgs e)
        {
            btn_startservice_cfg.IsEnabled = false;
            UiServices.SetBusyState();
            start_service();            
            check_service_status();
         }

        private void btn_restartservice_cfg_Click(object sender, RoutedEventArgs e)
        {
            btn_restartservice_cfg.IsEnabled = false;
            UiServices.SetBusyState();
            stop_service();
            start_service();
            check_service_status();
        }

        private void btn_stopservice_cfg_Click(object sender, RoutedEventArgs e)
        {
            btn_stopservice_cfg.IsEnabled = false;
            UiServices.SetBusyState();
            stop_service();
            check_service_status();
        }

        private bool start_service()
        {
            if (m_service == null)
                return false;

            try
            {
                m_service.Start();
                m_service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10.0));
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start " + m_service.DisplayName + " Service.\n" + ex.Message, "Start Service");
            }

            return false;
        }

        private bool stop_service()
        {
            if (m_service == null)
                return false;

            try
            {
                if (m_service.CanStop == true && m_service.Status == System.ServiceProcess.ServiceControllerStatus.Running)
                {
                    m_service.Stop();
                    m_service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10.0));                    
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to stop " + m_service.DisplayName + " Service.\n" + ex.Message, "Stop Service");
            }

            return false;
        }

        private void check_service_status()
        {
            // temporarily clear focusable            
            Keyboard.ClearFocus();
            btn_startservice_cfg.Focusable = false;
            btn_restartservice_cfg.Focusable = false;
            btn_stopservice_cfg.Focusable = false;

            btn_startservice_cfg.IsEnabled = false;
            btn_restartservice_cfg.IsEnabled = false;
            btn_stopservice_cfg.IsEnabled = false;

            try
            {
                m_service.Refresh();

                switch (m_service.Status)
                {
                    case ServiceControllerStatus.StartPending:
                    case ServiceControllerStatus.Running:
                        {
                            btn_startservice_cfg.IsEnabled = false;
                            btn_restartservice_cfg.IsEnabled = true;
                            btn_stopservice_cfg.IsEnabled = true;
                            partname_service_status.Source = null;
                            partname_service_status.Source = get_img_from_resrc("Resources\\Images\\ServiceStarted.png");
                        }
                        break;

                    case ServiceControllerStatus.Stopped:
                    case ServiceControllerStatus.StopPending:
                        {
                            btn_startservice_cfg.IsEnabled = true;
                            btn_restartservice_cfg.IsEnabled = false;
                            btn_stopservice_cfg.IsEnabled = false;
                            partname_service_status.Source = null;
                            partname_service_status.Source = get_img_from_resrc("Resources\\Images\\ServiceStopped.png");
                        }
                        break;

                    case ServiceControllerStatus.Paused:
                    case ServiceControllerStatus.PausePending:
                    case ServiceControllerStatus.ContinuePending:
                        {
                            btn_startservice_cfg.IsEnabled = false;
                            btn_restartservice_cfg.IsEnabled = true;
                            btn_stopservice_cfg.IsEnabled = true;
                            partname_service_status.Source = null;
                            partname_service_status.Source = get_img_from_resrc("Resources\\Images\\ServiceUndefined.png");
                        }
                        break;
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // now reset focusable
            btn_startservice_cfg.Focusable = true;
            btn_restartservice_cfg.Focusable = true;
            btn_stopservice_cfg.Focusable = true;
        }

        private BitmapImage get_img_from_resrc(string resourcePath)
        {
            var image = new BitmapImage();

            string moduleName = this.GetType().Assembly.GetName().Name;
            string resourceLocation =
                string.Format("pack://application:,,,/{0};component/{1}", moduleName, resourcePath);

            try
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.UriSource = new Uri(resourceLocation);
                image.EndInit();
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.ToString());
            }

            return image;
        }

        private string get_active_cfg_filename()
        {
            string activeconfig = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\Softel\\Subtitle Gateway\\subtitle_gateway_cfg.sga";
            return activeconfig;
        }

        private void btn_open_active_cfg_Click(object sender, RoutedEventArgs e)
        {
            if (m_dirty)
            {
                if (MessageBox.Show("Current changes are not going to be saved?\nClick OK to continue, or CANCEL to abort.", "Warning", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    return;
            }

            string activeconfig = get_active_cfg_filename();
            if (File.Exists(activeconfig))
            {
                m_active_cfg_to_load = activeconfig;
                open_cfg(true);
            }
            else
            {
                if (MessageBox.Show("Active configuration could not be located.\nClick OK to browse, or CANCEL to abort.", "Warning", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    open_cfg(false);
            }
        }

        private void btn_save_as_active_cfg_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel.Network.Nodes.Count > 0)
            {
                if (MessageBox.Show("This operation will overwrite your existing active configuration.\nClick OK to proceed or CANCEL to abort.", "Warning", MessageBoxButton.OKCancel) ==
                    MessageBoxResult.Cancel)
                    return;

                string activeconfig = get_active_cfg_filename();

                IEnumerable<NodeViewModel> nodes = this.ViewModel.Network.Nodes;
                IEnumerable<ConnectionViewModel> connections = this.ViewModel.Network.Connections;

                XElement root = new XElement("config");
                root.Add(new XAttribute("admin_version", partname_adminversion.Content)); // store the version of this administrator

                bool cfg_saved = false;
                if (serialise_nodes(nodes, root))
                    if (serialise_connections(connections, root))
                        cfg_saved = SaveAsActiveConfig(root, activeconfig);

                if (cfg_saved)
                {
                    string which = "start or restart";
                    try
                    {
                        m_service.Refresh();
                        switch (m_service.Status)
                        {
                            case ServiceControllerStatus.StartPending:
                            case ServiceControllerStatus.Running:
                                which = "restart";
                                break;

                            case ServiceControllerStatus.Stopped:
                            case ServiceControllerStatus.StopPending:
                                which = "start";
                                break;
                        }
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                    }

                    MessageBox.Show("Active configuration successfully saved.\nYou can now " + which + " the Service.", "Information");
                }
                else
                {
                    // Are you in Debug mode? if so, then make sure that in app.manifest, this is set:
                    // <requestedExecutionLevel level="highestAvailable" uiAccess="false" /> (asInvoker is not enough to use the Save functions)
                    MessageBox.Show("Failed to save as active configuration.", "Error");
                }
            }
            else
            {
                MessageBox.Show("Configuration not saved: Empty file", "Warning");
            }
        }

        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //if (e.OriginalSource is StackPanel)
            //{
            //}

        }

        private void Rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
