using System;
using MultiBranchTexter.Model;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using MultiBranchTexter.Controls;

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

        public TabItemViewModel()
        {
            IsModified = "";
            CharCount = 0;
            WordCount = 0;
        }
    }
}
