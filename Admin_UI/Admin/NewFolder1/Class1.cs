﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Admin.Folder
{
    public class CloseTabItemAction : TriggerAction<DependencyObject>
    {
        protected override void Invoke(object parameter)
        {
            if (Admin.UserControls.main_Control.close_Tab(this.TabItem) == false)
                return;
            this.TabControl.Items.Remove(this.TabItem);
        }

        public static readonly DependencyProperty TabControlProperty =
            DependencyProperty.Register("TabControl", typeof(TabControl), typeof(CloseTabItemAction), new PropertyMetadata(default(TabControl)));

        public TabControl TabControl
        {
            get { return (TabControl)GetValue(TabControlProperty); }
            set { SetValue(TabControlProperty, value); }
        }

        public static readonly DependencyProperty TabItemProperty =
            DependencyProperty.Register("TabItem", typeof(TabItem), typeof(CloseTabItemAction), new PropertyMetadata(default(TabItem)));

        public TabItem TabItem
        {
            get { return (TabItem)GetValue(TabItemProperty); }
            set { SetValue(TabItemProperty, value); }
        }
    }
}
