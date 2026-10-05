using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Música")]
    [SerializeField] AudioClip menuMusic;
    [SerializeField] AudioClip gameMusic;
    [SerializeField, Range(0f, 1f)] float musicVolume = 0.5f;
    [Tooltip("Fração do volume da música na pausa e nas telas finais")]
    [SerializeField, Range(0f, 1f)] float duckedFactor = 0.35f;
    [SerializeField] float musicFadeTime = 0.6f;

    [Header("Efeitos")]
    [SerializeField] AudioClip paddleHit;
    [SerializeField] AudioClip wallHit;
    [SerializeField] AudioClip brickHit;      // tijolo resistente levou dano
    [SerializeField] AudioClip brickBreak;
    [SerializeField] AudioClip lifeLost;
    [SerializeField] AudioClip levelCleared;
    [SerializeField] AudioClip victory;
    [SerializeField] AudioClip gameOver;
    [SerializeField, Range(0f, 1f)] float sfxVolume = 0.8f;
    [SerializeField, Range(0f, 0.3f)] float pitchVariation = 0.08f;

    [Header("Combo")]
    [Tooltip("Quanto o pitch sobe a cada tijolo quebrado sem tocar na paddle")]
    [SerializeField] float comboPitchStep = 0.05f;
    [SerializeField] float comboPitchMax = 0.5f;

    [SerializeField, Min(1)] int sfxVoices = 8;

    AudioSource music;
    AudioSource[] voices;
    int nextVoice;
    float musicTarget;
    int combo;

    void Awake()
    {
        music = gameObject.AddComponent<AudioSource>();
        music.loop = true;
        music.playOnAwake = false;
        music.volume = 0f;

        // Várias fontes para sons simultâneos, cada uma com seu pitch
        voices = new AudioSource[sfxVoices];
        for (int i = 0; i < sfxVoices; i++)
        {
            voices[i] = gameObject.AddComponent<AudioSource>();
            voices[i].playOnAwake = false;
        }
    }

    void OnEnable()
    {
        GameManager.StateChanged += HandleState;
        GameManager.LifeLost += HandleLifeLost;
        GameManager.LevelCleared += HandleLevelCleared;
        Ball.PaddleHit += HandlePaddleHit;
        Ball.WallHit += HandleWallHit;
        Brick.Hit += HandleBrickHit;
        Brick.Destroyed += HandleBrickDestroyed;
    }

    void OnDisable()
    {
        GameManager.StateChanged -= HandleState;
        GameManager.LifeLost -= HandleLifeLost;
        GameManager.LevelCleared -= HandleLevelCleared;
        Ball.PaddleHit -= HandlePaddleHit;
        Ball.WallHit -= HandleWallHit;
        Brick.Hit -= HandleBrickHit;
        Brick.Destroyed -= HandleBrickDestroyed;
    }

    void Update()
    {
        // Fade da música; tempo não escalado para funcionar na pausa
        float step = musicVolume / Mathf.Max(musicFadeTime, 0.01f) * Time.unscaledDeltaTime;
        music.volume = Mathf.MoveTowards(music.volume, musicTarget, step);
    }

    // ---------- API ----------

    public void PlaySfx(AudioClip clip, float pitch = 1f, float volumeScale = 1f)
    {
        if (clip == null) return;

        AudioSource source = voices[nextVoice];
        nextVoice = (nextVoice + 1) % voices.Length;

        source.pitch = pitch + Random.Range(-pitchVariation, pitchVariation);
        source.PlayOneShot(clip, sfxVolume * volumeScale);
    }

    void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (music.clip == clip && music.isPlaying) return; // já tocando: só ajusta o volume

        music.clip = clip;
        music.volume = 0f;
        music.Play();
    }

    // ---------- Handlers ----------

    void HandleState(GameState state)
    {
        switch (state)
        {
            case GameState.MainMenu:
                PlayMusic(menuMusic != null ? menuMusic : gameMusic);
                musicTarget = musicVolume;
                break;

            case GameState.Playing:
                combo = 0;
                PlayMusic(gameMusic != null ? gameMusic : menuMusic);
                musicTarget = musicVolume;
                break;

            case GameState.Paused:
                musicTarget = musicVolume * duckedFactor;
                break;

            case GameState.GameOver:
                PlaySfx(gameOver);
                musicTarget = musicVolume * duckedFactor;
                break;

            case GameState.Victory:
                PlaySfx(victory);
                musicTarget = musicVolume * duckedFactor;
                break;
        }
    }

    void HandlePaddleHit(Ball _)
    {
        combo = 0;
        PlaySfx(paddleHit);
    }

    void HandleWallHit(Ball _) => PlaySfx(wallHit, 1f, 0.5f);

    void HandleBrickHit(Brick _) => PlaySfx(brickHit);

    void HandleBrickDestroyed(Brick _)
    {
        // Sequência sem tocar na paddle deixa o som cada vez mais agudo
        float pitch = 1f + Mathf.Min(combo * comboPitchStep, comboPitchMax);
        combo++;
        PlaySfx(brickBreak, pitch);
    }

    void HandleLifeLost()
    {
        combo = 0;
        PlaySfx(lifeLost);
    }

    void HandleLevelCleared() => PlaySfx(levelCleared);
}
