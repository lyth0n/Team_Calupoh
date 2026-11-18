using UnityEngine;

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
    public KeyCode interactKey = KeyCode.E;
    private GameObject currentInteractable;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Asegúrate de congelar la rotación en Z para que el personaje no ruede
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Movimiento horizontal con A (izquierda) y D (derecha)
        horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A)) horizontalInput = -1f;
        if (Input.GetKey(KeyCode.D)) horizontalInput = 1f;

        // Salto con W (comprueba si está tocando el suelo)
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Interacción con objeto cercano usando la tecla E
        if (Input.GetKeyDown(interactKey) && currentInteractable != null)
        {
            Interact();
        }
    }

    void FixedUpdate()
    {
        // Aplicar velocidad horizontal manteniendo la vertical actual
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

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

    // Visualizar el radio del GroundCheck en el editor
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, 0.2f);
        }
    }
}
