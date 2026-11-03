using System;
using System.Collections;
using GslvSpace;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace GslvMission
{
    /// <summary>
    /// Receives the Mission Markers from the Timeline. The Timeline owns the flow (voice-over, UI,
    /// highlights, animation); this only does the two things a Timeline cannot: wait for the learner
    /// (gates) and swap the environment. Lives on the same GameObject as the PlayableDirector.
    /// </summary>
    [RequireComponent(typeof(PlayableDirector))]
    public class MissionFlow : MonoBehaviour, INotificationReceiver
    {
        [Serializable]
        public class GateButton
        {
            public string id;
            public Button button;
        }

        public PlayableDirector director;
        public SpaceEnvironmentSwitcher space;
        public QuizPanel quiz;
        public GameObject endMenu;
        public GateButton[] gateButtons;
        [Tooltip("Plays the wrong-answer voice-over while the Timeline waits.")]
        public AudioSource feedbackSource;
        public AudioClip wrongAnswerClip;
        [Tooltip("Testing only: answers every gate by itself (quiz: one wrong answer first).")]
        public bool autoPlay;
        public float autoPlayDelay = 1.2f;
        [Tooltip("A click within this many seconds of the previous one is ignored (double clicks, two buttons at once).")]
        public float clickCooldown = 0.35f;

        public string OpenGateId { get; private set; }
        float lastClick = -10f;

        void Reset()
        {
            director = GetComponent<PlayableDirector>();
        }

        void Awake()
        {
            if (director == null) director = GetComponent<PlayableDirector>();
            SetButtons(null, false);
        }

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            var marker = notification as MissionMarker;
            if (marker == null) return;
            switch (marker.command)
            {
                case MissionCommand.Scene:
                    Debug.Log("[Mission] " + marker.argument);
                    break;
                case MissionCommand.Gate:
                    OpenGate(marker.argument);
                    break;
                case MissionCommand.Quiz:
                    int number;
                    if (quiz != null && int.TryParse(marker.argument, out number)) quiz.Begin(number - 1);
                    break;
                case MissionCommand.ShowSpace:
                    if (space != null) space.ShowSpace();
                    break;
                case MissionCommand.ShowLaunchpad:
                    if (space != null) space.ShowLaunchpad();
                    break;
                case MissionCommand.EndMenu:
                    if (endMenu != null) endMenu.SetActive(true);
                    break;
            }
        }

        // Wired to the Explore / Launch / Deploy buttons (On Click).
        public void ButtonClicked(string id)
        {
            if (OpenGateId == null || OpenGateId != id || !AcceptClick()) return;
            CloseGate();
        }

        // Wired to the three quiz answer buttons (On Click).
        public void QuizOption(int index)
        {
            if (OpenGateId != "Quiz" || quiz == null || !AcceptClick()) return;
            if (quiz.Select(index)) CloseGate();
            else if (feedbackSource != null && wrongAnswerClip != null) feedbackSource.PlayOneShot(wrongAnswerClip);
        }

        bool AcceptClick()
        {
            if (Time.unscaledTime - lastClick < clickCooldown) return false;
            lastClick = Time.unscaledTime;
            return true;
        }

        void OpenGate(string id)
        {
            OpenGateId = id;
            SetButtons(id, true);
            if (id == "Quiz" && quiz != null) quiz.SetInteractable(true);
            SetSpeed(0.0);
            if (autoPlay) StartCoroutine(AutoClick(id));
        }

        void CloseGate()
        {
            string id = OpenGateId;
            OpenGateId = null;
            SetButtons(id, false);
            SetSpeed(1.0);
        }

        void SetSpeed(double speed)
        {
            if (director != null && director.playableGraph.IsValid())
                director.playableGraph.GetRootPlayable(0).SetSpeed(speed);
        }

        void SetButtons(string id, bool interactable)
        {
            if (gateButtons == null) return;
            foreach (var g in gateButtons)
            {
                if (g == null || g.button == null) continue;
                bool match = id != null && g.id == id;
                g.button.interactable = match && interactable;
                var col = g.button.GetComponent<Collider>();
                if (col != null) col.enabled = match && interactable;
            }
        }

        IEnumerator AutoClick(string id)
        {
            yield return new WaitForSecondsRealtime(autoPlayDelay);
            if (OpenGateId != id) yield break;
            if (id == "Quiz" && quiz != null)
            {
                int correct = quiz.CurrentCorrect;
                Click(quiz.optionButtons[(correct + 1) % quiz.optionButtons.Length]);
                yield return new WaitForSecondsRealtime(autoPlayDelay);
                if (OpenGateId == "Quiz") Click(quiz.optionButtons[correct]);
                yield break;
            }
            foreach (var g in gateButtons)
                if (g != null && g.id == id && g.button != null) Click(g.button);
        }

        static void Click(Button button)
        {
            var es = EventSystem.current;
            if (es != null) ExecuteEvents.Execute(button.gameObject, new PointerEventData(es), ExecuteEvents.pointerClickHandler);
            else button.onClick.Invoke();
        }
    }
}
