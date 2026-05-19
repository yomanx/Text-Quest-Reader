using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TextQuestReader.Cinematic;
using TextQuestReader.Cinematic.Procedural;
using TextQuestReader.Monetization;
using TextQuestReader.View;
using UnityEngine;

public class GamePanel : MonoBehaviour
{
    [SerializeField] private RectTransform mainPictureRect;
    [SerializeField] private RectTransform paramsRect;
    [SerializeField] private RectTransform paramsContent;
    [SerializeField] private RectTransform mainTextRect;
    [SerializeField] private RectTransform questionsRect;
    [SerializeField] private RectTransform questionsContent;
    [SerializeField] private RectTransform canvas;

    [SerializeField] private GameObject parameterTextPref;
    [SerializeField] private GameObject sourcesNode;
    [SerializeField] private GameObject blockerNode;

    [SerializeField] private QuestionCell victoryCell;
    [SerializeField] private QuestionCell defeatCell;
    [SerializeField] private QuestionCell nextCell;
    [SerializeField] private QuestionCell questionCellPref;

    [SerializeField] private SettingsPanel settingsPref;
    [SerializeField] private AliveText mainText;
    [SerializeField] private PictureNode pictureNode;
    [SerializeField] private QuestCell questCellPref;
    [SerializeField] private SourcesNode sourcesNodeScript;

    private Player player;
    public Player Player => player;

    public static GamePanel Instance { get; private set; }

    private Passage singlePassage;

    private TextParser textParser;
    public TextParser TextParser => textParser;

    public PictureNode PictureNode => pictureNode;

    private LocationDescriptionResolver locationDescriptionResolver;
    private PassageResolver passageResolver;
    private ParameterService parameterService;

    public LocationDescriptionResolver LocationDescriptionResolver => locationDescriptionResolver;
    public PassageResolver PassageResolver => passageResolver;
    public ParameterService ParameterService => parameterService;

    private QuestShort selectedQuest;
    private bool selectedQuestIsRemote;

    public event Action HandleLocalizationsEvent;
    public event Action RemoteQuestSelectionStarted;
    public event Action RemoteQuestSelectionEnded;
    public event Action StartQuestEnded;
    public event Action<Location> LocationShown;
    public event Action<Passage> PassageShown;
    public event Action<bool> QuestEnded;
    public event Action<string> MainTextRendered;
    public event Action<List<PassageInfo>> ChoicesReady;
    public event Action<Passage> SinglePassageReady;
    public event Action<List<TextQuestReader.Monetization.CatalogEntry>, Source, string> CatalogReady;
    public event Action<QuestShort, bool> QuestPreviewSelected;

    public enum Source { Local, Remote }
    public Source CurrentSource { get; private set; }

    List<QuestShort> remoteList;

    private string activeRemoteQuestFolder;

    private readonly List<IKeyboardSelectable> keyboardItems = new();
    private int keyboardIndex = -1;

    private bool isStartingQuest;
    public bool IsInputBlocked => isStartingQuest;

    private bool showingStartLocation;
    private bool cancelStartRequested;

    #region Inits

    private void Awake()
    {
        Instance = this;
        textParser = new TextParser(this);
        locationDescriptionResolver = new LocationDescriptionResolver(textParser);
        passageResolver = new PassageResolver(this, textParser);
        parameterService = new ParameterService(this, textParser, paramsContent, parameterTextPref,
                                                victoryCell, defeatCell, nextCell, questionsContent);

        EnsureCinematicService();
        EnsureMonetizationService();
        EnsureResultScreen();
        EnsureProceduralBackground();
        EnsureMainMenuSkin();
    }

    private ProceduralSceneRenderer proceduralBackground;

    private void EnsureProceduralBackground()
    {
        if (proceduralBackground != null) return;
        if (mainPictureRect == null) return;
        proceduralBackground = ProceduralSceneRenderer.Attach(mainPictureRect, siblingIndex: 0);
    }

    private MainMenuTerminalSkin mainMenuSkin;

    private void EnsureMainMenuSkin()
    {
        if (mainMenuSkin != null) return;
        if (canvas == null) return;
        mainMenuSkin = MainMenuTerminalSkin.Attach(canvas);
    }

