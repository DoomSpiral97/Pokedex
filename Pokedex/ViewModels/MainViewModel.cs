using Pokedex.Models;
using Pokedex.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NAudio.Wave;
using NAudio.Vorbis;
using System.Net.Http;
using System.IO;

namespace Pokedex.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {

        private PokemonModel? _latestResult;



        // Service — der einzige Ort der die API kennt
        private readonly PokeApiService _service = new PokeApiService();

        private readonly HttpClient _httpClient = new HttpClient();

        // -------------------------------------------------------
        // Properties
        // -------------------------------------------------------

        private PokemonModel? _currentPokemon;
        public PokemonModel? CurrentPokemon
        {
            get { return _currentPokemon; }
            set { _currentPokemon = value; OnPropertyChanged(); }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get { return _searchText; }
            set { _searchText = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        // Command

        public ICommand SearchCommand { get; }

        public ICommand PlaySoundCommand { get; }

        public ICommand NextPokemonCommand { get; }
        public ICommand PreviousPokemonCommand { get; }

        public MainViewModel()
        {
            SearchCommand = new RelayCommand(SearchPokemonAsync);
            PlaySoundCommand = new RelayCommand(PlayCryAsync);
            NextPokemonCommand = new RelayCommand(NextPokemonAsync);      
            PreviousPokemonCommand = new RelayCommand(PreviousPokemonAsync);
        }

        // Methoden
        private void UpdateUI()
        {
            // Wenn nix zurückgekommen ist → Fehlermeldung zeigen
            if (_latestResult == null)
            {
                ErrorMessage = "Pokémon nicht gefunden!";
            }
            else
            {
                // Alles gut → Pokémon ans ViewModel übergeben Bindings aktualisieren die UI automatisch
                CurrentPokemon = _latestResult;
            }

            // Ladeanimation wieder aus
            IsLoading = false;
        }

        private async Task SearchPokemonAsync()
        {
            // Nix tun wenn das Suchfeld leer ist
            if (string.IsNullOrWhiteSpace(SearchText)) return;

            // Ladeanimation an und alte Fehlermeldung wegräumen
            IsLoading = true;
            ErrorMessage = string.Empty;

            // Service fragen — läuft im Hintergrund, UI bleibt reaktiv-> Ergebnis im Zwischenspeicher parken damit UpdateUI rankommt
            _latestResult = await _service.GetPokemonAsync(SearchText);

            // Nach dem await sind wir evtl. auf einem fremden Thread,Dispatcher bringt uns zurück auf den UI-Thread und führt UpdateUI dort aus
            Application.Current.Dispatcher.Invoke(UpdateUI);
        }

        private async Task PlayCryAsync()
        {
            if (CurrentPokemon == null) return;
            if (string.IsNullOrEmpty(CurrentPokemon.SoundUrl)) return;  // ← war SoundUrl

            byte[] audioData = await _httpClient.GetByteArrayAsync(CurrentPokemon.SoundUrl);
            MemoryStream memoryStream = new MemoryStream(audioData);
            VorbisWaveReader vorbisReader = new VorbisWaveReader(memoryStream);
            WaveOutEvent waveOut = new WaveOutEvent();
            waveOut.Init(vorbisReader);
            waveOut.Play();

            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                await Task.Delay(100);
            }

            waveOut.Dispose();
            vorbisReader.Dispose();
            memoryStream.Dispose();
        }

        private async Task NextPokemonAsync()
        {
            // Nix tun wenn kein Pokémon geladen
            if (CurrentPokemon == null) return;

            // ID um 1 erhöhen und als String suchen
            SearchText = (CurrentPokemon.Id + 1).ToString();
            await SearchPokemonAsync();
        }

        private async Task PreviousPokemonAsync()
        {
            // Nix tun wenn kein Pokémon geladen oder schon bei #1
            if (CurrentPokemon == null) return;
            if (CurrentPokemon.Id <= 1) return;

            // ID um 1 verringern und als String suchen
            SearchText = (CurrentPokemon.Id - 1).ToString();
            await SearchPokemonAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



    }
}