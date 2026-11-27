using System;
using UnityEngine;

namespace Game.Dialogue
{
    public enum SpeakerSide
    {
        None,
        Left,
        Right
    }

    [Serializable]
    public struct DialogueLine
    {
        [Tooltip("Optional speaker name shown above or beside the dialogue box.")]
        public string speakerName;

        [Tooltip("Dialogue text displayed by the UI/dialogue script.")]
        [TextArea(2, 6)]
        public string text;

        [Tooltip("Left character portrait sprite displayed on screen.")]
        public Sprite leftPortrait;

        [Tooltip("Right character portrait sprite displayed on screen.")]
        public Sprite rightPortrait;

        [Tooltip("Indicates which portrait is actively speaking (used to highlight or dim).")]
        public SpeakerSide activeSpeaker;

        [Tooltip("Full-screen or scene illustration/CG sprite displayed on screen.")]
        public Sprite screenImage;

        [Tooltip("Optional voice clip or typing sound effect for this line.")]
        public AudioClip voiceClip;
    }
}