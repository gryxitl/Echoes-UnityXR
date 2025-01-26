using UnityEngine;
using UnityEngine.Events;
using Oculus.Voice;
using System.Reflection;
using Meta.WitAi.CallbackHandlers;
using TMPro; 


public class VoiceManager : MonoBehaviour
{
    [Header("Wit Configuration")]
    [SerializeField] private AppVoiceExperience appVoiceExperience; // App Voice Experience (AppVoiceExperience)
    [SerializeField] private WitResponseMatcher responseMatcher; // App Voice Experience (WitResponseMatcher)
    [SerializeField] private TextMeshProUGUI transcriptionText; // Unchanged

    [Header("Voice Events")]
    [SerializeField] private UnityEvent wakeWordDetected; // 1 method
    [SerializeField] private UnityEvent<string> completeTranscription; // 1 method

    private bool _voiceCommandReady;

    private void Awake()
    {
        appVoiceExperience.VoiceEvents.OnRequestCompleted.AddListener(ReactivateVoice);
        appVoiceExperience.VoiceEvents.OnPartialTranscription.AddListener(OnPartialTranscription);
        appVoiceExperience.VoiceEvents.OnFullTranscription.AddListener(OnFullTranscription);

        var eventField = typeof(WitResponseMatcher).GetField("onMultiValueEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventField != null && eventField.GetValue(responseMatcher) is MultiValueEvent onMultiValueEvent)
        {
            onMultiValueEvent.AddListener(WakeWordDetected);
        }
    }

    private void OnDestroy()
    {
        appVoiceExperience.VoiceEvents.OnRequestCompleted.RemoveListener(ReactivateVoice);
        appVoiceExperience.VoiceEvents.OnPartialTranscription.RemoveListener(OnPartialTranscription);
        appVoiceExperience.VoiceEvents.OnFullTranscription.RemoveListener(OnFullTranscription);

        var eventField = typeof(WitResponseMatcher).GetField("onMultiValueEvent", BindingFlags.NonPublic | BindingFlags.Instance);
        if (eventField != null && eventField.GetValue(responseMatcher) is MultiValueEvent onMultiValueEvent)
        {
            onMultiValueEvent.RemoveListener(WakeWordDetected);
        }
    }

    private void ReactivateVoice() => appVoiceExperience.Activate();

    private void WakeWordDetected(string[] arg0)
    {
        _voiceCommandReady = true;
        wakeWordDetected.Invoke();
    }

    private void OnPartialTranscription(string transcription)
    {
        if (_voiceCommandReady) return;
        transcriptionText.text = transcription;
    }

    private void OnFullTranscription(string transcription)
    {
        if (_voiceCommandReady) return;
        transcriptionText.text = transcription;
        completeTranscription.Invoke(transcription);
    }
}
