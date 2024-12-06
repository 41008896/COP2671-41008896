using UnityEngine;

namespace RhythmGameStarter
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Default Audio Clips")]
        [SerializeField] private AudioClip defaultBGM;
        [SerializeField] private AudioClip resultsClip;
        [SerializeField] private AudioClip buttonClick;

        [Range(0f, 1f)] public float bgmVolume = 1f;
        [Range(0f, 1f)] public float sfxVolume = 1f;

        private SongManager songManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (defaultBGM != null)
            {
                PlayBGM(defaultBGM);
            }
        }

        private void Update()
        {
            bgmSource.volume = bgmVolume;
            sfxSource.volume = sfxVolume;
        }

        public void PlayBGM(AudioClip clip)
        {
            if (clip == null) return;
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            bgmSource.Stop();
        }

        public void PlayButtonClick()
        {
            if (buttonClick == null) return;
            sfxSource.PlayOneShot(buttonClick);
        }

        public void PlayResultsClip()
        {
            if (resultsClip == null) return;
            bgmSource.clip = resultsClip;
            bgmSource.loop = false;
            bgmSource.Play();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip);
        }
        public void StopSFX()
        {
            sfxSource.Stop();
        }
    }
}
