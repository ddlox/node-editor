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
using System.Xml.Linq;
using System.Windows.Media.Animation;
using System.Windows.Markup;
using NetworkUI;
using NetworkModel;
using System.ComponentModel;

// Sort of based on this: http://www.codeproject.com/Articles/82464/How-to-Embed-Arbitrary-Content-in-a-WPF-Control

namespace SG_Administrator
{
    /// <summary>
    /// Interaction logic for Details.xaml
    /// </summary>   
    public partial class Details : UserControl, INotifyPropertyChanged
    {
        // we could use DependencyProperties as well to inform others of property changes
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        
        //private DesignerItem m_currentitem = null;
        private NodeViewModel m_currentitem = null;
        bool m_editdetails_expanded = false;

        public Details()
        {
            InitializeComponent();

            cycloneinputsrcdetails.Height = 0;
            switcherdetails.Height = 0;
            abswitcherdetails.Height = 0;
            xapdestinationdetails.Height = 0;
            xapsourcedetails.Height = 0;
            selftestsrcdetails.Height = 0;
            netnw4srcdetails.Height = 0;
            cyclonedestinationdetails.Height = 0;
            smart3destinationdetails.Height = 0;
            smart3inputsourcedetails.Height = 0;
        }

        private void collapse_currentitem()
        {
            if (m_editdetails_expanded && (m_currentitem != null))
            {
                if (m_currentitem.HomelyType == homely_type.e_cyclone_input_source)
                {
                    Storyboard storyBoard = (Storyboard)this.Resources["collapse_cycloneinputsrcedit"];
                    storyBoard.Begin();
                }
                else if (m_currentitem.HomelyType == homely_type.e_net_nw4_source)
                {
                    Storyboard storyBoard = (Storyboard)this.Resources["collapse_netnw4srcedit"];
                    storyBoard.Begin();
                }
                else if (m_currentitem.HomelyType == homely_type.e_switcher)
                {
                    Storyboard storyBoard = (Storyboard)this.Resources["collapse_switcheredit"];
                    storyBoard.Begin();
                }
                else if (m_currentitem.HomelyType == homely_type.e_abswitcher)
                {
                    Storyboard storyBoard = (Storyboard)this.Resources["collapse_abswitcheredit"];
                    storyBoard.Begin();
                }
                else if (m_currentitem.HomelyType == homely_type.e_xap_destination)
                {
                    Storyboard storyBoard = (Storyboard)this.Resources["collapse_xapdestinationedit"];
                    storyBoard.Begin();
                }
                else if (m_currentitem.HomelyType == homely_type.e_xap_source)
                {
                    Storyboard storyBoard = (Storyboard)this.Resources["collapse_xapsourceedit"];
                    storyBoard.Begin();
                }
                else if (m_currentitem.HomelyType == homely_type.e_selftest_source)
                {
                    Storyboard storyBoard = (Storyboard)this.Resources["collapse_selftestsrcedit"];
                    storyBoard.Begin();
                }
                else if (m_currentitem.HomelyType == homely_type.e_cyclone_destination)
                {
                    Storyboard storyBoard = (Storyboard)this.Resources["collapse_cyclonedestinationedit"];
                    storyBoard.Begin();
                }
                else if (m_currentitem.HomelyType == homely_type.e_smart3_destination)
                {
                    Storyboard storyBoard = (Storyboard)this.Resources["collapse_smart3destinationedit"];
                    storyBoard.Begin();
                }
                else if (m_currentitem.HomelyType == homely_type.e_smart3_source)
                {
                    Storyboard storyBoard = (Storyboard)this.Resources["collapse_smart3sourceedit"];
                    storyBoard.Begin();
                }
            }
        }

