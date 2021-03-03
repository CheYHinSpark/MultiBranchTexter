﻿using MultiBranchTexter.Controls;
using MultiBranchTexter.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
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

        private bool _isFullScreen;
        public bool IsFullScreen
        {
            get { return _isFullScreen; }
            set 
            {
                _isFullScreen = value; 
                if (value)
                { ViewModelFactory.Settings.TitleBarHeight = 0; }
                else
                { ViewModelFactory.Settings.TitleBarHeight = 24; }
                RaisePropertyChanged("IsFullScreen"); 
            }
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

        private bool _isModified;
        public bool IsModified
        {
            get { return _isModified; }
            set 
            { 
                _isModified = value;
                //如果没有在提示过程中，则会改要显示的东西
                if (_hintTask == null || _hintTask.IsCompleted)
                { InText = FileName + (value ? " *" : ""); }
                RaisePropertyChanged("IsModified"); 
            }
        }

        #region 提示文本
        private bool _isHintAwake;
        public bool IsHintAwake
        {
            get { return _isHintAwake; }
            set
            { _isHintAwake = value; RaisePropertyChanged("IsHintAwake"); }
        }

        private string _inText;
        public string InText
        {
            get { return _inText; }
            set
            { _inText = value; RaisePropertyChanged("InText"); }
        }

        private string _outText;
        public string OutText
        {
            get { return _outText; }
            set
            { _outText = value; RaisePropertyChanged("OutText"); }
        }

        private Task _hintTask;
        #endregion
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

        /// <summary> 当前正在工作的tabitem </summary>
        public MBTabItem WorkingTab
        { get { return _workTabs[_selectedIndex]; } }
        /// <summary> 当前正在工作的tabitem的viewmodel </summary>
        public TabItemViewModel WorkingViewModel
        { get { return _workTabs[_selectedIndex].ViewModel; } }
        #endregion

        #region 命令

        #region 字体命令
        public ICommand FontSizeUpCommand => new RelayCommand((t) =>
        { TextFontSize++; });
        public ICommand FontSizeDownCommand => new RelayCommand((t) =>
        { TextFontSize--; });
        #endregion

        public ICommand ExitFullScreenCommand => new RelayCommand((t) =>
        { IsFullScreen = false; });

        #region 左右框框调节
        public ICommand RightPullCommand => new RelayCommand((t) =>
        { 
            if (FlowChartWidth == "0")
            { FlowChartWidth = "*"; }
            else
            { WorkTabWidth = "0"; }
        });

        public ICommand LeftPullCommand => new RelayCommand((t) =>
        {
            if (WorkTabWidth == "0")
            { WorkTabWidth = "*"; }
            else
            { FlowChartWidth = "0"; }
        });
        #endregion

        #region 文件新建、打开、保存
        //新建文件命令
        public ICommand NewFileCommand => new RelayCommand((t) =>
        {
            try
            {
                // 检查是否需要保存现有文件
                if (IsModified)
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
                         Filter = "多分支导航文件|*.mbjson"
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
        public ICommand OpenFileCommand => new RelayCommand((t) =>
        {
            try
            {
                // 检查是否需要保存现有文件
                if (IsModified)
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
                        Filter = "多分支导航文件|*.mbjson"
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
        public ICommand SaveNodeCommand => new RelayCommand((t) =>
        {
            try
            {
                if (!IsWorkGridVisible)
                { return; }
                if (WorkTabWidth == "*" && WorkTabs.Count > 0)
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
        public ICommand SaveFileCommand => new RelayCommand((t) =>
        {
            if (!IsWorkGridVisible)
            { return; }
            SaveFile();
        });

        //整个文件另存为
        public ICommand SaveAsFileCommand => new RelayCommand((t) =>
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
                        Filter = "多分支导航文件|*.mbjson"
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
        /// <summary>
        /// 导出为JSON，游戏开发
        /// </summary>
        public ICommand OutputAsJSONCommand => new RelayCommand(async (t) =>
        {
            if (FileName == "" || _fileDirPath == "")
            { return; }
            List<TextNode> nodes = ViewModelFactory.FCC.GetTextNodeList();
            List<OperationTextNode> newNodes = new List<OperationTextNode>();
            for (int i = 0; i < nodes.Count; i++)
            {
                newNodes.Add(nodes[i].ToOperationTextNode());
            }
            await Task.Run(() =>
            {
                MetadataFile.WriteJSONNodes(FileName, newNodes);
            });
            await Task.Delay(10);
            Process.Start("explorer.exe", _fileDirPath);
        });
        #endregion

        #endregion

        public MainViewModel()
        {
            IsWorkGridVisible = false;
            FileName = "";
            TextFontSize = 14;
            FlowChartWidth = "*";
            WorkTabWidth = "0";
            WorkTabs = new ObservableCollection<MBTabItem>();
            WorkTabs.CollectionChanged += WorkTabs_CollectionChanged;
            SelectedIndex = 0;
            IsHintAwake = false;
            InText = "";
            OutText = "";
            //IsFullScreen = false;//这个不能要。。。
        }

        private void WorkTabs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (_workTabs.Count == 0)
                {
                    FlowChartWidth = "*";
                    WorkTabWidth = "0";
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                if (_workTabs.Count == 0)
                {
                    FlowChartWidth = "*";
                    WorkTabWidth = "0";
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
                if (WorkTabs[i].TextNode == node)
                {
                    SelectedIndex = i;
                    //打开worktab
                    WorkTabWidth = "*";
                    //IsWorkTabShowing = true;
                    //ViewModelFactory.FCC.
                        RaiseHint("打开节点" + node.Name);
                    return;
                }
            }
            WorkTabs.Add(new MBTabItem(node));
            SelectedIndex = WorkTabs.Count - 1;
            //打开worktab
            WorkTabWidth = "*";
            //IsWorkTabShowing = true;
            RaiseHint("打开节点" + node.Name);
        }

        /// <summary>
        /// 重置某个标签页的页尾
        /// </summary>
        public void ReLoadTab(TextNode node)
        {
            foreach (MBTabItem item in WorkTabs)
            {
                if (item.TextNode == node)
                { item.ReLoadTab(); return; }
            }
        }

        /// <summary>
        /// 删除某个标签页
        /// </summary>
        public void DeleteTab(TextNode node)
        {
            foreach (MBTabItem item in WorkTabs)
            {
                if (item.TextNode == node)
                {
                    MBTabItem theItem = item;
                    theItem.Close(true);
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
                            Filter = "多分支导航文件|*.mbjson"
                        };

                    if (Directory.Exists(_fileDirPath))
                    { dialog.InitialDirectory = _fileDirPath; }
                    if (dialog.ShowDialog() == true)
                    {
                        FileName = dialog.FileName;
                    }
                }
                //保存文件
                MetadataFile.WriteTextNodes(_fileName, ViewModelFactory.FCC.GetTextNodeWithLeftTopList());
                IsModified = false;
                Debug.WriteLine("文件 " + _fileName + " 保存成功");
                RaiseHint("文件 " + _fileName[(_fileName.LastIndexOf('\\') + 1)..] + " 保存成功");
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
            IsWorkGridVisible = true;
            IsModified = false;
            ViewModelFactory.FCC.Load(path);
        }

        public void CreateFile(string path)
        {
            FileName = path;

            var node = new TextNode { Name = "new-node-0" };
            var n = new TextNodeWithLeftTop(node, 100, 100);
            MetadataFile.WriteTextNodes(path, new List<TextNodeWithLeftTop> { n });
        }

        public void ReCountCharForAll()
        {
            foreach (MBTabItem tab in _workTabs)
            { tab.ViewModel.CountCharWord(true); }
        }

        /// <summary> 启动提示文本 </summary>
        public async void RaiseHint(string newHint)
        {
            OutText = InText; 
            InText = newHint;
            IsHintAwake = false;
            IsHintAwake = true;//这样设置一次，可以启动动画

            _hintTask = Task.Delay(4000);
            int i = _hintTask.Id;//存下当时的id

            await _hintTask;

            if (i == _hintTask.Id)//说明没有插入新的提示
            {
                OutText = InText;
                InText = FileName + (IsModified ? " *" : "");
                IsHintAwake = false;
                IsHintAwake = true;
            }
        }
        #endregion
    }
}
