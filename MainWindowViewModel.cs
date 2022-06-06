﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using NetworkModel;
using System.Windows;
using System.Diagnostics;

namespace SG_Administrator
{
    /// <summary>
    /// The view-model for the main window.
    /// </summary>
    public class MainWindowViewModel : AbstractModelBase
    {
        #region Internal Data Members

        /// <summary>
        /// This is the network that is displayed in the window.
        /// It is the main part of the view-model.
        /// </summary>
        public NetworkViewModel network = null;

        ///
        /// The current scale at which the content is being viewed.
        /// 
        private double contentScale = 1;

        ///
        /// The X coordinate of the offset of the viewport onto the content (in content coordinates).
        /// 
        private double contentOffsetX = 0;

        ///
        /// The Y coordinate of the offset of the viewport onto the content (in content coordinates).
        /// 
        private double contentOffsetY = 0;

        ///
        /// The width of the content (in content coordinates).
        /// 
        private double contentWidth = 1000;

        ///
        /// The heigth of the content (in content coordinates).
        /// 
        private double contentHeight = 1000;

        ///
        /// The width of the viewport onto the content (in content coordinates).
        /// The value for this is actually computed by the main window's ZoomAndPanControl and update in the
        /// view-model so that the value can be shared with the overview window.
        /// 
        private double contentViewportWidth = 0;

        ///
        /// The height of the viewport onto the content (in content coordinates).
        /// The value for this is actually computed by the main window's ZoomAndPanControl and update in the
        /// view-model so that the value can be shared with the overview window.
        /// 
        private double contentViewportHeight = 0;

        #endregion Internal Data Members

        public MainWindowViewModel()
        {
            // Add some test data to the view-model.
           // PopulateWithTestData();
            this.Network = new NetworkViewModel();
        }

        /// <summary>
        /// This is the network that is displayed in the window.
        /// It is the main part of the view-model.
        /// </summary>
        public NetworkViewModel Network
        {
            get
            {
                return network;
            }
            set
            {
                network = value;

                OnPropertyChanged("Network");
            }
        }

        ///
        /// The current scale at which the content is being viewed.
        /// 
        public double ContentScale
        {
            get
            {
                return contentScale;
            }
            set
            {
                contentScale = value;

                OnPropertyChanged("ContentScale");
            }
        }

        ///
        /// The X coordinate of the offset of the viewport onto the content (in content coordinates).
        /// 
        public double ContentOffsetX
        {
            get
            {
                return contentOffsetX;
            }
            set
            {
                contentOffsetX = value;

                OnPropertyChanged("ContentOffsetX");
            }
        }

        ///
        /// The Y coordinate of the offset of the viewport onto the content (in content coordinates).
        /// 
        public double ContentOffsetY
        {
            get
            {
                return contentOffsetY;
            }
            set
            {
                contentOffsetY = value;

                OnPropertyChanged("ContentOffsetY");
            }
        }

        ///
        /// The width of the content (in content coordinates).
        /// 
        public double ContentWidth
        {
            get
            {
                return contentWidth;
            }
            set
            {
                contentWidth = value;

                OnPropertyChanged("ContentWidth");
            }
        }

        ///
        /// The heigth of the content (in content coordinates).
        /// 
        public double ContentHeight
        {
            get
            {
                return contentHeight;
            }
            set
            {
                contentHeight = value;

                OnPropertyChanged("ContentHeight");
            }
        }

        ///
        /// The width of the viewport onto the content (in content coordinates).
        /// The value for this is actually computed by the main window's ZoomAndPanControl and update in the
        /// view-model so that the value can be shared with the overview window.
        /// 
        public double ContentViewportWidth
        {
            get
            {
                return contentViewportWidth;
            }
            set
            {
                contentViewportWidth = value;

                OnPropertyChanged("ContentViewportWidth");
            }
        }

        ///
        /// The heigth of the viewport onto the content (in content coordinates).
        /// The value for this is actually computed by the main window's ZoomAndPanControl and update in the
        /// view-model so that the value can be shared with the overview window.
        /// 
        public double ContentViewportHeight
        {
            get
            {
                return contentViewportHeight;
            }
            set
            {
                contentViewportHeight = value;

                OnPropertyChanged("ContentViewportHeight");
            }
        }

