using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameManager game;

    [Header("Painéis")]
    [SerializeField] GameObject menuPanel;
    [SerializeField] GameObject hudPanel;
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject endPanel;

    [Header("HUD")]
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text livesText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] UIPunch scorePunch;
    [SerializeField] UIPunch livesPunch;

    [Header("Fim de jogo")]
    [SerializeField] TMP_Text endTitleText;
    [SerializeField] TMP_Text finalScoreText;
    [SerializeField] TMP_Text recentScoresText;

    [Header("Botões")]
    [SerializeField] Button playButton;
    [SerializeField] Button resumeButton;
    [SerializeField] Button pauseMenuButton;
    [SerializeField] Button restartButton;
    [SerializeField] Button endMenuButton;

    void Awake()
    {
        playButton.onClick.AddListener(game.StartGame);
        resumeButton.onClick.AddListener(game.TogglePause);
        pauseMenuButton.onClick.AddListener(game.GoToMenu);
        restartButton.onClick.AddListener(game.StartGame);
        endMenuButton.onClick.AddListener(game.GoToMenu);
    }

    void OnEnable()
    {
        GameManager.StateChanged += HandleState;
        GameManager.ScoreChanged += HandleScore;
        GameManager.LivesChanged += HandleLives;
        GameManager.LevelStarted += HandleLevel;
        GameManager.LifeLost += HandleLifeLost;
    }

    void OnDisable()
    {
        GameManager.StateChanged -= HandleState;
        GameManager.ScoreChanged -= HandleScore;
        GameManager.LivesChanged -= HandleLives;
        GameManager.LevelStarted -= HandleLevel;
        GameManager.LifeLost -= HandleLifeLost;
    }

    void HandleState(GameState state)
    {
        bool isEnd = state is GameState.GameOver or GameState.Victory;

        menuPanel.SetActive(state == GameState.MainMenu);
        hudPanel.SetActive(state is GameState.Playing or GameState.Paused);
        pausePanel.SetActive(state == GameState.Paused);
        endPanel.SetActive(isEnd);

        if (isEnd) FillEndPanel(state);
    }

    void HandleScore(int score)
    {
        scoreText.text = score.ToString();
        if (score > 0) scorePunch.Play();
    }

    void HandleLives(int lives) => livesText.text = $"VIDAS {lives}";
    void HandleLevel(int level) => levelText.text = $"NÍVEL {level}";
    void HandleLifeLost() => livesPunch.Play();

    void FillEndPanel(GameState state)
    {
        endTitleText.text = state == GameState.Victory ? "VITÓRIA!" : "GAME OVER";
        finalScoreText.text = $"PONTUAÇÃO: {game.Score}";

        var sb = new StringBuilder("ÚLTIMAS PARTIDAS\n");
        int[] scores = game.RecentScores;
        for (int i = 0; i < scores.Length; i++)
        {
            sb.Append($"{i + 1}.  {scores[i]}");
            if (i == 0) sb.Append("  <color=#FFD60A>(esta)</color>");
            sb.Append('\n');
        }
        recentScoresText.text = sb.ToString();
    }
}
