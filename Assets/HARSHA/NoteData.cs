using UnityEngine;
using TMPro;

/// <summary>
/// ScriptableObject asset representing a readable note in the game.
/// Create new notes via Right Click -> Create -> Horror Game -> Note Data.
/// </summary>
[CreateAssetMenu(fileName = "NewNoteData", menuName = "Horror Game/Note Data")]
public class NoteData : ScriptableObject
{
    [Header("Note Header")]
    [Tooltip("Title or subject shown at top of the note (e.g. 'Patient Report #14')")]
    public string noteTitle = "Diary Entry";

    [Header("Note Content")]
    [Tooltip("The actual message written on the paper")]
    [TextArea(8, 25)]
    public string noteContent = "I can hear something moving in the darkness...";

    [Header("Visual Styling (Optional)")]
    [Tooltip("Custom paper background sprite (leave empty for default)")]
    public Sprite paperSprite;
    [Tooltip("Paper background tint color")]
    public Color paperColor = Color.white;
    [Tooltip("Custom handwriting or typewriter font (leave empty for default)")]
    public TMP_FontAsset customFont;
    [Range(14f, 48f)]
    public float fontSize = 28f;

    [Header("Audio (Optional)")]
    [Tooltip("Sound played when opening this note (paper rustle, diary open)")]
    public AudioClip openSound;
    [Tooltip("Sound played when closing this note")]
    public AudioClip closeSound;

    [Header("Text Reveal Animation")]
    [Tooltip("If enabled, reveals text letter by letter like typing or reading")]
    public bool useTypewriterEffect = false;
    [Range(10f, 100f)]
    public float typewriterSpeed = 35f;
}
