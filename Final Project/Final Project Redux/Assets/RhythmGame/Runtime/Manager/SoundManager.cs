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

            // The prblem was the f-ing clip and not the event.....................
            //// Find the GameObject with the "RhythmCore" tag
            //GameObject rhythmCoreObject = GameObject.FindGameObjectWithTag("RhythmCore");
            //if (rhythmCoreObject == null)
            //{
            //    Debug.LogError("No GameObject with the 'RhythmCore' tag found.");
            //    return;
            //}

            //// Try to get the SongManager component from the tagged GameObject
            //songManager = rhythmCoreObject.GetComponent<SongManager>();
            //if (songManager != null)
            //{
            //    // Subscribe to the 'onSongFinished' event
            //    songManager.onSongFinished.AddListener(() => OnSongFinished(resultsClip));
            //    Debug.Log("SongManager instance found and event subscribed.");
            //}
            //else
            //{
            //    Debug.LogError("No SongManager component found on the 'RhythmCore' GameObject.");
            //}
        }

        private void Update()
        {
            bgmSource.volume = bgmVolume;
            sfxSource.volume = sfxVolume;
        }

        //private void OnDestroy()
        //{
        //    // Unsubscribe when the SoundManager is destroyed
        //    if (songManager != null)
        //    {
        //        songManager.onSongFinished.RemoveListener(() => OnSongFinished(resultsClip));
        //    }
        //}

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

        //private void OnSongFinished(AudioClip clip)
        //{
        //    if (clip == null) return;
        //    sfxSource.PlayOneShot(clip);
        //}

        public void PlayButtonClick()
        {
            if (buttonClick == null) return;
            sfxSource.PlayOneShot(buttonClick);
        }

        public void PlayResultsClip()
        {
            if (buttonClick == null) return;
            sfxSource.PlayOneShot(resultsClip);
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip);
        }
    }
}
