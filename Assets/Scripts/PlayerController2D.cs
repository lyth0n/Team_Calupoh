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

    [Header("Interacción")]
    private GameObject currentInteractable;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Congela la rotación en Z para que el personaje no ruede
        rb.freezeRotation = true;
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
        // En Unity 6 se usa linearVelocity en lugar de velocity
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    // --- Callbacks del Input System (Invoke Unity Events) ---

    public void OnMove(InputAction.CallbackContext context)
    {
        // Soporta tanto Value (float) como Value (Vector2) si usas un binding 2D Vector
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
        Debug.Log("¡Interactuando con: " + currentInteractable.name + "!");
        // Aquí puedes invocar eventos o métodos del objeto interactuable
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Interactable"))
        {
            currentInteractable = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Interactable") && collision.gameObject == currentInteractable)
        {
            currentInteractable = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        }
    }
}