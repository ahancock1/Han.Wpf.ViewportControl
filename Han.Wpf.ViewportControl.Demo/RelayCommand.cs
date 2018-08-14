// -----------------------------------------------------------------------
//   Copyright (C) 2018 Adam Hancock
//    
//   RelayCommand.cs Licensed under the MIT License. See LICENSE file in
//   the project root for full license information.  
// -----------------------------------------------------------------------

namespace Han.Wpf.ViewportControl.Demo
{
    using System;
    using System.Windows.Input;

    public class RelayCommand : ICommand
    {
        private readonly Func<bool> _canExecute;
        private readonly Action _execute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _execute();
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}