using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [SerializeField] private AudioSource _titleMusic;
    [SerializeField] private AudioSource _lobbyMusic;
    [SerializeField] private AudioSource _fastBoardMusic;
    [SerializeField] private AudioSource _slowBoardMusic;
    [SerializeField] private AudioSource[] _sfx;
    [SerializeField] private AudioSource[] _bg; // Ideally built for more than 1 Background tune

    public AudioSource TitleMusic => _titleMusic;
    public AudioSource LobbyMusic => _lobbyMusic;
    public AudioSource FastBoardMusic => _fastBoardMusic;
    public AudioSource SlowBoardMusic => _slowBoardMusic;

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
        StopLobby();
        StopSlowBoard();
        StopFastBoard();

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
            _titleMusic.Stop();
            //_titleMusic.volume = 0f;
        }
    }
    
    public void PlayLobby() // Play main menu music
    {
        StopTitle();
        StopFastBoard();
        StopSlowBoard();

        if (_lobbyMusic != null)
            _lobbyMusic.Play();
    }

    public void StopLobby() // Stop the title music (after hitting play)
    {
        if (_lobbyMusic != null)
        {
            //_titleMusic.Stop();
            _lobbyMusic.Stop(); // Because stop doesnt work for some reason
        }
    }

    public void PlaySlowBoard() // Play main menu music
    {
        StopTitle();
        StopLobby();
        StopFastBoard();

        if (_slowBoardMusic != null)
            _slowBoardMusic.Play();
    }
    public void StopSlowBoard() // Play main menu music
    {
        if (_slowBoardMusic != null)
            _slowBoardMusic.Stop();
    }
    
    public void PlayFastBoard() // Play main menu music
    {
        StopSlowBoard();

        if (_fastBoardMusic != null)
            _fastBoardMusic.Play();
    }
    public void LowerFastBoard() // Play main menu music
    {
        if (_fastBoardMusic != null)
            _fastBoardMusic.volume = 0.2f;
    }
    public void IncreaseFastBoard() // Play main menu music
    {
        if (_fastBoardMusic != null)
            _fastBoardMusic.volume = 0.495f;
    }
    public void StopFastBoard() // Play main menu music
    {
        if (_fastBoardMusic != null)
            _fastBoardMusic.Stop();
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