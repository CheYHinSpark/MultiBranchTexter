using MultiBranchTexter.Controls;
using MultiBranchTexter.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiBranchTexter.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region 左右显示相关
        private Visibility _isWorkGridVisible;
        public Visibility IsWorkGridVisible
        {
            get { return _isWorkGridVisible; }
            set { _isWorkGridVisible = value; RaisePropertyChanged("IsWorkGridVisible"); }
        }
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

        #region 文件相关
        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set 
            {
                _fileName = value;
                if (value != "")
                { _fileDirPath = value.Substring(0, value.LastIndexOfAny(new char[] { '\\', '/' }) + 1); }
                RaisePropertyChanged("FileName");
            }
        }
        private string _fileDirPath;
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
        public ICommand FontSizeUpCommand => new RelayCommand((t) =>
        { TextFontSize++; });
        public ICommand FontSizeDownCommand => new RelayCommand((t) =>
        { TextFontSize--; });
        //新建文件命令
        public ICommand NewFileCommand => new RelayCommand((container) =>
        {
            try
            {
                Debug.WriteLine("开始新建文件");
                FlowChartContainer flowChart = container as FlowChartContainer;
                //TODO: 检查是否需要保存现有文件
                // 文件夹对话框
                Microsoft.Win32.SaveFileDialog dialog =
                    new Microsoft.Win32.SaveFileDialog
                    {
                        RestoreDirectory = true,
                        Filter = "多分支导航文件|*.mbtxt"
                    };
                if (Directory.Exists(_fileDirPath))
                { dialog.InitialDirectory = _fileDirPath; }
                if (dialog.ShowDialog() == true)
                {
                    Stream myStream;
                    if ((myStream = dialog.OpenFile()) != null)
                    {
                        myStream.Write(new byte[] { 0 }, 0, 0);
                        myStream.Close();
                    }
                    FileName = dialog.FileName;
                    //关闭原有标签页
                    WorkTabs.Clear();
                    //打开新文件
                    flowChart.Load(dialog.FileName);
                    IsWorkGridVisible = Visibility.Visible;
                }
            }
            catch { }
        });
        //打开文件命令
        public ICommand OpenFileCommand => new RelayCommand((container) =>
        {
            try
            {
                Debug.WriteLine("开始打开文件");
                FlowChartContainer flowChart = container as FlowChartContainer;
                //TODO: 检查是否需要保存现有文件
                // 文件夹对话框
                Microsoft.Win32.OpenFileDialog dialog =
                    new Microsoft.Win32.OpenFileDialog
                    {
                        RestoreDirectory = true,
                        Filter = "多分支导航文件|*.mbtxt"
                    };
                if (Directory.Exists(_fileDirPath))
                { dialog.InitialDirectory = _fileDirPath; }
                if (dialog.ShowDialog() == true)
                {
                    FileName = dialog.FileName;
                    //关闭原有标签页
                    WorkTabs.Clear();
                    //打开新文件
                    flowChart.Load(dialog.FileName);
                    IsWorkGridVisible = Visibility.Visible;
                }
            }
            catch { }
        });
        //保存单个节点命令，但是在worktab没有打开时将执行savefile
        public ICommand SaveNodeCommand => new RelayCommand((container) =>
        {
            try
            {
                if (IsWorkTabShowing == true && WorkTabs.Count > 0)
                { 
                    Debug.WriteLine("开始保存单个节点");
                    WorkTabs[SelectedIndex].Save();
                }
                else
                { SaveFile(container as FlowChartContainer); }
            }
            catch { }
        });
        public ICommand SaveFileCommand => new RelayCommand((container) =>
        { SaveFile(container as FlowChartContainer); });
        public ICommand SaveAsFileCommand => new RelayCommand((container) =>
        {
            try
            {
                Debug.WriteLine("开始另存为文件");
                //TODO让每个worktab保存
                FlowChartContainer flowChart = container as FlowChartContainer;
                // TODO
                //flowChart.GetTextNodeWithLeftTopList();
                Microsoft.Win32.SaveFileDialog dialog =
                    new Microsoft.Win32.SaveFileDialog
                    {
                        RestoreDirectory = true,
                        Filter = "多分支导航文件|*.mbtxt"
                    };

                if (Directory.Exists(_fileDirPath))
                { dialog.InitialDirectory = _fileDirPath; }
                if (dialog.ShowDialog() == true)
                {
                    FileName = dialog.FileName;
                }
                //TODO保存文件
            }
            catch { }
        });
        #endregion

        public MainViewModel()
        {
            IsWorkGridVisible = Visibility.Hidden;
            FileName = "";
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
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                if (_workTabs.Count == 0)
                {
                    IsFlowChartShowing = true;
                    IsWorkTabShowing = false;
                }
            }
            RaisePropertyChanged("WorkTabs");
        }

        #region 方法
        //打开标签页
        public void OpenMBTabItem(TextNode node)
        {
            //遍历已有的标签页看看是否已经存在同标签
            for (int i = 0; i < WorkTabs.Count; i++)
            {
                if (WorkTabs[i].textNode == node)
                {
                    SelectedIndex = i;
                    //打开worktab
                    IsWorkTabShowing = true;
                    return;
                }
            }
            WorkTabs.Add(new MBTabItem(node));
            SelectedIndex = WorkTabs.Count - 1;
            //打开worktab
            IsWorkTabShowing = true;
        }
        /// <summary>
        /// 重置某个标签页的页尾
        /// </summary>
        public void ReLoadTab(TextNode node)
        {
            foreach (MBTabItem item in WorkTabs)
            {
                if (item.textNode == node)
                { item.ReLoadTabEnd(); return; }
            }
        }

        /// <summary>
        /// 删除某个标签页
        /// </summary>
        public void DeleteTab(TextNode node)
        {
            MBTabItem theItem = null;
            foreach (MBTabItem item in WorkTabs)
            {
                if (item.textNode == node)
                { theItem = item; break; }
            }
            theItem.Close();
        }

        public void SaveFile(FlowChartContainer flowChart)
        {
            try
            {
                Debug.WriteLine("开始保存文件");
                //TODO让每个worktab保存
                //TODO获得textNodewithlefttop
                //flowChart.GetTextNodeWithLeftTopList();
                // 如果文件名不存在
                if (!File.Exists(_fileName))
                {
                    Microsoft.Win32.SaveFileDialog dialog =
                        new Microsoft.Win32.SaveFileDialog
                        {
                            RestoreDirectory = true,
                            Filter = "多分支导航文件|*.mbtxt"
                        };

                    if (Directory.Exists(_fileDirPath))
                    { dialog.InitialDirectory = _fileDirPath; }
                    if (dialog.ShowDialog() == true)
                    {
                        FileName = dialog.FileName;
                    }
                }
                //TODO保存文件
            }
            catch { }
        }
        #endregion
    }
}
