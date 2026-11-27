using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Dialogue
{
    [Serializable]
    public struct DialogueChoice
    {
        [Tooltip("Text displayed on the choice UI button.")]
        public string buttonLabel;

        [Tooltip("Index of the DialogueNode in the tree to jump to when this option is clicked (-1 ends the cutscene).")]
        public int targetNodeIndex;
    }

    [Serializable]
    public class DialogueNode
    {
        [Tooltip("Editor label to help identify this branch (e.g., 'Intro', 'Accept Quest', 'Refuse Quest').")]
        public string nodeName = "New Branch";

        [Tooltip("Personalized sequence of DialogueLine entries played in this branch.")]
        public List<DialogueLine> lines = new List<DialogueLine>();

        [Header("Branching Options (Shown after the last line of this node)")]
        [Tooltip("Enable branching choices at the end of this node. If false, the cutscene ends after the last line.")]
        public bool hasChoices;

        public DialogueChoice option1 = new DialogueChoice { buttonLabel = "Option 1", targetNodeIndex = -1 };
        public DialogueChoice option2 = new DialogueChoice { buttonLabel = "Option 2", targetNodeIndex = -1 };
    }
}