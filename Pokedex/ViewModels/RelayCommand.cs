using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Pokedex.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;

        public RelayCommand(Func<Task> execute)
        {
            _execute = execute;
        }

        // Gibt an ob der Command ausführbar ist 
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        // Wird aufgerufen wenn der Button geklickt wird
        public async void Execute(object? parameter)
        {
            await _execute();
        }

        // Wird benötigt weil ICommand es vorschreibt
        public event EventHandler? CanExecuteChanged;
    }
}