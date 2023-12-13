using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static string workingDirectory {  get; private set; }
    public static string gamesDirectory {  get; private set; }

    List<AppHelper> runningApps = new List<AppHelper>();

    public GamesList gamesList;
    public GameObject uploadScreen;
    public RectTransform displaySection;

    [Header("Debug")]
    public int targetGame = 0;
    public int displayedGame = -1;
    public int selectedImage = 1;
    int startingPreviewImageDisplayed = 0;

    [Header("Selection Settings")]
    public float gameSwitchCooldown = 0.1f;
    public float imageSwitchCooldown = 0.1f;
    bool movingBanners;
    int movingBannersReceivingInput; // used similarly to a bool, but uses -1 and 1 as true, indicating the updated direction for moving the banners
    bool movingImages;

    [Space]
    public Vector2 gameSwitchDisplayMovement = new Vector2(10, 10);
    public AnimationCurve gameSwitchDisplayMovementCurve;

    [Space]
    public Vector2 selectedBannerOffset = new Vector2(1, 0);
    public AnimationCurve gameSwitchBannerPullInCurve;
    public AnimationCurve gameSwitchBannerMovementCurve;
    public AnimationCurve gameSwitchBannerPullOutCurve;

    [Space]
    public bool hideTextWhenBannerIsLoaded = true;
    public Color normalColor = Color.white;
    public Color runningColor = Color.green;

    [Header("Preview Data")]
    public RawImage gamePreviewImage;
    public TextMeshProUGUI gamePreviewTitle;
    public TextMeshProUGUI gamePreviewDescription;
    public List<RawImage> gameSubPreviewImages;
    RawImage gameSubPreviewImageHidden;
    public Image gamePreviewSelectorImage;

    Thread imageLoadingThread;
    List<TextureData> imageLoadingThreadData;

    DataManager dataManager;
    List<GameData> gameData = new List<GameData>();
    List<Texture> textures = new List<Texture>();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        Texture.allowThreadedTextureCreation = true;

#if UNITY_EDITOR
        workingDirectory = Application.persistentDataPath;
#else
        workingDirectory = Application.dataPath;
