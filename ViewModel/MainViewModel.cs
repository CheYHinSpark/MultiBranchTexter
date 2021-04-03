using MultiBranchTexter.View;
using MultiBranchTexter.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Text.RegularExpressions;
using MultiBranchTexter.Resources;

namespace MultiBranchTexter.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region 字段

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

        private double _workTabActualWidth;
        public double WorkTabActualWidth
        {
            get { return _workTabActualWidth; }
            set 
            {
                _workTabActualWidth = value;
                ReSizeTabs();
                RaisePropertyChanged("WorkTabActualWidth");
            }
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
                { _fileDirPath = Path.GetDirectoryName(value); }
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

        #endregion

        #region 命令

        #region 字体命令
        public ICommand FontSizeUpCommand => new RelayCommand((_) =>
        { TextFontSize++; });
        public ICommand FontSizeDownCommand => new RelayCommand((_) =>
        { TextFontSize--; });
        #endregion

        #region 界面命令，全屏、设置页
        public ICommand ExitFullScreenCommand => new RelayCommand((_) =>
        { IsFullScreen = false; });

        public ICommand SwitchOptionsPanelCommand => new RelayCommand((_) =>
        { (Application.Current.MainWindow as MetroWindow).SwitchShowOptions(); });
        #endregion

        #region 左右框框调节
        public ICommand RightPullCommand => new RelayCommand((_) =>
        { 
            if (FlowChartWidth == "0")
            { FlowChartWidth = "*"; }
            else
            { WorkTabWidth = "0"; }
        });

        public ICommand LeftPullCommand => new RelayCommand((_) =>
        {
            if (WorkTabWidth == "0")
            { WorkTabWidth = "*"; }
            else
            { FlowChartWidth = "0"; }
        });
        #endregion

        #region 文件新建、打开、保存
        //新建文件命令
        public ICommand NewFileCommand => new RelayCommand((_) =>
        {
            try
            {
                // 检查是否需要保存现有文件
                if (IsModified)
                {
                    MessageBoxResult warnResult = MessageBox.Show
                    (
                        Application.Current.MainWindow,
                        LanguageManager.Instance["Msg_SaveFile"],
                        LanguageManager.Instance["Win_Warn"],
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
                         Filter = LanguageManager.Instance["Sys_Extname"] + "|*.mbjson"
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
        public ICommand OpenFileCommand => new RelayCommand((_) =>
        {
            try
            {
                // 检查是否需要保存现有文件
                if (IsModified)
                {
                    MessageBoxResult warnResult = MessageBox.Show
                    (
                        Application.Current.MainWindow,
                         LanguageManager.Instance["Msg_SaveFile"],
                        LanguageManager.Instance["Win_Warn"],
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
                        Filter = LanguageManager.Instance["Sys_Extname"] + "|*.mbjson|" +
                        LanguageManager.Instance["Sys_AllExt"] + "|*.*"
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

        //保存单个节点命令，但是在worktab没有打开时或者在已经保存好的tab上save将执行savefile，
        public ICommand SaveNodeCommand => new RelayCommand((_) =>
        {
            try
            {
                if (!IsWorkGridVisible)
                { return; }
                if (WorkTabWidth == "*" && WorkTabs.Count > 0)
                {
                    if (WorkingViewModel.IsModified == "*")
                    {
                        Debug.WriteLine("开始保存单个节点");
                        WorkTabs[SelectedIndex].Save();
                        return;
                    }
                    else if (!ViewModelFactory.Settings.DoubleSaveAwake)
                    { return; }
                }
                SaveFile();
            }
            catch { }
        });

        //保存整个文件
        public ICommand SaveFileCommand => new RelayCommand((_) =>
        {
            if (!IsWorkGridVisible)
            { return; }
            SaveFile();
        });

        //整个文件另存为
        public ICommand SaveAsFileCommand => new RelayCommand((_) =>
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
                        Filter = LanguageManager.Instance["Sys_Extname"] + "|*.mbjson"
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
        /// <summary> 导出为JSON，游戏开发 </summary>
        public ICommand OutputAsJSONCommand => new RelayCommand(async (_) =>
        {
            if (FileName == "" || _fileDirPath == "")
            { return; }
            RaiseHint(LanguageManager.Instance["Hint_OutputJSON"]);
            SaveFile();
            List<OperationTextNode> newNodes = new List<OperationTextNode>();
            foreach (TextNode node in ViewModelFactory.FCC.Nodes)
            {
                newNodes.Add(node.ToOperationTextNode());
            }
            await Task.Run(() =>
            {
                MetadataFile.WriteJSONNodes(FileName, newNodes);
            });
            await Task.Delay(10);
            Process.Start("explorer.exe", _fileDirPath);
        });

        /// <summary> 导出为txt </summary>
        public ICommand OutputAsTxtCommand => new RelayCommand((_) =>
        {
            if (FileName == "" || _fileDirPath == "")
            { return; }
            SaveFile();
            new OutputAsTxtWindow() 
            {
                Owner = Application.Current.MainWindow,
                SavePath = new Regex(@"\.mbjson$", RegexOptions.IgnoreCase).Replace(FileName, "")
            }.Show();
        });

        /// <summary> 导出节点图 </summary>
        public ICommand OutputFCCCommand => new RelayCommand((_) =>
        {
            RaiseHint(LanguageManager.Instance["Hint_OutputPic"]);
            SaveFile();
            var imgUrl = new Regex(@"\.mbjson$", RegexOptions.IgnoreCase).Replace(FileName, ".png");
            ViewModelFactory.FCC.OutputImg(imgUrl);
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

        #region 标签页
        /// <summary> 打开标签页 </summary>
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
                    RaiseHint(LanguageManager.Instance["Hint_OpenNode"] + node.Name);
                    return;
                }
            }
            WorkTabs.Add(new MBTabItem(node));
            SelectedIndex = WorkTabs.Count - 1;
            //打开worktab
            WorkTabWidth = "*";
            RaiseHint(LanguageManager.Instance["Hint_OpenNode"] + node.Name);
        }

        /// <summary> 重置某个标签页的页尾 </summary>
        public void ReLoadTab(TextNode node)
        {
            foreach (MBTabItem item in WorkTabs)
            {
                if (item.TextNode == node)
                { item.ReLoadTab(); return; }
            }
        }

        /// <summary> 删除某个标签页 </summary>
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

        /// <summary> 重新计算所有宽度 </summary>
        public void ReSizeTabs()
        {
            //保持约定宽度item的临界个数
            int criticalCount = (int)((WorkTabActualWidth - 5) / 120.0);
            double targetWidth;
            if (WorkTabs.Count <= criticalCount)
            {
                //小于等于临界个数 等于约定宽度
                targetWidth = 120;
            }
            else
            {
                //大于临界个数 等于平均宽度
                targetWidth = (WorkTabActualWidth - 5) / WorkTabs.Count;
            }
            for (int i = 0; i < WorkTabs.Count; i++)
            {
                WorkTabs[i].ToWidth(Math.Max(0, targetWidth));
            }
        }

        // 关闭所有标签页
        public void CloseAll()
        {
            bool needWarn = false;
            for (int i = 0; i < WorkTabs.Count; i++)
            {
                needWarn |= WorkTabs[i].ViewModel.IsModified == "*";
            }
            if (needWarn)
            {
                // 警告
                MessageBoxResult warnResult = MessageBox.Show
                 (
                     Application.Current.MainWindow,
                     LanguageManager.Instance["Msg_SaveNodes"],
                     LanguageManager.Instance["Win_Warn"],
                     MessageBoxButton.YesNoCancel
                 );
                if (warnResult == MessageBoxResult.Yes)
                {
                    for (int i = 0; i < WorkTabs.Count; i++)
                    { WorkTabs[i].Save(); }
                }
                else if (warnResult == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            WorkTabs.Clear();
        }

        public void CloseOther(object save)
        {
            for (int i = 0; i < WorkTabs.Count; i++)
            {
                if (WorkTabs[i].ViewModel == save)
                {
                    SelectedIndex = i;
                    break;
                }
            }

            bool needWarn = false;
            for (int i = 0; i < WorkTabs.Count; i++)
            {
                if (i != SelectedIndex)
                { needWarn |= WorkTabs[i].ViewModel.IsModified == "*"; }
            }
            if (needWarn)
            {
                // 警告
                MessageBoxResult warnResult = MessageBox.Show
                    (
                    Application.Current.MainWindow,
                    LanguageManager.Instance["Msg_SaveNodes"],
                    LanguageManager.Instance["Win_Warn"],
                    MessageBoxButton.YesNoCancel
                );
                if (warnResult == MessageBoxResult.Yes)
                {
                    for (int i = 0; i < WorkTabs.Count; i++)
                    {
                        if (i != SelectedIndex)
                        { WorkTabs[i].Save(); }
                    }
                }
                else if (warnResult == MessageBoxResult.Cancel)
                {
                    return;
                }
            }
            for (int i = WorkTabs.Count - 1; i > SelectedIndex; i--)
            {
                WorkTabs[i].ToWidth(0, true);
            }
            for (int i = SelectedIndex - 1; i >= 0; i--)
            {
                WorkTabs[i].ToWidth(0, true);
            }
        }


        #endregion

        #region 文件相关
        public void SaveFile()
        {
            try
            {
                Debug.WriteLine("开始保存文件");
                //让每个worktab保存
                for (int i = 0; i < WorkTabs.Count; i++)
                { WorkTabs[i].Save(); }

                //保存文件
                MetadataFile.WriteTextNodes(_fileName, ViewModelFactory.FCC.GetTextNodeWithLeftTopList());
                IsModified = false;
                Debug.WriteLine("文件 " + _fileName + " 保存成功");
                RaiseHint(LanguageManager.Instance["Hint_File"] 
                    + _fileName[(_fileName.LastIndexOf('\\') + 1)..] 
                    + LanguageManager.Instance["Hint_SaveSuccess"]);
            }
            catch
            {
                Microsoft.Win32.SaveFileDialog dialog =
                    new Microsoft.Win32.SaveFileDialog
                    {
                        RestoreDirectory = true,
                        Filter = LanguageManager.Instance["Sys_Extname"] + "|*.mbjson"
                    };

                if (Directory.Exists(_fileDirPath))
                { dialog.InitialDirectory = _fileDirPath; }
                if (dialog.ShowDialog() == true)
                {
                    FileName = dialog.FileName;
                }
                MetadataFile.WriteTextNodes(_fileName, ViewModelFactory.FCC.GetTextNodeWithLeftTopList());
                IsModified = false;
                Debug.WriteLine("文件 " + _fileName + " 保存成功");
                RaiseHint(LanguageManager.Instance["Hint_File"]
                    + _fileName[(_fileName.LastIndexOf('\\') + 1)..]
                    + LanguageManager.Instance["Hint_SaveSuccess"]);
            }
        }

        /// <summary> 打开文件 </summary>
        public async void OpenFile(string path)
        {
            FileName = path;

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
            IsModified = false;

            await Task.Delay(20);
            await Task.Run(new Action(() => { ViewModelFactory.FCC.Load(path); }));
        }

        /// <summary> 创建文件 </summary>
        public void CreateFile(string path)
        {
            FileName = path;

            var node = new TextNode("new-node-0");
            var n = new TextNodeWithLeftTop(node, 100, 100);
            MetadataFile.WriteTextNodes(path, new List<TextNodeWithLeftTop> { n });
        }

        /// <summary> 显示工作区 </summary>
        public void ShowWorkGrid()
        {
            RaiseHint(LanguageManager.Instance["Hint_CreateFC"]);
            IsWorkGridVisible = true;
        }
        #endregion

        #region 提示文本
        /// <summary> 启动提示文本 </summary>
        public async void RaiseHint(string newHint)
        {
            OutText = InText; 
            if (InText == newHint)
            { return; }
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
                if (OutText != InText)
                {
                    IsHintAwake = false;
                    IsHintAwake = true;
                }
            }
        }

        public void QuickEndHint()
        {
            if (_hintTask == null || _hintTask.IsCompleted)
            { return; }
            RaiseHint(FileName + (IsModified ? " *" : ""));
        }
        #endregion

        #endregion
    }
}
