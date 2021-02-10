using MultiBranchTexter.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiBranchTexter.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region 左右显示相关
        private bool? _isFlowChartShowing;
        public bool? IsFlowChartShowing
        {
            get { return _isFlowChartShowing; }
            set 
            { 
                _isFlowChartShowing = value;
                if (value == true)
                { 
                    FlowChartWidth = "*"; 
                    CanHideWorkTab = true;
                }
                else
                {
                    FlowChartWidth = "0";
                    CanHideWorkTab = false;
                    CanHideFlowChart = true;
                }
                RaisePropertyChanged("IsFlowChartShowing"); }
        }
        private bool? _isWorkTabShowing;
        public bool? IsWorkTabShowing
        {
            get { return _isWorkTabShowing; }
            set 
            {
                _isWorkTabShowing = value;
                if (value == true)
                { 
                    WorkTabWidth = "*";
                    CanHideFlowChart = true;
                }
                else
                {
                    WorkTabWidth = "0";
                    CanHideFlowChart = false;
                    CanHideWorkTab = true;
                }
                RaisePropertyChanged("IsWorkTabShowing");
            }
        }
        private bool _canHideFlowChart;
        public bool CanHideFlowChart
        {
            get { return _canHideFlowChart; }
            set { _canHideFlowChart = value; RaisePropertyChanged("CanHideFlowChart"); }
        }
        private bool _canHideWorkTab;
        public bool CanHideWorkTab
        {
            get { return _canHideWorkTab; }
            set { _canHideWorkTab = value; RaisePropertyChanged("CanHideWorkTab"); }
        }

        private string _flowChartWidth;
        public string FlowChartWidth
        {
            get { return _flowChartWidth; }
            set { _flowChartWidth = value; RaisePropertyChanged("FlowChartWidth"); }
        }

        private string _workTabWidth;
        public string WorkTabWidth
        {
            get { return _workTabWidth; }
            set { _workTabWidth = value; RaisePropertyChanged("WorkTabWidth"); }
        }
        #endregion

        #region workTab相关
        private int _textFontSize;
        public int TextFontSize
        {
            get { return _textFontSize; }
            set
            {
                _textFontSize = value;
                if (_textFontSize < 6)
                { _textFontSize = 6; }
                else if (_textFontSize > 36)
                { _textFontSize = 36; }
                RaisePropertyChanged("TextFontSize");
            }
        }

        private ObservableCollection<MBTabItem> _workTabs;
        public ObservableCollection<MBTabItem> WorkTabs
        {
            get { return _workTabs; }
            set
            { _workTabs = value; RaisePropertyChanged("WorkTabs"); }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value; RaisePropertyChanged("SelectedIndex"); }
        }
        #endregion

        #region 命令
        public ICommand FontSizeUpCommand => new RelayCommand(() =>
        { TextFontSize++; });
        public ICommand FontSizeDownCommand => new RelayCommand(() =>
        { TextFontSize--; });
        #endregion

        public MainViewModel()
        {
            TextFontSize = 14;
            IsFlowChartShowing = true;
            IsWorkTabShowing = false;
            CanHideWorkTab = true;
            CanHideFlowChart = false;
            WorkTabs = new ObservableCollection<MBTabItem>();
            SelectedIndex = 0;
            WorkTabs.CollectionChanged += WorkTabs_CollectionChanged;
        }

        private void WorkTabs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (_workTabs.Count == 0)
                {
                    IsFlowChartShowing = true;
                    IsWorkTabShowing = false;
                }
                //foreach (MBTabItem item in e.OldItems)
                //{
                //    //Removed items
                //    //item.PropertyChanged -= EntityViewModelPropertyChanged;
                //}
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //foreach (MBTabItem item in e.NewItems)
                //{
                //    //Added items
                //    //item.PropertyChanged += EntityViewModelPropertyChanged;
                //}
            }
            RaisePropertyChanged("WorkTabs");
        }
    }
}