        public void set_current_item(NodeViewModel node) // object sender, EventArgs e)
        {
            collapse_currentitem();

            //DesignerItemChangedEventArgs dicea = e as DesignerItemChangedEventArgs;
            m_currentitem = node;// dicea.designeritem;
            if (m_currentitem != null)
            {
                partnametextbox.Text = m_currentitem.Name;
                // If you need to find the UIElement in the template, this is how it's done:
                //TextBox ptttb = Template.FindName("parttypetextbox", this) as TextBox;
                //ptttb.Text = values[0];
                //If you need to parse the XAML string, this is how you do it:
                //XDocument doc = XDocument.Parse(m_currentitem.Xaml);
                //var values = (from element in doc.Elements() //.Descendants()
                //              where element.Attribute("ToolTip") != null
                //              select element.Attribute("ToolTip").Value).ToArray();
                //parttypetextbox.Text = values[0];

                if (m_currentitem.HomelyType == homely_type.e_cyclone_input_source)
                {
                    parttypetextbox.Text = "Cyclone Input Source";
                    CycloneDetailsViewModel node_cyclone = node as CycloneDetailsViewModel;
                    partnameserverport.Text = node_cyclone.ServerPort;
                    partnamesvcid.Text = node_cyclone.SvcID;

                    Storyboard storyBoard = (Storyboard)this.Resources["expand_cycloneinputsrcedit"];
                    storyBoard.Begin();
                }
                else if (m_currentitem.HomelyType == homely_type.e_net_nw4_source)
                {
                    parttypetextbox.Text = "Net. Newfor Source";
                    NetNW4DetailsViewModel node_nw4 = node as NetNW4DetailsViewModel;
                    partnamenetnw4_serverport.Text = node_nw4.ServerPort;
                    partnamenetnw4_svcid.Text = node_nw4.SvcID;
                    partnamenetnw4_sendack.IsChecked = node_nw4.SendAck;

                    Storyboard storyBoard = (Storyboard)this.Resources["expand_netnw4srcedit"];
                    storyBoard.Begin();
                }
                else if (m_currentitem.HomelyType == homely_type.e_selftest_source)
                {
                    parttypetextbox.Text = "Self-Test Source";
                    SelfTestSourceDetailsViewModel node_selftest = node as SelfTestSourceDetailsViewModel;
                    partnameselftest_cb_xaptype.Text = node_selftest.XAPType;
                    partnameselftest_cb_txtfgdcol.Text = node_selftest.TxtFgCol;
                    partnameselftest_rowtimeout.Text = node_selftest.RowTimeout;
                    partnameselftest_svcid.Text = node_selftest.SvcID;

                    Storyboard storyBoard = (Storyboard)this.Resources["expand_selftestsrcedit"];
                    storyBoard.Begin();
                }
                else if (m_currentitem.HomelyType == homely_type.e_switcher)
                {
                    parttypetextbox.Text = "XAP Switcher";
                    SwitcherDetailsViewModel node_switcher = node as SwitcherDetailsViewModel;
                    partdiscrepancytime.Text = node_switcher.DiscrepancyTime;

                    Storyboard storyBoard = (Storyboard)this.Resources["expand_switcheredit"];
                    storyBoard.Begin();
                }
                else if (m_currentitem.HomelyType == homely_type.e_abswitcher)
                {
                    parttypetextbox.Text = "XAP ABSwitcher";
                    ABSwitcherDetailsViewModel node_switcher = node as ABSwitcherDetailsViewModel;
                    partabdiscrepancytime.Text = node_switcher.DiscrepancyTime;
                    partabmasterreceivetimeout.Text = node_switcher.MasterReceiveTimeout;

                    Storyboard storyBoard = (Storyboard)this.Resources["expand_abswitcheredit"];
                    storyBoard.Begin();
                }
                else if (m_currentitem.HomelyType == homely_type.e_router)
                {
                    parttypetextbox.Text = "XAP Router";
                }
                else if (m_currentitem.HomelyType == homely_type.e_xap_destination)
                {
                    parttypetextbox.Text = "XAP Destination";
                    XAPDestinationDetailsViewModel node_xap_dst = node as XAPDestinationDetailsViewModel;
                    partname_xapdst_host.Text = node_xap_dst.Host;
                    partname_xapdst_port.Text = node_xap_dst.Port;
                    partname_xapdst_svcid.Text = node_xap_dst.SvcID;
                    partname_xapdst_host.IsEnabled = node_xap_dst.IsXAPClient;
                    partname_xapdst_client.IsCheckedExt = node_xap_dst.IsXAPClient;
                    partname_xapdst_server.IsCheckedExt = !node_xap_dst.IsXAPClient;

                    Storyboard storyBoard = (Storyboard)this.Resources["expand_xapdestinationedit"];
                    storyBoard.Begin();
                }
                else if (m_currentitem.HomelyType == homely_type.e_cyclone_destination)
                {
                    parttypetextbox.Text = "Cyclone Destination";
                    CycloneDestinationDetailsViewModel node_cyclone_dst = node as CycloneDestinationDetailsViewModel;
                    partname_cyclonedst_host.Text = node_cyclone_dst.Host;
                    partname_cyclonedst_port.Text = node_cyclone_dst.Port;
                    partname_cyclonedst_cb_lang.Text = node_cyclone_dst.DefaultLang;
                    partname_cyclonedst_discard_ids.Text = node_cyclone_dst.SvcID;

                    Storyboard storyBoard = (Storyboard)this.Resources["expand_cyclonedestinationedit"];
                    storyBoard.Begin();
                }
                else if (m_currentitem.HomelyType == homely_type.e_smart3_destination)
                {
                    parttypetextbox.Text = "Smart III Destination";
                    Smart3DestinationDetailsViewModel node_smart3_dst = node as Smart3DestinationDetailsViewModel;

                    partname_smart3dst_tcp_client.IsCheckedExt = node_smart3_dst.IsTCPClient;
                    partname_smart3dst_serial.IsCheckedExt = !node_smart3_dst.IsTCPClient;
                    partname_smart3dst_host.Text = node_smart3_dst.Host;
                    partname_smart3dst_tcp_port.Text = node_smart3_dst.TCPPort;
                    partname_smart3dst_serial_port.Text = node_smart3_dst.SerialPort;
                    partname_smart3dst_baud.Text = node_smart3_dst.BaudRate;
                    partname_smart3dst_parity.Text = node_smart3_dst.Parity;
                    partname_smart3dst_databits.Text = node_smart3_dst.DataBits;
                    partname_smart3dst_stopbits.Text = node_smart3_dst.StopBits;
                    partname_smart3dst_cb_default_mode.Text = node_smart3_dst.EncoderDefaultMode;
                    partname_smart3dst_real_time_rows.Text = node_smart3_dst.EncoderRealTimeRows;
                    partname_smart3dst_real_time_baserow.Text = node_smart3_dst.EncoderRealTimeBaseRow;
                    partname_smart3dst_user_defined_mode.Text = node_smart3_dst.EncoderUserDefinedMode;
					
					partname_smart3dst_host.IsEnabled = node_smart3_dst.IsTCPClient;
					partname_smart3dst_tcp_port.IsEnabled = node_smart3_dst.IsTCPClient;
					partname_smart3dst_serial_port.IsEnabled = !node_smart3_dst.IsTCPClient;
					partname_smart3dst_baud.IsEnabled = !node_smart3_dst.IsTCPClient;
					partname_smart3dst_parity.IsEnabled = !node_smart3_dst.IsTCPClient;
					partname_smart3dst_databits.IsEnabled = !node_smart3_dst.IsTCPClient;
					partname_smart3dst_stopbits.IsEnabled = !node_smart3_dst.IsTCPClient;
					
					partname_smart3dst_real_time_rows.IsEnabled = node_smart3_dst.EncoderDefaultMode=="realtime";
					partname_smart3dst_real_time_baserow.IsEnabled = node_smart3_dst.EncoderDefaultMode=="realtime";
					partname_smart3dst_user_defined_mode.IsEnabled = node_smart3_dst.EncoderDefaultMode=="userdefined";
                    partname_smart3dst_removetopbit_chkb.IsChecked = node_smart3_dst.RemoveTopBit;
					
                    Storyboard storyBoard = (Storyboard)this.Resources["expand_smart3destinationedit"];
                    storyBoard.Begin();
                }
                else if (m_currentitem.HomelyType == homely_type.e_splitter)
                {
                    parttypetextbox.Text = "XAP Splitter";
                }
                else if (m_currentitem.HomelyType == homely_type.e_aggregator)
                {
                    parttypetextbox.Text = "XAP Aggregator";
                }
                else if (m_currentitem.HomelyType == homely_type.e_xap_source)
                {
                    parttypetextbox.Text = "XAP Source";
                    XAPSourceDetailsViewModel node_xap_src = node as XAPSourceDetailsViewModel;
                    partname_xapsrc_host.Text = node_xap_src.Host;
                    partname_xapsrc_port.Text = node_xap_src.Port;
                    partname_xapsrc_svcid.Text = node_xap_src.SvcID;
                    partname_xapsrc_host.IsEnabled = node_xap_src.IsXAPClient;
                    partname_xapsrc_client.IsCheckedExt = node_xap_src.IsXAPClient;
                    partname_xapsrc_server.IsCheckedExt = !node_xap_src.IsXAPClient;
                    partname_xapsrc_enableendlive.IsChecked = node_xap_src.SendEndLiveOnDisconnect;

                    Storyboard storyBoard = (Storyboard)this.Resources["expand_xapsourceedit"];
                    storyBoard.Begin();
                }
                else if (m_currentitem.HomelyType == homely_type.e_smart3_source)
                {
                    parttypetextbox.Text = "Smart III Source";
                    Smart3SourceDetailsViewModel node_smart3_src = node as Smart3SourceDetailsViewModel;
                    partname_smart3src_serverport.Text = node_smart3_src.ServerPort;
                    partname_smart3src_svcid.Text = node_smart3_src.SvcID;
                    partname_smart3src_framerate_cb.Text = node_smart3_src.CDP_FrameRate;

                    Storyboard storyBoard = (Storyboard)this.Resources["expand_smart3sourceedit"];
                    storyBoard.Begin();
                }

            }
            else
            {                
                partnametextbox.Text = "";
                parttypetextbox.Text = "";
            }
        }

