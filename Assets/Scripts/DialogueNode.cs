using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueNode", menuName = "Sistema de Dialogos/Nodo de Dialogo")]
public class DialogueNode : ScriptableObject
{
    [Header("Identificador Único")]
    public string nodeId;

    [Header("Contenido")]
    [TextArea(2, 4)]
    public string dialogueText;
    public Sprite speakerPortrait;
    public string speakerName;

    [Header("Opciones de Respuesta")]
    public List<DialogueChoice> choices = new List<DialogueChoice>();

    [Header("Estado Opcional")]
    public bool marksDialogueAsComplete = false;
}

[System.Serializable]
public class DialogueCondition
{
    public string variableKey; // Llave en el GameManager (ej: "tiene_llave", "oro")

    public enum ConditionType { EqualsBool, GreaterThanInt, EqualsInt }
    public ConditionType conditionType;

    public bool boolValue;
    public int intValue;
}

[System.Serializable]
public class DialogueAction
{
    public string variableKey;

    public enum ActionType { SetBoolTrue, SetBoolFalse, AddToInt, SetInt }
    public ActionType actionType;

    public bool boolValue;
    public int intValue;
}

[System.Serializable]
public class DialogueChoice
{
    [TextArea(1, 2)]
    public string choiceText;
    public DialogueNode nextNode;

    [Header("Requisitos para mostrar esta opción")]
    public List<DialogueCondition> conditions = new List<DialogueCondition>();

    [Header("Acciones / Recompensas al elegir esta opción")]
    public List<DialogueAction> actions = new List<DialogueAction>();
}
