using NAudio.Vorbis;
using NAudio.Wave;
using Pokedex.Models;
using Pokedex.Services;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Pokedex.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    // -------------------------------------------------------
    // Felder
    // -------------------------------------------------------

    private readonly PokeApiService _service = new PokeApiService();
    private readonly HttpClient _httpClient = new HttpClient();
    private PokemonModel? _latestResult;
    private PokemonModel? _currentPokemon;
    private int?[] _slots = new int?[10];   // nur IDs speichern
    private int _selectedSlotIndex = -1;
    private bool _isSaveMode = false;
    private string _searchText = string.Empty;
    private string _errorMessage = string.Empty;
    private string _statusMessage = string.Empty;



    // -------------------------------------------------------
    // Properties
    // -------------------------------------------------------

    public PokemonModel? CurrentPokemon
    {
        get => _currentPokemon;
        set { _currentPokemon = value; OnPropertyChanged(); }
    }

    public int SelectedSlotIndex
    {
        get => _selectedSlotIndex;
        set { _selectedSlotIndex = value; OnPropertyChanged(); }
    }

    public bool IsSaveMode
    {
        get => _isSaveMode;
        set { _isSaveMode = value; OnPropertyChanged(); }
    }

    public string SearchText
    {
        get => _searchText;
        set { _searchText = value; OnPropertyChanged(); }
    }


    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    // -------------------------------------------------------
    // Commands
    // -------------------------------------------------------

    public ICommand SearchCommand { get; }
    public ICommand PlaySoundCommand { get; }
    public ICommand NextPokemonCommand { get; }
    public ICommand PreviousPokemonCommand { get; }
    public ICommand ActivateSaveModeCommand { get; }

    public ICommand DeleteCommand { get; }


    // -------------------------------------------------------
    // Konstruktor
    // -------------------------------------------------------

    public MainViewModel()
    {
        SearchCommand = new RelayCommand(SearchPokemonAsync);
        PlaySoundCommand = new RelayCommand(PlayCryAsync);
        NextPokemonCommand = new RelayCommand(NextPokemonAsync);
        PreviousPokemonCommand = new RelayCommand(PreviousPokemonAsync);
        DeleteCommand = new RelayCommand(DeleteSlot);
        ActivateSaveModeCommand = new RelayCommand(ActivateSaveMode);

    }

    // -------------------------------------------------------
    // Methoden
    // -------------------------------------------------------

    // Ergebnis aus dem Service ans ViewModel übergeben
    private void UpdateUI()
    {
        if (_latestResult == null)
            StatusMessage = "Pokémon nicht gefunden!";
        else
            CurrentPokemon = _latestResult;

    }

    // Pokémon per Name oder ID aus der API laden
    private async Task SearchPokemonAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText)) return;

        StatusMessage = string.Empty;

        _latestResult = await _service.GetPokemonAsync(SearchText);

        Application.Current.Dispatcher.Invoke(UpdateUI);
    }

    // Cry als OGG-Stream laden und abspielen
    private async Task PlayCryAsync()
    {
        if (CurrentPokemon == null) return;
        if (string.IsNullOrEmpty(CurrentPokemon.SoundUrl)) return;

        byte[] audioData = await _httpClient.GetByteArrayAsync(CurrentPokemon.SoundUrl);
        MemoryStream memoryStream = new MemoryStream(audioData);
        VorbisWaveReader vorbisReader = new VorbisWaveReader(memoryStream);
        WaveOutEvent waveOut = new WaveOutEvent();
        waveOut.Init(vorbisReader);
        waveOut.Play();

        while (waveOut.PlaybackState == PlaybackState.Playing)
            await Task.Delay(100);

        waveOut.Dispose();
        vorbisReader.Dispose();
        memoryStream.Dispose();
    }

    // Nächstes Pokémon laden (ID + 1)
    private async Task NextPokemonAsync()
    {
        if (CurrentPokemon == null) return;
        SearchText = (CurrentPokemon.Id + 1).ToString();
        await SearchPokemonAsync();
    }

    // Vorheriges Pokémon laden (ID - 1, min. 1)
    private async Task PreviousPokemonAsync()
    {
        if (CurrentPokemon == null || CurrentPokemon.Id <= 1) return;
        SearchText = (CurrentPokemon.Id - 1).ToString();
        await SearchPokemonAsync();
    }

    // Slot laden oder speichern je nach Modus
    public async Task SlotClick(int index)
    {
        if (IsSaveMode)
        {
            if (CurrentPokemon == null) return;

            if (_slots[index] != null)
            {
                StatusMessage = $"⚠ Slot {index + 1} ist bereits belegt!";
                IsSaveMode = false;
                return;
            }

            _slots[index] = CurrentPokemon.Id;
            SelectedSlotIndex = index;
            IsSaveMode = false;
            StatusMessage = $"✓ {CurrentPokemon.Name} in Slot {index + 1} gespeichert!";
        }
        else
        {
            if (_slots[index] == null)
            {
                StatusMessage = $"⚠ Slot {index + 1} ist leer";
                return;
            }

            SearchText = _slots[index].ToString()!;
            SelectedSlotIndex = index;
            await SearchPokemonAsync();
            StatusMessage = $"Slot {index + 1} aktuell von {CurrentPokemon.Name} belegt.";
        }
    }

    private void DeleteSlot()
    {
        if (_selectedSlotIndex == -1)
        {
            StatusMessage = "⚠ Kein Slot ausgewählt";
            return;
        }

        _slots[_selectedSlotIndex] = null;
        StatusMessage = $"{CurrentPokemon?.Name} wurde aus Slot {_selectedSlotIndex + 1} entfernt. ";
        _selectedSlotIndex = -1;
    }

    private void ActivateSaveMode()
    {
        if (CurrentPokemon == null)
        {
            StatusMessage = "⚠ Kein Pokémon zum Speichern ausgewählt!";
            return;
        }

        IsSaveMode = true;
        StatusMessage = $"Wähle einen Slot für {CurrentPokemon.Name}";
    }

    // -------------------------------------------------------
    // INotifyPropertyChanged
    // -------------------------------------------------------
    public event PropertyChangedEventHandler? PropertyChanged;


    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}