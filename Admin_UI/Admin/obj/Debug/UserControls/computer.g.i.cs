﻿#pragma checksum "..\..\..\UserControls\computer.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "1CDE786017F6839A17EFBF151E7D52C1"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Admin.UserControls {
    
    
    /// <summary>
    /// computer
    /// </summary>
    public partial class computer : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 10 "..\..\..\UserControls\computer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.TranslateTransform UserControlToolTipXY;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\..\UserControls\computer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas border_Canvas;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\UserControls\computer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Rectangle grid_Left;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\UserControls\computer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Rectangle grid_Top;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\..\UserControls\computer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Rectangle grid_Right;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\UserControls\computer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Shapes.Rectangle grid_bottom;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\UserControls\computer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas main_Canvas;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\UserControls\computer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ProgressBar progressBar;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Admin;component/usercontrols/computer.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\UserControls\computer.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.UserControlToolTipXY = ((System.Windows.Media.TranslateTransform)(target));
            return;
            case 2:
            this.border_Canvas = ((System.Windows.Controls.Canvas)(target));
            return;
            case 3:
            this.grid_Left = ((System.Windows.Shapes.Rectangle)(target));
            return;
            case 4:
            this.grid_Top = ((System.Windows.Shapes.Rectangle)(target));
            return;
            case 5:
            this.grid_Right = ((System.Windows.Shapes.Rectangle)(target));
            return;
            case 6:
            this.grid_bottom = ((System.Windows.Shapes.Rectangle)(target));
            return;
            case 7:
            this.main_Canvas = ((System.Windows.Controls.Canvas)(target));
            
            #line 21 "..\..\..\UserControls\computer.xaml"
            this.main_Canvas.MouseMove += new System.Windows.Input.MouseEventHandler(this.main_Canvas_MouseMove);
            
            #line default
            #line hidden
            
            #line 21 "..\..\..\UserControls\computer.xaml"
            this.main_Canvas.MouseLeave += new System.Windows.Input.MouseEventHandler(this.main_Canvas_MouseLeave);
            
            #line default
            #line hidden
            return;
            case 8:
            this.progressBar = ((System.Windows.Controls.ProgressBar)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

