using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DialogueController : MonoBehaviour
{
    [Header("Referencias de UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;

    private string currentDialogueId;
    private DialogueNode currentNode;

    public void StartDialogue(string dialogueId, DialogueNode startingNode)
    {
        currentDialogueId = dialogueId;
        dialoguePanel.SetActive(true);
        DisplayNode(startingNode);
    }

    private void DisplayNode(DialogueNode node)
    {
        currentNode = node;

        // Actualizar textos de UI
        speakerNameText.text = node.speakerName;
        dialogueText.text = node.dialogueText;

        // Limpiar botones anteriores
        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }

        // Guardar progreso en GameManager
        GameManager.Instance.SaveDialogueProgress(
            dialogueId: currentDialogueId,
            currentNodeId: currentNode.nodeId,
            visitedNodeId: currentNode.nodeId,
            completed: currentNode.marksDialogueAsComplete
        );

        // Crear botones evaluando condiciones
        foreach (var choice in node.choices)
        {
            // Validar si cumple con las condiciones del GameManager
            if (!CheckConditions(choice.conditions))
            {
                continue; // Si no cumple, omitimos esta opción
            }

            GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer);
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = choice.choiceText;
            }

            Button btn = buttonObj.GetComponent<Button>();
            var capturedChoice = choice; // Prevenir problemas de closure en el bucle

            btn.onClick.AddListener(() => OnChoiceSelected(capturedChoice));
        }
    }

    private bool CheckConditions(List<DialogueCondition> conditions)
    {
        foreach (var cond in conditions)
        {
            switch (cond.conditionType)
            {
                case DialogueCondition.ConditionType.EqualsBool:
                    bool currentBool = GameManager.Instance.GetVariable<bool>(cond.variableKey, false);
                    if (currentBool != cond.boolValue) return false;
                    break;

                case DialogueCondition.ConditionType.GreaterThanInt:
                    int currentIntG = GameManager.Instance.GetVariable<int>(cond.variableKey, 0);
                    if (currentIntG <= cond.intValue) return false;
                    break;

                case DialogueCondition.ConditionType.EqualsInt:
                    int currentIntE = GameManager.Instance.GetVariable<int>(cond.variableKey, 0);
                    if (currentIntE != cond.intValue) return false;
                    break;
            }
        }
        return true; // Todas las condiciones se cumplieron
    }

    private void OnChoiceSelected(DialogueChoice choice)
    {
        // Ejecutar las acciones o recompensas en el GameManager
        ExecuteActions(choice.actions);

        if (choice.nextNode != null)
        {
            DisplayNode(choice.nextNode);
        }
        else
        {
            CloseDialogue();
        }
    }

    private void ExecuteActions(List<DialogueAction> actions)
    {
        foreach (var action in actions)
        {
            switch (action.actionType)
            {
                case DialogueAction.ActionType.SetBoolTrue:
                    GameManager.Instance.SetVariable(action.variableKey, true);
                    break;

                case DialogueAction.ActionType.SetBoolFalse:
                    GameManager.Instance.SetVariable(action.variableKey, false);
                    break;

                case DialogueAction.ActionType.AddToInt:
                    int currentVal = GameManager.Instance.GetVariable<int>(action.variableKey, 0);
                    GameManager.Instance.SetVariable(action.variableKey, currentVal + action.intValue);
                    break;

                case DialogueAction.ActionType.SetInt:
                    GameManager.Instance.SetVariable(action.variableKey, action.intValue);
                    break;
            }
        }
    }

    public void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
    }
}
