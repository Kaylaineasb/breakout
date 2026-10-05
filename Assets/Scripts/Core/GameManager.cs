using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState { MainMenu, Playing, Paused, GameOver, Victory }

public class GameManager : MonoBehaviour
{
    public static event Action<GameState> StateChanged;
    public static event Action<int> ScoreChanged;
    public static event Action<int> LivesChanged;
    public static event Action<int> LevelStarted; // número do nível (1, 2, 3...)
    public static event Action LifeLost;
    public static event Action LevelCleared; // nível limpo (não dispara no último: lá vira Victory)
    public static event Action<int[]> RecentScoresChanged; // mais recente primeiro

    [SerializeField] PaddleController paddle;
    [SerializeField] BrickSpawner spawner;
    [SerializeField] LevelConfig[] levels;
    [SerializeField] int startingLives = 3;
    [SerializeField] float levelTransitionDelay = 0.6f;

    public GameState State { get; private set; }
    public int Score => score;
    public int Level => levelIndex + 1;
    public int[] RecentScores => recentScores;

    BreakoutControls controls;
    int[] recentScores = Array.Empty<int>();
    int score;
    int lives;
    int levelIndex;

    void Awake() => controls = new BreakoutControls();

    void OnEnable()
    {
        controls.Gameplay.Enable();
        controls.Gameplay.Pause.performed += OnPausePressed;
        Ball.Lost += HandleBallLost;
        Brick.Destroyed += HandleBrickDestroyed;
    }

    void OnDisable()
    {
        controls.Gameplay.Pause.performed -= OnPausePressed;
        controls.Gameplay.Disable();
        Ball.Lost -= HandleBallLost;
        Brick.Destroyed -= HandleBrickDestroyed;
    }

    void OnDestroy()
    {
        controls.Dispose();
        Time.timeScale = 1f;
    }

    void Start()
    {
        recentScores = ScoreHistory.Load();
        GoToMenu();
    }

    // ---------- API pública (chamada pelos botões da UI) ----------

    public void StartGame()
    {
        StopAllCoroutines();
        score = 0;
        lives = startingLives;
        levelIndex = 0;
        ScoreChanged?.Invoke(score);
        LivesChanged?.Invoke(lives);

        LoadLevel(levelIndex);
        SetState(GameState.Playing);
    }

    public void TogglePause()
    {
        if (State == GameState.Playing) SetState(GameState.Paused);
        else if (State == GameState.Paused) SetState(GameState.Playing);
    }

    // O nível 1 fica de fundo atrás do menu
    public void GoToMenu()
    {
        StopAllCoroutines();
        levelIndex = 0;
        LoadLevel(levelIndex);
        SetState(GameState.MainMenu);
    }

    // ---------- Fluxo interno ----------

    void SetState(GameState newState)
    {
        State = newState;
        Time.timeScale = newState == GameState.Paused ? 0f : 1f;
        paddle.enabled = newState == GameState.Playing; // desliga input da paddle fora do jogo
        StateChanged?.Invoke(newState);
    }

    // Fim de partida (game over ou vitória): registra a pontuação
    void EndGame(GameState endState)
    {
        recentScores = ScoreHistory.Add(score);
        RecentScoresChanged?.Invoke(recentScores);
        SetState(endState);
    }

    void LoadLevel(int index)
    {
        LevelConfig cfg = levels[index];
        spawner.Spawn(cfg);
        ResetBalls(cfg.ballSpeed);
        LevelStarted?.Invoke(index + 1);
    }

    // Mantém só uma bola, presa à paddle
    void ResetBalls(float speed)
    {
        for (int i = Ball.Active.Count - 1; i >= 1; i--)
            RemoveBall(Ball.Active[i]);

        if (Ball.Active.Count == 0)
        {
            Debug.LogError("Nenhuma bola ativa na cena.");
            return;
        }

        Ball main = Ball.Active[0];
        main.Speed = speed;
        main.AttachToPaddle();
    }

    static void RemoveBall(Ball ball)
    {
        ball.gameObject.SetActive(false); // sai da lista Active na hora
        Destroy(ball.gameObject);
    }

    void AddScore(int amount)
    {
        score += amount;
        ScoreChanged?.Invoke(score);
    }

    // ---------- Handlers de eventos ----------

    void HandleBallLost(Ball ball)
    {
        if (State != GameState.Playing)
        {
            ball.AttachToPaddle();
            return;
        }

        // Multiball: perder uma bola extra não custa vida
        if (Ball.Active.Count > 1)
        {
            RemoveBall(ball);
            return;
        }

        lives--;
        LivesChanged?.Invoke(lives);
        LifeLost?.Invoke();
        ball.AttachToPaddle();

        if (lives <= 0) EndGame(GameState.GameOver);
    }

    void HandleBrickDestroyed(Brick brick)
    {
        if (State != GameState.Playing) return;

        AddScore(brick.Points);
        if (spawner.NotifyBrickDestroyed())
            StartCoroutine(LevelClearedRoutine());
    }

    IEnumerator LevelClearedRoutine()
    {
        yield return null; // sai do callback de física antes de mexer nas bolas
        ResetBalls(levels[levelIndex].ballSpeed);

        if (levelIndex + 1 >= levels.Length)
        {
            EndGame(GameState.Victory);
            yield break;
        }

        LevelCleared?.Invoke();
        yield return new WaitForSeconds(levelTransitionDelay);
        levelIndex++;
        LoadLevel(levelIndex);
    }

    void OnPausePressed(InputAction.CallbackContext _) => TogglePause();
}
