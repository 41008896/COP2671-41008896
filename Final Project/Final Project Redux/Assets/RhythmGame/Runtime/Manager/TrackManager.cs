using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace RhythmGameStarter
{
    [HelpURL("https://bennykok.gitbook.io/rhythm-game-starter/hierarchy-overview/track-manager")]
    public class TrackManager : MonoBehaviour
    {
        [Comment("Responsible for communication between different tracks, handling note's prefab pool, also track's synchronization with music.")]
        [Title("Mappings", false, 1, order = 1)]
        public MidiTrackMapping midiTracksMapping;
        public NotePrefabMapping notePrefabMapping;

        [Title("Properties", 1)]
        [Space]
        [Tooltip("Offset in position local space, useful for extra audio latency tuning in game")]
        public float hitOffset;
        [Tooltip("The spacing for each note, higher the value will cause the note speed faster, by using IndividualNote SyncMode, you can change this in realtime")]
        public float beatSize;
        [Tooltip("If using notePool, the note object will be recycled instead of created and destroyed in runtime")]
        public bool useNotePool = true;
        public float poolLookAheadTime = 5;
        [Tooltip("Two different mode (Track, IndividualNote) for synchronizing the node postion with music")]
        public SyncMode syncMode;
        [Tooltip("Applying extra smoothing with Time.deltaTime in Track syncMode, and using Vector3.Lerp in IndividualNote syncMode, the smoothing could be insignificant under some situration")]
        public bool syncSmoothing;

        [Title("Events", 1)]
        [CollapsedEvent] public NoteComponentEvent onNoteInit;
        [CollapsedEvent] public NoteComponentEvent onNoteTriggered;

        private Track[] tracks;
        private SongManager songManager;
        private Transform notesPoolParent;
        private List<Note> pooledNotes = new List<Note>();
        private Dictionary<Track, int> nextNoteIndices = new Dictionary<Track, int>();

        public enum SyncMode
        {
            Track, IndividualNote
        }

        private void Awake()
        {
            tracks = GetComponentsInChildren<Track>();

            if (useNotePool)
                InitNotePool();
        }

        private MidiTrackMapping previousMidiTracksMapping;
        private NotePrefabMapping previousNotePrefabMapping;

        public void Init(SongManager songManager)
        {
            this.songManager = songManager;
        }

        public void SetupForNewSong()
        {
            if (useNotePool)
                SetUpNotePool();
            else
                CreateAllNoteNow();
        }

        public void UpdateTrack(float songPosition, float secPerBeat)
        {
            if (useNotePool)
                UpdateNoteInPool();

            foreach (var track in tracks)
            {
                switch (syncMode)
                {
                    case SyncMode.Track:
                        var target = track.notesParent;
                        var songPositionInBeats = (songPosition + songManager.delay) / secPerBeat;
                        if (syncSmoothing)
                        {
                            var syncPosY = -songPositionInBeats * beatSize + track.lineArea.transform.localPosition.y + hitOffset;
                            target.Translate(new Vector3(0, -1, 0) * (1 / secPerBeat) * beatSize * Time.deltaTime);
                            target.localPosition = new Vector3(0, (syncPosY + target.localPosition.y) / 2, 0);
                        }
                        else
                        {
                            target.localPosition = new Vector3(0, -songPositionInBeats * beatSize + track.lineArea.transform.localPosition.y + hitOffset, 0);
                        }
                        break;
                    case SyncMode.IndividualNote:
                        foreach (Note note in track.runtimeNote)
                        {
                            if (!note || !note.inUse) continue;
                            if (!note.gameObject.activeSelf)
                            {
                                note.gameObject.SetActive(true);
                            }

                            if (syncSmoothing)
                            {
                                var originalY = ((note.noteTime + 1) / secPerBeat) * beatSize;
                                note.transform.localPosition = Vector3.LerpUnclamped(new Vector3(0, originalY, 0), new Vector3(0, hitOffset, 0), (songPosition + 1) / (note.noteTime + 1));
                            }
                            else
                            {
                                var songPositionInBeats2 = (songPosition - note.noteTime) / secPerBeat;
                                var syncPosY = -songPositionInBeats2 * beatSize;
                                note.transform.localPosition = new Vector3(0, syncPosY + hitOffset, 0);
                            }
                        }
                        break;
                }
            }
        }

        public void ClearAllTracks()
        {
            foreach (var track in tracks)
            {
                track.ResetTrack();

                if (useNotePool)
                    track.RecycleAllNotes(this);
                else
                    track.DestoryAllNotes();
            }
        }

        private void SetUpNotePool()
        {
            for (int i = 0; i < tracks.Count(); i++)
            {
                var track = tracks[i];

                if (i > midiTracksMapping.mapping.Count - 1)
                {
                    Debug.Log("Mapping has not enough track count!");
                    continue;
                }

                var x = midiTracksMapping.mapping[i];

                track.allNotes = songManager.currnetNotes.Where(n =>
                {
                    return midiTracksMapping.CompareMidiMapping(x, n);
                });

                nextNoteIndices[track] = 0;  // Initialize/reset index
                track.RecycleAllNotes(this);
            }
        }

        private void UpdateNoteInPool()
        {
            foreach (var track in tracks)
            {
                if (track.allNotes == null) continue;

                float currentPosition = songManager.songPosition;
                float lookAheadPosition = currentPosition + poolLookAheadTime;

                var notes = track.allNotes.ToList();
                int currentIndex = nextNoteIndices[track];

                while (currentIndex < notes.Count)
                {
                    var note = notes[currentIndex];

                    if (note.time > lookAheadPosition)
                        break;

                    if (!note.created && note.time <= lookAheadPosition)
                    {
                        note.created = true;
                        var noteType = notePrefabMapping.GetNoteType(note);
                        var newNoteObject = GetUnUsedNote(noteType);
                        track.AttachNote(newNoteObject);
                        InitNote(newNoteObject, note);
                    }

                    currentIndex++;
                }

                nextNoteIndices[track] = currentIndex;
            }
        }

        private void InitNote(GameObject newNoteObject, SongItem.MidiNote note)
        {
            var pos = Vector3.zero;
            var time = note.time;
            var beatUnit = time / songManager.secPerBeat;
            pos.y = beatUnit * beatSize + (songManager.delay / songManager.secPerBeat * beatSize);

            newNoteObject.transform.localPosition = pos;

            var noteScript = newNoteObject.GetComponent<Note>();
            noteScript.songManager = songManager;
            noteScript.InitNoteLength(note.noteLength);
            noteScript.noteTime = note.time;

            if (syncMode == SyncMode.Track)
                newNoteObject.SetActive(true);

            onNoteInit.Invoke(noteScript);
        }

        public void ResetNoteToPool(GameObject noteObject)
        {
            var note = noteObject.GetComponent<Note>();
            if (!note) return;
            note.ResetForPool();
            note.transform.SetParent(notesPoolParent);
            note.gameObject.SetActive(false);
            note.transform.localPosition = Vector3.zero;
        }

        private GameObject GetUnUsedNote(int noteType)
        {
            var note = pooledNotes.Find(x => !x.inUse && x.noteType == noteType);

            if (note == null)
            {
                note = GetNewNoteObject(noteType);
            }

            note.inUse = true;
            return note.gameObject;
        }

        private Note GetNewNoteObject(int noteType)
        {
            if (notePrefabMapping.notesPrefab[noteType].prefab == null)
            {
                Debug.LogError("The prefab type index at " + noteType + " shouldn't be null, please check the NotePrefabMapping asset");
            }
            var o = Instantiate(notePrefabMapping.notesPrefab[noteType].prefab);

            var originalLocalScale = o.transform.localScale;
            o.transform.SetParent(notesPoolParent);
            o.transform.localScale = originalLocalScale;

            o.SetActive(false);

            var note = o.GetComponent<Note>();
            note.noteType = noteType;
            note.inUse = false;
            pooledNotes.Add(note);

            return note;
        }

        private void InitNotePool()
        {
            notesPoolParent = new GameObject("NotesPool").transform;
            notesPoolParent.SetParent(transform);
            notesPoolParent.localScale = Vector3.one;
            for (int i = 0; i < notePrefabMapping.notesPrefab.Count; i++)
            {
                for (int j = 0; j < notePrefabMapping.notesPrefab[i].poolSize; j++)
                {
                    GetNewNoteObject(i);
                }
            }
        }

        public static int CalculateMaxConcurrentNotes(SongItem songItem, float beatSize, float visibleBeatsInPlayArea)
        {
            if (songItem == null || songItem.notes == null || songItem.notes.Count == 0)
                return 0;

            var sortedNotes = songItem.notes.OrderBy(n => n.beatIndex).ToList();
            int maxConcurrent = 0;
            int currentConcurrent = 0;
            int currentNoteIndex = 0;
            float windowStart = sortedNotes[0].beatIndex;

            while (currentNoteIndex < sortedNotes.Count)
            {
                // Add notes entering window
                while (currentNoteIndex < sortedNotes.Count &&
                       sortedNotes[currentNoteIndex].beatIndex <= windowStart + visibleBeatsInPlayArea)
                {
                    currentConcurrent++;
                    currentNoteIndex++;
                }

                // Remove notes that left window
                for (int i = 0; i < currentNoteIndex; i++)
                {
                    if (sortedNotes[i].beatIndex < windowStart)
                    {
                        currentConcurrent--;
                    }
                }

                maxConcurrent = Mathf.Max(maxConcurrent, currentConcurrent);

                // Advance window to next note if available
                if (currentNoteIndex < sortedNotes.Count)
                {
                    windowStart = sortedNotes[currentNoteIndex].beatIndex;
                }
            }
            Debug.Log("Max concurrent notes: " + maxConcurrent);
            return maxConcurrent;
        }

        private void CreateAllNoteNow()
        {
            for (int i = 0; i < tracks.Count(); i++)
            {
                var track = tracks[i];
                var x = midiTracksMapping.mapping[i];

                track.allNotes = songManager.currnetNotes.Where(n =>
                {
                    return midiTracksMapping.CompareMidiMapping(x, n);
                });

                track.DestoryAllNotes();

                if (track.allNotes == null) continue;

                foreach (var note in track.allNotes)
                {
                    var newNoteObject = track.CreateNote(notePrefabMapping.GetNotePrefab(note));
                    InitNote(newNoteObject, note);
                }
            }
        }
    }

    [System.Serializable]
    public class NoteComponentEvent : UnityEvent<Note> { }
}