    private void EnsureResultScreen()
    {
        Canvas hostCanvas = canvas != null ? canvas.GetComponentInParent<Canvas>() : null;
        if (hostCanvas == null) hostCanvas = FindAnyObjectByType<Canvas>();
        if (hostCanvas != null)
            ResultScreen.Bootstrap(this, hostCanvas);
    }

    private void EnsureCinematicService()
    {
        if (CinematicEffectsService.Instance == null)
        {
            Canvas hostCanvas = canvas != null ? canvas.GetComponentInParent<Canvas>() : null;
            if (hostCanvas == null) hostCanvas = FindAnyObjectByType<Canvas>();

            RectTransform shakeRoot = canvas != null ? canvas.GetComponent<RectTransform>() : null;
            CinematicEffectsService.Bootstrap(hostCanvas, shakeRoot);
        }
        else if (canvas != null)
        {
            CinematicEffectsService.Instance.SetShakeRoot(canvas);
        }
    }

    private void EnsureMonetizationService()
    {
        if (MonetizationService.Instance == null)
        {
            GameObject go = new GameObject("MonetizationService");
            go.AddComponent<MonetizationService>();
        }
    }

    private void Start()
    {
        mainPictureRect.sizeDelta = new Vector2(mainPictureRect.rect.height, mainPictureRect.sizeDelta.y);
        paramsRect.sizeDelta = new Vector2(mainPictureRect.rect.height, paramsRect.sizeDelta.y);
        mainTextRect.sizeDelta = new Vector2(canvas.rect.width - mainPictureRect.sizeDelta.x, mainTextRect.sizeDelta.y);
        questionsRect.sizeDelta = new Vector2(canvas.rect.width - mainPictureRect.sizeDelta.x, questionsRect.sizeDelta.y);

        blockerNode.SetActive(false);

        HandleLocalizations();

        if (proceduralBackground != null)
            proceduralBackground.ShowPreset("terminal_room", instant: true);

        Player loadedPlayer = SaveLoadManager.Instance.LoadPlayer();

        if (loadedPlayer == null)
            UpdateLocalQuests();
        else
            player = loadedPlayer;

        if (player == null)
            return;

        sourcesNode.SetActive(false);

        ShowCurrentLocation();
    }

    private void Update()
    {
        if (IsInputBlocked)
            return;

        if (player != null && Input.GetKeyDown(KeyCode.Escape))
        {
            AudioManager.Instance.PlaySfx(SoundType.Click);
            AbandonQuest();
            return;
        }

        if (player == null)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                AudioManager.Instance.PlaySfx(SoundType.Click);
                sourcesNodeScript.SelectLocal();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                AudioManager.Instance.PlaySfx(SoundType.Click);
                sourcesNodeScript.SelectRemote();
            }
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            AudioManager.Instance.PlaySfx(SoundType.Click);
            MoveKeyboardSelection(-1);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            AudioManager.Instance.PlaySfx(SoundType.Click);
            MoveKeyboardSelection(1);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (mainText != null && mainText.IsTyping)
            {
                mainText.FinishImmediately();
                return;
            }

