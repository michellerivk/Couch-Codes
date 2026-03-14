using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] private AudioSource _titleMusic;
    [SerializeField] private AudioSource _lobbyMusic;
    [SerializeField] private AudioSource _boardMusic;
    [SerializeField] private AudioSource[] _sfx;
    [SerializeField] private AudioSource[] _bg; // Ideally built for more than 1 Background tune

    public AudioSource TitleMusic => _titleMusic;
    public AudioSource LobbyMusic => _lobbyMusic;
    public AudioSource BoardMusic => _boardMusic;
    public void Awake()
    {
        // Singleton data structure
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public void PlayTitle() // Play main menu music
    {
        if (_titleMusic != null)
            _titleMusic.Play();
    }

    public void LowerTitle() // Lower the title music (in the settings for example)
    {
        if (_titleMusic != null)
            _titleMusic.volume = 0.2f;
    }

    public void IncreaseTitle() // Increase the title music (after closing the settings for example)
    {
        if (_titleMusic != null)
            _titleMusic.volume = 0.6f;
    }

    public void StopTitle() // Stop the title music (after hitting play)
    {
        if (_titleMusic != null)
        {
            //_titleMusic.Stop();
            _titleMusic.volume = 0f; // Because stop doesnt work for some reason
        }
    }
    
    public void PlayLobby() // Play main menu music
    {
        StopTitle();

        if (_lobbyMusic != null)
            _lobbyMusic.Play();
    }

    public void LowerLobby() // Lower the title music (in the settings for example)
    {
        if (_lobbyMusic != null)
            _lobbyMusic.volume = 0.2f;
    }

    public void IncreaseLobby() // Increase the title music (after closing the settings for example)
    {
        if (_lobbyMusic != null)
            _lobbyMusic.volume = 0.6f;
    }

    public void StopLobby() // Stop the title music (after hitting play)
    {
        if (_lobbyMusic != null)
        {
            //_titleMusic.Stop();
            _lobbyMusic.volume = 0f; // Because stop doesnt work for some reason
        }
    }

    public void PlayBoard() // Play main menu music
    {
        StopTitle();
        StopLobby();

        if (_boardMusic != null)
            _boardMusic.Play();
    }

    public void PlaySFX(int sfxToPlay) // Dragging an item, item dropping, cracks on the floor etc...
    {
        _sfx[sfxToPlay].Stop(); // If a sound effect is already playing, stop it
        _sfx[sfxToPlay].Play();
    }

    public void PlaySFXPitchAdjusted(int sfxToPlay) // Changing the pitch on a sound that you hear over and over
    {
        _sfx[sfxToPlay].pitch = Random.Range(0.8f, 1.2f);

        PlaySFX(sfxToPlay);
    }

    public void PlayBG(int bgToPlay) // Play background music after hitting start
    {
        _bg[bgToPlay].Stop();
        _bg[bgToPlay].Play();
    }
}