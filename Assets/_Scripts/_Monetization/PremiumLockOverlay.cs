using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TextQuestReader.Monetization
{
    /// <summary>
    /// Runtime overlay that attaches to a QuestCell GameObject and visually marks
    /// it as premium-locked or owned. Adds a tinted veil + lock badge + Unlock
    /// CTA. Doesn't touch the prefab — built from code so existing scenes work.
    /// </summary>
    public class PremiumLockOverlay : MonoBehaviour
    {
        private GameObject veilGo;
        private Image veilImage;
        private GameObject badgeGo;
        private Image badgeImage;
        private TMP_Text badgeText;
        private GameObject ctaGo;
        private Button ctaButton;
        private TMP_Text ctaLabel;

        private QuestAccessState state;
        private ProductDefinition product;
        private string questName;
        private bool isFeatured;

        private Action<ProductDefinition> onUnlockClicked;

        public void Setup(string questName, QuestAccessState state, ProductDefinition product, bool isFeatured, Action<ProductDefinition> onUnlockClicked)
        {
            this.questName = questName;
            this.state = state;
            this.product = product;
            this.isFeatured = isFeatured;
            this.onUnlockClicked = onUnlockClicked;

            EnsureBuilt();
            Refresh();
        }

        private void EnsureBuilt()
        {
            RectTransform host = (RectTransform)transform;

            if (veilGo == null)
            {
                veilGo = new GameObject("PremiumVeil", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                veilGo.transform.SetParent(host, false);
                RectTransform rt = (RectTransform)veilGo.transform;
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                veilImage = veilGo.GetComponent<Image>();
                veilImage.raycastTarget = false;
            }

            if (badgeGo == null)
            {
                badgeGo = new GameObject("PremiumBadge", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                badgeGo.transform.SetParent(host, false);
                RectTransform rt = (RectTransform)badgeGo.transform;
                rt.anchorMin = new Vector2(1f, 1f);
                rt.anchorMax = new Vector2(1f, 1f);
                rt.pivot = new Vector2(1f, 1f);
                rt.anchoredPosition = new Vector2(-10f, -10f);
                rt.sizeDelta = new Vector2(120f, 28f);
                badgeImage = badgeGo.GetComponent<Image>();
                badgeImage.raycastTarget = false;
                badgeImage.color = new Color(0.10f, 0.10f, 0.14f, 0.95f);

                GameObject labelGo = new GameObject("Label", typeof(RectTransform));
                labelGo.transform.SetParent(badgeGo.transform, false);
                badgeText = labelGo.AddComponent<TextMeshProUGUI>();
                RectTransform lrt = badgeText.rectTransform;
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = new Vector2(8f, 2f);
                lrt.offsetMax = new Vector2(-8f, -2f);
                badgeText.fontSize = 14f;
                badgeText.alignment = TextAlignmentOptions.Center;
                badgeText.color = new Color(0.95f, 0.85f, 0.55f, 1f);
                badgeText.text = "PREMIUM";
            }

            if (ctaGo == null)
            {
                ctaGo = new GameObject("UnlockCTA", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                ctaGo.transform.SetParent(host, false);
                RectTransform rt = (RectTransform)ctaGo.transform;
                rt.anchorMin = new Vector2(1f, 0f);
                rt.anchorMax = new Vector2(1f, 0f);
                rt.pivot = new Vector2(1f, 0f);
                rt.anchoredPosition = new Vector2(-10f, 10f);
                rt.sizeDelta = new Vector2(150f, 36f);
                Image bg = ctaGo.GetComponent<Image>();
                bg.color = new Color(0.95f, 0.55f, 0.20f, 1f);
                bg.raycastTarget = true;
                ctaButton = ctaGo.GetComponent<Button>();
                ctaButton.targetGraphic = bg;
                ctaButton.onClick.AddListener(OnCtaClicked);

                GameObject labelGo = new GameObject("Label", typeof(RectTransform));
                labelGo.transform.SetParent(ctaGo.transform, false);
                ctaLabel = labelGo.AddComponent<TextMeshProUGUI>();
                RectTransform lrt = ctaLabel.rectTransform;
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = new Vector2(8f, 2f);
                lrt.offsetMax = new Vector2(-8f, -2f);
                ctaLabel.fontSize = 16f;
                ctaLabel.alignment = TextAlignmentOptions.Center;
                ctaLabel.color = Color.white;
                ctaLabel.text = "Unlock";
                ctaLabel.fontStyle = FontStyles.Bold;
            }
        }

        public void Refresh()
        {
            EnsureBuilt();

            bool showVeil = state == QuestAccessState.LockedPremium;
            bool showBadge = state != QuestAccessState.FreeOpen || isFeatured;
            bool showCta = state == QuestAccessState.LockedPremium && product != null;

            if (veilImage != null)
                veilImage.color = showVeil ? new Color(0f, 0f, 0f, 0.55f) : new Color(0f, 0f, 0f, 0f);

            if (badgeGo != null)
            {
                badgeGo.SetActive(showBadge);

                if (state == QuestAccessState.OwnedPremium)
                {
                    badgeText.text = "OWNED";
                    badgeText.color = new Color(0.4f, 0.95f, 0.6f, 1f);
                }
                else if (state == QuestAccessState.LockedPremium)
                {
                    badgeText.text = "PREMIUM";
                    badgeText.color = new Color(0.95f, 0.85f, 0.55f, 1f);
                }
                else if (isFeatured)
                {
                    badgeText.text = "FEATURED";
                    badgeText.color = new Color(0.55f, 0.85f, 0.95f, 1f);
                }
            }

            if (ctaGo != null)
            {
                ctaGo.SetActive(showCta);
                if (showCta && product != null)
                    ctaLabel.text = $"Unlock · {product.price}";
            }
        }

        private void OnCtaClicked()
        {
            if (onUnlockClicked != null && product != null)
                onUnlockClicked(product);
        }
    }
}
