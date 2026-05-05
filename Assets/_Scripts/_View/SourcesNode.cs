using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using System.Collections.Generic;

public class SourcesNode : MonoBehaviour
{
    [SerializeField] private GamePanel gamePanel;
    [SerializeField] private TMP_Text startQuestText;
    [SerializeField] private TMP_Text addQuestText;
    [SerializeField] private TMP_Text refreshText;
    [SerializeField] private TMP_Text sourceText;
    [SerializeField] private TMP_Text localText;
    [SerializeField] private TMP_Text remoteText;
    [SerializeField] private GameObject localRefreshButton;
    [SerializeField] private GameObject localNode;
    [SerializeField] private GameObject remoteNode;
    [SerializeField] private Toggle localToggle;
    [SerializeField] private Toggle remoteToggle;
    [SerializeField] private Button startButton;

    private bool showingRemote;
    private bool serverAvailable;

    private void Start()
    {
        localRefreshButton.SetActive(false);

        gamePanel.HandleLocalizationsEvent += HandleLocalizations;
        gamePanel.RemoteQuestSelectionStarted += DisableStartButton;
        gamePanel.RemoteQuestSelectionEnded += EnableStartButton;
        gamePanel.StartQuestEnded += HandleStartQuestEnded;

        CheckServerAvailability();
    }

    private void OnDestroy()
    {
        gamePanel.HandleLocalizationsEvent -= HandleLocalizations;
        gamePanel.RemoteQuestSelectionStarted -= DisableStartButton;
        gamePanel.RemoteQuestSelectionEnded -= EnableStartButton;
        gamePanel.StartQuestEnded -= HandleStartQuestEnded;
    }

    public void ActionStart()
    {
        if (gamePanel.IsInputBlocked)
            return;

        AudioManager.Instance.PlaySfx(SoundType.Click);
        localRefreshButton.SetActive(false);
        SetControlsInteractable(false);
        gamePanel.StartQuest();
    }

    public void ActionAddLocalQuest()
    {
        if (gamePanel.IsInputBlocked)
            return;

        AudioManager.Instance.PlaySfx(SoundType.Click);
        SaveLoadManager.Instance.OpenQuestsOuterFolder();

        localRefreshButton.SetActive(true);
    }

    public void ActionLocalRefresh()
    {
        if (gamePanel.IsInputBlocked)
            return;

        AudioManager.Instance.PlaySfx(SoundType.Click);
        gamePanel.UpdateLocalQuests();
        localRefreshButton.SetActive(false);
    }

    public void OnLocalToggle(Toggle toggle)
    {
        if (gamePanel.IsInputBlocked)
            return;

        if (!toggle.isOn)
            return;

        AudioManager.Instance.PlaySfx(SoundType.Click);
        SelectLocal();
    }

    public void OnRemoteToggle(Toggle toggle)
    {
        if (gamePanel.IsInputBlocked)
            return;

        if (!toggle.isOn)
            return;

        AudioManager.Instance.PlaySfx(SoundType.Click);
        SelectRemote();
    }

    private void HandleLocalizations()
    {
        startQuestText.text = Localization.Get(LocKeys.StartQuest);
        addQuestText.text = Localization.Get(LocKeys.AddQuests);
        refreshText.text = Localization.Get(LocKeys.Refresh);
        sourceText.text = Localization.Get(LocKeys.Source);
        localText.text = Localization.Get(LocKeys.Local);
        remoteText.text = Localization.Get(LocKeys.Remote);
    }

    private void DisableStartButton() => startButton.interactable = false;
    private void EnableStartButton()
    {
        if (!gamePanel.IsInputBlocked)
            startButton.interactable = true;
    }

    private void HandleStartQuestEnded() => SetControlsInteractable(true);

    private void SetControlsInteractable(bool interactable)
    {
        startButton.interactable = interactable;
        localToggle.interactable = interactable;
        remoteToggle.interactable = interactable && serverAvailable;
    }

    private void CheckServerAvailability()
    {
        remoteToggle.interactable = false;

        ApiManager.Instance.GetAllQuests(
            (string result) =>
            {
                serverAvailable = true;
                remoteToggle.interactable = !gamePanel.IsInputBlocked;
            },
            (string error) =>
            {
                serverAvailable = false;
                remoteToggle.interactable = false;
            }
        );
    }

    private void ShowRemoteFolders()
    {
        ApiManager.Instance.GetAllQuests((string result) =>
        {
            if (!showingRemote)
                return;

            try
            {
                var list = JsonConvert.DeserializeObject<List<QuestShort>>(result, SaveLoadManager.JsonSettings);

                if (list == null)
                {
                    Debug.LogWarning("No quests found!");
                    // Director.Instance.WarningWithText("No quests found!");
                    return;
                }

                if (list.Count > 0)
                {
                    gamePanel.UpdateRemoteQuests(list);
                }
            }
            catch (JsonException ex)
            {
                Debug.LogWarning("Error parsing quests: " + ex.Message);
                // Director.Instance.WarningWithText("Error parsing quests: " + ex.Message);
            }
        },
        (string error) =>
        {
            if (!showingRemote)
                return;
            Debug.LogWarning("Error fetching quests: " + error);
            // Director.Instance.WarningWithText("Error fetching quests: " + error);
        });
    }

    public void SelectLocal()
    {
        if (gamePanel.IsInputBlocked)
            return;

        if (gamePanel.CurrentSource == GamePanel.Source.Local)
            return;

        localToggle.SetIsOnWithoutNotify(true);
        localNode.SetActive(true);
        remoteNode.SetActive(false);

        showingRemote = false;

        gamePanel.UpdateLocalQuests();
    }

    public void SelectRemote()
    {
        if (gamePanel.IsInputBlocked)
            return;

        if (gamePanel.CurrentSource == GamePanel.Source.Remote || !remoteToggle.interactable)
            return;

        remoteToggle.SetIsOnWithoutNotify(true);
        localNode.SetActive(false);
        remoteNode.SetActive(true);

        showingRemote = true;

        ShowRemoteFolders();
    }

}
