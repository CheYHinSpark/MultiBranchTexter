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
            set { _isModified = value;RaisePropertyChanged("IsModified"); }
        }

        private string _nodeText;
        public string NodeText
        {
            get { return _nodeText; }
            set 
            {
                _nodeText = value;
                IsModified = "*";
                RaisePropertyChanged("NodeText");
            }
        }

        public TabItemViewModel()
        { }
    }
}