        /// <summary>
        /// Called when the user has started to drag out a connector, thus creating a new connection.
        /// </summary>
        public ConnectionViewModel ConnectionDragStarted(ConnectorViewModel draggedOutConnector, Point curDragPoint)
        {
            //
            // Create a new connection to add to the view-model.
            //
            var connection = new ConnectionViewModel();

            if (draggedOutConnector.Type == ConnectorType.Output)
            {
                //
                // The user is dragging out a source connector (an output) and will connect it to a destination connector (an input).
                //
                connection.SourceConnector = draggedOutConnector;
                connection.DestConnectorHotspot = curDragPoint;
            }
            else
            {
                //
                // The user is dragging out a destination connector (an input) and will connect it to a source connector (an output).
                //
                connection.DestConnector = draggedOutConnector;
                connection.SourceConnectorHotspot = curDragPoint;
            }

            //
            // Add the new connection to the view-model.
            //
            this.Network.Connections.Add(connection);

            return connection;
        }

        /// <summary>
        /// Called to query the application for feedback while the user is dragging the connection.
        /// </summary>
        public void QueryConnnectionFeedback(ConnectorViewModel draggedOutConnector, ConnectorViewModel draggedOverConnector, out object feedbackIndicator, out bool connectionOk)
        {
            if (draggedOutConnector == draggedOverConnector)
            {
                //
                // Can't connect to self!
                // Provide feedback to indicate that this connection is not valid!
                //
                feedbackIndicator = new ConnectionBadIndicator();
                connectionOk = false;
            }
            else
            {
                var sourceConnector = draggedOutConnector;
                var destConnector = draggedOverConnector;

                //
                // Only allow connections from output connector to input connector (ie each
                // connector must have a different type).
                // Also only allocation from one node to another, never one node back to the same node.
                //
                connectionOk = sourceConnector.ParentNode != destConnector.ParentNode &&
                                 sourceConnector.Type != destConnector.Type;

                if (connectionOk)
                {
                    // 
                    // Yay, this is a valid connection!
                    // Provide feedback to indicate that this connection is ok!
                    //
                    feedbackIndicator = new ConnectionOkIndicator();
                }
                else
                {
                    //
                    // Connectors with the same connector type (eg input & input, or output & output)
                    // can't be connected.
                    // Only connectors with separate connector type (eg input & output).
                    // Provide feedback to indicate that this connection is not valid!
                    //
                    feedbackIndicator = new ConnectionBadIndicator();
                }
            }
        }

        /// <summary>
        /// Called as the user continues to drag the connection.
        /// </summary>
        public void ConnectionDragging(Point curDragPoint, ConnectionViewModel connection)
        {
            if (connection.DestConnector == null)
            {
                connection.DestConnectorHotspot = curDragPoint;
            }
            else
            {
                connection.SourceConnectorHotspot = curDragPoint;
            }
        }

        private bool is_connection_permitted(ConnectorViewModel connectordraggedout, ConnectorViewModel connectordraggedover)
        {
            bool bapply_constraints = false; // Currently not needed
            if (bapply_constraints)
            {
                if ((connectordraggedout.ParentNode.HomelyType == homely_type.e_cyclone_input_source) ||
                    (connectordraggedout.ParentNode.HomelyType == homely_type.e_switcher) ||
                    (connectordraggedout.ParentNode.HomelyType == homely_type.e_abswitcher) ||
                    (connectordraggedout.ParentNode.HomelyType == homely_type.e_aggregator))
                {
                    if (connectordraggedout.IsConnected)
                    {
                        string name = connectordraggedout.ParentNode.Name + "." + connectordraggedout.Name;
                        MessageBox.Show("Connecting from this XAP output '" + name + "' connector more than once is not possible. Subtitle Gateway Service does not support it.", "Connector");
                        return false; // We cannot connect FROM these XAP outputs more than once. Service doesn't support it.
                    }
                }

                if (connectordraggedover.ParentNode.HomelyType == homely_type.e_splitter)
                {
                    if (connectordraggedover.IsConnected)
                    {
                        string name = connectordraggedover.ParentNode.Name + "." + connectordraggedover.Name;
                        MessageBox.Show("Connecting to this XAP input '" + name + "' connector more than once is not possible. Subtitle Gateway Service does not support it.", "Connector");
                        return false; // We cannot connect TO these XAP inputs more than once. Service doesn't support it.
                    }
                }

                return true;
            }

            return true;
        }


