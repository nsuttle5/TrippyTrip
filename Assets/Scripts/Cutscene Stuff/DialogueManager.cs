using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Speech Bubble (world space, follows speaker)")]
    public Canvas speechBubbleCanvas;
    public TMP_Text speechBubbleSpeakerName;
    public TMP_Text speechBubbleText;
    public Vector3 speechBubbleOffset = new Vector3(0f, 2.2f, 0f);

    [Header("Response UI (screen space)")]
    public GameObject responsePanel;
    public Button responseButtonPrefab;
    public Transform responseButtonContainer;

    [Header("Behaviour")]
    [Tooltip("How long linear line stays on screen before auto-advancing.")]
    public float autoAdvanceLineDelay = 2.5f;

    private DialogueData _currentData;
    private Transform _speakerTransform;
    private Action _onDialogueComplete;
    private Coroutine _autoAdvanceRoutine;
    private readonly List<GameObject> _spawnedButtons = new List<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        HideAll();
    }

    void LateUpdate()
    {
        if (_speakerTransform == null || speechBubbleCanvas == null || !speechBubbleCanvas.gameObject.activeSelf)
            return;

        speechBubbleCanvas.transform.position = _speakerTransform.position + speechBubbleOffset;

        if (Camera.main != null)
        {
            Vector3 dir = speechBubbleCanvas.transform.position - Camera.main.transform.position;
            if (dir.sqrMagnitude > 0.0001f)
                speechBubbleCanvas.transform.rotation = Quaternion.LookRotation(dir);
        }
    }


    public void StartDialogue(DialogueData data, Transform speaker, Action onComplete)
    {
        _currentData = data;
        _speakerTransform = speaker;
        _onDialogueComplete = onComplete;
        ShowNode(data.startNodeId);
    }

    private void ShowNode(int nodeId)
    {
        if (nodeId < 0)
        {
            EndDialogue();
            return;
        }

        DialogueNode node = _currentData.GetNode(nodeId);
        if (node == null)
        {
            Debug.LogWarning($"DialogueManager: node id {nodeId} not found, ending dialogue.");
            EndDialogue();
            return;
        }

        speechBubbleCanvas.gameObject.SetActive(true);
        speechBubbleSpeakerName.text = node.speakerName;
        speechBubbleText.text = node.text;

        if (node.voiceClip != null)
            AudioSource.PlayClipAtPoint(node.voiceClip, _speakerTransform.position);

        ClearResponseButtons();

        bool hasChoices = node.responses != null && node.responses.Length > 0;
        responsePanel.SetActive(hasChoices);

        if (hasChoices)
        {
            foreach (var response in node.responses)
            {
                Button btn = Instantiate(responseButtonPrefab, responseButtonContainer);
                var label = btn.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = response.responseText;

                int nextId = response.nextNodeId; // capture for closure
                btn.onClick.AddListener(() => OnResponseChosen(nextId));
                _spawnedButtons.Add(btn.gameObject);
            }
        }
        else
        {
            if (_autoAdvanceRoutine != null) StopCoroutine(_autoAdvanceRoutine);
            _autoAdvanceRoutine = StartCoroutine(AutoAdvance(node.autoAdvanceNextId));
        }
    }

    private IEnumerator AutoAdvance(int nextId)
    {
        yield return new WaitForSeconds(autoAdvanceLineDelay);
        ShowNode(nextId);
    }

    private void OnResponseChosen(int nextId)
    {
        ClearResponseButtons();
        responsePanel.SetActive(false);
        ShowNode(nextId);
    }

    private void ClearResponseButtons()
    {
        foreach (var b in _spawnedButtons) Destroy(b);
        _spawnedButtons.Clear();
    }

    private void EndDialogue()
    {
        HideAll();
        Action callback = _onDialogueComplete;
        _currentData = null;
        _speakerTransform = null;
        _onDialogueComplete = null;
        callback?.Invoke();
    }

    private void HideAll()
    {
        if (speechBubbleCanvas != null) speechBubbleCanvas.gameObject.SetActive(false);
        if (responsePanel != null) responsePanel.SetActive(false);
        ClearResponseButtons();
    }
}
