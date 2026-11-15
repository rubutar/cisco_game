// Stream B. Quest 4's incident report (PRD-v2 4.8, B-11): put the 5 evidence cards in the order the attack
// happened (scan, phishing, push spam, 8080 probe, flood). The cards are shuffled in a pool; the player taps
// them in order and each right tap moves the card into the next numbered slot. A wrong tap flashes the card
// pink and nothing else happens (never punished). When the timeline is complete, the Report button goes on.
// Text from dialogue.json incident {prompt, cards[] in the correct order}.
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class IncidentReport : MonoBehaviour
    {
        public const int MaxCards = 5;

        [SerializeField] GameObject overlay;
        [SerializeField] TMP_Text promptText;
        [SerializeField] Button[] poolButtons;
        [SerializeField] Image[] poolImages;
        [SerializeField] TMP_Text[] poolLabels;
        [SerializeField] Image[] slotImages;
        [SerializeField] TMP_Text[] slotLabels;
        [SerializeField] Button doneButton;
        [SerializeField] TMP_Text doneLabel;

        string[] cards;
        int[] poolOrder;   // poolOrder[button] = index of the card in the correct order
        int placed;
        Action onDone;

        public bool HasCards
        {
            get
            {
                var c = Services.Data?.Dialogue?.incident?.cards;
                return c != null && c.Length > 0;
            }
        }

        void Awake()
        {
            for (var i = 0; i < poolButtons.Length; i++)
            {
                var b = i;
                poolButtons[i].onClick.AddListener(() => Tap(b));
            }
            doneButton.onClick.AddListener(Finish);
            overlay.SetActive(false);
        }

        void OnEnable() => GameEvents.StateChanged += OnStateChanged;
        void OnDisable() => GameEvents.StateChanged -= OnStateChanged;

        void OnStateChanged(GameState from, GameState to)
        {
            if (overlay.activeSelf && to != GameState.Reveal)
            {
                overlay.SetActive(false);
                onDone = null;
            }
        }

        public void Begin(Action done)
        {
            var incident = Services.Data?.Dialogue?.incident;
            cards = incident?.cards;
            if (cards == null || cards.Length == 0)
            {
                done?.Invoke();
                return;
            }

            onDone = done;
            placed = 0;
            var count = Mathf.Min(cards.Length, MaxCards);
            promptText.text = incident.prompt ?? "";

            // Shuffle, but never leave the pool already in the right order.
            poolOrder = new int[count];
            for (var i = 0; i < count; i++) poolOrder[i] = i;
            for (var tries = 0; tries < 10; tries++)
            {
                for (var i = count - 1; i > 0; i--)
                {
                    var j = UnityEngine.Random.Range(0, i + 1);
                    (poolOrder[i], poolOrder[j]) = (poolOrder[j], poolOrder[i]);
                }
                if (count < 2 || !InOrder()) break;
            }

            for (var i = 0; i < poolButtons.Length; i++)
            {
                var has = i < count;
                poolButtons[i].gameObject.SetActive(has);
                poolButtons[i].interactable = true;
                if (has)
                {
                    poolLabels[i].text = cards[poolOrder[i]];
                    poolImages[i].color = UiStyle.White;
                }
            }
            for (var i = 0; i < slotImages.Length; i++)
            {
                slotImages[i].gameObject.SetActive(i < count);
                slotImages[i].color = UiStyle.Locked;
                slotLabels[i].text = (i + 1).ToString();
                slotLabels[i].color = Palette.ByteCream;
            }
            doneLabel.text = UiStrings.Ui.report ?? "";
            doneButton.gameObject.SetActive(false);

            Services.Dialogue.Hide();
            overlay.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)overlay.transform);
        }

        bool InOrder()
        {
            for (var i = 0; i < poolOrder.Length; i++)
                if (poolOrder[i] != i) return false;
            return true;
        }

        void Tap(int button)
        {
            if (!overlay.activeSelf || placed >= poolOrder.Length) return;
            if (poolOrder[button] != placed)
            {
                StartCoroutine(FlashWrong(button));
                return;
            }

            poolButtons[button].gameObject.SetActive(false);
            slotImages[placed].color = Palette.SignalBlue;
            slotLabels[placed].text = $"{placed + 1}  {cards[placed]}";
            placed++;
            if (placed >= poolOrder.Length) doneButton.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)overlay.transform);
        }

        IEnumerator FlashWrong(int button)
        {
            poolImages[button].color = UiStyle.Danger;
            yield return new WaitForSecondsRealtime(0.35f);
            poolImages[button].color = UiStyle.White;
        }

        void Finish()
        {
            if (!overlay.activeSelf || placed < poolOrder.Length) return;
            overlay.SetActive(false);
            var done = onDone;
            onDone = null;
            done?.Invoke();
        }
    }
}