        /// <summary>
        /// Called when the user has finished dragging out the new connection.
        /// </summary>
        public bool ConnectionDragCompleted(ConnectionViewModel newConnection, ConnectorViewModel connectorDraggedOut, ConnectorViewModel connectorDraggedOver)
        {
            if (connectorDraggedOver == null)
            {
                //
                // The connection was unsuccessful.
                // Maybe the user dragged it out and dropped it in empty space.
                //
                this.Network.Connections.Remove(newConnection);
                return false;
            }

            //
            // Only allow connections from output connector to input connector (ie each
            // connector must have a different type).
            // Also only allocation from one node to another, never one node back to the same node.
            //
            bool connectionOk = connectorDraggedOut.ParentNode != connectorDraggedOver.ParentNode &&
                                connectorDraggedOut.Type != connectorDraggedOver.Type;

            if (!connectionOk)
            {
                //
                // Connections between connectors that have the same type,
                // eg input -> input or output -> output, are not allowed,
                // Remove the connection.
                //
                this.Network.Connections.Remove(newConnection);
                return false;
            }

            //
            // The user has dragged the connection on top of another valid connector.
            //
            if (is_connection_permitted(connectorDraggedOut, connectorDraggedOver))
            {
                //
                // Remove any existing connection between the same two connectors.
                //
                var existingConnection = FindConnection(connectorDraggedOut, connectorDraggedOver);
                if (existingConnection != null)
                {
                    this.Network.Connections.Remove(existingConnection);
                }

                //
                // Finalize the connection by attaching it to the connector
                // that the user dragged the mouse over.
                //
                if (newConnection.DestConnector == null)
                {
                    newConnection.DestConnector = connectorDraggedOver;
                }
                else
                {
                    newConnection.SourceConnector = connectorDraggedOver;
                }

                return true;
            }
            else
            {
                this.Network.Connections.Remove(newConnection);
            }

            return false;
        }

        /// <summary>
        /// Retrieve a connection between the two connectors.
        /// Returns null if there is no connection between the connectors.
        /// </summary>
        public ConnectionViewModel FindConnection(ConnectorViewModel connector1, ConnectorViewModel connector2)
        {
            Trace.Assert(connector1.Type != connector2.Type);

            //
            // Figure out which one is the source connector and which one is the
            // destination connector based on their connector types.
            //
            var sourceConnector = connector1.Type == ConnectorType.Output ? connector1 : connector2;
            var destConnector = connector1.Type == ConnectorType.Output ? connector2 : connector1;

            //
            // Now we can just iterate attached connections of the source
            // and see if it each one is attached to the destination connector.
            //

            foreach (var connection in sourceConnector.AttachedConnections)
            {
                if (connection.DestConnector == destConnector)
                {
                    //
                    // Found a connection that is outgoing from the source connector
                    // and incoming to the destination connector.
                    //
                    return connection;
                }
            }

            return null;
        }

        /// <summary>
        /// Delete the currently selected nodes from the view-model.
        /// </summary>
        public void DeleteSelectedNodes()
        {
            // Take a copy of the selected nodes list so we can delete nodes while iterating.
            var nodesCopy = this.Network.Nodes.ToArray();
            foreach (var node in nodesCopy)
            {
                if (node.IsSelected)
                {
                    DeleteNode(node);
                }
            }
        }

        /// <summary>
        /// Delete the node from the view-model.
        /// Also deletes any connections to or from the node.
        /// </summary>
        public void DeleteNode(NodeViewModel node)
        {
            //
            // Remove all connections attached to the node.
            //
            this.Network.Connections.RemoveRange(node.AttachedConnections);

            //
            // Remove the node from the network.
            //
            this.Network.Nodes.Remove(node);
        }

