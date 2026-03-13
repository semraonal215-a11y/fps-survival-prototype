using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music Source (Loop)")]
    public AudioSource musicSource;

    [Header("SFX Source (2D, OneShots)")]
    public AudioSource sfx2DSource;

    [Header("Build Indexes")]
    public int menuSceneBuildIndex = 0;
    public int gameSceneBuildIndex = 1;

    [Header("Music Clips")]
    public AudioClip menuMusic;
    public AudioClip gameMusic;

    [Header("Normal (Game) Settings")]
    [Range(0f, 1f)] public float normalVolume = 0.5f;
    [Range(0.5f, 2f)] public float normalPitch = 1f;

    [Header("Boss Boost (Same Music)")]
    [Range(0f, 1f)] public float bossVolume = 0.8f;
    [Range(0.5f, 2f)] public float bossPitch = 1.12f;

    private bool bossBoostActive;

  
    [Header("UI SFX (Menu Buttons)")]
    public AudioClip uiHoverSfx;
    public AudioClip uiClickSfx;

    [Range(0f, 1f)] public float uiHoverVolume = 1f;
    [Range(0f, 1f)] public float uiClickVolume = 1f;

    public float uiHoverCooldown = 0.06f;

    private float nextUiHoverTime;
   

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // MUSIC SOURCE 
        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.enabled = true;

        // SFX SOURCE
        if (sfx2DSource == null)
        {
            
            var sources = GetComponents<AudioSource>();
            if (sources != null && sources.Length >= 2)
            {
                
                for (int i = 0; i < sources.Length; i++)
                {
                    if (sources[i] != musicSource)
                    {
                        sfx2DSource = sources[i];
                        break;
                    }
                }
            }

            if (sfx2DSource == null)
                sfx2DSource = gameObject.AddComponent<AudioSource>();
        }

        sfx2DSource.playOnAwake = false;
        sfx2DSource.loop = false;
        sfx2DSource.spatialBlend = 0f; 
        sfx2DSource.enabled = true;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        ApplyMusicForScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyMusicForScene(scene.buildIndex);
    }

    private void ApplyMusicForScene(int buildIndex)
    {
        bossBoostActive = false;

        if (buildIndex == menuSceneBuildIndex)
        {
            PlayClip(menuMusic, normalVolume, normalPitch);
            return;
        }

        if (buildIndex == gameSceneBuildIndex)
        {
            PlayClip(gameMusic, normalVolume, normalPitch);
            return;
        }
    }

    private void PlayClip(AudioClip clip, float volume, float pitch)
    {
        if (musicSource == null)
        {
            Debug.LogError("AudioManager: musicSource NULL");
            return;
        }

        if (clip == null)
        {
            Debug.LogError("AudioManager: Clip NULL ");
            return;
        }

        if (musicSource.clip != clip)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }

        musicSource.volume = volume;
        musicSource.pitch = pitch;
    }

    public void SetBossBoost(bool enable)
    {
        if (musicSource == null) return;

        bossBoostActive = enable;

        if (enable)
        {
            musicSource.volume = bossVolume;
            musicSource.pitch = bossPitch;
        }
        else
        {
            musicSource.volume = normalVolume;
            musicSource.pitch = normalPitch;
        }
    }

    public bool IsBossBoostActive() => bossBoostActive;

   
    public void PlaySfx2D(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        if (sfx2DSource == null) return;
        if (!sfx2DSource.isActiveAndEnabled) return;

        sfx2DSource.PlayOneShot(clip, volume);
    }

   
    public void PlayUIHover()
    {
        if (Time.time < nextUiHoverTime) return;
        nextUiHoverTime = Time.time + uiHoverCooldown;

        if (uiHoverSfx == null) return;
        PlaySfx2D(uiHoverSfx, uiHoverVolume);
    }

    public void PlayUIClick()
    {
        if (uiClickSfx == null) return;
        PlaySfx2D(uiClickSfx, uiClickVolume);
    }
   
}