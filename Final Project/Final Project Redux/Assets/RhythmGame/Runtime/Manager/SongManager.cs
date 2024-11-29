using UnityEngine;
using MidiPlayerTK;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace RhythmGameStarter
{
    [HelpURL("https://bennykok.gitbook.io/rhythm-game-starter/hierarchy-overview/song-manager")]
    [RequireComponent(typeof(TrackManager))]
    public class SongManager : MonoBehaviour
    {
        [Comment("Responsible for song control and MIDI playback events")]
        private MidiFilePlayer midiFilePlayer;

        [Title("Properties", 0)]
        [Space]
        [Tooltip("Start playing the default song when scene loads")]
        public bool playOnAwake = true;

        [Tooltip("The default song to play if none is specified")]
        public SongItem defaultSong;

        [Tooltip("If true, song will restart when finished")]
        public bool looping;

        [Title("Display", 0)]
        [Tooltip("Show progress as percentage instead of time")]
        public bool progressAsPercentage = true;

        [Tooltip("Invert the fill amount (1 - progress)")]
        public bool inverseProgressFill = false;

        // Hidden properties used by track system
        [HideInInspector] public float secPerBeat;    // Duration of one beat in seconds
        [HideInInspector] public float songPosition;   // Current playback position in seconds
        [HideInInspector] public IEnumerable<SongItem.MidiNote> currnetNotes; // All notes for current song

        [Title("Events", 0)]
        [CollapsedEvent("Triggered every frame when song is playing, passes current time in seconds")]
        public FloatEvent onSongProgress;

        [CollapsedEvent("Triggered every frame, passes normalized progress value [0,1] for UI fill")]
        public FloatEvent onSongProgressFill;

        [CollapsedEvent("Triggered every frame, provides formatted time/percentage string")]
        public StringEvent onSongProgressDisplay;

        [CollapsedEvent("Triggered when MIDI playback begins")]
        public UnityEvent onSongStart;

        [CollapsedEvent("Triggered immediately when Play is called, before playback")]
        public UnityEvent onSongStartPlay;

        [CollapsedEvent("Triggered when song finishes or is stopped")]
        public UnityEvent onSongFinished;

        #region RUNTIME_FIELD
        // Runtime state tracking
        [NonSerialized] public bool songPaused;        // Current pause state
        [NonSerialized] public bool songHasStarted;    // Indicates if a song is currently active
        [NonSerialized] public SongItem currentSongItem; // Currently playing song
        [NonSerialized] public ComboSystem comboSystem; // Reference to the combo system
        [NonSerialized] public TrackManager trackManager; // Reference to track management
        [NonSerialized] public float delay = 0f;
        #endregion

        private void Awake()
        {
            Debug.Log("SongManager: Initializing");
            trackManager = GetComponent<TrackManager>();
            comboSystem = GetComponent<ComboSystem>();

            midiFilePlayer = GameObject.FindWithTag("MidiFilePlayer")?.GetComponent<MidiFilePlayer>();
            if (midiFilePlayer == null)
            {
                Debug.LogError("SongManager: MidiFilePlayer not found in the scene!");
                return;
            }

            midiFilePlayer.MPTK_PlayOnStart = false;
            trackManager.Init(this);
        }

        private void Start()
        {
            if (playOnAwake && defaultSong)
            {
                PlaySong(defaultSong);
            }
        }

        public void PlaySong()
        {
            if (defaultSong)
                PlaySong(defaultSong);
            else
                Debug.LogWarning("Default song is not set!");
        }

        public void PlaySongSelected(SongItem songItem)
        {
            PlaySong(songItem);
        }

        public void SetDefaultSong(SongItem songItem)
        {
            defaultSong = songItem;
        }

        public void PlaySong(SongItem songItem)
        {
            Debug.Log($"SongManager: Playing song {songItem.name}");
            currentSongItem = songItem;
            secPerBeat = 60.0f / currentSongItem.bpm;

            midiFilePlayer.MPTK_Stop();
            midiFilePlayer.MPTK_MidiName = songItem.midiReference;
            midiFilePlayer.MPTK_Speed = songItem.speedModifier;
            
            // Preload the MIDI File
            midiFilePlayer.MPTK_Load();

            currnetNotes = songItem.GetNotes();
            if (currnetNotes == null || !currnetNotes.Any())
            {
                Debug.LogError($"SongManager: No notes found for song {songItem.name}.");
                return;
            }

            trackManager.SetupForNewSong();  // Only this setup call needed

            songHasStarted = true;
            songPaused = false;
            onSongStartPlay.Invoke();

            //Start the play-pause-rewind-play routine
            //StartCoroutine(PlayPauseRewindPlay());

            midiFilePlayer.MPTK_Play();
        }

        //IEnumerator PlayPauseRewindPlay()
        //{
        //    Debug.Log("Starting playback to spin up...");
        //    midiFilePlayer.MPTK_Play();

        //    // Wait briefly to allow the system to initialize
        //    yield return new WaitForSeconds(0.1f); // Adjust the delay as needed

        //    Debug.Log("Pausing playback after spin-up...");
        //    midiFilePlayer.MPTK_Pause();

        //    // Wait for the system to stabilize if needed (optional)
        //    yield return new WaitForSeconds(3f); // Add additional delay if necessary

        //    // Reset playback position to the start
        //    Debug.Log("Resetting playback position to the beginning...");
        //    midiFilePlayer.MPTK_Position = 0f;

        //    Debug.Log("Resuming playback from the beginning...");
        //    midiFilePlayer.MPTK_Play();
        //}

        public void PauseSong()
        {
            if (!songPaused)
            {
                songPaused = true;
                midiFilePlayer.MPTK_Pause();
            }
        }

        public void ResumeSong()
        {
            if (!songHasStarted)
            {
                PlaySong();
                return;
            }
            if (!songPaused) return;

            songPaused = false;
            midiFilePlayer.MPTK_UnPause();
        }

        public void StopSong(bool dontInvokeEvent = false)
        {
            midiFilePlayer.MPTK_Stop();
            songHasStarted = false;

            if (!dontInvokeEvent)
                onSongFinished.Invoke();

            trackManager.ClearAllTracks();
        }

        void Update()
        {
            if (!songPaused && songHasStarted)
            {
                // Get current position in seconds from MIDI player
                songPosition = (float)midiFilePlayer.MPTK_Position / 1000f;

                // Update track positions and trigger progress events
                trackManager.UpdateTrack(songPosition, secPerBeat);
                onSongProgress.Invoke(songPosition);

                // Calculate and send normalized progress for UI
                float normalizedProgress = songPosition / (midiFilePlayer.MPTK_DurationMS / 1000f);
                onSongProgressFill.Invoke(inverseProgressFill ? 1 - normalizedProgress : normalizedProgress);

                // Update progress display
                if (songPosition >= 0)
                {
                    if (progressAsPercentage)
                        onSongProgressDisplay.Invoke(Math.Truncate(normalizedProgress * 100) + "%");
                    else
                    {
                        var now = new DateTime((long)songPosition * TimeSpan.TicksPerSecond);
                        onSongProgressDisplay.Invoke(now.ToString("mm:ss"));
                    }
                }
            }

            // Check for song completion
            if (songHasStarted && !midiFilePlayer.MPTK_IsPlaying)
            {
                songHasStarted = false;
                onSongFinished.Invoke();
                trackManager.ClearAllTracks();

                if (looping)
                    PlaySong(currentSongItem);
            }
        }
    }
}