using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Dissonance;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    [Header("Main References")]
    [SerializeField] private GameObject canvaObj;
    [SerializeField] private AudioMixer masterMixer;

    [Header("Settings Menu References")]
    [SerializeField] private GameObject gameplayMenu; // object with all of the settings
    [SerializeField] private GameObject screenMenu;
    [SerializeField] private GameObject controlsMenu;

    [Header("Gameplay Settings UI References")]
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_InputField sensitivityInputField;
    [SerializeField] private Toggle enableHUDToggle;
    [SerializeField] private Toggle enableChatToggle;
    [SerializeField] private Toggle moderateChatToggle;
    [SerializeField] private Toggle showStatsUIToggle;

    [Header("Screen Sub Menus")]
    [SerializeField] private GameObject videoSubMenu;
    [SerializeField] private GameObject graphicsSubMenu;
    [SerializeField] private GameObject audioSubMenu;

    [Header("Video Settings UI References")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private TextMeshProUGUI fullscreenModeText;
    [SerializeField] private Toggle vSyncToggle;
    [SerializeField] private TextMeshProUGUI limitFPSText;

    [Header("Graphics Settings UI References")]
    [SerializeField] private TextMeshProUGUI qualitySettingText;
    [SerializeField] private TextMeshProUGUI antiAliasingText;
    [SerializeField] private Toggle postProcToggle;

    [Header("Audio Settings UI References")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Toggle enableProximityChatToggle;

    // fullscreen settings
    private int currentFullscreenModeIndex = 0; // 0 = Exclusive Fullscreen, 1 = Fullscreen Window, 2 = Maximized Window, 3 = Windowed

    // fps settings
    private int currentFPSSettings = 0;
    private int[] fpsOptions = { 30, 60, 144, 240, -1 };
    private int fpsIndex = 2; // default to 144 (index 2)

    // anti aliasing
    private int currentAntiAliasingSettings; // 0 = off, 1 = 2x, 2 = 4x, 3 = 8x

    // quality
    private int currentQualitySettings = 0; // 0 - fancy, 1 - performative, 2 - balanced

    // resolutions
    private Resolution[] resolutions;
    private int currentResolutionIndex = 0;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }

        LoadSettings();
    }

    private void LoadSettings()
    {
        // sensitivity
        float sens = PlayerPrefs.GetFloat("Sens", sensitivitySlider.value);
        sens = Mathf.Clamp(sens, sensitivitySlider.minValue, sensitivitySlider.maxValue);
        sensitivitySlider.value = sens;
        sensitivityInputField.text = sens.ToString();

        // HUD | Chat | Stats UI
        enableHUDToggle.isOn = PlayerPrefs.GetInt("HUD", 1) == 1;
        enableChatToggle.isOn = PlayerPrefs.GetInt("Chat", 1) == 1;
        moderateChatToggle.isOn = PlayerPrefs.GetInt("ModerateChat", 1) == 1;
        showStatsUIToggle.isOn = PlayerPrefs.GetInt("ShowStatsUI", 0) == 1;

        // resolution
        PopulateResolutionDropdown(); // builds the list AND figures out the current screen's index
        currentResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", currentResolutionIndex);
        if (currentResolutionIndex < 0 || currentResolutionIndex >= resolutions.Length)
            currentResolutionIndex = 0;
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        // saved as 0 = fullscreen, 1 = not fullscreen
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 0) == 0;
        fullScreenToggle.isOn = isFullscreen;
        Screen.fullScreen = isFullscreen;

        currentFullscreenModeIndex = PlayerPrefs.GetInt("FullscreenMode", currentFullscreenModeIndex);
        UpdateFullscreenMode(); // sets Screen.fullScreenMode + updates fullscreenModeText

        // vsync
        vSyncToggle.isOn = PlayerPrefs.GetInt("VSync", 0) == 1;
        QualitySettings.vSyncCount = vSyncToggle.isOn ? 1 : 0;

        // fps limit
        int savedFPS = PlayerPrefs.GetInt("FPS", fpsOptions[fpsIndex]);
        int foundIndex = System.Array.IndexOf(fpsOptions, savedFPS);
        fpsIndex = foundIndex >= 0 ? foundIndex : fpsIndex;
        UpdateFPSSettings();

        // quality
        currentQualitySettings = PlayerPrefs.GetInt("Quality", currentQualitySettings);
        currentQualitySettings = Mathf.Clamp(currentQualitySettings, 0, 2);
        UpdateQuality(); 

        // anti aliasing
        currentAntiAliasingSettings = PlayerPrefs.GetInt("AntiAliasing", currentAntiAliasingSettings);
        currentAntiAliasingSettings = Mathf.Clamp(currentAntiAliasingSettings, 0, 3);
        UpdateAntiAliasing(); 

        // post processing
        postProcToggle.isOn = PlayerPrefs.GetInt("PostProcessing", 1) == 1;
        UpdatePostProc(); 

        // audio
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", masterVolumeSlider.value);
        masterVolumeSlider.value = masterVol;
        masterMixer.SetFloat("Master", Mathf.Log10(Mathf.Max(masterVol, 0.0001f)) * 20);

        float musicVol = PlayerPrefs.GetFloat("MusicVolume", musicVolumeSlider.value);
        musicVolumeSlider.value = musicVol;
        masterMixer.SetFloat("Music", Mathf.Log10(Mathf.Max(musicVol, 0.0001f)) * 20);

        enableProximityChatToggle.isOn = PlayerPrefs.GetInt("ProximityChat", 1) == 1;
    }

    public void ResetToDefault()
    {
        PlayerPrefs.DeleteAll();
        LoadSettings(); 
    }

    public void OpenSettings()
    {
        canvaObj.SetActive(true);
    }

    public void CloseSettings()
    {
        canvaObj.SetActive(false);

        if (SceneManager.GetActiveScene().name.Equals("Lobby"))
        {
            HandleLobbyUI.instance.EnableMainUIObj(true);
        }
    }

    #region Header Button References

    public void OpenGameplayMenu()
    {
        gameplayMenu.SetActive(true);
        screenMenu.SetActive(false);
        controlsMenu.SetActive(false);
    }

    public void OpenScreenMenu()
    {
        screenMenu.SetActive(true);
        gameplayMenu.SetActive(false);
        controlsMenu.SetActive(false);
    }

    public void OpenControlsMenu()
    {
        gameplayMenu.SetActive(true);
        screenMenu.SetActive(false);
        controlsMenu.SetActive(false);
    }

    #endregion

    #region Gameplay Settings 

    #region Sensitivity Settings
    public void SaveSensitivityThroughSlider()
    {
        float sens = sensitivitySlider.value;
        sensitivityInputField.text = sens.ToString();

        // save
        PlayerPrefs.SetFloat("Sens", sens);
        PlayerPrefs.Save();
    }

    public void SaveSensitivityThroughInputField()
    {
        if (float.TryParse(sensitivityInputField.text, out float sens))
        {
            if (sens < sensitivitySlider.minValue)
            {
                sens = sensitivitySlider.minValue;
            }

            else if (sens > sensitivitySlider.maxValue)
            {
                sens = sensitivitySlider.maxValue;
            }

            sensitivitySlider.value = sens;

            PlayerPrefs.SetFloat("Sens", sens);
            PlayerPrefs.Save();
        }
    }

    #endregion

    #region HUD Settings

    public void SaveHUDSettings()
    {
        PlayerPrefs.SetInt("HUD", enableHUDToggle.isOn ? 1 : 0); // 1 == on; 0 == off
    }

    #endregion

    #region Enable Chat Settings

    public void SaveEnableChatSettings()
    {
        PlayerPrefs.SetInt("Chat", enableChatToggle.isOn ? 1 : 0); // 1 == on; 0 == off
    }

    #endregion

    #region Moderate Chat Settings

    public void SaveModerateChatSettings()
    {
        PlayerPrefs.SetInt("ModerateChat", moderateChatToggle.isOn ? 1 : 0); // 1 == on; 0 == off
    }

    #endregion

    #region Show Stats UI Settings

    public void SaveShowStatsUISettings()
    {
        PlayerPrefs.SetInt("ShowStatsUI", showStatsUIToggle.isOn ? 1 : 0); // 1 == on; 0 == off
    }

    #endregion

    #endregion

    #region Screen Sub Menu Functions

    public void OpenVideoSubMenu()
    {
        videoSubMenu.SetActive(true);
        graphicsSubMenu.SetActive(false);
        audioSubMenu.SetActive(false);
    }

    public void OpenGraphicsMenu()
    {
        graphicsSubMenu.SetActive(true);
        videoSubMenu.SetActive(false);
        audioSubMenu.SetActive(false);
    }

    public void OpenAudioMenu()
    {
        audioSubMenu.SetActive(true);
        graphicsSubMenu.SetActive(false);
        videoSubMenu.SetActive(false);
    }

    #endregion

    #region Video Sub Menu Functions

    #region Resolutions

    public void SetResolution(int resolutionIndex)
    {
        currentResolutionIndex = resolutionIndex;
    }

    public void ApplyResolution()
    {
        Resolution res = resolutions[currentResolutionIndex];

        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode, res.refreshRateRatio);

        Debug.Log(currentResolutionIndex);

        PlayerPrefs.SetInt("ResolutionIndex", currentResolutionIndex);
        PlayerPrefs.Save();
    }

    private void PopulateResolutionDropdown()
    {
        resolutions = Screen.resolutions.Distinct().ToArray();
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            int refreshRate = Mathf.RoundToInt((float)resolutions[i].refreshRateRatio.value);
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + refreshRate + "Hz";
            options.Add(option);

            Resolution screenRes = Screen.currentResolution;
            int screenRefresh = Mathf.RoundToInt((float)screenRes.refreshRateRatio.value);
            if (resolutions[i].width == screenRes.width &&
                resolutions[i].height == screenRes.height &&
                refreshRate == screenRefresh)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    #endregion

    #region Fullscreen And Fullscreen Mode

    public void UpdateFullscreen()
    {
        // 0 = full screen, 1 = not fullscreen
        Screen.fullScreen = fullScreenToggle.isOn;
        PlayerPrefs.SetInt("Fullscreen", fullScreenToggle.isOn ? 0 : 1);
        PlayerPrefs.Save();
    }

    public void UpdateFullscreenMode()
    {
        // handle the text
        switch (currentFullscreenModeIndex)
        {
            case 0:
                fullscreenModeText.text = "Exclusive Fullscreen";
                break;
            case 1:
                fullscreenModeText.text = "Fullscreen Window";
                break;
            case 2:
                fullscreenModeText.text = "Maximized Window";
                break;
            case 3:
                fullscreenModeText.text = "Windowed";
                break;
        }

        Screen.fullScreenMode = (FullScreenMode)currentFullscreenModeIndex;
        PlayerPrefs.SetInt("FullscreenMode", currentFullscreenModeIndex);
        PlayerPrefs.Save();
    }

    public void GoToNextFullscreenMode()
    {
        currentFullscreenModeIndex++;

        if (currentFullscreenModeIndex > 3)
            currentFullscreenModeIndex = 0;

        UpdateFullscreenMode();
    }

    public void GoToPreviousFullscreenMode()
    {
        currentFullscreenModeIndex--;

        if (currentFullscreenModeIndex < 0)
            currentFullscreenModeIndex = 3;

        UpdateFullscreenMode();
    }

    #endregion

    #region Vsync

    public void UpdateVsync()
    {
        PlayerPrefs.SetInt("VSync", vSyncToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();

        // 0 = off, 1 = on
        QualitySettings.vSyncCount = vSyncToggle.isOn ? 1 : 0;
    }

    #endregion

    #region Limit FPS

    private void UpdateFPSSettings()
    {
        currentFPSSettings = fpsOptions[fpsIndex];

        if (currentFPSSettings == -1)
            limitFPSText.text = "Unlimited";
        else
            limitFPSText.text = currentFPSSettings.ToString();

        Application.targetFrameRate = currentFPSSettings;

        PlayerPrefs.SetInt("FPS", currentFPSSettings);
        PlayerPrefs.Save();
    }

    public void GoNextFPSLimit()
    {
        fpsIndex++;

        if (fpsIndex >= fpsOptions.Length)
            fpsIndex = 0;

        UpdateFPSSettings();
    }

    public void GoBackFPSLimit()
    {
        fpsIndex--;

        if (fpsIndex < 0)
            fpsIndex = fpsOptions.Length - 1;

        UpdateFPSSettings();
    }

    #endregion

    #endregion

    #region Graphics Sub Menu Functions

    #region Quality Settings

    private void UpdateQuality()
    {
        QualitySettings.SetQualityLevel(currentQualitySettings, true);

        qualitySettingText.text = QualitySettings.names[currentQualitySettings];

        PlayerPrefs.SetInt("Quality", currentQualitySettings);
        PlayerPrefs.Save();
    }

    public void GoToNextQuality()
    {
        currentQualitySettings++;

        if (currentQualitySettings > 2)
            currentQualitySettings = 0;

        UpdateQuality();
    }

    public void GoToPreviousQuality()
    {
        currentQualitySettings--;

        if (currentQualitySettings < 0)
            currentQualitySettings = 2;

        UpdateQuality();
    }

    #endregion

    #region Anti-Aliasing Setting

    private void UpdateAntiAliasing()
    {
        switch (currentAntiAliasingSettings)
        {
            case 0: // off
                QualitySettings.antiAliasing = 0;
                antiAliasingText.text = "Off";
                break;
            case 1: // 2x
                QualitySettings.antiAliasing = 2;
                antiAliasingText.text = "2x";
                break;
            case 2: // 4x
                QualitySettings.antiAliasing = 4;
                antiAliasingText.text = "4x";
                break;
            case 3: // 8x
                QualitySettings.antiAliasing = 8;
                antiAliasingText.text = "8x";
                break;
        }

        PlayerPrefs.SetInt("AntiAliasing", currentAntiAliasingSettings);
        PlayerPrefs.Save();
    }

    public void GoToNextAntiAliasingSetting()
    {
        currentAntiAliasingSettings++;

        if (currentAntiAliasingSettings > 3)
            currentAntiAliasingSettings = 0;

        UpdateAntiAliasing();
    }

    public void GoToPreviousAntiAliasingSetting()
    {
        currentAntiAliasingSettings--;

        if (currentAntiAliasingSettings < 0)
            currentAntiAliasingSettings = 3;

        UpdateAntiAliasing();
    }

    #endregion

    #region Enable Post Proc Settings

    public void UpdatePostProc()
    {
        // 0 = off, 1 = on
        if (postProcToggle.isOn)
        {
            UniversalAdditionalCameraData[] allCameras = FindObjectsByType<UniversalAdditionalCameraData>(FindObjectsSortMode.None);
            foreach (var camData in allCameras)
            {
                camData.renderPostProcessing = true;
            }

            PlayerPrefs.SetInt("PostProcessing", 1);
            PlayerPrefs.Save();
        }

        else
        {
            UniversalAdditionalCameraData[] allCameras = FindObjectsByType<UniversalAdditionalCameraData>(FindObjectsSortMode.None);
            foreach (var camData in allCameras)
            {
                camData.renderPostProcessing = false;
            }

            PlayerPrefs.SetInt("PostProcessing", 0);
            PlayerPrefs.Save();
        }
    }

    #endregion

    #endregion

    #region Audio Sub Menu Functions

    #region Master Volume Settings

    public void UpdateMasterVolume()
    {
        float volume = masterVolumeSlider.value;

        masterMixer.SetFloat("Master", Mathf.Log10(volume) * 20);

        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    #endregion

    #region Music Volume Settings

    public void UpdateMusicVolume()
    {
        float volume = musicVolumeSlider.value;

        masterMixer.SetFloat("Music", Mathf.Log10(volume) * 20);

        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    #endregion



    #endregion
}
