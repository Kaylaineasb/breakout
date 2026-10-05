using UnityEngine;

public class BrickSpawner : MonoBehaviour
{
    [SerializeField] Brick brickPrefab;

    [Header("Área (unidades de mundo)")]
    [Tooltip("Distância entre as faces internas das paredes laterais")]
    [SerializeField] float areaWidth = 17.18f;
    [Tooltip("Y do centro da primeira linha")]
    [SerializeField] float topY = 3.35f;
    [SerializeField] float brickHeight = 0.4f;
    [SerializeField] float spacing = 0.1f;

    int remaining;

    public int Remaining => remaining;

    public void Spawn(LevelConfig cfg)
    {
        Clear();

        // Largura calculada para a grade ocupar exatamente a área
        float brickWidth = (areaWidth - (cfg.columns + 1) * spacing) / cfg.columns;
        float left = -areaWidth / 2f + spacing + brickWidth / 2f;

        for (int r = 0; r < cfg.rows; r++)
        {
            Color color = cfg.rowColors.Length > 0
                ? cfg.rowColors[r % cfg.rowColors.Length]
                : Color.white;
            int points = (cfg.rows - r) * cfg.pointsPerRow;

            for (int c = 0; c < cfg.columns; c++)
            {
                Vector2 pos = new(left + c * (brickWidth + spacing),
                                  topY - r * (brickHeight + spacing));

                Brick brick = Instantiate(brickPrefab, pos, Quaternion.identity, transform);
                brick.transform.localScale = new Vector3(brickWidth, brickHeight, 1f);
                brick.Setup(color, RollHitPoints(cfg), points);
                remaining++;
            }
        }
    }

    public void Clear()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);
        remaining = 0;
    }

    // Chamado pelo GameManager; retorna true quando o nível foi limpo
    public bool NotifyBrickDestroyed()
    {
        remaining--;
        return remaining <= 0;
    }

    static int RollHitPoints(LevelConfig cfg)
    {
        float roll = Random.value;
        if (roll < cfg.threeHitChance) return 3;
        if (roll < cfg.threeHitChance + cfg.twoHitChance) return 2;
        return 1;
    }
}
