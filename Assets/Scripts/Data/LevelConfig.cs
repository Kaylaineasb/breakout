using UnityEngine;

[CreateAssetMenu(menuName = "Breakout/Level Config", fileName = "Level_")]
public class LevelConfig : ScriptableObject
{
    [Header("Grade")]
    [Min(1)] public int rows = 6;
    [Min(1)] public int columns = 12;

    [Tooltip("Uma cor por linha, de cima para baixo (repete se houver menos cores que linhas)")]
    public Color[] rowColors =
    {
        new Color(1f, 0.30f, 0.43f),   // vermelho
        new Color(1f, 0.62f, 0.11f),   // laranja
        new Color(1f, 0.84f, 0.04f),   // amarelo
        new Color(0.29f, 0.87f, 0.50f),// verde
        new Color(0.13f, 0.83f, 0.93f),// ciano
        new Color(0.65f, 0.55f, 0.98f) // roxo
    };

    [Header("Tijolos resistentes")]
    [Range(0f, 1f)] public float twoHitChance = 0.15f;
    [Range(0f, 1f)] public float threeHitChance = 0.05f;

    [Header("Pontuação e bola")]
    [Tooltip("Pontos da linha de baixo; cada linha acima vale esse valor a mais")]
    public int pointsPerRow = 10;
    public float ballSpeed = 8f;
}
