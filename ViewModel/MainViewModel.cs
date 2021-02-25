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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiBranchTexter.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region 左右显示相关
        private bool _isWorkGridVisible;
        public bool IsWorkGridVisible
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
                RaisePropertyChanged("IsFlowChartShowing");
            }
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

        private string _isModified;
        public string IsModified
        {
            get { return _isModified; }
            set { _isModified = value; RaisePropertyChanged("IsModified"); }
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

        #region 字体命令
        public ICommand FontSizeUpCommand => new RelayCommand((t) =>
        { TextFontSize++; });
        public ICommand FontSizeDownCommand => new RelayCommand((t) =>
        { TextFontSize--; });
        #endregion

        //新建文件命令
        public ICommand NewFileCommand => new RelayCommand((container) =>
        {
            try
            {
                FlowChartContainer flowChart = container as FlowChartContainer;
                // 检查是否需要保存现有文件
                if (IsModified == "*")
                {
                    MessageBoxResult warnResult = MessageBox.Show
                    (
                        Application.Current.MainWindow,
                        "尚未保存，是否保存？",
                        "警告",
                        MessageBoxButton.YesNoCancel
                    );
                    if (warnResult == MessageBoxResult.Yes)
                    {
                        SaveFile();
                    }
                    else if (warnResult == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }
                Debug.WriteLine("开始新建文件");
                // 文件夹对话框
                Microsoft.Win32.SaveFileDialog dialog =
                     new Microsoft.Win32.SaveFileDialog
                     {
                         RestoreDirectory = true,
                         Filter = "多分支导航文件|*.meta.json"
                     };
                if (Directory.Exists(_fileDirPath))
                { dialog.InitialDirectory = _fileDirPath; }
                if (dialog.ShowDialog() == true)
                {
                    CreateFile(dialog.FileName);
                    OpenFile(dialog.FileName);
                }
            }
            catch { }
        });
        //打开文件命令
        public ICommand OpenFileCommand => new RelayCommand((container) =>
        {
            try
            {
                FlowChartContainer flowChart = container as FlowChartContainer;
                // 检查是否需要保存现有文件
                if (IsModified == "*")
                {
                    MessageBoxResult warnResult = MessageBox.Show
                    (
                        Application.Current.MainWindow,
                        "尚未保存，是否保存？",
                        "警告",
                        MessageBoxButton.YesNoCancel
                    );
                    if (warnResult == MessageBoxResult.Yes)
                    {
                        SaveFile();
                    }
                    else if (warnResult == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }
                Debug.WriteLine("开始打开文件");
                // 文件夹对话框
                Microsoft.Win32.OpenFileDialog dialog =
                    new Microsoft.Win32.OpenFileDialog
                    {
                        RestoreDirectory = true,
                        Filter = "多分支导航文件|*.meta.json"
                    };
                if (Directory.Exists(_fileDirPath))
                { dialog.InitialDirectory = _fileDirPath; }
                if (dialog.ShowDialog() == true)
                {
                    OpenFile(dialog.FileName);
                }
            }
            catch { }
        });

        //保存单个节点命令，但是在worktab没有打开时将执行savefile
        public ICommand SaveNodeCommand => new RelayCommand((container) =>
        {
            try
            {
                if (!IsWorkGridVisible)
                { return; }
                if (IsWorkTabShowing == true && WorkTabs.Count > 0)
                {
                    Debug.WriteLine("开始保存单个节点");
                    WorkTabs[SelectedIndex].Save();
                }
                else
                { SaveFile(); }
            }
            catch { }
        });

        //保存整个文件
        public ICommand SaveFileCommand => new RelayCommand((container) =>
        {
            if (!IsWorkGridVisible)
            { return; }
            SaveFile();
        });

        //整个文件另存为
        public ICommand SaveAsFileCommand => new RelayCommand((container) =>
        {
            try
            {
                if (!IsWorkGridVisible)
                { return; }
                Debug.WriteLine("开始另存为文件");
                Microsoft.Win32.SaveFileDialog dialog =
                    new Microsoft.Win32.SaveFileDialog
                    {
                        RestoreDirectory = true,
                        Filter = "多分支导航文件|*.meta.json"
                    };

                if (Directory.Exists(_fileDirPath))
                { dialog.InitialDirectory = _fileDirPath; }
                if (dialog.ShowDialog() == true)
                {
                    FileName = dialog.FileName;
                }
                //保存文件
                SaveFile();
            }
            catch { }
        });
        #endregion

        #region 导出命令

        #endregion

        public MainViewModel()
        {
            IsWorkGridVisible = false;
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
            if (node == null)
            { return; }
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
            foreach (MBTabItem item in WorkTabs)
            {
                if (item.textNode == node)
                {
                    MBTabItem theItem = item;
                    theItem.Close();
                    return;
                }
            }
        }

        public void SaveFile()
        {
            try
            {
                Debug.WriteLine("开始保存文件");
                //让每个worktab保存
                for (int i = 0; i < WorkTabs.Count; i++)
                { WorkTabs[i].Save(); }
                // 如果文件名不存在
                if (!File.Exists(_fileName))
                {
                    Microsoft.Win32.SaveFileDialog dialog =
                        new Microsoft.Win32.SaveFileDialog
                        {
                            RestoreDirectory = true,
                            Filter = "多分支导航文件|*.meta.json"
                        };

                    if (Directory.Exists(_fileDirPath))
                    { dialog.InitialDirectory = _fileDirPath; }
                    if (dialog.ShowDialog() == true)
                    {
                        FileName = dialog.FileName;
                    }
                }
                //保存文件
                MetadataFile.WriteNodes(_fileName, ViewModelFactory.FCC.GetTextNodeWithLeftTopList());
                IsModified = "";
                Debug.WriteLine("文件" + _fileName + "保存成功");
            }
            catch { }
        }

        /// <summary> 打开文件 </summary>
        public async void OpenFile(string path)
        {
            if (!File.Exists(path))
            { return; }

            //如果此时东西开着
            if (IsWorkGridVisible)
            {
                IsWorkGridVisible = false;
                await Task.Delay(400);//等待动画放完
            }

            //关闭原有标签页
            WorkTabs.Clear();
            //新的文件名
            FileName = path;
            //打开新文件
            ViewModelFactory.FCC.Load(path);
            IsWorkGridVisible = true;
        }

        public void CreateFile(string path)
        {
            FileName = path;

            var node = new TextNode { Name = "new-node-0" };
            var n = new TextNodeWithLeftTop(node, 100, 100);
            MetadataFile.WriteNodes(path, new List<TextNodeWithLeftTop> { n });
        }

        public void ReCountCharForAll()
        {
            foreach (MBTabItem tab in _workTabs)
            { tab.CountChar(true); }
        }
        #endregion
    }
}