        /// <summary>
        /// Create a node and add it to the view-model.
        /// </summary>
        public NodeViewModel CreateNode(string name, homely_type type, Point nodeLocation, int numinputs, int numoutputs, bool centerNode)
        {
            NodeViewModel node = null;

            switch (type)
            {
                case homely_type.e_aggregator:
                    node = new AggregatorDetailsViewModel(name);
                    break;

                case homely_type.e_cyclone_input_source:
                    node = new CycloneDetailsViewModel(name);
                    //DependencyObject rect; 
                    //node.FindChild<Rectangle>(rect);
                    break;

                case homely_type.e_net_nw4_source:
                    node = new NetNW4DetailsViewModel(name);
                    break;

                case homely_type.e_router:
                    node = new RouterDetailsViewModel(name);
                    break;

                case homely_type.e_splitter:
                    node = new SplitterDetailsViewModel(name);
                    break;

                case homely_type.e_switcher:
                    node = new SwitcherDetailsViewModel(name);
                    break;

                case homely_type.e_abswitcher:
                    node = new ABSwitcherDetailsViewModel(name);
                    break;

                case homely_type.e_xap_destination:
                    node = new XAPDestinationDetailsViewModel(name);
                    break;

                case homely_type.e_cyclone_destination:
                    node = new CycloneDestinationDetailsViewModel(name);
                    break;

                case homely_type.e_smart3_destination:
                    node = new Smart3DestinationDetailsViewModel(name);
                    break;

                case homely_type.e_xap_source:
                    node = new XAPSourceDetailsViewModel(name);
                    break;

                case homely_type.e_selftest_source:
                    node = new SelfTestSourceDetailsViewModel(name);
                    break;

                case homely_type.e_smart3_source:
                    node = new Smart3SourceDetailsViewModel(name);
                    break;
            }
                
            node.X = nodeLocation.X;
            node.Y = nodeLocation.Y;

            for (int i = 0; i < numinputs; ++i)
            {
                int n = i + 1;
                node.InputConnectors.Add(new ConnectorViewModel("In" + n.ToString()));
            }

            for (int o = 0; o < numoutputs; ++o)
            {
                int n = o + 1;
                node.OutputConnectors.Add(new ConnectorViewModel("Out" + n.ToString()));
            }

            if (centerNode)
            {
                // 
                // We want to center the node.
                //
                // For this to happen we need to wait until the UI has determined the 
                // size based on the node's data-template.
                //
                // So we define an anonymous method to handle the SizeChanged event for a node.
                //
                // Note: If you don't declare sizeChangedEventHandler before initializing it you will get
                //       an error when you try and unsubscribe the event from within the event handler.
                //
                EventHandler<EventArgs> sizeChangedEventHandler = null;
                sizeChangedEventHandler =
                    delegate(object sender, EventArgs e)
                    {
                        //
                        // This event handler will be called after the size of the node has been determined.
                        // So we can now use the size of the node to modify its position.
                        //
                        node.X -= node.Size.Width / 2;
                        node.Y -= node.Size.Height / 2;

                        //
                        // Don't forget to unhook the event, after the initial centering of the node
                        // we don't need to be notified again of any size changes.
                        //
                        node.SizeChanged -= sizeChangedEventHandler;
                    };

                //
                // Now we hook the SizeChanged event so the anonymous method is called later
                // when the size of the node has actually been determined.
                //
                node.SizeChanged += sizeChangedEventHandler;
            }
            
            //
            // Add the node to the view-model.
            //
            this.Network.Nodes.Add(node);

            return node;
        }

        /// <summary>
        /// Utility method to delete a connection from the view-model.
        /// </summary>
        public void DeleteConnection(ConnectionViewModel connection)
        {
            this.Network.Connections.Remove(connection);
        }

        /// <summary>
        /// A function to conveniently populate the view-model with test data.
        /// </summary>
      /*  public void PopulateWithTestData()
        {
            //
            // Create a network, the root of the view-model.
            //            

            //
            // Create some nodes and add them to the view-model.
            //
            NodeViewModel node1 = CreateNode("Node1", new Point(100, 60), false);
            NodeViewModel node2 = CreateNode("Node2", new Point(350, 80), false);

            //
            // Create a connection between the nodes.
            //
            ConnectionViewModel connection = new ConnectionViewModel();
            connection.SourceConnector = node1.OutputConnectors[0];
            connection.DestConnector = node2.InputConnectors[0];

            //
            // Add the connection to the view-model.
            //
            this.Network.Connections.Add(connection);
        }*/

        public bool has_selected_node()
        {
            // Take a copy of the selected nodes list so we can delete nodes while iterating.
            var nodesCopy = this.Network.Nodes.ToArray();
            foreach (var node in nodesCopy)
            {
                if (node.IsSelected)
                {
                    return true;
                }
            }
            return false;
        }

        public NodeViewModel get_first_selected_node()
        {
            var nodesCopy = this.Network.Nodes.ToArray();
            foreach (var node in nodesCopy)
            {
                if (node.IsSelected)
                {
                    return node;
                }
            }
            return null;
        }

        public NodeViewModel get_node_by_name(string s)
        {
            if ((s == null) || (s == ""))
                return null;

            string nodename = s.Substring(0, s.LastIndexOf('.'));
            if ((nodename != null) && (nodename != ""))
            {
                var nodes = this.Network.Nodes.ToArray();
                foreach (var node in nodes)
                {
                    if (node.Name == nodename)
                        return node;
                }
            }
            return null;
        }
    }
}