            AudioManager.Instance.PlaySfx(SoundType.Click);
            SubmitKeyboardSelection();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (mainText != null && mainText.IsTyping)
            {
                mainText.FinishImmediately();
                return;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (mainText != null && mainText.IsTyping)
            {
                mainText.FinishImmediately();
            }
        }
    }

    public void HandleLocalizations()
    {
        if (!PlayerPrefs.HasKey(Localization.LANGUAGE_KEY))
        {
            string lang = Localization.GetLangCode(Application.systemLanguage);
            PlayerPrefs.SetString(Localization.LANGUAGE_KEY, lang);
        }

        Localization.SetCurrentLanguage(PlayerPrefs.GetString(Localization.LANGUAGE_KEY, "en"));

        HandleLocalizationsEvent?.Invoke();
    }

    #endregion

    #region Publics

    public void StartQuest()
    {
        if (isStartingQuest)
            return;

        cancelStartRequested = false;

        AudioManager.Instance.PlaySfx(SoundType.Click);

        if (selectedQuest == null)
        {
            StartQuestEnded?.Invoke();
            return;
        }

        if (!IsQuestAccessAllowed(selectedQuest))
        {
            PromptPremiumUnlock(selectedQuest);
            StartQuestEnded?.Invoke();
            return;
        }

        isStartingQuest = true;

        blockerNode.SetActive(true);

        System.Action launch = () =>
        {
            if (selectedQuestIsRemote)
                StartRemoteQuest(selectedQuest.Id);
            else
                StartLocalQuest(selectedQuest.QuestName);
        };

        if (CinematicEffectsService.Instance != null)
            CinematicEffectsService.Instance.FadeOut(0.35f, launch);
        else
            launch();
    }

    private bool IsQuestAccessAllowed(QuestShort quest)
    {
        if (quest == null) return false;
        if (MonetizationService.Instance == null) return true;
        return MonetizationService.Instance.IsQuestUnlocked(quest.QuestName);
    }

    private void PromptPremiumUnlock(QuestShort quest)
    {
        if (quest == null || MonetizationService.Instance == null) return;

        ProductDefinition product = MonetizationService.Instance.FindProductForQuest(quest.QuestName);
        if (product == null)
        {
            Debug.LogWarning($"Premium quest {quest.QuestName} has no product defined in monetization.json — cannot prompt.");
            return;
        }

        Canvas hostCanvas = canvas != null ? canvas.GetComponentInParent<Canvas>() : null;
        if (hostCanvas == null) hostCanvas = FindAnyObjectByType<Canvas>();
        if (hostCanvas == null) return;

        UnlockModal.Show(hostCanvas, product, confirmed =>
        {
            if (!confirmed) return;
            MonetizationService.Instance.BeginPurchase(product, result =>
            {
                if (result.Status == PurchaseStatus.Success || result.Status == PurchaseStatus.AlreadyOwned)
                {
                    Debug.Log($"Premium quest unlocked: {quest.QuestName}");
                    if (CurrentSource == Source.Local) UpdateLocalQuests(quest.QuestName);
                    else UpdateRemoteQuests(remoteList);
                }
                else
                {
                    Debug.LogWarning($"Purchase failed: {result.Message}");
                }
            });
        });
    }

    public void ActionNext()
    {
        if (IsInputBlocked)
            return;

        if (singlePassage == null)
            return;

        AudioManager.Instance.PlaySfx(SoundType.Click);

        player.locationID = singlePassage.to;
        player.passageID = singlePassage.id;

        ShowPassage(singlePassage);
    }

    public void ActionSettings()
    {
        if (IsInputBlocked)
            return;

        AudioManager.Instance.PlaySfx(SoundType.Click);

        SettingsPanel panel = Instantiate(settingsPref, canvas);
        panel.Init(this);

        if (panel.GetComponent<ModernSettingsExtension>() == null)
            panel.gameObject.AddComponent<ModernSettingsExtension>();
    }

    public void ActionCloseBlocker()
    {
        AudioManager.Instance.PlaySfx(SoundType.Click);
        cancelStartRequested = true;
        blockerNode.SetActive(false);
    }

    public void AbandonQuest()
    {
        CinematicEffectsService.Instance?.StopAllEffects();

        ClearQuestions();
        parameterService.ClearParams();
        pictureNode.ClearPicturesColor();

        if (proceduralBackground != null)
            proceduralBackground.ShowPreset("terminal_room");

        sourcesNode.SetActive(true);
        nextCell.gameObject.SetActive(false);
        victoryCell.gameObject.SetActive(false);
        defeatCell.gameObject.SetActive(false);

        player = null;
        singlePassage = null;

        switch (CurrentSource)
        {
            case Source.Local: UpdateLocalQuests(); break;
            case Source.Remote: UpdateRemoteQuests(remoteList); break;
        }

        SaveLoadManager.Instance.ClearPlayerSaveData();
        ClearActiveRemoteQuestFolder();
    }

    public void ShowPassage(Passage passage)
    {
        if (player == null || player.gameOver)
            return;

        singlePassage = null;

        parameterService.ApplyInfluences(passage, ShowMainText);

        if (!passage.ignoreDemonstration)
            parameterService.Demonstrate(passage);

        passage.visitCounter++;

        PassageShown?.Invoke(passage);

        Location location = player.quest.FindLocationWith(player.locationID);

        if (string.IsNullOrEmpty(passage.description) || location.locationType == LocationType.Empty)
        {
            if (location.locationType == LocationType.Empty && !string.IsNullOrEmpty(passage.description))
                location.descriptions[0] = passage.description;

            ShowCurrentLocation();
        }
        else
        {
            ShowMainText(textParser.Parse(passage.description));
            ClearQuestions();

            Passage next = new Passage
            {
                to = passage.to,
                question = Localization.GetForLanguage(LocKeys.Next, player.quest.lang),
                ignoreDemonstration = true
            };

            singlePassage = next;

            SinglePassageReady?.Invoke(next);

            nextCell.StartAsNext(this);
            nextCell.gameObject.SetActive(true);

            RegisterKeyboardItem(nextCell);
            SelectKeyboardItem(0);
        }
    }

    public void TriggerPassageById(int passageId)
    {
        if (player == null || player.gameOver) return;
        Passage passage = player.quest.FindPassageWith(passageId);
        if (passage == null) return;
        player.locationID = passage.to;
        player.passageID = passage.id;
        ShowPassage(passage);
    }

    public void TriggerNextSinglePassage()
    {
        ActionNext();
    }

    public bool HasActivePlayer => player != null;

    public void DiselectAllQuestCells()
    {
        foreach (Transform cell in questionsContent)
        {
            if (cell.TryGetComponent(out QuestCell questCell))
                questCell.Diselect();
        }
    }

    public void SelectQuest(QuestShort questShort)
    {
        if (IsInputBlocked)
            return;

        if (questShort == null)
            return;

        if (selectedQuestIsRemote)
        {
            SelectRemoteQuestPreview(questShort);
            return;
        }

        selectedQuest = questShort;

        string title = string.IsNullOrEmpty(questShort.DisplayName) ? questShort.QuestName : questShort.DisplayName;

        mainText.SetText($"<b>{title}</b>\n\n{questShort.Description}");
        pictureNode.SetNewPicture(questShort.StartImage, questShort.QuestName, mayBeSame: true);
        AudioManager.Instance.PlayMusic(questShort.StartMusic, questShort.QuestName, stoppable: true);

        QuestPreviewSelected?.Invoke(questShort, false);

        if (proceduralBackground != null)
        {
            string presetKey = ResolveCatalogPresetKey(questShort);
            proceduralBackground.ShowPreset(presetKey);
        }
    }

    private string ResolveCatalogPresetKey(QuestShort questShort)
    {
        if (questShort == null) return "deep_space";
        if (!string.IsNullOrEmpty(questShort.StartImage)) return questShort.StartImage;

        string name = questShort.QuestName?.ToLowerInvariant() ?? string.Empty;
        if (name.Contains("asteroid") || name.Contains("space")) return "deep_space";
        if (name.Contains("victory")) return "victory_scene";
        return "terminal_room";
    }

    public void UpdateLocalQuests(string questNameToSelect = null)
    {
        CurrentSource = Source.Local;

        selectedQuestIsRemote = false;

        ClearQuestions();

        var builtInQuestFolders = QuestHelper.GetAllQuestFolders();
        var userQuestFolders = QuestHelper.GetUserQuestFolders();

        List<string> allQuestFolders = new List<string>();

        allQuestFolders.AddRange(userQuestFolders);

        foreach (string folder in builtInQuestFolders)
        {
            if (!allQuestFolders.Contains(folder))
                allQuestFolders.Add(folder);
        }

        List<QuestShort> quests = new List<QuestShort>();

        foreach (string folder in allQuestFolders)
        {
            QuestShort questShort = SaveLoadManager.Instance.LoadQuestShortFromFolder(folder);

            if (questShort == null)
                continue;

            quests.Add(questShort);
        }

        quests = quests.OrderBy(q => q.Order).ThenBy(q => q.QuestName).ToList();

        ShowQuestShortList(quests, questNameToSelect, isRemote: false);
    }

    public void UpdateRemoteQuests(List<QuestShort> list)
    {
        CurrentSource = Source.Remote;

        selectedQuestIsRemote = true;

        ClearQuestions();

        if (list == null || list.Count == 0)
            return;

        list = list.OrderBy(x => x.Order).ThenBy(x => x.QuestName).ToList();

        string selectedQuestName = selectedQuest != null ? selectedQuest.QuestName : null;

        remoteList = list;

        ShowQuestShortList(list, selectedQuestName, isRemote: true);
    }

    #endregion

    #region Quest Preview

    private void ShowQuestShortList(List<QuestShort> quests, string questNameToSelect, bool isRemote)
    {
        QuestShort firstQuest = null;
        QuestShort questToSelect = null;

        QuestCatalogService catalogService = new QuestCatalogService();
        List<CatalogEntry> entries = catalogService.BuildEntries(quests, isRemote ? QuestSourceKind.Remote : QuestSourceKind.Local);

        CatalogReady?.Invoke(entries, isRemote ? Source.Remote : Source.Local, questNameToSelect);

        for (int i = 0; i < entries.Count; i++)
        {
            CatalogEntry entry = entries[i];
            QuestShort quest = entry.QuestShort;

            bool isSelected = !string.IsNullOrEmpty(questNameToSelect) ? quest.QuestName == questNameToSelect : i == 0;

            QuestCell cell = Instantiate(questCellPref, questionsContent);
            cell.StartWith(this, quest, isSelected);
            RegisterKeyboardItem(cell);

            if (entry.AccessState != QuestAccessState.FreeOpen || entry.IsFeatured)
            {
                PremiumLockOverlay lockOverlay = cell.GetComponent<PremiumLockOverlay>();
                if (lockOverlay == null) lockOverlay = cell.gameObject.AddComponent<PremiumLockOverlay>();
                lockOverlay.Setup(quest.QuestName, entry.AccessState, entry.Product, entry.IsFeatured, OnUnlockButtonClicked);
            }
            else
            {
                PremiumLockOverlay stale = cell.GetComponent<PremiumLockOverlay>();
                if (stale != null) Destroy(stale);
            }

            if (i == 0)
                firstQuest = quest;

            if (isSelected)
                questToSelect = quest;
        }

        SelectKeyboardItem(questToSelect != null ? quests.IndexOf(questToSelect) : 0);

        if (questToSelect == null)
            questToSelect = firstQuest;

        if (questToSelect == null)
            return;

        if (isRemote)
            SelectRemoteQuestPreview(questToSelect);
        else
        {
            pictureNode.InitImages(questToSelect.StartImage, questToSelect.QuestName);
            SelectQuest(questToSelect);
        }
    }

    private void OnUnlockButtonClicked(ProductDefinition product)
    {
        if (product == null) return;

        Canvas hostCanvas = canvas != null ? canvas.GetComponentInParent<Canvas>() : null;
        if (hostCanvas == null) hostCanvas = FindAnyObjectByType<Canvas>();
        if (hostCanvas == null) return;

        UnlockModal.Show(hostCanvas, product, confirmed =>
        {
            if (!confirmed) return;
            MonetizationService.Instance.BeginPurchase(product, result =>
            {
                string selected = selectedQuest != null ? selectedQuest.QuestName : null;
                if (CurrentSource == Source.Local) UpdateLocalQuests(selected);
                else UpdateRemoteQuests(remoteList);
            });
        });
    }

    #endregion

    #region Start Quest

    private void LocalizeQuestButtons()
    {
        if (player == null || player.quest == null)
            return;

        nextCell.SetText(Localization.GetForLanguage(LocKeys.Next, player.quest.lang));
        victoryCell.SetText(Localization.GetForLanguage(LocKeys.YouWin, player.quest.lang));
        defeatCell.SetText(Localization.GetForLanguage(LocKeys.YouLose, player.quest.lang));
    }

    private void StartLocalQuest(string questName)
    {
        Quest quest = SaveLoadManager.Instance.LoadQuestFromFolder(questName);

        if (quest == null)
        {
            Debug.LogWarning("Quest not found: " + questName);
            EndStartQuestBlock();
            return;
        }

        CreatePlayer(quest);
        sourcesNode.SetActive(false);
        LocalizeQuestButtons();
        ShowCurrentLocation();
        EndStartQuestBlock();

        CinematicEffectsService.Instance?.FadeIn(0.55f);
    }

    private void StartRemoteQuest(int questId)
    {
        ApiManager.Instance.DownloadQuestPackage(questId, (bytes) =>
        {
            if (cancelStartRequested)
            {
                Debug.Log("Start quest cancelled");
                EndStartQuestBlock();
                return;
            }

            string tempRoot = null;
            string tempZipPath = null;

            try
            {
                if (bytes == null || bytes.Length == 0)
                {
                    Debug.LogWarning("Downloaded package is empty.");
                    return;
                }

                tempRoot = Path.Combine(Application.temporaryCachePath, "RemoteQuestImport");
                Directory.CreateDirectory(tempRoot);

                tempZipPath = Path.Combine(tempRoot, $"quest_{questId}.zip");
                string extractFolder = Path.Combine(tempRoot, $"quest_{questId}");

                if (Directory.Exists(extractFolder))
                    Directory.Delete(extractFolder, true);

                if (File.Exists(tempZipPath))
                    File.Delete(tempZipPath);

                File.WriteAllBytes(tempZipPath, bytes);
                System.IO.Compression.ZipFile.ExtractToDirectory(tempZipPath, extractFolder);

                string questJsonPath = Path.Combine(extractFolder, "quest.json");

                if (!File.Exists(questJsonPath))
                {
                    Debug.LogWarning("Downloaded package does not contain quest.json.");
                    return;
                }

                string json = File.ReadAllText(questJsonPath);
                Quest quest = JsonConvert.DeserializeObject<Quest>(json, SaveLoadManager.JsonSettings);

                if (quest == null)
                {
                    Debug.LogWarning("Quest data is empty.");
                    return;
                }

                activeRemoteQuestFolder = extractFolder;
                pictureNode.SetRemoteQuestFolder(activeRemoteQuestFolder);
                AudioManager.Instance.SetRemoteQuestFolder(activeRemoteQuestFolder);

                CreatePlayer(quest);

                sourcesNode.SetActive(false);

                LocalizeQuestButtons();

                ShowCurrentLocation();

                CinematicEffectsService.Instance?.FadeIn(0.55f);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("Error loading remote quest: " + ex.Message);
            }
            finally
            {
                blockerNode.SetActive(false);

                try
                {
                    if (!string.IsNullOrEmpty(tempZipPath) && File.Exists(tempZipPath))
                        File.Delete(tempZipPath);
                }
                catch (Exception cleanupEx)
                {
                    Debug.LogWarning("Remote import cleanup warning: " + cleanupEx.Message);
                }

                EndStartQuestBlock();
            }
        },
        (error) =>
        {
            Debug.LogWarning("Error downloading quest package: " + error);
            EndStartQuestBlock();
        });
    }


    private void SetBlockerVisible(bool visible)
    {
        if (blockerNode != null)
            blockerNode.SetActive(visible);
    }

    private void EndStartQuestBlock()
    {
        isStartingQuest = false;
        SetBlockerVisible(false);
        StartQuestEnded?.Invoke();
    }

    private void ClearActiveRemoteQuestFolder()
    {
        pictureNode.ClearRemoteQuestFolder();
        AudioManager.Instance.ClearRemoteQuestFolder();

        if (string.IsNullOrEmpty(activeRemoteQuestFolder))
            return;

        try
        {
            if (Directory.Exists(activeRemoteQuestFolder))
                Directory.Delete(activeRemoteQuestFolder, true);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Failed to delete remote quest folder: " + ex.Message);
        }

        activeRemoteQuestFolder = null;
    }

    private void SelectRemoteQuestPreview(QuestShort questShort)
    {
        if (questShort == null)
            return;

        selectedQuest = questShort;

        string title = string.IsNullOrEmpty(questShort.DisplayName) ? questShort.QuestName : questShort.DisplayName;
        mainText.SetText($"<b>{title}</b>\n\n{questShort.Description}");

        AudioManager.Instance.StopMusic();

        RemoteQuestSelectionStarted?.Invoke();

        bool imageDone = false;
        bool musicDone = true;

        void TryEnd()
        {
            if (imageDone && musicDone)
                RemoteQuestSelectionEnded?.Invoke();
        }

        ApiManager.Instance.GetQuestPreviewImage(questShort.Id,
            sprite =>
            {
                pictureNode.SetSpriteRemote(sprite, questShort.QuestName, mayBeSame: true);
                imageDone = true;
                TryEnd();
            },
            error =>
            {
                Debug.LogWarning("Preview image error: " + error);
                imageDone = true;
                TryEnd();
            });
    }

    private void CreatePlayer(Quest quest)
    {
        Quest questClone = (Quest)quest.Clone();

        player = new Player
        {
            locationID = quest.FindStartLocation().id,
            quest = questClone
        };

        foreach (Location location in questClone.locations)
            location.visitCounter = 0;

        foreach (Passage passage in questClone.passages)
        {
            passage.visitCounter = 0;
            passage.FindControversials(questClone);
        }

        foreach (Parameter parameter in questClone.parameters)
            parameter.value = parameter.startValue;

        singlePassage = null;
    }

    #endregion

    #region Privates

    private Quest CreateLocalizedQuestWithProgress(Quest oldQuest)
    {
        Quest loadedQuest = SaveLoadManager.Instance.LoadQuestFromFolder(oldQuest.questName);
        Quest newQuest = (Quest)loadedQuest.Clone();

        for (int i = 0; i < newQuest.parameters.Count && i < oldQuest.parameters.Count; i++)
        {
            newQuest.parameters[i].value = oldQuest.parameters[i].value;
            newQuest.parameters[i].isActive = oldQuest.parameters[i].isActive;
        }

        for (int i = 0; i < newQuest.locations.Count && i < oldQuest.locations.Count; i++)
            newQuest.locations[i].visitCounter = oldQuest.locations[i].visitCounter;

        for (int i = 0; i < newQuest.passages.Count && i < oldQuest.passages.Count; i++)
            newQuest.passages[i].visitCounter = oldQuest.passages[i].visitCounter;

        foreach (Passage passage in newQuest.passages)
            passage.FindControversials(newQuest);

        return newQuest;
    }

    private void ShowCurrentLocation()
    {
        nextCell.gameObject.SetActive(false);
        victoryCell.gameObject.SetActive(false);
        defeatCell.gameObject.SetActive(false);

        Location location = player.quest.FindLocationWith(player.locationID);

        ShowLocationContent(location);

        if (location.locationType == LocationType.Victory)
        {
            player.gameOver = true;
            ClearQuestions();

            victoryCell.gameObject.SetActive(true);
            RegisterKeyboardItem(victoryCell);
            SelectKeyboardItem(0);

            ClearActiveRemoteQuestFolder();
            QuestEnded?.Invoke(true);
        }
        else if (location.locationType == LocationType.Fail)
        {
            player.gameOver = true;
            ClearQuestions();

            defeatCell.gameObject.SetActive(true);
            RegisterKeyboardItem(defeatCell);
            SelectKeyboardItem(0);

            ClearActiveRemoteQuestFolder();
            QuestEnded?.Invoke(false);
        }
        else
        {
            List<PassageInfo> visiblePassages = ShowLocationPassages(location);

            if (visiblePassages != null && visiblePassages.Count == 0)
                Debug.LogWarning("Error: no available transitions!");
        }

        location.visitCounter++;
        LocationShown?.Invoke(location);
    }

    private void Final()
    {
        player.gameOver = true;
        ClearQuestions();
        ClearActiveRemoteQuestFolder();
    }

    private void ShowLocationContent(Location location)
    {
        parameterService.ApplyInfluences(location, ShowMainText);
        parameterService.Demonstrate(location);

        showingStartLocation = location.locationType == LocationType.Start;

        string description = locationDescriptionResolver.Resolve(location);
        ShowMainText(textParser.Parse(description));

        showingStartLocation = false;

        ClearQuestions();
    }

    private List<PassageInfo> ShowLocationPassages(Location location)
    {
        if (player == null || player.gameOver)
            return null;

        List<PassageInfo> visiblePassages = passageResolver.ResolveVisiblePassages(location);

        singlePassage = null;
        nextCell.gameObject.SetActive(false);

        ChoicesReady?.Invoke(visiblePassages);

        const float interval = 120f;

        for (int index = 0; index < visiblePassages.Count; index++)
        {
            PassageInfo info = visiblePassages[index];

            QuestionCell cell = Instantiate(questionCellPref, questionsContent);
            cell.StartWith(this, info.pass, index * 0.15f);

            bool disabled = !info.isAllConditions && info.pass.alwaysShow;
            if (disabled)
                cell.DisableButton();

            ChoiceVisualState visualState = cell.GetComponent<ChoiceVisualState>();
            if (visualState == null) visualState = cell.gameObject.AddComponent<ChoiceVisualState>();
            ChoiceMood mood = disabled ? ChoiceMood.Locked : ChoiceVisualState.InferFromText(info.pass.question);
            visualState.SetMood(mood);

            string strippedQuestion = ChoiceVisualState.StripMoodTags(info.pass.question);
            cell.SetText(textParser.Parse(strippedQuestion));

            RegisterKeyboardItem(cell);
        }

        SelectKeyboardItem(0);

        RectTransform viewPort = (RectTransform)questionsContent.parent;
        questionsContent.sizeDelta = new Vector2(questionsContent.sizeDelta.x, Mathf.Max(viewPort.rect.height, visiblePassages.Count * interval));

        return visiblePassages;
    }

    private void ShowMainText(string text)
    {
        string imageName = textParser.ExtractLastTagValue(ref text, "im");
        string musicName = textParser.ExtractLastTagValue(ref text, "mu");
        string soundName = textParser.ExtractLastTagValue(ref text, "so");
        string bgName = textParser.ExtractLastTagValue(ref text, "bg");

        List<CinematicTagInfo> cinematicTags = CinematicTagParser.ExtractFromText(ref text, textParser);

        if (!string.IsNullOrEmpty(imageName))
            pictureNode.SetNewPicture(imageName, player.quest.questName, mayBeSame: false);

        string proceduralKey = !string.IsNullOrEmpty(bgName) ? bgName : imageName;
        if (!string.IsNullOrEmpty(proceduralKey) && proceduralBackground != null)
            proceduralBackground.ShowPreset(proceduralKey);

        if (!string.IsNullOrEmpty(musicName))
            AudioManager.Instance.PlayMusic(musicName, player.quest.questName, stoppable: false);
        else if (showingStartLocation && !string.IsNullOrEmpty(player.quest.startMusic))
            AudioManager.Instance.PlayMusic(player.quest.startMusic, player.quest.questName, stoppable: false);

        AudioManager.Instance.PlaySfx(soundName, player.quest.questName);

        if (cinematicTags.Count > 0 && CinematicEffectsService.Instance != null)
            CinematicEffectsService.Instance.PlayTags(cinematicTags);

        mainText.SetText(text);
        MainTextRendered?.Invoke(text);
    }

    private void ClearQuestions()
    {
        ClearKeyboardItems();

        foreach (Transform tr in questionsContent)
            Destroy(tr.gameObject);
    }

    #endregion

    private void ClearKeyboardItems()
    {
        foreach (var item in keyboardItems)
            item.SetKeyboardSelected(false);

        keyboardItems.Clear();
        keyboardIndex = -1;
    }

    private void RegisterKeyboardItem(IKeyboardSelectable item)
    {
        if (item == null || !item.IsKeyboardSelectable)
            return;

        keyboardItems.Add(item);
    }

    private void SelectKeyboardItem(int index)
    {
        if (keyboardItems.Count == 0)
            return;

        index = Mathf.Clamp(index, 0, keyboardItems.Count - 1);

        if (keyboardIndex >= 0 && keyboardIndex < keyboardItems.Count)
            keyboardItems[keyboardIndex].SetKeyboardSelected(false);

        keyboardIndex = index;
        keyboardItems[keyboardIndex].SetKeyboardSelected(true);
    }

    private void MoveKeyboardSelection(int direction)
    {
        if (keyboardItems.Count == 0)
            return;

        int nextIndex = keyboardIndex + direction;

        if (nextIndex < 0)
            nextIndex = keyboardItems.Count - 1;

        if (nextIndex >= keyboardItems.Count)
            nextIndex = 0;

        SelectKeyboardItem(nextIndex);
    }

    private void SubmitKeyboardSelection()
    {
        if (keyboardIndex < 0 || keyboardIndex >= keyboardItems.Count)
            return;

        keyboardItems[keyboardIndex].SubmitKeyboard();
    }
}