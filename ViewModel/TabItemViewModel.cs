using System;
using MultiBranchTexter.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using MultiBranchTexter.View;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows;

namespace MultiBranchTexter.ViewModel
{
    public class TabItemViewModel: ViewModelBase
    {
        #region 字段

        #region 文本，字数
        private string _isModified;
        public string IsModified
        {
            get { return _isModified; }
            set 
            { _isModified = value; RaisePropertyChanged("IsModified"); }
        }

        private int _charCount;
        public int CharCount
        {
            get { return _charCount; }
            set
            { _charCount = value; RaisePropertyChanged("CharCount"); }
        }

        private int _wordCount;
        public int WordCount
        {
            get { return _wordCount; }
            set
            { _wordCount = value; RaisePropertyChanged("WordCount"); }
        }

        private ObservableCollection<TextFragment> _textFragments;
        public ObservableCollection<TextFragment> TextFragments
        {
            get { return _textFragments; }
            set
            { _textFragments = value; RaisePropertyChanged("TextFragments"); }
        }
        #endregion

        #region undo redo
        private int _focusIndex;
        public int FocusIndex
        {
            get { return _focusIndex; }
            set { _focusIndex = value; }
        }

        private bool? _focusInfo;
        /// <summary> true表示在content，false表示在comment，null表示没选中 </summary>
        public bool? FocusInfo
        {
            get { return _focusInfo; }
            set { _focusInfo = value; IsFocused = value != null; }
        }
        private int _selectionStart;
        public int SelectionStart
        {
            get { return _selectionStart; }
            set { _selectionStart = value; }
        }

        private int _selectionLength;
        public int SelectionLength
        {
            get { return _selectionLength; }
            set { _selectionLength = value; }
        }

        //数据缓冲
        (ObservableCollection<TextFragment>, int, bool?, int, int) _bufferData;

        //用撤销栈保存所有编辑前的数据列表
        readonly Stack<(ObservableCollection<TextFragment>, int, bool?, int, int)> _previousData;

        //将撤销前的数据源保存到前进栈中
        readonly Stack<(ObservableCollection<TextFragment>, int, bool?, int, int)> _nextData;

        Task _undoRedoTask;
        Task _saveUndoTask;
        #endregion

        #region 外观绑定用
        private bool _isFocused;

        public bool IsFocused
        {
            get { return _isFocused; }
            set { _isFocused = value; RaisePropertyChanged("IsFocused"); }
        }
        #endregion

        #region 文本搜索
        private string _searchStr;

        private string _searchBoxVisi;

        public string SearchBoxVisi
        {
            get { return _searchBoxVisi; }
            set { _searchBoxVisi = value; RaisePropertyChanged("SearchBoxVisi"); }
        }

        private List<(int, bool, int)> _searchedResult = new List<(int, bool, int)>();

        private int _searchedLength = 0;

        private int _searchedIndex = 0;
        #endregion

        #endregion

        #region 构造方法
        public TabItemViewModel()
        {
            IsModified = "";
            CharCount = 0;
            WordCount = 0;
            _textFragments = new ObservableCollection<TextFragment>();
            TextFragments.CollectionChanged += TextFragments_CollectionChanged;
            _previousData = new Stack<(ObservableCollection<TextFragment>, int, bool?, int, int)>();
            _nextData = new Stack<(ObservableCollection<TextFragment>, int, bool?, int, int)>();
            FocusIndex = 0;
            FocusInfo = null;
            SelectionStart = 0;
            SelectionLength = 0;
            SearchBoxVisi = "0";
        }
        #endregion

