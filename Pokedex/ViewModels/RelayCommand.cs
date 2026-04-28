using System;
using System.Threading.Tasks;
using System.Windows.Input;


namespace Pokedex.ViewModels
{
    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;


        // Konstruktor für async-Methoden (z.B. SearchPokemonAsync, PlayCryAsync)
        // Func<Task> = Methode die einen Task zurückgibt
        public RelayCommand(Func<Task> execute)
        {
            _execute = execute;
        }

        // Gibt an ob  Button klickbar ist — immer true, nie deaktiviert
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        // Wird von WPF aufgerufen wenn der Button geklickt wird
        // async void statt async Task — ICommand schreibt void vor
        public async void Execute(object? parameter)
        {
            await _execute();
        }

        // Konstruktor für normale void-Methoden (z.B. DeleteSlot, ActivateSaveMode)
        // Action = Methode die void zurückgibt — passt nicht direkt in Func<Task>
        public RelayCommand(Action execute)
        {
            // execute() kann nicht direkt gespeichert werden (falscher Typ)
            // → neue anonyme Methode erstellen die Func<Task> erfüllt
            _execute = () =>
            {
                execute();               // die eigentliche Methode aufrufen
                return Task.CompletedTask; // leeren Task zurückgeben damit der Typ stimmt
            };
        }

        // Wird benötigt weil ICommand es vorschreibt
        public event EventHandler? CanExecuteChanged;
    }
}