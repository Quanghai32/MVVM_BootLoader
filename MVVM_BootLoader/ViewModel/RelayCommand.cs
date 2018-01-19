using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;

namespace MVVM_BootLoader
{
	class RelayCommand<T>:ICommand
	{
		private readonly Predicate<T> _canExecute;
		private readonly Action<T> _execute;

		public RelayCommand( Predicate<T> canExecute,Action<T> execute )
		{
			if ( execute == null )
				throw new ArgumentNullException( "execute" );
			_canExecute = canExecute;
			_execute = execute;
		}

		public bool CanExecute( object parameter )
		{
			return _canExecute == null ? true : _canExecute( (T)parameter );
		}

		public void Execute( object parameter )
		{
			_execute( (T)parameter );
		}

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}
	}

	class RelayCommand:ICommand
	{
		readonly Action _action = null;
		readonly Func<bool> _predicate = null;

		public RelayCommand( Action action )
			: this( null,action )
		{

		}

		public RelayCommand( Func<bool> predicate,Action action )
		{
			_predicate = predicate;
			_action = action;
		}

		public bool CanExecute( object parameter )
		{
			return _predicate == null ? true : _predicate();
		}

		public void Execute( object parameter )
		{
			_action();
		}

		public event EventHandler CanExecuteChanged;
	}
}
