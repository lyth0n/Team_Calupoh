using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Objeto a activar")]
    [Tooltip("Arrastra aquí el GameObject que se activará al interactuar.")]
    public GameObject targetObjectToActivate;

    [Tooltip("Si está activo, alterna entre activado y desactivado en cada interacción.")]
    public bool toggleActiveState = false;

    private static int totalInteractions = 0;

    /// <summary>
    /// Total number of interactions performed across all InteractableObjects in the scene.
    /// Other scripts can read this value via InteractableObject.TotalInteractions.
    /// </summary>
    public static int TotalInteractions => totalInteractions;

    /// <summary>
    /// Increments and returns the total interaction count.
    /// </summary>
    public static int RecordInteraction()
    {
        totalInteractions++;
        return totalInteractions;
    }

    /// <summary>
    /// Resets the scene interaction counter (useful when reloading or resetting a scene).
    /// </summary>
    public static void ResetInteractionCount()
    {
        totalInteractions = 0;
    }

    public void Interact()
    {
        Debug.Log("¡Interactuando con: " + gameObject.name + "!");

        if (targetObjectToActivate == null)
        {
            Debug.LogWarning("No se asignó 'targetObjectToActivate' en " + gameObject.name);
            return;
        }

        if (toggleActiveState)
        {
            targetObjectToActivate.SetActive(!targetObjectToActivate.activeSelf);
        }
        else
        {
            targetObjectToActivate.SetActive(true);
        }

        int count = RecordInteraction();
        Debug.Log("Total de interacciones en la escena: " + count);
    }
}