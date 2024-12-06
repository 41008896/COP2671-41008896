using RhythmGameStarter;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableSongItem
{
    public string name;
    public string author;
    public string midiReference;
    public SongItem.NoteName rootKey;
    public int bpm;
    public float speedModifier;
    public int timeSignatureNumerator;
    public int timeSignatureDenominator;

    // Serializable note collection
    [System.Serializable]
    public struct MidiNoteStruct
    {
        public SongItem.NoteName noteName;
        public int noteOctave;
        public float time;
        public float noteLength;
        public float beatIndex;
        public float beatLengthIndex;

        public MidiNoteStruct(SongItem.MidiNote note)
        {
            noteName = note.noteName;
            noteOctave = note.noteOctave;
            time = note.time;
            noteLength = note.noteLength;
            beatIndex = note.beatIndex;
            beatLengthIndex = note.beatLengthIndex;
        }

        public SongItem.MidiNote ToMidiNote()
        {
            return new SongItem.MidiNote
            {
                noteName = noteName,
                noteOctave = noteOctave,
                time = time,
                noteLength = noteLength,
                beatIndex = beatIndex,
                beatLengthIndex = beatLengthIndex
            };
        }
    }

    public List<MidiNoteStruct> notes;

    public SerializableSongItem(SongItem song)
    {
        name = song.name;
        author = song.author;
        midiReference = song.midiReference;
        rootKey = song.rootKey;
        bpm = song.bpm;
        speedModifier = song.speedModifier;
        timeSignatureNumerator = song.timeSignatureNumerator;
        timeSignatureDenominator = song.timeSignatureDenominator;

    notes = new List<MidiNoteStruct>();
        foreach (var note in song.notes)
        {
            notes.Add(new MidiNoteStruct(note));
        }
    }

    public SongItem ToSongItem()
    {
        var song = ScriptableObject.CreateInstance<SongItem>();
        song.name = name;
        song.author = author;
        song.midiReference = midiReference;
        song.rootKey = rootKey;
        song.bpm = bpm;
        song.speedModifier = speedModifier;

        song.notes = new List<SongItem.MidiNote>();
        foreach (var noteStruct in notes)
        {
            song.notes.Add(noteStruct.ToMidiNote());
        }

        return song;
    }
}