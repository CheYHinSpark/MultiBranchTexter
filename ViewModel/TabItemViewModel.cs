using System;
using MultiBranchTexter.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using MultiBranchTexter.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MultiBranchTexter.ViewModel
{
    public class TabItemViewModel: ViewModelBase
    {
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

        public TabItemViewModel()
        {
            IsModified = "";
            CharCount = 0;
            WordCount = 0;
            _textFragments = new ObservableCollection<TextFragment>();
            TextFragments.CollectionChanged += TextFragments_CollectionChanged;
        }

        private void TextFragments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged("TextFragments");
        }

        /// <summary> 统计字数，参数表示是否需要完全重新统计 </summary>
        public async void CountChar(bool totalReCount)
        {
            await Task.Delay(10);// <--不然有许多bug
            await Task.Run(new Action(
                delegate
                {
                    int c = 0, w = 0;
                    foreach (TextFragment tfp in TextFragments)
                    {
                        tfp.ShouldRecount |= totalReCount;
                        w += tfp.CountCharWord().Item1;
                        c += tfp.CountCharWord().Item2;
                    }
                    CharCount = c;
                    WordCount = w;
                }));
        }
    }
}
