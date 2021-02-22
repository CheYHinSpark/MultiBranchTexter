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
            set { _isModified = value; RaisePropertyChanged("IsModified"); }
        }

        public TabItemViewModel()
        { }
    }
}
