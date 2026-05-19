using System.Reflection;
using UnityEngine;

namespace TextQuestReader.CinematicShell
{
    /// <summary>
    /// Walks GamePanel's serialized RectTransform fields and adds a CanvasGroup
    /// to each, forcing alpha=0 and non-interactable. The legacy UI keeps
    /// functioning logically (text gets set, choices get instantiated) — it's
    /// only invisible and non-clickable. The cinematic shell renders its own
    /// surfaces on top.
    /// </summary>
    public class LegacyUiSuppressor : MonoBehaviour
    {
        private GamePanel target;

        public void Suppress(Canvas canvas, GamePanel panel)
        {
            target = panel;
            if (panel == null) return;

            string[] fieldsToHide =
            {
                "mainPictureRect",
                "paramsRect",
                "paramsContent",
                "mainTextRect",
                "questionsRect",
                "questionsContent",
                "sourcesNode",
                "blockerNode"
            };

            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;
            System.Type t = panel.GetType();

            foreach (string fieldName in fieldsToHide)
            {
                FieldInfo info = t.GetField(fieldName, flags);
                if (info == null) continue;
                object value = info.GetValue(panel);

                RectTransform rt = value as RectTransform;
                if (rt != null) { HideRect(rt); continue; }

                GameObject go = value as GameObject;
                if (go != null) { HideRect((RectTransform)go.transform); continue; }
            }

            if (canvas != null)
            {
                Transform legacyHeader = canvas.transform.Find("MainMenuTerminalSkin");
                if (legacyHeader != null) HideRect((RectTransform)legacyHeader);
            }
        }

        private void HideRect(RectTransform rt)
        {
            if (rt == null) return;
            CanvasGroup group = rt.GetComponent<CanvasGroup>();
            if (group == null) group = rt.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }
}
