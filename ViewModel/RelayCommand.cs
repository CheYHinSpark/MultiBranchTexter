using System;
using System.Diagnostics;
using System.Windows.Input;

namespace MultiBranchTexter.ViewModel
{
    public class RelayCommand : ICommand
    {

        #region 字段
        private readonly Func<object, bool> _canExecute;
        private readonly Action<object> _execute;
        #endregion

        #region 构造函数
        public RelayCommand(Action<object> execute) : this(execute, null) { }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
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

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
        #endregion
    }
}