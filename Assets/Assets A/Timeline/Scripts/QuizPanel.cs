using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GslvMission
{
    [Serializable]
    public class QuizQuestion
    {
        [TextArea(1, 3)] public string question;
        public string[] options = new string[3];
        public int correctIndex;
    }

    /// <summary>One panel reused for all five questions. The Timeline picks the question with a Mission Marker.</summary>
    public class QuizPanel : MonoBehaviour
    {
        public QuizQuestion[] questions;
        public TMP_Text counterText;
        public TMP_Text questionText;
        public Button[] optionButtons;
        public TMP_Text[] optionTexts;
        public Image[] optionImages;
        public Color normalColor = new Color(0.10f, 0.32f, 0.55f, 0.95f);
        public Color correctColor = new Color(0.15f, 0.78f, 0.38f, 1f);
        public Color wrongColor = new Color(1f, 0.66f, 0.12f, 1f);
        [Tooltip("Optional UI kit skins for the answer rows; when set they replace the colours above.")]
        public Sprite normalSprite, correctSprite, wrongSprite;

        int current;

        public int CurrentCorrect { get { return questions[current].correctIndex; } }

        public void Begin(int index)
        {
            if (questions == null || questions.Length == 0) return;
            current = Mathf.Clamp(index, 0, questions.Length - 1);
            var q = questions[current];
            if (counterText != null) counterText.text = "QUESTION " + (current + 1) + " OF " + questions.Length;
            if (questionText != null) questionText.text = q.question;
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionTexts[i] != null) optionTexts[i].text = (char)('A' + i) + ".   " + q.options[i];
                Paint(i, normalSprite, normalColor, false);
            }
            StopAllCoroutines();
            SetInteractable(false);
        }

        public void SetInteractable(bool on)
        {
            foreach (var b in optionButtons)
            {
                b.interactable = on;
                var col = b.GetComponent<Collider>();
                if (col != null) col.enabled = on;
            }
        }

        // True when the answer is correct.
        public bool Select(int index)
        {
            if (index == CurrentCorrect)
            {
                Paint(index, correctSprite, correctColor, false);
                SetInteractable(false);
                return true;
            }
            StartCoroutine(Flash(index));
            return false;
        }

        IEnumerator Flash(int index)
        {
            Paint(index, wrongSprite, wrongColor, true);
            yield return new WaitForSecondsRealtime(0.7f);
            Paint(index, normalSprite, normalColor, false);
        }

        // overHover: also show the skin while the pointer still rests on the button (its hover skin would hide it).
        void Paint(int index, Sprite sprite, Color color, bool overHover)
        {
            var img = optionImages[index];
            if (sprite == null)
            {
                img.color = color;
                return;
            }
            img.sprite = sprite;
            img.overrideSprite = overHover ? sprite : null;
            img.color = Color.white;
        }
    }
}