        #region 事件
        private void TextFragments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TextFragment tf in e.NewItems)
                {
                    tf.PropertyChanged += FragmentItem_PropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (TextFragment tf in e.OldItems)
                {
                    tf.PropertyChanged -= FragmentItem_PropertyChanged;
                }
            }
            CountCharWord(false);
            IsModified = "*";
            ViewModelFactory.Main.IsModified = true;
            SavePrevious();
            RaisePropertyChanged("TextFragments");
        }

        private void FragmentItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CountCharWord(false);
            IsModified = "*";
            ViewModelFactory.Main.IsModified = true;
            SavePrevious();
            _searchedResult.Clear();
        }
        #endregion

        #region 命令

        #region 标签页右键菜单命令
        /// <summary> 关闭其他命令 </summary>
        public ICommand CloseOtherCommand => new RelayCommand((_) =>
        { ViewModelFactory.Main.CloseOther(this); });

        /// <summary> 关闭所有命令 </summary>
        public ICommand CloseAllCommand => new RelayCommand((_) =>
        { ViewModelFactory.Main.CloseAll(); });
        #endregion

        #region undoredo
        /// <summary> 撤销命令 </summary>
        public ICommand UndoCommand => new RelayCommand(async (_) =>
        {
            if (_undoRedoTask != null && !_undoRedoTask.IsCompleted)
            { return; }
            Undo();
            _undoRedoTask = Task.Delay(50);
            await _undoRedoTask;
        });

        /// <summary> 重做命令 </summary>
        public ICommand RedoCommand => new RelayCommand(async (_) =>
        {
            if (_undoRedoTask != null && !_undoRedoTask.IsCompleted)
            { return; }
            Redo();
            _undoRedoTask = Task.Delay(50);
            await _undoRedoTask;
        });
        #endregion

        #region 文本
        /// <summary> 开始搜索命令 </summary>
        public ICommand StartSearchCommand => new RelayCommand((_) =>
        { SearchBoxVisi = "Auto"; });
        #endregion

        #endregion

        #region 方法

        #region 字数统计
        /// <summary> 统计字数，参数表示是否需要完全重新统计 </summary>
        public async void CountCharWord(bool totalReCount)
        {
            await Task.Delay(10);// <--不然有许多bug
            await Task.Run(new Action(
                delegate
                {
                    int c = 0, w = 0;
                    foreach (TextFragment tfp in TextFragments)
                    {
                        tfp.ShouldRecount |= totalReCount;
                        var ccw = tfp.CountCharWord();
                        w += ccw.Item1;
                        c += ccw.Item2;
                    }
                    CharCount = c;
                    WordCount = w;
                }));
            ViewModelFactory.FCC.CountCharWord(totalReCount);
        }
        #endregion

        #region undo redo准备
        public void SetFocusSelectInfo(int index, bool? focus, int selectS, int selectL)
        {
            FocusIndex = index;
            FocusInfo = focus;
            SelectionStart = selectS;
            SelectionLength = selectL;
        }

        /// <summary>
        /// 深拷贝一个列表
        /// </summary>
        ObservableCollection<TextFragment> Clone(ObservableCollection<TextFragment> sourceList)
        {
            var clonePre = new List<TextFragment>();
            sourceList.ToList().ForEach(a => clonePre.Add(CopyByReflaction(a)));
            return new ObservableCollection<TextFragment>(clonePre);
        }

        //利用反射和泛型来拷贝一个数据对象
        T CopyByReflaction<T>(T source)
        {
            T result = Activator.CreateInstance<T>();
            for (int i = 0; i < source.GetType().GetProperties().Count(); i++)
            {
                var sValue = source.GetType().GetProperties()[i].GetValue(source, null);
                result.GetType().GetProperties()[i].SetValue(result, sValue, null);
            }
            return result;
        }
        #endregion

        #region undo redo逻辑
        /// <summary> 撤销 </summary>
        public void Undo()
        {
            if (_previousData.Count == 0)
            { return; }

            _nextData.Push(_bufferData);
            UndoRedoAction(_previousData.Pop());
        }
        /// <summary> 重做 </summary>
        public void Redo()
        {
            if (_nextData.Count == 0)
            { return; }

            _previousData.Push(_bufferData);
            UndoRedoAction(_nextData.Pop());
        }

        /// <summary> undoredo的数据更新 </summary>
        public async void UndoRedoAction((ObservableCollection<TextFragment>, int, bool?, int, int) nd)
        {
            TextFragments = Clone(nd.Item1);

            await Task.Delay(10);

            if (nd.Item3 != null)
            {
                ContentPresenter cp = ViewModelFactory.Main.WorkingTab.
                    fragmentContainer.ItemContainerGenerator
                    .ContainerFromIndex(nd.Item2) as ContentPresenter;
                DataTemplate MyDataTemplate = cp.ContentTemplate;

                if (MyDataTemplate.FindName("TFP", cp) is TextFragmentPresenter tfp)
                {
                    tfp.SetFocus(nd.Item4, nd.Item3 == true, nd.Item5);
                }
            }

            //更新缓冲数据
            _bufferData = (Clone(TextFragments), FocusIndex, FocusInfo, SelectionStart, SelectionLength);

            //必须把事件恢复回去
            TextFragments.CollectionChanged += TextFragments_CollectionChanged;
            for (int i = 0; i < TextFragments.Count; i++)
            {
                TextFragments[i].PropertyChanged += FragmentItem_PropertyChanged;
            }
            //重新统计字数
            CountCharWord(true);
        }

        /// <summary>
        /// 压栈存储当前数据源
        /// </summary>
        public async void SavePrevious()
        {
            if (_saveUndoTask == null || _saveUndoTask.IsCompleted)
            {
                //每次开始编辑时都应该清除前进栈中的数据
                _nextData.Clear();
                //缓存区数据存入
                _bufferData.Item2 = FocusIndex;
                _bufferData.Item3 = FocusInfo;
                _bufferData.Item4 = SelectionStart;
                _bufferData.Item5 = SelectionLength;
                _previousData.Push(_bufferData);
            }

            _saveUndoTask = Task.Delay(300);
            //连续写入判断时间 0.2秒，
            //如果saveprevious小于这段时间触发，是不会添加undo栈和重写缓冲的
            int i = _saveUndoTask.Id;

            await _saveUndoTask;

            if (_saveUndoTask.Id != i)//表示task更新，意味着后面有一段连续写入，这个地方就不用更新缓冲了
            { return; }

            //如果一段连续写入结束了，重写缓冲区
            _bufferData = (Clone(TextFragments), FocusIndex, FocusInfo, SelectionStart, SelectionLength);
            Debug.WriteLine("存入结束");
        }

        public void ClearUndoStack()
        {
            _previousData.Clear();
            Debug.WriteLine("清除了undo栈");
            _bufferData = (Clone(TextFragments), FocusIndex, FocusInfo, SelectionStart, SelectionLength);
        }
        #endregion

        #region 搜索
        public void Search(string str)
        {
            _searchedLength = str.Length;
            _searchStr = str;
            if (_searchedLength == 0)
            { return; }
            _searchedResult.Clear();
            for (int i = 0; i < _textFragments.Count; i++)
            {
                int re = _textFragments[i].Comment.IndexOf(str);
                while (re > 0)
                {
                    _searchedResult.Add((i, false, re));
                    re = _textFragments[i].Comment.IndexOf(str, re + _searchedLength);
                }
                re = _textFragments[i].Content.IndexOf(str);
                while (re > 0)
                {
                    _searchedResult.Add((i, true, re));
                    re = _textFragments[i].Content.IndexOf(str, re + _searchedLength);
                }
            }

            if (_searchedResult.Count == 0)
            { return; }

            for (int i = 0; i < _searchedResult.Count; i++)
            {
                if (_searchedResult[i].Item1 < FocusIndex)
                { continue; }

                _searchedIndex = i;
                break;
            }

            Debug.WriteLine(_searchedResult[_searchedIndex]);
            ContentPresenter cp = ViewModelFactory.Main.WorkingTab.
                 fragmentContainer.ItemContainerGenerator
                 .ContainerFromIndex(_searchedResult[_searchedIndex].Item1) as ContentPresenter;
            DataTemplate MyDataTemplate = cp.ContentTemplate;

            if (MyDataTemplate.FindName("TFP", cp) is TextFragmentPresenter tfp)
            {
                tfp.SetFocus(_searchedResult[_searchedIndex].Item3, _searchedResult[_searchedIndex].Item2, _searchedLength);
            }
        }

        public void SearchNext(bool reverse = false)
        {
            if (_searchedResult.Count == 0)
            { Search(_searchStr); }
            if (_searchedResult.Count == 0)
            { return; }

            if (reverse)
            {
                _searchedIndex--;
                if (_searchedIndex < 0)
                { _searchedIndex = _searchedResult.Count - 1; }
            }
            else
            {
                _searchedIndex++;
                if (_searchedIndex >= _searchedResult.Count)
                { _searchedIndex = 0; }
            }

            ContentPresenter cp = ViewModelFactory.Main.WorkingTab.
                    fragmentContainer.ItemContainerGenerator
                    .ContainerFromIndex(_searchedResult[_searchedIndex].Item1) as ContentPresenter;
            DataTemplate MyDataTemplate = cp.ContentTemplate;

            if (MyDataTemplate.FindName("TFP", cp) is TextFragmentPresenter tfp)
            {
                tfp.SetFocus(_searchedResult[_searchedIndex].Item3, _searchedResult[_searchedIndex].Item2, _searchedLength);
            }
        }

        public void ClearSearch()
        { _searchedResult.Clear(); }
        #endregion

        #endregion
    }
}
