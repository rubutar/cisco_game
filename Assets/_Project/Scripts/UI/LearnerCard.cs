// Stream B. The Learner Card (PRD-v2 4.9, B-07): the outcomes ticked, Field Notes x/8, the check score as
// pips (green = right), and the QR code and link to Cisco Networking Academy. Shown on the peace screen and
// later in the toolkit panel. Text from dialogue.json learnerCard and ui.fieldNotesFormat; the QR image is
// Resources/Sprites/UI/qr_netacad (a PNG from Person A, A-09) and hides itself if it's missing.
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ByteWorld
{
    public class LearnerCard : MonoBehaviour
    {
        public const int MaxOutcomes = 9;
        public const int MaxCheckPips = 8;
        public const string QrResource = "Sprites/UI/qr_netacad"; // Person A, A-09

        [SerializeField] TMP_Text titleText;
        [SerializeField] GameObject[] outcomeRows;
        [SerializeField] Image[] outcomeTicks;
        [SerializeField] TMP_Text[] outcomeTexts;
        [SerializeField] TMP_Text fieldNotesText;
        [SerializeField] Image[] checkPips;
        [SerializeField] Image qrImage;
        [SerializeField] TMP_Text ctaText;
        [SerializeField] Button linkButton;
        [SerializeField] TMP_Text linkText;

        string url;

        void Awake()
        {
            if (linkButton != null) linkButton.onClick.AddListener(OpenLink);
        }

        /// <summary>Fills the card. unlocked = the game is finished, so every outcome is ticked.</summary>
        public void Refresh(bool unlocked)
        {
            var data = Services.Data?.Dialogue?.learnerCard;
            titleText.text = data?.title ?? "";

            var outcomes = data?.outcomes;
            for (var i = 0; i < outcomeRows.Length; i++)
            {
                var has = outcomes != null && i < outcomes.Length && !string.IsNullOrEmpty(outcomes[i]);
                outcomeRows[i].SetActive(has);
                if (!has) continue;
                outcomeTexts[i].text = outcomes[i];
                outcomeTicks[i].color = unlocked ? UiStyle.Done : UiStyle.Locked;
            }

            fieldNotesText.text = UiStrings.Format(UiStrings.Ui.fieldNotesFormat, LearnerProgress.FieldNotes, LearnerProgress.FieldNotesTotal);
            fieldNotesText.gameObject.SetActive(!string.IsNullOrEmpty(fieldNotesText.text));

            // One pip per question answered: green if right, grey if not.
            var answered = Mathf.Min(LearnerProgress.ChecksAnswered, checkPips.Length);
            for (var i = 0; i < checkPips.Length; i++)
            {
                checkPips[i].gameObject.SetActive(i < answered);
                checkPips[i].color = i < LearnerProgress.ChecksCorrect ? UiStyle.Done : UiStyle.Locked;
            }

            var qr = Resources.Load<Sprite>(QrResource);
            qrImage.sprite = qr;
            qrImage.gameObject.SetActive(qr != null);

            ctaText.text = data?.cta ?? "";
            ctaText.gameObject.SetActive(!string.IsNullOrEmpty(ctaText.text));
            url = data?.url;
            linkText.text = url ?? "";
            linkButton.gameObject.SetActive(!string.IsNullOrEmpty(url));

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }

        void OpenLink()
        {
            if (!string.IsNullOrEmpty(url)) Application.OpenURL(url);
        }
    }
}
