using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0f, 2f, -10f);

    void LateUpdate()
    {
        if (target == null) return;

        // Posición deseada: sigue la X del objetivo, pero mantiene fija la altura Y de la cámara (o ajustada por el offset)
        float desiredX = target.position.x + offset.x;
        float smoothedX = Mathf.Lerp(transform.position.x, desiredX, smoothSpeed);

        transform.position = new Vector3(smoothedX, transform.position.y + offset.y, offset.z);
    }
}
