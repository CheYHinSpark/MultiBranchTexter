using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MultiBranchTexter.Controls
{
    /// <summary>
    /// 为了使用下面的依赖属性，只能多套一层
    /// </summary>
    public class FlowChartBase : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static DependencyProperty SelectedNodesProperty =
            DependencyProperty.Register("SelectedNodes", //属性名称
                typeof(List<NodeButton>), //属性类型
                typeof(FlowChartBase), //该属性所有者，即将该属性注册到那个类上
                new PropertyMetadata(new List<NodeButton>())//属性默认值
                );

        public List<NodeButton> SelectedNodes
        {
            get { return (List<NodeButton>)GetValue(SelectedNodesProperty); }
            set 
            { 
                SetValue(SelectedNodesProperty, value);
                if (this.PropertyChanged != null)
                { this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("SelectedNodes")); }
            }
        }
    }
}
