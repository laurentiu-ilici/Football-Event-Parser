﻿#pragma checksum "..\..\..\MenuWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "525880FCF77872670E92430D8A5B8FE2"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
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


namespace FootballManagerUI {
    
    
    /// <summary>
    /// MenuWindow
    /// </summary>
    public partial class MenuWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 6 "..\..\..\MenuWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button buildImages;
        
        #line default
        #line hidden
        
        
        #line 7 "..\..\..\MenuWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button viewPCABtn;
        
        #line default
        #line hidden
        
        
        #line 8 "..\..\..\MenuWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button kernelBtn;
        
        #line default
        #line hidden
        
        
        #line 9 "..\..\..\MenuWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button clusterBtn;
        
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
            System.Uri resourceLocater = new System.Uri("/FootballManagerUI;component/menuwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\MenuWindow.xaml"
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
            this.buildImages = ((System.Windows.Controls.Button)(target));
            
            #line 6 "..\..\..\MenuWindow.xaml"
            this.buildImages.Click += new System.Windows.RoutedEventHandler(this.buildImages_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.viewPCABtn = ((System.Windows.Controls.Button)(target));
            
            #line 7 "..\..\..\MenuWindow.xaml"
            this.viewPCABtn.Click += new System.Windows.RoutedEventHandler(this.viewPCABtn_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.kernelBtn = ((System.Windows.Controls.Button)(target));
            
            #line 8 "..\..\..\MenuWindow.xaml"
            this.kernelBtn.Click += new System.Windows.RoutedEventHandler(this.kernelBtn_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.clusterBtn = ((System.Windows.Controls.Button)(target));
            
            #line 9 "..\..\..\MenuWindow.xaml"
            this.clusterBtn.Click += new System.Windows.RoutedEventHandler(this.clusterBtn_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

