using System;
using System.Diagnostics;
using System.Windows.Input;

namespace MultiBranchTexter.ViewModel
{
    public class RelayCommand : ICommand
    {

        #region 字段

        readonly Func<bool> _canExecute;

        readonly Action _execute;

        #endregion


        #region 构造函数

        public RelayCommand(Action execute) : this(execute, null) { }

        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException("execute");
            _canExecute = canExecute;
        }
        #endregion

        #region ICommand的成员
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested += value;
            }
            remove
            {
                if (_canExecute != null)
                    CommandManager.RequerySuggested -= value;
            }
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        { return _canExecute == null ? true : _canExecute(); }

        public void Execute(object parameter)
        { _execute(); }
        #endregion
    }
}