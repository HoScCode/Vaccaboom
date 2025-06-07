#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button playButton;
    public Button exitButton;
    public Button creditsButton;
    public Toggle soundToggle;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    public CanvasGroup fadeOverlay; // ← Zieh dein schwarzes UI-Overlay hier rein
    public float fadeDuration = 0.5f;

#if UNITY_EDITOR
    public SceneAsset gameSceneAsset;
    public SceneAsset creditsSceneAsset;
#endif

    [HideInInspector] public string gameSceneName;
    [HideInInspector] public string creditsSceneName;

    private Image toggleImage;
    private static MainMenuController instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
#if UNITY_EDITOR
        if (gameSceneAsset != null)
            gameSceneName = gameSceneAsset.name;
        if (creditsSceneAsset != null)
            creditsSceneName = creditsSceneAsset.name;
#endif

        playButton.onClick.AddListener(() => StartCoroutine(PlayGameWithFade()));
        exitButton.onClick.AddListener(ExitGame);
        creditsButton.onClick.AddListener(ShowCredits);

        toggleImage = soundToggle.targetGraphic as Image;
        soundToggle.onValueChanged.AddListener(OnSoundToggle);

        soundToggle.isOn = AudioListener.volume > 0.5f;
        UpdateSoundIcon();

        if (fadeOverlay != null)
        {
            fadeOverlay.alpha = 0;
            fadeOverlay.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenuVisibility();
        }
    }

    IEnumerator PlayGameWithFade()
    {
        yield return StartCoroutine(FadeIn());

        SceneManager.LoadScene(gameSceneName);

        yield return new WaitForSeconds(0.2f); // Mini-Puffer nach dem Laden

        HideMenu();

        yield return StartCoroutine(FadeOut());
    }

    IEnumerator FadeIn()
    {
        if (fadeOverlay == null) yield break;

        fadeOverlay.gameObject.SetActive(true);
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeOverlay.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        fadeOverlay.alpha = 1;
    }

    IEnumerator FadeOut()
    {
        if (fadeOverlay == null) yield break;

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeOverlay.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }

        fadeOverlay.alpha = 0;
        fadeOverlay.gameObject.SetActive(false);
    }

    void ExitGame()
    {
        Application.Quit();
    }

    void ShowCredits()
    {
        if (!string.IsNullOrEmpty(creditsSceneName))
            SceneManager.LoadScene(creditsSceneName);
    }

    void OnSoundToggle(bool isOn)
    {
        AudioListener.volume = isOn ? 1f : 0f;
        UpdateSoundIcon();
    }

    void UpdateSoundIcon()
    {
        if (toggleImage == null) return;

        toggleImage.sprite = soundToggle.isOn ? soundOnSprite : soundOffSprite;
        toggleImage.SetNativeSize();
    }

    void HideMenu()
    {
        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            canvas.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void ToggleMenuVisibility()
    {
        Canvas canvas = GetComponentInChildren<Canvas>(true);
        if (canvas == null) return;

        bool newState = !canvas.gameObject.activeSelf;
        canvas.gameObject.SetActive(newState);

        Cursor.lockState = newState ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = newState;
    }
}
