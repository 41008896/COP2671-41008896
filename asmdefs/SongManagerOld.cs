using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MidiPlayerTK;

namespace RhythmGameStarter
{
    [HelpURL("https://bennykok.gitbook.io/rhythm-game-starter/hierarchy-overview/song-manager")]
    [RequireComponent(typeof(TrackManager))]
    public class SongManager : MonoBehaviour
    {
        [Comment("Responsible for song control, handling song related events.")]
        public MidiFilePlayer midiFilePlayer;  // Replacing AudioSource with MidiFilePlayer

        [Title("Properties", 0)]
        [Space]
        public bool playOnAwake = true;
        public SongItem defaultSong;
        public float delay;
        public bool looping;

        [Tooltip("Automatically handle play/pause when timescale set to 0, or back to 1")]
        public bool autoTimeScalePause;

        [Title("Display", 0)]
        public bool progressAsPercentage = true;
        public bool inverseProgressFill = false;

        [HideInInspector] public float secPerBeat;
        [HideInInspector] public float songPosition;
        [HideInInspector] public IEnumerable<SongItem.MidiNote> currnetNotes;

        [Title("Events", 0)]
        [CollapsedEvent("Triggered every frame when a song progress")] public FloatEvent onSongProgress;
        [CollapsedEvent("Triggered every frame when a song progress with a float between [0,1] useful for Image fill")] public FloatEvent onSongProgressFill;
        [CollapsedEvent("Triggered every frame the song progress with a string variable of the progress display")] public StringEvent onSongProgressDisplay;
        [CollapsedEvent("Triggered when the song play, after the delay wait time")] public UnityEvent onSongStart;
        [CollapsedEvent("Triggered when the song play, before the delay wait time")] public UnityEvent onSongStartPlay;
        [CollapsedEvent("Triggered when the song finished")] public UnityEvent onSongFinished;

        #region RUNTIME_FIELD

        [NonSerialized] public bool songPaused;
        [NonSerialized] public SongItem currentSongItem;
        [NonSerialized] public ComboSystem comboSystem;
        [NonSerialized] public TrackManager trackManager;

        private bool songHasStarted;
        private bool songStartEventInvoked;
        private double dspStartTime;
        private double dspPausedTime;
        private double accumulatedPauseTime;

        #endregion

        private void Awake()
        {
            trackManager = GetComponent<TrackManager>();
            comboSystem = GetComponent<ComboSystem>();

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

        public void PlaySong(SongItem songItem, double specificStartTime = 0)
        {
            currentSongItem = songItem;
            songPaused = false;
            songHasStarted = true;
            accumulatedPauseTime = 0;
            dspPausedTime = 0;
            songPosition = -1;

            // Set MIDI file name and playback properties
            if (midiFilePlayer)
            {
                midiFilePlayer.MPTK_Stop();
                midiFilePlayer.MPTK_MidiName = songItem.midiFileName;  // Assume midiFileName is in SongItem
                midiFilePlayer.MPTK_Play();
                midiFilePlayer.MPTK_Speed = songItem.speedModifier;
            }

            if (songItem.customMidiMapping || songItem.customPrefabMapping)
                trackManager.OverrideMapping(songItem.customMidiMapping, songItem.customPrefabMapping);
            else
                trackManager.ResetMappingOverride();

            currnetNotes = songItem.GetNotes();
            secPerBeat = 60f / songItem.bpm;

            dspStartTime = AudioSettings.dspTime - specificStartTime;

            trackManager.SetupForNewSong();
            onSongStartPlay.Invoke();
        }

        public void PauseSong()
        {
            if (songPaused) return;

            songPaused = true;
            midiFilePlayer?.MPTK_Pause();

            dspPausedTime = AudioSettings.dspTime;
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
            midiFilePlayer?.MPTK_Play();

            accumulatedPauseTime += AudioSettings.dspTime - dspPausedTime;
        }

        public void StopSong(bool dontInvokeEvent = false)
        {
            midiFilePlayer?.MPTK_Stop();
            songHasStarted = false;
            songStartEventInvoked = false;

            if (!dontInvokeEvent)
                onSongFinished.Invoke();

            trackManager.ClearAllTracks();
        }

        void Update()
        {
            if (!songStartEventInvoked && songHasStarted && songPosition >= 0)
            {
                songStartEventInvoked = true;
                onSongStart.Invoke();
            }

            if (songHasStarted && songStartEventInvoked && autoTimeScalePause)
            {
                if (!songPaused && Time.timeScale == 0)
                {
                    PauseSong();
                }
                else if (songPaused && Time.timeScale == 1)
                {
                    ResumeSong();
                }
            }

            if (!songPaused && songHasStarted)
            {
                songPosition = (float)(AudioSettings.dspTime - dspStartTime - delay - accumulatedPauseTime) * midiFilePlayer.MPTK_Speed;

                trackManager.UpdateTrack(songPosition, secPerBeat);

                onSongProgress.Invoke(songPosition);

                if (inverseProgressFill)
                    onSongProgressFill.Invoke(1 - (songPosition / currentSongItem.clip.length));
                else
                    onSongProgressFill.Invoke(songPosition / currentSongItem.clip.length);

                if (songPosition >= 0)
                {
                    if (progressAsPercentage)
                        onSongProgressDisplay.Invoke(Math.Truncate(songPosition / currentSongItem.clip.length * 100) + "%");
                    else
                    {
                        var now = new DateTime((long)songPosition * TimeSpan.TicksPerSecond);
                        onSongProgressDisplay.Invoke(now.ToString("mm:ss"));
                    }
                }
            }

            if (songHasStarted && currentSongItem.clip && songPosition >= currentSongItem.clip.length)
            {
                songHasStarted = false;
                songStartEventInvoked = false;
                onSongFinished.Invoke();

                trackManager.ClearAllTracks();

                if (looping)
                    PlaySong(currentSongItem);
            }
        }
    }
}