#endif

        gamesDirectory = workingDirectory + "/Games/";

        dataManager = new DataManager(gamesDirectory);
        gameData = dataManager.GetAllGameData();

        if (gameSubPreviewImages.Count > 0) {
            gameSubPreviewImageHidden = Instantiate(gameSubPreviewImages[0]);
            gameSubPreviewImageHidden.enabled = false;
        }

        gamesList.banners[0].GetComponent<RectTransform>().anchoredPosition += selectedBannerOffset;

        if (gameData.Count > 0) {
            PickDisplayGame();
            UpdateBannersAndPreview();
        }
    }

    private void Update()
    {
        if (gameData.Count <= 0) {
            UpdateAllSelectionTextCustom("Please Upload A Game");
        } else {
            GamesFoundUpdate();
        }

        if (Input.GetKeyDown(KeyCode.F12)) {
            SetGameUploadScreenActive(!uploadScreen.active);
        }
    }

    /// <summary>
    /// The regular update loop assuming at least one game has been uploaded
    /// </summary>
    void GamesFoundUpdate()
    {
        if (!uploadScreen.active) {
            for (int i = 0; i < runningApps.Count; ++i) {
                if (runningApps[i] != null) {
                    if (runningApps[i].hasExited) {
                        runningApps.RemoveAt(i);
                        --i;

                        // in case the game being played was currently selected
                        UpdateBannersAndPreview();
                    }
                }
            }

            // select a game
            float verticalInput = Input.GetAxis("Vertical");

            if (verticalInput > 0.5f) {
                if (movingBannersReceivingInput < 1) movingBannersReceivingInput = 1;
                StartCoroutine(MoveBanners(1, gameSwitchCooldown));
            } else if (verticalInput < -0.5f) {
                if (movingBannersReceivingInput > -1) movingBannersReceivingInput = -1;
                StartCoroutine(MoveBanners(-1, gameSwitchCooldown));
            } else {
                if (movingBannersReceivingInput != 0) movingBannersReceivingInput = 0;
            }

            // select a preview image
            float horizontalInput = Input.GetAxis("Horizontal");

            if (horizontalInput > 0.5f) {
                StartCoroutine(MovePicture(1, imageSwitchCooldown));
            } else if (horizontalInput < -0.5f) {
                StartCoroutine(MovePicture(-1, imageSwitchCooldown));
            }

            // start running a game
            if (Input.GetButtonDown("Select")) {
                AppHelper targetApp = GetAppIfRunning(gameData[displayedGame]);

                if (targetApp != null) {
                    targetApp.KillApp();
                } else {
                    runningApps.Add(new AppHelper(gameData[displayedGame]));
                }

                UpdateBannersAndPreview();
            }

            if (imageLoadingThread != null && !imageLoadingThread.IsAlive) {
                UploadTextures(imageLoadingThreadData);
                imageLoadingThread = null;
            }
        }
    }

    public void SetGameUploadScreenActive(bool active)
    {
        uploadScreen.SetActive(active);
    }

    /// <summary>
    /// Opens a choose file dialog and outputs the selected file path to the given input field
    /// </summary>
    /// <param name="inputField"></param>
    public void SelectFileDialogExe(TMP_InputField inputField)
    {
        string[] outs = FileManager.FileDialog();
        if (outs.Length > 0) inputField.text = outs[0];
    }

    /// <summary>
    /// Opens a choose file dialog and outputs the selected file path to the given input field
    /// </summary>
    /// <param name="inputField"></param>
    public void SelectFileDialogImage(InputFieldExtension inputField)
    {
        SFB.ExtensionFilter[] extensions = new SFB.ExtensionFilter[1];
        extensions[0] = new SFB.ExtensionFilter();
        extensions[0].Extensions = new string[] {
            "png",
            "jpg",
        };

        string[] outs = FileManager.FileDialog(extensions, true);
        if (outs.Length > 0) inputField.ApplyNewStrings(outs);
    }

    /// <summary>
    /// Adds GameData to the list of games the launcher is tracking.
    /// </summary>
    /// <param name="data"></param>
    void AddExistingGameData(GameData data, bool updateText = false)
    {
        if (gameData.Contains(data)) return;
        for (int i = 0; i < gameData.Count; ++i) {
            if (gameData[i].gameTitle == data.gameTitle) {
#if UNITY_EDITOR
                Debug.LogWarning("There is already a game with the name: \"" + data.gameTitle + "\"");
#endif

                return;
            }
        }

        gameData.Add(data);
        dataManager.WriteGameData(data);
        if (updateText) UpdateBannersAndPreview();
    }

    /// <summary>
    /// Sets displayedGame to targetGame, but accounts for going above max gameData, or below 0
    /// </summary>
    void PickDisplayGame()
    {
        if (targetGame < 0) targetGame = gameData.Count - 1;
        else if (targetGame >= gameData.Count) targetGame = 0;
        displayedGame = targetGame;
    }

    /// <summary>
    /// Updates the banner with the relative index to the middle to have the correct text
    /// </summary>
    /// <param name="index"></param>
    void UpdateBanner(int index)
    {
        GameObject targetBanner = gamesList.GetListObject(index);
        GameData targetGameData = SafeGetGameData(displayedGame + index);

        TextMeshProUGUI targetText = targetBanner.GetComponentInChildren<TextMeshProUGUI>();

        if (targetText != null) {
            targetText.text = targetGameData.gameTitle;
            if (CheckAppRunning(SafeGetGameData(displayedGame + index))) {
                targetText.color = runningColor;
            } else {
                targetText.color = normalColor;
            }
        }

        RawImage targetImage = targetBanner.GetComponentInChildren<RawImage>();

        if (targetImage != null) {
            if (targetGameData.bannerTexture == null) {
                dataManager.LoadBannerData(ref targetGameData);
            }

            if (targetGameData.bannerTexture != null) {
                targetImage.texture = targetGameData.bannerTexture;
            } else {
                targetImage.texture = null;
            }
        }

        if (hideTextWhenBannerIsLoaded && targetText != null && targetImage != null & targetGameData.bannerTexture != null) {
            targetText.enabled = false;
        } else {
            targetText.enabled = true;
        }
    }

    /// <summary>
    /// Updates all banners and the preview data to be correct
    /// </summary>
    void UpdateBannersAndPreview()
    {
        UpdateBanner(0);

        for (int i = 1; i < gamesList.layers + 1; i++) {
            UpdateBanner(i);
            UpdateBanner(-i);
        }

        UpdatePreviewData();
    }

    /// <summary>
    /// Forces all banners to have the set text
    /// </summary>
    /// <param name="txt"></param>
    void UpdateAllSelectionTextCustom(string txt)
    {
        for (int i = -gamesList.layers + 1; i < gamesList.layers; i++) {
            gamesList.GetListObject(i).GetComponentInChildren<TextMeshProUGUI>().text = txt;
        } 
    }

    /// <summary>
    /// Updates the game preview to display the correct data, and handles running a thread to load new texture data.
    /// </summary>
    void UpdatePreviewData()
    {
        GameData game = SafeGetGameData(displayedGame);
        gamePreviewTitle.text = game.gameTitle;
        gamePreviewDescription.text = game.gameDescription;

        if (game.textures != null) {
            textures = game.textures;
            selectedImage = 0;
            UpdatePreviewImage(selectedImage);
        }

        if (imageLoadingThread != null && imageLoadingThread.IsAlive) {
            //imageLoadingThread.Abort();
            Debug.Log("Joining thread. There is likely a significant frame drop");
            imageLoadingThread.Join();
        }

        imageLoadingThread = new Thread(() => GetTextureDataAsync(gameData[displayedGame].folderPath));
        imageLoadingThread.Start();
    }

    void GetTextureDataAsync(string filepath)
    {
        dataManager.GetTextureData(filepath, out imageLoadingThreadData);
    }

    void UploadTextures(List<TextureData> textureData)
    {
        if (gameData[displayedGame].textures == null || textureData.Count != gameData[displayedGame].textures.Count) {
            gameData[displayedGame].textures = dataManager.LoadTextureData(textureData);
            textures = gameData[displayedGame].textures;
            selectedImage = 0;
            UpdatePreviewImage(selectedImage);
        }
    }

    /// <summary>
    /// Updates the preview image to display the specified image. Does a simple wrap if exceeding the limits of available images.
    /// Also updates the smaller preview images, ensuring that at least the next image is visible.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="setTexture"></param>
    void UpdatePreviewImage(int index)
    {
        if (textures.Count > 0) {
            if (selectedImage >= textures.Count) selectedImage = 0;
            else if (selectedImage < 0) selectedImage = textures.Count - 1;
            //gamePreviewImage.texture = textures[selectedImage];
            gamePreviewImage.material.SetTexture("_StartTex", textures[selectedImage]);
            gamePreviewImage.material.SetTexture("_NextTex", textures[selectedImage]);
            gamePreviewImage.material.SetFloat("_Completion", 0.0f);
            if (!gamePreviewImage.enabled) gamePreviewImage.enabled = true;
        } else {
            gamePreviewImage.texture = null;
            gamePreviewImage.enabled = false;
        }

        if (selectedImage >= startingPreviewImageDisplayed + gameSubPreviewImages.Count - 1) {
            startingPreviewImageDisplayed = selectedImage + 2 - gameSubPreviewImages.Count;
            if (startingPreviewImageDisplayed + gameSubPreviewImages.Count > textures.Count) startingPreviewImageDisplayed = textures.Count - gameSubPreviewImages.Count;
        } else if (selectedImage <= startingPreviewImageDisplayed) {
            startingPreviewImageDisplayed = selectedImage - 1;
            if (startingPreviewImageDisplayed < 0) startingPreviewImageDisplayed = 0;
        }

        for (int i = 0; i < gameSubPreviewImages.Count; i++) {
            if (textures.Count > startingPreviewImageDisplayed + i) {
                gameSubPreviewImages[i].texture = textures[startingPreviewImageDisplayed + i];
                if (!gameSubPreviewImages[i].enabled) gameSubPreviewImages[i].enabled = true;
            } else {
                gameSubPreviewImages[i].enabled = false;
            }
        }

        //gamePreviewSelectorImage.rectTransform.position = gameSubPreviewImages[selectedImage - startingPreviewImageDisplayed].rectTransform.position;
        gamePreviewSelectorImage.transform.SetParent(gameSubPreviewImages[selectedImage - startingPreviewImageDisplayed].transform, false);
    }

    /// <summary>
    /// Returns gameData[index] accounting for going over gameData.Count or under 0
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    GameData SafeGetGameData(int index)
    {
        if (index >= gameData.Count) {
            index %= gameData.Count;
        } else if (index < 0) {
            index = (gameData.Count - index * -1 % gameData.Count) % gameData.Count;
        }

        return gameData[index];
    }

    /// <summary>
    /// Returns true if the app tied to the given data is currently running
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    bool CheckAppRunning(GameData data)
    {
        for (int i = 0; i < runningApps.Count; ++i) {
            if (runningApps[i].getData == data) return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the AppHelper of the given data if it is currently running
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    AppHelper GetAppIfRunning(GameData data)
    {
        for (int i = 0; i < runningApps.Count; ++i) {
            if (runningApps[i].getData == data) return runningApps[i];
        }

        return null;
    }

    /// <summary>
    /// Moves the banners in the given direction over a set duration. Updates all displays and the selected game after the move is complete
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    IEnumerator MoveBanners(int direction, float duration)
    {
        if (direction == 0 || movingBanners) yield break;
        if (direction < 0) direction = -1;
        else direction = 1;

        movingBanners = true;

        int[] accessors = new int[gamesList.banners.Count - 2];
        void CalculateAccessors(ref int[] accessors)
        {
            for (int i = 0; i < accessors.Length; i++) {
                if (direction > 0) {
                    if (i % 2 == 0) {
                        accessors[i] = i + 2;
                    } else {
                        accessors[i] = i - 2;
                        if (accessors[i] < 0) {
                            accessors[i] = 0;
                        }
                    }
                } else {
                    if (i % 2 == 1) {
                        accessors[i] = i + 2;
                    } else {
                        accessors[i] = i - 2;
                        if (accessors[i] < 0) {
                            accessors[i] = 1;
                        }
                    }
                }
            }
        }

        CalculateAccessors(ref accessors);

        Vector2[] startingPositions = new Vector2[gamesList.banners.Count];
        Vector2[] targetPositions = new Vector2[gamesList.banners.Count];
        RectTransform[] bannerRectTransforms = new RectTransform[gamesList.banners.Count];
        for (int i = 0; i < startingPositions.Length; i++) {
            bannerRectTransforms[i] = gamesList.banners[i].GetComponent<RectTransform>();

            if (i == 0) { // i == 0 is the middle banner, and therefore the one that is jutting out
                startingPositions[i] = bannerRectTransforms[i].anchoredPosition - selectedBannerOffset;
            } else {
                startingPositions[i] = bannerRectTransforms[i].anchoredPosition;
            }
        }

        void CalculateTargetPositions()
        {
            for (int i = 0; i < accessors.Length; i++) {
                RectTransform parentRect = bannerRectTransforms[i].parent.GetComponent<RectTransform>();
                RectTransform targetParentRect = bannerRectTransforms[accessors[i]].parent.GetComponent<RectTransform>();

                targetPositions[i] = targetParentRect.anchoredPosition - parentRect.anchoredPosition;
            }
        }

        CalculateTargetPositions();

        Vector2 displaySectionOriginPos = displaySection.anchoredPosition;
        Vector2 displaySectionStartPos = displaySection.anchoredPosition;
        Vector2 displaySectionMove = gameSwitchDisplayMovement;
        if (direction < 0) displaySectionMove.y *= -1;

        float elapsedTime = 0.0f;
        bool halftime = false;
        bool loopsave = false;
        while (elapsedTime < duration) {
            elapsedTime += Time.deltaTime;
            float completionPercent = elapsedTime / duration;
            if (completionPercent > 1) completionPercent = 1;
            if (!halftime && completionPercent > 0.5f) {
                halftime = true;
                displaySectionStartPos = displaySection.anchoredPosition;

                if (!loopsave) {
                    targetGame -= direction; // moving upwards gets us a "lower" game in the gameData list
                    PickDisplayGame();
                    UpdatePreviewData();
                }
            }

            if (completionPercent <= 1.0f / 3.0f) {
                float eval = gameSwitchBannerPullInCurve.Evaluate(completionPercent * 3);
                bannerRectTransforms[0].anchoredPosition = Vector2.Lerp(startingPositions[0] + selectedBannerOffset, startingPositions[0], eval);

                if (gamesList.highlightedGame != null) {
                    Color c = gamesList.highlightedGame.color;
                    c.a = 1 - eval;
                    gamesList.highlightedGame.color = c;
                }
            } else if (completionPercent <= 2.0f / 3.0f) {
                float eval = gameSwitchBannerMovementCurve.Evaluate(completionPercent % (1.0f / 3.0f) * 3);
                for (int i = 0; i < accessors.Length; i++) {
                    bannerRectTransforms[i].anchoredPosition = Vector2.Lerp(startingPositions[i], targetPositions[i], eval);
                }
            } else {
                if (!loopsave) {
                    if (movingBannersReceivingInput != 0) {
                        displaySectionStartPos = displaySection.anchoredPosition;

                        if (movingBannersReceivingInput > 0 && direction < 1) {
                            direction = 1;
                            displaySectionMove.y *= -1;
                            CalculateAccessors(ref accessors);
                            CalculateTargetPositions();
                        } else if (movingBannersReceivingInput < 0 && direction > -1) {
                            direction = -1;
                            displaySectionMove.y *= -1;
                            CalculateAccessors(ref accessors);
                            CalculateTargetPositions();
                        }

                        halftime = false;

                        for (int i = 0; i < startingPositions.Length; i++) {
                            bannerRectTransforms[i].anchoredPosition = startingPositions[i];
                        }
                        UpdateBannersAndPreview();

                        if (gamesList.highlightedGame != null) {
                            Color c = gamesList.highlightedGame.color;
                            c.a = 0.0f;
                            gamesList.highlightedGame.color = c;
                        }

                        elapsedTime = duration / 3.0f - elapsedTime % (duration / 3.0f);
                        loopsave = true;
                        continue;
                    }

                    int pullOutIndex;
                    if (direction < 0) pullOutIndex = 2;
                    else pullOutIndex = 1;

                    float eval = gameSwitchBannerPullOutCurve.Evaluate(completionPercent % (1.0f / 3.0f) * 3);
                    bannerRectTransforms[pullOutIndex].anchoredPosition = Vector2.Lerp(targetPositions[accessors[0]], targetPositions[accessors[0]] + selectedBannerOffset, eval);
                    
                    if (gamesList.highlightedGame != null) {
                        if (gamesList.highlightedGame.transform.parent == gamesList.banners[0].transform) {
                            gamesList.highlightedGame.transform.SetParent(gamesList.GetListObject(-direction).transform, false);
                        }

                        Color c = gamesList.highlightedGame.color;
                        c.a = eval;
                        gamesList.highlightedGame.color = c;
                    }
                } else {
                    for (int i = 0; i < accessors.Length; i++) {
                        bannerRectTransforms[i].anchoredPosition = startingPositions[accessors[i]];
                    }
                }
            }

            if (completionPercent <= 0.5f) {
                displaySection.anchoredPosition = Vector3.Lerp(displaySectionStartPos, displaySectionOriginPos + displaySectionMove, gameSwitchDisplayMovementCurve.Evaluate(completionPercent * 2));
            } else {
                displaySection.anchoredPosition = Vector3.Lerp(displaySectionStartPos, displaySectionOriginPos, gameSwitchDisplayMovementCurve.Evaluate(completionPercent * 2 - 1));
            }

            loopsave = false;

            yield return new WaitForEndOfFrame();
        }

        startingPositions[0] += selectedBannerOffset;
        for (int i = 0; i < startingPositions.Length; i++) {
            bannerRectTransforms[i].anchoredPosition = startingPositions[i];
        }

        if (gamesList.highlightedGame != null) {
            gamesList.highlightedGame.transform.SetParent(gamesList.banners[0].transform, false);
        }

        movingBanners = false;

        //targetGame -= direction; // moving upwards gets us a "lower" game in the gameData list
        //PickDisplayGame();
        UpdateBannersAndPreview();
        displaySection.anchoredPosition = displaySectionOriginPos;

        yield break;
    }

    /// <summary>
    /// Changes the selected picture and handles anything during that process. 
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    IEnumerator MovePicture(int direction, float duration)
    {
        if (direction == 0 || movingImages) yield break;
        if (direction < 0) direction = -1;
        else direction = 1;

        movingImages = true;

        float elapsedTime = 0;

        Material fadeMat = gamePreviewImage.material;
        Texture startTex = textures[selectedImage];

        selectedImage += direction;
        UpdatePreviewImage(selectedImage);

        fadeMat.SetTexture("_StartTex", startTex);
        //fadeMat.SetTexture("_NextTex", textures[selectedImage]); // _NextTex is implicitly set by calling UpdatePreviewImage

        while (elapsedTime < duration) {
            elapsedTime += Time.deltaTime;

            fadeMat.SetFloat("_Completion", elapsedTime / duration);

            yield return new WaitForEndOfFrame();
        }

        fadeMat.SetTexture("_StartTex", textures[selectedImage]);
        fadeMat.SetFloat("_Completion", 1.0f);

        movingImages = false;

        yield break;
    }
}
