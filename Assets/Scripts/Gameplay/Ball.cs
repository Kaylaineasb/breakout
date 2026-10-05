using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    public static event Action<Ball> Lost;
    public static event Action<Ball> PaddleHit;
    public static event Action<Ball> WallHit;

    // Bolas ativas na cena (multiball); a primeira é sempre a que volta para a paddle
    public static readonly List<Ball> Active = new();

    [SerializeField] PaddleController paddle;
    [SerializeField] float speed = 8f;

    [Tooltip("Ângulo máximo (em relação à vertical) ao bater na borda da paddle")]
    [SerializeField, Range(10f, 75f)] float maxBounceAngle = 60f;

    [Tooltip("Componente vertical mínima da direção; evita a bola quase horizontal")]
    [SerializeField, Range(0.1f, 0.9f)] float minVertical = 0.3f;

    [SerializeField] float attachOffsetY = 0.35f;

    Rigidbody2D rb;
    bool attached;
    bool initialized;
    int wallLayer;
    int deathLayer;

    public bool IsAttached => attached;
    public float Speed { get => speed; set => speed = value; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        wallLayer = LayerMask.NameToLayer("Wall");
        deathLayer = LayerMask.NameToLayer("DeathZone");
    }

    void OnEnable()
    {
        Active.Add(this);
        Subscribe();
    }

    void OnDisable()
    {
        Active.Remove(this);
        Unsubscribe();
    }

    void Start()
    {
        // Bolas criadas por código (multiball) já chegam inicializadas
        if (!initialized) AttachToPaddle();
    }

    // Usado ao instanciar bolas por código
    public void Init(PaddleController p)
    {
        Unsubscribe();
        paddle = p;
        Subscribe();
    }

    void Subscribe()
    {
        if (paddle != null) paddle.LaunchPressed += Launch;
    }

    void Unsubscribe()
    {
        if (paddle != null) paddle.LaunchPressed -= Launch;
    }

    public void AttachToPaddle()
    {
        initialized = true;
        attached = true;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
    }

    public void LaunchFrom(Vector2 position, Vector2 direction)
    {
        initialized = true;
        attached = false;
        transform.position = position;
        rb.simulated = true;
        rb.position = position;
        rb.linearVelocity = direction.normalized * speed;
    }

    void Launch()
    {
        if (!attached) return;

        attached = false;
        rb.simulated = true;
        rb.position = transform.position;

        float angle = UnityEngine.Random.Range(-20f, 20f);
        rb.linearVelocity = Quaternion.Euler(0f, 0f, angle) * Vector2.up * speed;
    }

    void Update()
    {
        if (attached && paddle != null)
            transform.position = paddle.transform.position + Vector3.up * attachOffsetY;
    }

    void FixedUpdate()
    {
        if (attached) return;

        Vector2 v = rb.linearVelocity;
        Vector2 dir = v.sqrMagnitude > 0.0001f ? v.normalized : Vector2.down;

        // Corrige ângulos quase horizontais
        if (Mathf.Abs(dir.y) < minVertical)
        {
            float signX = dir.x >= 0f ? 1f : -1f;
            float signY = dir.y >= 0f ? 1f : -1f;
            dir = new Vector2(signX * Mathf.Sqrt(1f - minVertical * minVertical), signY * minVertical);
        }

        // Velocidade constante
        rb.linearVelocity = dir * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out PaddleController p))
        {
            // Só redireciona se bateu na parte de cima da paddle
            if (transform.position.y > p.transform.position.y)
            {
                // -1 (borda esquerda) .. 0 (centro) .. 1 (borda direita)
                float offset = Mathf.Clamp(
                    (transform.position.x - p.transform.position.x) / p.HalfWidth, -1f, 1f);

                Vector2 dir = Quaternion.Euler(0f, 0f, -offset * maxBounceAngle) * Vector2.up;
                rb.linearVelocity = dir * speed;
            }
            PaddleHit?.Invoke(this);
        }
        else if (collision.gameObject.layer == wallLayer)
        {
            WallHit?.Invoke(this);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != deathLayer) return;

        if (Lost != null) Lost(this);
        else AttachToPaddle(); // fallback até o GameManager existir (etapa 3)
    }
}
