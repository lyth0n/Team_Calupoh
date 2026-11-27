using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace Game.Dialogue
{
    public class CutsceneActivator : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Root panel of the dialogue/cutscene UI.")]
        [SerializeField] private GameObject dialoguePanel;

        [SerializeField] private TMP_Text speakerNameText;
        [SerializeField] private TMP_Text dialogueBodyText;
        [SerializeField] private Image leftPortraitImage;
        [SerializeField] private Image rightPortraitImage;
        [SerializeField] private Image screenIllustrationImage;
        [SerializeField] private AudioSource voiceAudioSource;

        [Header("Portrait Speaker Highlighting")]
        [Tooltip("Color applied to the portrait currently speaking.")]
        [SerializeField] private Color activePortraitColor = Color.white;

        [Tooltip("Color applied to the non-speaking portrait when both are visible.")]
        [SerializeField] private Color inactivePortraitColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        [Tooltip("Scale multiplier applied to the active speaker portrait.")]
        [SerializeField] private float activePortraitScale = 1.05f;

        [Tooltip("Scale multiplier applied to the inactive speaker portrait.")]
        [SerializeField] private float inactivePortraitScale = 0.95f;

        [Header("Navigation & Choice Buttons")]
        [SerializeField] private Button nextButton;
        [SerializeField] private Button option1Button;
        [SerializeField] private TMP_Text option1ButtonText;
        [SerializeField] private Button option2Button;
        [SerializeField] private TMP_Text option2ButtonText;

        [Header("Serialized Dialogue Tree")]
        [Tooltip("Node 0 is the starting branch of the cutscene.")]
        [SerializeField] private List<DialogueNode> dialogueTree = new List<DialogueNode>();

        [Header("Cutscene Events")]
        public UnityEvent onCutsceneStarted;
        public UnityEvent onCutsceneEnded;

        private int _currentNodeIndex = -1;
        private int _currentLineIndex = -1;
        private bool _isPlaying;

        public IReadOnlyList<DialogueNode> DialogueTree => dialogueTree;
        public bool IsPlaying => _isPlaying;

        private void Awake()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            SetChoiceButtonsActive(false);
        }

        private void OnEnable()
        {
            if (nextButton != null)
                nextButton.onClick.AddListener(AdvanceDialogue);

            if (option1Button != null)
                option1Button.onClick.AddListener(SelectOption1);

            if (option2Button != null)
                option2Button.onClick.AddListener(SelectOption2);
        }

        private void OnDisable()
        {
            if (nextButton != null)
                nextButton.onClick.RemoveListener(AdvanceDialogue);

            if (option1Button != null)
                option1Button.onClick.RemoveListener(SelectOption1);

            if (option2Button != null)
                option2Button.onClick.RemoveListener(SelectOption2);
        }

        /// <summary>
        /// Starts the cutscene from Node 0. Call from a trigger, interaction script, or UnityEvent.
        /// </summary>

        [SerializeField] private bool playOnSceneStart = true;

        private void Start()
        {
            if (playOnSceneStart)
            {
                StartCutscene();
            }
        }

        public void StartCutscene()
        {
            if (dialogueTree == null || dialogueTree.Count == 0)
            {
                Debug.LogWarning($"[{nameof(CutsceneActivator)}] Dialogue tree is empty on {gameObject.name}.", this);
                return;
            }

            _isPlaying = true;
            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);

            onCutsceneStarted?.Invoke();
            EnterNode(0);
        }

        /// <summary>
        /// Advances to the next DialogueLine in the current node, or reveals choices / ends the cutscene.
        /// </summary>
        public void AdvanceDialogue()
        {
            if (!_isPlaying || !IsValidNodeIndex(_currentNodeIndex)) return;

            DialogueNode currentNode = dialogueTree[_currentNodeIndex];
            int nextLine = _currentLineIndex + 1;

            if (nextLine < currentNode.lines.Count)
            {
                _currentLineIndex = nextLine;
                DisplayLine(currentNode.lines[_currentLineIndex]);
                UpdateNavigationState(currentNode);
            }
            else if (!currentNode.hasChoices)
            {
                EndCutscene();
            }
        }

        public void SelectOption1()
        {
            if (!_isPlaying || !IsValidNodeIndex(_currentNodeIndex)) return;
            int targetIndex = dialogueTree[_currentNodeIndex].option1.targetNodeIndex;
            HandleChoiceSelection(targetIndex);
        }

        public void SelectOption2()
        {
            if (!_isPlaying || !IsValidNodeIndex(_currentNodeIndex)) return;
            int targetIndex = dialogueTree[_currentNodeIndex].option2.targetNodeIndex;
            HandleChoiceSelection(targetIndex);
        }

        private void HandleChoiceSelection(int targetNodeIndex)
        {
            SetChoiceButtonsActive(false);

            if (IsValidNodeIndex(targetNodeIndex))
            {
                EnterNode(targetNodeIndex);
            }
            else
            {
                EndCutscene();
            }
        }

        private void EnterNode(int nodeIndex)
        {
            if (!IsValidNodeIndex(nodeIndex))
            {
                EndCutscene();
                return;
            }

            _currentNodeIndex = nodeIndex;
            DialogueNode node = dialogueTree[_currentNodeIndex];

            if (node.lines == null || node.lines.Count == 0)
            {
                UpdateNavigationState(node);
                return;
            }

            _currentLineIndex = 0;
            DisplayLine(node.lines[_currentLineIndex]);
            UpdateNavigationState(node);
        }

        private void DisplayLine(DialogueLine line)
        {
            if (speakerNameText != null)
                speakerNameText.text = line.speakerName;

            if (dialogueBodyText != null)
                dialogueBodyText.text = line.text;

            ApplyPortrait(
                leftPortraitImage,
                line.leftPortrait,
                isActive: line.activeSpeaker == SpeakerSide.Left,
                hasOtherActive: line.activeSpeaker == SpeakerSide.Right
            );

            ApplyPortrait(
                rightPortraitImage,
                line.rightPortrait,
                isActive: line.activeSpeaker == SpeakerSide.Right,
                hasOtherActive: line.activeSpeaker == SpeakerSide.Left
            );

            if (screenIllustrationImage != null)
            {
                screenIllustrationImage.sprite = line.screenImage;
                screenIllustrationImage.enabled = line.screenImage != null;
            }

            if (voiceAudioSource != null)
            {
                voiceAudioSource.Stop();
                if (line.voiceClip != null)
                {
                    voiceAudioSource.clip = line.voiceClip;
                    voiceAudioSource.Play();
                }
            }
        }

        private void ApplyPortrait(Image portraitSlot, Sprite sprite, bool isActive, bool hasOtherActive)
        {
            if (portraitSlot == null) return;

            portraitSlot.sprite = sprite;
            portraitSlot.enabled = sprite != null;

            if (sprite == null) return;

            if (isActive)
            {
                portraitSlot.color = activePortraitColor;
                portraitSlot.rectTransform.localScale = Vector3.one * activePortraitScale;
            }
            else if (hasOtherActive)
            {
                portraitSlot.color = inactivePortraitColor;
                portraitSlot.rectTransform.localScale = Vector3.one * inactivePortraitScale;
            }
            else
            {
                portraitSlot.color = activePortraitColor;
                portraitSlot.rectTransform.localScale = Vector3.one;
            }
        }

        private void UpdateNavigationState(DialogueNode node)
        {
            bool isLastLineInNode = node.lines == null || _currentLineIndex >= node.lines.Count - 1;

            if (isLastLineInNode && node.hasChoices)
            {
                if (nextButton != null)
                    nextButton.gameObject.SetActive(false);

                SetChoiceButtonsActive(true);

                if (option1ButtonText != null)
                    option1ButtonText.text = node.option1.buttonLabel;

                if (option2ButtonText != null)
                    option2ButtonText.text = node.option2.buttonLabel;
            }
            else
            {
                SetChoiceButtonsActive(false);

                if (nextButton != null)
                    nextButton.gameObject.SetActive(true);
            }
        }

        private void SetChoiceButtonsActive(bool isActive)
        {
            if (option1Button != null)
                option1Button.gameObject.SetActive(isActive);

            if (option2Button != null)
                option2Button.gameObject.SetActive(isActive);
        }

        public void EndCutscene()
        {
            _isPlaying = false;
            _currentNodeIndex = -1;
            _currentLineIndex = -1;

            if (voiceAudioSource != null)
                voiceAudioSource.Stop();

            SetChoiceButtonsActive(false);

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            onCutsceneEnded?.Invoke();
        }

        private bool IsValidNodeIndex(int index)
        {
            return dialogueTree != null && index >= 0 && index < dialogueTree.Count;
        }
    }
}