using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PaddleController : MonoBehaviour
{
    public event Action LaunchPressed;

    [Tooltip("X da face interna das paredes laterais")]
    [SerializeField] float boundaryX = 8.59f;
    [SerializeField] float keyboardSpeed = 14f;
    [SerializeField] Camera cam;

    BreakoutControls controls;
    Rigidbody2D rb;
    BoxCollider2D col;
    float targetX;
    Vector2 lastPointer;

    // Lido dos bounds para continuar correto quando o power-up aumentar a escala
    public float HalfWidth => col.bounds.extents.x;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        if (cam == null) cam = Camera.main;

        controls = new BreakoutControls();
        targetX = rb.position.x;
    }

    void OnEnable()
    {
        controls.Gameplay.Enable();
        controls.Gameplay.Launch.performed += OnLaunch;
    }

    void OnDisable()
    {
        controls.Gameplay.Launch.performed -= OnLaunch;
        controls.Gameplay.Disable();
    }

    void OnDestroy() => controls.Dispose();

    void OnLaunch(InputAction.CallbackContext _) => LaunchPressed?.Invoke();

    void Update()
    {
        float axis = controls.Gameplay.Move.ReadValue<float>();
        Vector2 pointer = controls.Gameplay.Point.ReadValue<Vector2>();

        // Teclado tem prioridade; o mouse só assume quando se move
        if (Mathf.Abs(axis) > 0.01f)
            targetX += axis * keyboardSpeed * Time.unscaledDeltaTime;
        else if ((pointer - lastPointer).sqrMagnitude > 0.5f)
            targetX = cam.ScreenToWorldPoint(pointer).x;

        lastPointer = pointer;

        float limit = boundaryX - HalfWidth;
        targetX = Mathf.Clamp(targetX, -limit, limit);
    }

    void FixedUpdate()
    {
        rb.MovePosition(new Vector2(targetX, rb.position.y));
    }
}
