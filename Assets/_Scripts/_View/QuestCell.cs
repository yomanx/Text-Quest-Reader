using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuestCell : MonoBehaviour, IKeyboardSelectable
{
    [SerializeField] private AliveText nameText;
    [SerializeField] private AliveText infoText;
    [SerializeField] private GameObject selectedImage;
    [SerializeField] private GameObject circle;
    [SerializeField] private Button button;

    GamePanel gamePanel;
    QuestShort questShort;

    string nameString;
    string infoString;

    // Lazy runtime-built left accent stripe. Sits behind nameText/infoText
    // (sibling 0) so it never covers letters. Visible only when the cell
    // is the currently selected one.
    private Image accentStripe;

    public bool IsKeyboardSelectable => gameObject.activeInHierarchy && button != null && button.enabled;

    public void StartWith(GamePanel gamePanel, QuestShort questShort, bool selected)
    {
        this.gamePanel = gamePanel;
        this.questShort = questShort;

        nameString = string.IsNullOrEmpty(questShort.DisplayName) ? questShort.QuestName : questShort.DisplayName;

        infoString = "";

        if (!string.IsNullOrEmpty(questShort.Author))
            infoString = $"by {questShort.Author}";

        if (!string.IsNullOrEmpty(questShort.Lang))
        {
            // Middle-dot separator reads cleaner than triple-space and keeps
            // the metadata line scannable: "by Author · [EN]".
            if (!string.IsNullOrEmpty(infoString))
                infoString += "  ·  ";

            infoString += $"[{questShort.Lang.ToUpper()}]";
        }

        EnsureAccentStripe();
        selectedImage.SetActive(selected);
        UpdateAccentVisibility(selected);
    }

    private void EnsureAccentStripe()
    {
        if (accentStripe != null) return;

        GameObject go = new GameObject("AccentStripe", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);
        go.transform.SetAsFirstSibling();   // behind everything else

        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.offsetMin = new Vector2(0f, 4f);
        rt.offsetMax = new Vector2(0f, -4f);
        rt.sizeDelta = new Vector2(3f, 0f);

        accentStripe = go.GetComponent<Image>();
        accentStripe.raycastTarget = false;
        accentStripe.color = new Color(0.40f, 0.78f, 0.98f, 0f);   // hidden until selected
    }

    private void UpdateAccentVisibility(bool selected)
    {
        if (accentStripe == null) return;
        Color c = accentStripe.color;
        accentStripe.color = new Color(c.r, c.g, c.b, selected ? 0.92f : 0f);
    }

    private void OnEnable()
    {
        circle.SetActive(false);
        StartCoroutine(ShowWithDelay());
    }

    private IEnumerator ShowWithDelay()
    {
        yield return new WaitForSeconds(0.2f);

        circle.SetActive(true);
        nameText.SetText(nameString);
        infoText.SetText(infoString);
    }

    public void ActionSelect()
    {
        gamePanel.DiselectAllQuestCells();

        AudioManager.Instance.PlaySfx(SoundType.Click);
        gamePanel.SelectQuest(questShort);

        selectedImage.SetActive(true);
    }

    public void SetKeyboardSelected(bool selected)
    {
        selectedImage.SetActive(selected);
        UpdateAccentVisibility(selected);

        if (selected)
            gamePanel.SelectQuest(questShort);
    }

    public void SubmitKeyboard()
    {
        gamePanel.StartQuest();
    }

    public void Diselect()
    {
        selectedImage.SetActive(false);
        UpdateAccentVisibility(false);
    }
}