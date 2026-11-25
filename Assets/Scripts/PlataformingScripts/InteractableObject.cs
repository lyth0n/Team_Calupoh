using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Objeto a activar")]
    [Tooltip("Arrastra aquí el GameObject que se activará al interactuar.")]
    public GameObject targetObjectToActivate;

    [Tooltip("Si está activo, alterna entre activado y desactivado en cada interacción.")]
    public bool toggleActiveState = false;

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
    }
}