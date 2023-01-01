using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitWallpaper.Helpers
{
    using System;
    using System.Windows.Input;

    namespace UICommand1
    {
        public class GenericRelayCommand<T> : ICommand
        {
            private readonly Action<T> execute;

            public GenericRelayCommand(Action<T> execute) : this(execute, p => true)
            {
            }

            public GenericRelayCommand(Action<T> execute, Predicate<T> canExecuteFunc)
            {
                this.execute = execute;
                this.CanExecuteFunc = canExecuteFunc;
            }

            public event EventHandler CanExecuteChanged;
            public void RaiseCanExecuteChanged()
            {
                var handler = CanExecuteChanged;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }


            public Predicate<T> CanExecuteFunc { get; private set; }

            public bool CanExecute(object parameter)
            {
                if (parameter != null)
                {
                    var canExecute = this.CanExecuteFunc((T)parameter);
                    return canExecute;
                }
                else
                {
                    return false;
                }
            }

            public void Execute(object parameter)
            {
                this.execute((T)parameter);
            }
        }

        public class RelayCommand : ICommand
        {
            private Action methodToExecute;
            private Func<bool> canExecuteEvaluator;

            public RelayCommand(Action methodToExecute, Func<bool> canExecuteEvaluator)
            {
                this.methodToExecute = methodToExecute;
                this.canExecuteEvaluator = canExecuteEvaluator;
            }

            public RelayCommand(Action methodToExecute)
                : this(methodToExecute, null)
            {
            }

            /*
            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
            */
            public event EventHandler CanExecuteChanged;
            public void RaiseCanExecuteChanged()
            {
                var handler = CanExecuteChanged;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }

            public bool CanExecute(object parameter)
            {
                if (this.canExecuteEvaluator == null)
                {
                    return true;
                }
                else
                {
                    bool result = this.canExecuteEvaluator.Invoke();
                    return result;
                }
            }

            public void Execute(object parameter)
            {
                this.methodToExecute.Invoke();
            }
        }

        /*
        /// <summary>
        /// A command whose sole purpose is to relay its functionality 
        /// to other objects by invoking delegates. 
        /// The default return value for the CanExecute method is 'true'.
        /// <see cref="RaiseCanExecuteChanged"/> needs to be called whenever
        /// <see cref="CanExecute"/> is expected to return a different value.
        /// </summary>
        public class RelayCommand : ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool> _canExecute;

            /// <summary>
            /// Raised when RaiseCanExecuteChanged is called.
            /// </summary>
            public event EventHandler CanExecuteChanged;

            /// <summary>
            /// Creates a new command that can always execute.
            /// </summary>
            /// <param name="execute">The execution logic.</param>
            public RelayCommand(Action execute)
                : this(execute, null)
            {
            }

            /// <summary>
            /// Creates a new command.
            /// </summary>
            /// <param name="execute">The execution logic.</param>
            /// <param name="canExecute">The execution status logic.</param>
            public RelayCommand(Action execute, Func<bool> canExecute)
            {
                if (execute == null)
                    throw new ArgumentNullException("execute");
                _execute = execute;
                _canExecute = canExecute;
            }

            /// <summary>
            /// Determines whether this <see cref="RelayCommand"/> can execute in its current state.
            /// </summary>
            /// <param name="parameter">
            /// Data used by the command. If the command does not require 
            /// data to be passed, this object can be set to null.
            /// </param>
            /// <returns>true if this command can be executed; otherwise, false.</returns>
            public bool CanExecute(object parameter)
            {
                return _canExecute == null ? true : _canExecute();
            }

            /// <summary>
            /// Executes the <see cref="RelayCommand"/> on the current command target.
            /// </summary>
            /// <param name="parameter">
            /// Data used by the command. If the command does not require 
            /// data to be passed, this object can be set to null.
            /// </param>
            public void Execute(object parameter)
            {
                _execute();
            }

            /// <summary>
            /// Method used to raise the <see cref="CanExecuteChanged"/> event
            /// to indicate that the return value of the <see cref="CanExecute"/>
            /// method has changed.
            /// </summary>
            public void RaiseCanExecuteChanged()
            {
                var handler = CanExecuteChanged;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }
        }
        */
    }
}
