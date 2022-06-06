using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SG_Administrator
{
    public class RadioButtonExtended : RadioButton
    {
        public static readonly DependencyProperty IsCheckedExtProperty =
            DependencyProperty.Register("IsCheckedExt", typeof(bool?), typeof(RadioButtonExtended),
                                        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, IsCheckedRealChanged));

        private static bool _isChanging;

        public RadioButtonExtended()
        {
            Checked += RadioButtonExtendedChecked;
            Unchecked += RadioButtonExtendedUnchecked;
        }

        public bool? IsCheckedExt
        {
            get { return (bool?)GetValue(IsCheckedExtProperty); }
            set { SetValue(IsCheckedExtProperty, value); }
        }

        public static void IsCheckedRealChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            _isChanging = true;
            ((RadioButtonExtended)d).IsChecked = (bool)e.NewValue;
            _isChanging = false;
        }

        private void RadioButtonExtendedChecked(object sender, RoutedEventArgs e)
        {
            if (!_isChanging)
                IsCheckedExt = true;
        }

        private void RadioButtonExtendedUnchecked(object sender, RoutedEventArgs e)
        {
            if (!_isChanging)
                IsCheckedExt = false;
        }
    }
}
