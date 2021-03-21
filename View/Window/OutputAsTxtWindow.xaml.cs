﻿using MultiBranchTexter.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace MultiBranchTexter.View
{
    /// <summary>
    /// OutputAsTxtWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OutputAsTxtWindow : OutputWindowBase
    {
        public OutputAsTxtWindow()
        {
            InitializeComponent();
            this.DataContext = ViewModelFactory.Output;
        }

        protected override void Output()
        {
            Debug.WriteLine("导出成功了");
        }
    }
}
