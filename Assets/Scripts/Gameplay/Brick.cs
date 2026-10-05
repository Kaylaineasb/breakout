using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class Brick : MonoBehaviour
{
    public static event Action<Brick> Hit;        // levou dano mas não quebrou
    public static event Action<Brick> Destroyed;

    SpriteRenderer sr;
    Color baseColor;
    int hp;

    public Color Color => baseColor;
    public int Points { get; private set; }

    void Awake() => sr = GetComponent<SpriteRenderer>();

    public void Setup(Color color, int hitPoints, int points)
    {
        baseColor = color;
        hp = hitPoints;
        Points = points * hitPoints; // tijolo resistente vale mais
        UpdateVisual();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<Ball>(out _))
            TakeHit();
    }

    public void TakeHit()
    {
        if (hp <= 0) return; // evita dano duplo no mesmo frame (multiball)

        hp--;
        if (hp <= 0)
        {
            Destroyed?.Invoke(this);
            Destroy(gameObject);
        }
        else
        {
            Hit?.Invoke(this);
            UpdateVisual();
        }
    }

    // Quanto mais vida, mais claro; escurece até a cor base a cada hit
    void UpdateVisual() => sr.color = Color.Lerp(baseColor, Color.white, (hp - 1) * 0.3f);
}