        private void partnametextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
                m_currentitem.Name = partnametextbox.Text;
            OnPropertyChanged("edit");
        }

        private void expanddetailsedit_storyboardcompleted(object sender, EventArgs e)
        {           
            partnametextbox.Focus();
            m_editdetails_expanded = true;
        }

        private void collapsedetailsedit_storyboardcompleted(object sender, EventArgs e)
        {
            m_editdetails_expanded = false;
        }

        public void clear_edits()
        {
            partnametextbox.Text = "";
            parttypetextbox.Text = "";
            collapse_currentitem();
            m_currentitem = null;
        }

        void my_tb_gotfocus(object sender, RoutedEventArgs args)
        {
            TextBox tb = (TextBox)sender;
            tb.Text = tb.Text.Trim();
        }

        void my_tb_lostfocus(object sender, RoutedEventArgs args)
        {
            TextBox tb = (TextBox)sender;
            tb.Text = tb.Text.Trim();
            if (tb.Text == "")
                tb.Text = "  "; // reset it
        }

        private void on_btn_click_addroute(object sender, RoutedEventArgs e)
        {
        }

        private void partnameserverport_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                CycloneDetailsViewModel c = m_currentitem as CycloneDetailsViewModel;
                c.ServerPort = partnameserverport.Text;
            }
            OnPropertyChanged("edit");
        }
        
        private void partnamesvcid_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                CycloneDetailsViewModel c = m_currentitem as CycloneDetailsViewModel;
                c.SvcID = partnamesvcid.Text;
            }
            OnPropertyChanged("edit");
        }
            
        private void partdiscrepancytime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                SwitcherDetailsViewModel c = m_currentitem as SwitcherDetailsViewModel;
                c.DiscrepancyTime = partdiscrepancytime.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_xapdst_svcid_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                XAPDestinationDetailsViewModel c = m_currentitem as XAPDestinationDetailsViewModel;
                c.SvcID = partname_xapdst_svcid.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_xapdst_host_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                XAPDestinationDetailsViewModel c = m_currentitem as XAPDestinationDetailsViewModel;
                c.Host = partname_xapdst_host.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_xapdst_port_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                XAPDestinationDetailsViewModel c = m_currentitem as XAPDestinationDetailsViewModel;
                c.Port = partname_xapdst_port.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_xapdst_server_Click(object sender, RoutedEventArgs e)
        {
            partname_xapdst_host.IsEnabled = false;
            if (m_currentitem != null)
            {
                XAPDestinationDetailsViewModel c = m_currentitem as XAPDestinationDetailsViewModel;
                c.IsXAPClient = false;
            }
            OnPropertyChanged("edit");
        }

        private void partname_xapdst_client_Click(object sender, RoutedEventArgs e)
        {
            partname_xapdst_host.IsEnabled = true;
            if (m_currentitem != null)
            {
                XAPDestinationDetailsViewModel c = m_currentitem as XAPDestinationDetailsViewModel;
                c.IsXAPClient = true;
            }
            OnPropertyChanged("edit");
        }

        private void partname_xapsrc_svcid_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                XAPSourceDetailsViewModel c = m_currentitem as XAPSourceDetailsViewModel;
                c.SvcID = partname_xapsrc_svcid.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_xapsrc_host_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                XAPSourceDetailsViewModel c = m_currentitem as XAPSourceDetailsViewModel;
                c.Host = partname_xapsrc_host.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_xapsrc_port_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                XAPSourceDetailsViewModel c = m_currentitem as XAPSourceDetailsViewModel;
                c.Port = partname_xapsrc_port.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_xapsrc_server_Click(object sender, RoutedEventArgs e)
        {
            partname_xapsrc_host.IsEnabled = false;
            if (m_currentitem != null)
            {
                XAPSourceDetailsViewModel c = m_currentitem as XAPSourceDetailsViewModel;
                c.IsXAPClient = false;
            }
            OnPropertyChanged("edit");
        }

        private void partname_xapsrc_client_Click(object sender, RoutedEventArgs e)
        {
            partname_xapsrc_host.IsEnabled = true;
            if (m_currentitem != null)
            {
                XAPSourceDetailsViewModel c = m_currentitem as XAPSourceDetailsViewModel;
                c.IsXAPClient = true;
            }
            OnPropertyChanged("edit");
        }

        private void partnameselftest_rowtimeout_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                SelfTestSourceDetailsViewModel c = m_currentitem as SelfTestSourceDetailsViewModel;
                c.RowTimeout = partnameselftest_rowtimeout.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partnameselftest_cb_txtfgdcol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                SelfTestSourceDetailsViewModel c = m_currentitem as SelfTestSourceDetailsViewModel;
                ComboBoxItem ci = partnameselftest_cb_txtfgdcol.SelectedItem as ComboBoxItem;
                c.TxtFgCol = ci.Content.ToString();
            }
            OnPropertyChanged("edit");
        }

        private void partnameselftest_svcid_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                SelfTestSourceDetailsViewModel c = m_currentitem as SelfTestSourceDetailsViewModel;
                c.SvcID = partnameselftest_svcid.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partnameselftest_cb_xaptype_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                SelfTestSourceDetailsViewModel c = m_currentitem as SelfTestSourceDetailsViewModel;
                ComboBoxItem ci = partnameselftest_cb_xaptype.SelectedItem as ComboBoxItem;
                c.XAPType = ci.Content.ToString();
            }
            OnPropertyChanged("edit");
        }

        private void partnamenetnw4_serverport_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                NetNW4DetailsViewModel c = m_currentitem as NetNW4DetailsViewModel;
                c.ServerPort = partnamenetnw4_serverport.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partnamenetnw4_svcid_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                NetNW4DetailsViewModel c = m_currentitem as NetNW4DetailsViewModel;
                c.SvcID = partnamenetnw4_svcid.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partnamenetnw4_sendack_Checked(object sender, RoutedEventArgs e)
        {
            if (m_currentitem != null)
            {
                NetNW4DetailsViewModel c = m_currentitem as NetNW4DetailsViewModel;
                c.SendAck = true;
            }
            OnPropertyChanged("edit");
        }

        private void partnamenetnw4_sendack_Unchecked(object sender, RoutedEventArgs e)
        {
            if (m_currentitem != null)
            {
                NetNW4DetailsViewModel c = m_currentitem as NetNW4DetailsViewModel;
                c.SendAck = false;
            }
            OnPropertyChanged("edit");
        }

        private void partname_cyclonedst_host_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                CycloneDestinationDetailsViewModel c = m_currentitem as CycloneDestinationDetailsViewModel;
                c.Host = partname_cyclonedst_host.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_cyclonedst_port_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                CycloneDestinationDetailsViewModel c = m_currentitem as CycloneDestinationDetailsViewModel;
                c.Port = partname_cyclonedst_port.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_cyclonedst_cb_lang_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                CycloneDestinationDetailsViewModel c = m_currentitem as CycloneDestinationDetailsViewModel;
                ComboBoxItem ci = partname_cyclonedst_cb_lang.SelectedItem as ComboBoxItem;
                c.DefaultLang = ci.Content.ToString();
            }
            OnPropertyChanged("edit");
        }

        private void partname_cyclonedst_discard_ids_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                CycloneDestinationDetailsViewModel c = m_currentitem as CycloneDestinationDetailsViewModel;
                c.SvcID = partname_cyclonedst_discard_ids.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_smart3dst_tcpclient_Click(object sender, RoutedEventArgs e)
        {
            partname_smart3dst_host.IsEnabled = true;
            partname_smart3dst_tcp_port.IsEnabled = true;
            partname_smart3dst_serial_port.IsEnabled = false;
            partname_smart3dst_baud.IsEnabled = false;
            partname_smart3dst_parity.IsEnabled = false;
            partname_smart3dst_databits.IsEnabled = false;
            partname_smart3dst_stopbits.IsEnabled = false;
            if (m_currentitem != null)
            {
                Smart3DestinationDetailsViewModel c = m_currentitem as Smart3DestinationDetailsViewModel;
                c.IsTCPClient = true;
            }
            OnPropertyChanged("edit");
        }

        private void partname_smart3dst_serial_Click(object sender, RoutedEventArgs e)
        {
            partname_smart3dst_host.IsEnabled = false;
            partname_smart3dst_tcp_port.IsEnabled = false;
            partname_smart3dst_serial_port.IsEnabled = true;
            partname_smart3dst_baud.IsEnabled = true;
            partname_smart3dst_parity.IsEnabled = true;
            partname_smart3dst_databits.IsEnabled = true;
            partname_smart3dst_stopbits.IsEnabled = true;
            if (m_currentitem != null)
            {
                Smart3DestinationDetailsViewModel c = m_currentitem as Smart3DestinationDetailsViewModel;
                c.IsTCPClient = false;
            }
            OnPropertyChanged("edit");
        }

        private void partname_smart3dst_host_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                Smart3DestinationDetailsViewModel c = m_currentitem as Smart3DestinationDetailsViewModel;
                c.Host = partname_smart3dst_host.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_smart3dst_tcp_port_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                Smart3DestinationDetailsViewModel c = m_currentitem as Smart3DestinationDetailsViewModel;
                c.TCPPort = partname_smart3dst_tcp_port.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_smart3dst_serial_port_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                Smart3DestinationDetailsViewModel c = m_currentitem as Smart3DestinationDetailsViewModel;
                c.SerialPort = partname_smart3dst_serial_port.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_smart3dst_baud_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                Smart3DestinationDetailsViewModel c = m_currentitem as Smart3DestinationDetailsViewModel;
                c.BaudRate = partname_smart3dst_baud.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_smart3dst_parity_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                Smart3DestinationDetailsViewModel c = m_currentitem as Smart3DestinationDetailsViewModel;
                c.Parity = partname_smart3dst_parity.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_smart3dst_databits_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                Smart3DestinationDetailsViewModel c = m_currentitem as Smart3DestinationDetailsViewModel;
                c.DataBits = partname_smart3dst_databits.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_smart3dst_stopbits_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                Smart3DestinationDetailsViewModel c = m_currentitem as Smart3DestinationDetailsViewModel;
                c.StopBits = partname_smart3dst_stopbits.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_smart3dst_cb_default_mode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                Smart3DestinationDetailsViewModel c = m_currentitem as Smart3DestinationDetailsViewModel;
                ComboBoxItem ci = partname_smart3dst_cb_default_mode.SelectedItem as ComboBoxItem;
                c.EncoderDefaultMode = ci.Content.ToString();
				partname_smart3dst_real_time_rows.IsEnabled = c.EncoderDefaultMode=="realtime";
				partname_smart3dst_real_time_baserow.IsEnabled = c.EncoderDefaultMode=="realtime";
				partname_smart3dst_user_defined_mode.IsEnabled = c.EncoderDefaultMode=="userdefined";
            }
            OnPropertyChanged("edit");
        }

        private void partname_smart3dst_real_time_rows_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                Smart3DestinationDetailsViewModel c = m_currentitem as Smart3DestinationDetailsViewModel;
                c.EncoderRealTimeRows = partname_smart3dst_real_time_rows.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_smart3dst_real_time_baserow_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                Smart3DestinationDetailsViewModel c = m_currentitem as Smart3DestinationDetailsViewModel;
                c.EncoderRealTimeBaseRow = partname_smart3dst_real_time_baserow.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_smart3dst_user_defined_mode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                Smart3DestinationDetailsViewModel c = m_currentitem as Smart3DestinationDetailsViewModel;
                c.EncoderUserDefinedMode = partname_smart3dst_user_defined_mode.Text;
            }
            OnPropertyChanged("edit");
        }

        private void abpartdiscrepancytime_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                ABSwitcherDetailsViewModel c = m_currentitem as ABSwitcherDetailsViewModel;
                c.DiscrepancyTime = partabdiscrepancytime.Text;
            }
            OnPropertyChanged("edit");
        }

        private void abpartmasterreceivetimeout_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                ABSwitcherDetailsViewModel c = m_currentitem as ABSwitcherDetailsViewModel;
                c.MasterReceiveTimeout = partabmasterreceivetimeout.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_xapsrc_Checked(object sender, RoutedEventArgs e)
        {
            if (m_currentitem != null)
            {
                XAPSourceDetailsViewModel c = m_currentitem as XAPSourceDetailsViewModel;
                c.SendEndLiveOnDisconnect = true;
            }
            OnPropertyChanged("edit");
        }

        private void partname_xapsrc_Unchecked(object sender, RoutedEventArgs e)
        {
            if (m_currentitem != null)
            {
                XAPSourceDetailsViewModel c = m_currentitem as XAPSourceDetailsViewModel;
                c.SendEndLiveOnDisconnect = false;
            }
            OnPropertyChanged("edit");
        }

        private void partname_smart3dst_removetopbit_chkb_handlecheck(object sender, RoutedEventArgs e)
        {
            if (m_currentitem != null)
            {
                Smart3DestinationDetailsViewModel c = m_currentitem as Smart3DestinationDetailsViewModel;
                c.RemoveTopBit = true;
            }
            OnPropertyChanged("edit");

        }

        private void partname_smart3dst_removetopbit_chkb_handleunchecked(object sender, RoutedEventArgs e)
        {
            if (m_currentitem != null)
            {
                Smart3DestinationDetailsViewModel c = m_currentitem as Smart3DestinationDetailsViewModel;
                c.RemoveTopBit = false;
            }
            OnPropertyChanged("edit");
        }

        private void partname_smart3src_serverport_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                Smart3SourceDetailsViewModel c = m_currentitem as Smart3SourceDetailsViewModel;
                c.ServerPort = partname_smart3src_serverport.Text;
            }
            OnPropertyChanged("edit");
        }

        private void partname_smart3src_framerate_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                Smart3SourceDetailsViewModel c = m_currentitem as Smart3SourceDetailsViewModel;
                ComboBoxItem ci = partname_smart3src_framerate_cb.SelectedItem as ComboBoxItem;
                c.CDP_FrameRate = ci.Content.ToString();
            }
            OnPropertyChanged("edit");
        }

        private void partname_smart3src_svcid_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (m_currentitem != null)
            {
                Smart3SourceDetailsViewModel c = m_currentitem as Smart3SourceDetailsViewModel;
                c.SvcID = partname_smart3src_svcid.Text;
            }
            OnPropertyChanged("edit");
        }
    }
}
