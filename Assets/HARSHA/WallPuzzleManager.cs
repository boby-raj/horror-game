using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Renderer))]
public class WallPuzzleManager : MonoBehaviour, IInteractable
{
    [Header("Wall Visuals")]
    public GameObject[] partsOnWall;

    [Header("Puzzle Status")]
    public int totalPartsRequired = 6;
    public GameObject lockedDoor;

    [Header("Glow Settings")]
    [ColorUsage(true, true)]
    public Color glowColor = Color.cyan * 2f;

    [Header("UI Elements (Drag TMP objects here)")]
    [Tooltip("The single UI text element used for all hover and interaction messages.")]
    public TMP_Text mainPromptUI;
    
    [Tooltip("The separate UI text element that turns on when the puzzle is done.")]
    public TMP_Text puzzleCompleteUI;

    [Header("UI Messages (Type your text here)")]
    public string noPartsMessage = "Find parts to place here";
    public string hasPartsMessage = "Press [E] to place parts";
    public string missingFeedback = "No parts to place!";
    public string placedFeedback = "Parts placed!";

    private List<int> partsHeldByPlayer = new List<int>();
    private int totalPlaced = 0;

    private Renderer meshRenderer;
    private Material wallMaterial;
    private Color originalEmissionColor;
    private bool originallyHadEmission;
    
    private bool isHovering = false;
    private Coroutine feedbackCoroutine;

    void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        if (meshRenderer != null)
        {
            wallMaterial = meshRenderer.material;
            originallyHadEmission = wallMaterial.IsKeywordEnabled("_EMISSION");
            if (wallMaterial.HasProperty("_EmissionColor"))
            {
                originalEmissionColor = wallMaterial.GetColor("_EmissionColor");
            }
        }

        if (mainPromptUI != null) mainPromptUI.gameObject.SetActive(false);
        if (puzzleCompleteUI != null) puzzleCompleteUI.gameObject.SetActive(false);
    }

    public void CollectPart(BodyPart collectedPart)
    {
        partsHeldByPlayer.Add(collectedPart.partID);
        Destroy(collectedPart.gameObject);
    }

    public void Interact()
    {
        if (totalPlaced >= totalPartsRequired) return; 

        if (partsHeldByPlayer.Count > 0)
        {
            foreach (int id in partsHeldByPlayer)
            {
                if (id >= 0 && id < partsOnWall.Length)
                {
                    partsOnWall[id].SetActive(true);
                    totalPlaced++;
                }
            }
            partsHeldByPlayer.Clear();

            if (totalPlaced >= totalPartsRequired)
            {
                UnlockDoor();
            }
            else
            {
                ShowFeedback(placedFeedback, 2f);
            }
        }
        else
        {
            ShowFeedback(missingFeedback, 2f);
        }
    }

    public void OnHoverEnter()
    {
        isHovering = true;

        if (wallMaterial != null && totalPlaced < totalPartsRequired)
        {
            wallMaterial.EnableKeyword("_EMISSION");
            wallMaterial.SetColor("_EmissionColor", glowColor);
        }

        if (feedbackCoroutine == null && totalPlaced < totalPartsRequired) 
        {
            UpdateHoverUI();
        }
    }

    public void OnHoverExit()
    {
        isHovering = false;

        if (wallMaterial != null)
        {
            if (!originallyHadEmission) wallMaterial.DisableKeyword("_EMISSION");
            if (wallMaterial.HasProperty("_EmissionColor")) wallMaterial.SetColor("_EmissionColor", originalEmissionColor);
        }

        if (mainPromptUI != null) mainPromptUI.gameObject.SetActive(false);
    }

    private void UpdateHoverUI()
    {
        if (mainPromptUI == null) return;
        mainPromptUI.gameObject.SetActive(true);

        if (partsHeldByPlayer.Count > 0)
        {
            mainPromptUI.text = hasPartsMessage;
        }
        else
        {
            mainPromptUI.text = noPartsMessage;
        }
    }

    private void UnlockDoor()
    {
        if (lockedDoor != null) lockedDoor.SetActive(false);
        
        if (wallMaterial != null && !originallyHadEmission) 
        {
            wallMaterial.DisableKeyword("_EMISSION");
        }

        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        
        if (mainPromptUI != null) mainPromptUI.gameObject.SetActive(false);
        if (puzzleCompleteUI != null) puzzleCompleteUI.gameObject.SetActive(true);
    }

    private void ShowFeedback(string message, float duration)
    {
        if (mainPromptUI == null) return;

        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(FeedbackRoutine(message, duration));
    }

    private IEnumerator FeedbackRoutine(string message, float duration)
    {
        mainPromptUI.text = message;
        mainPromptUI.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(duration);
        
        feedbackCoroutine = null;

        // Revert back to standard text if still looking, otherwise turn off
        if (isHovering && totalPlaced < totalPartsRequired)
        {
            UpdateHoverUI();
        }
        else
        {
            mainPromptUI.gameObject.SetActive(false);
        }
    }
}