using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 8f;
    private float horizontalInput;

    [Header("Salto")]
    public float jumpForce = 12f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("Punto de Inicio (Respawn)")]
    [Tooltip("Opcional: asigna un GameObject vacío como punto de inicio. Si queda vacío, usa la posición inicial del jugador.")]
    public Transform startPoint;
    private Vector3 staticStartPosition;

    [Header("Interacción")]
    private GameObject currentInteractable;
    private InteractableObject currentInteractableComponent;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        // Guarda el punto de inicio estático al arrancar la escena
        staticStartPosition = startPoint != null ? startPoint.position : transform.position;
    }

    void Update()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    public void RespawnAtStart()
    {
        Vector3 targetPosition = startPoint != null ? startPoint.position : staticStartPosition;

        transform.position = targetPosition;
        rb.linearVelocity = Vector2.zero;
        horizontalInput = 0f;
        currentInteractable = null;
        currentInteractableComponent = null;
    }

    // --- Callbacks del Input System (Invoke Unity Events) ---

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.valueType == typeof(Vector2))
        {
            horizontalInput = context.ReadValue<Vector2>().x;
        }
        else
        {
            horizontalInput = context.ReadValue<float>();
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && currentInteractable != null)
        {
            Interact();
        }
    }

    // -------------------------------------------------------

    void Interact()
    {
        if (currentInteractableComponent != null)
        {
            currentInteractableComponent.Interact();
        }
        else if (currentInteractable != null)
        {
            Debug.Log("¡Interactuando con: " + currentInteractable.name + "!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Interactable"))
        {
            currentInteractable = collision.gameObject;
            currentInteractableComponent = collision.GetComponent<InteractableObject>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Interactable") && collision.gameObject == currentInteractable)
        {
            currentInteractable = null;
            currentInteractableComponent = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        }

        // Visualiza el punto de inicio en el editor
        Gizmos.color = Color.green;
        Vector3 gizmoStart = startPoint != null ? startPoint.position : (Application.isPlaying ? staticStartPosition : transform.position);
        Gizmos.DrawWireSphere(gizmoStart, 0.35f);
    }
}