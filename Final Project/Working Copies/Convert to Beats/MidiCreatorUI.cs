using UnityEngine;
using System.Collections.Generic;
using MidiPlayerTK;
using RhythmGameStarter;
using System.Linq;

public class MidiTestCreatorRuntime : MonoBehaviour
{
    private Rect windowRect = new Rect(0, 0, Screen.width, Screen.height); // Fill screen
    private Vector2 scrollPosition;
    private string[] midiFiles;
    private int selectedMidiIndex = -1;
    private int selectedChannel = -1;
    private int selectedKeyIndex = 0;
    private SongItem.NoteName selectedRootNote = SongItem.NoteName.C;
    private string authorName = "Test Author";
    private int difficulty = 2;
    private MidiLoad previewMidiLoad;
    private Dictionary<int, int> channelInstruments = new Dictionary<int, int>();
    private bool showWindow = false;
    private GameObject songItemList;
    private SongListHandler songListHandler;

    private readonly string[] instrumentNames = {
        "Acoustic Grand Piano", "Bright Acoustic Piano", "Electric Grand Piano", "Honky-tonk Piano",
        "Electric Piano 1", "Electric Piano 2", "Harpsichord", "Clavinet",
        "Celesta", "Glockenspiel", "Music Box", "Vibraphone",
        "Marimba", "Xylophone", "Tubular Bells", "Dulcimer",
        "Drawbar Organ", "Percussive Organ", "Rock Organ", "Church Organ",
        "Reed Organ", "Accordion", "Harmonica", "Tango Accordion",
        "Acoustic Guitar (nylon)", "Acoustic Guitar (steel)", "Electric Guitar (jazz)", "Electric Guitar (clean)",
        "Electric Guitar (muted)", "Overdriven Guitar", "Distortion Guitar", "Guitar Harmonics",
        "Acoustic Bass", "Electric Bass (finger)", "Electric Bass (pick)", "Fretless Bass",
        "Slap Bass 1", "Slap Bass 2", "Synth Bass 1", "Synth Bass 2",
        "Violin", "Viola", "Cello", "Contrabass",
        "Tremolo Strings", "Pizzicato Strings", "Orchestral Harp", "Timpani",
        "String Ensemble 1", "String Ensemble 2", "Synth Strings 1", "Synth Strings 2",
        "Choir Aahs", "Voice Oohs", "Synth Choir", "Orchestra Hit",
        "Trumpet", "Trombone", "Tuba", "Muted Trumpet",
        "French Horn", "Brass Section", "Synth Brass 1", "Synth Brass 2",
        "Soprano Sax", "Alto Sax", "Tenor Sax", "Baritone Sax",
        "Oboe", "English Horn", "Bassoon", "Clarinet",
        "Piccolo", "Flute", "Recorder", "Pan Flute",
        "Blown Bottle", "Shakuhachi", "Whistle", "Ocarina",
        "Lead 1 (square)", "Lead 2 (sawtooth)", "Lead 3 (calliope)", "Lead 4 (chiff)",
        "Lead 5 (charang)", "Lead 6 (voice)", "Lead 7 (fifths)", "Lead 8 (bass + lead)",
        "Pad 1 (new age)", "Pad 2 (warm)", "Pad 3 (polysynth)", "Pad 4 (choir)",
        "Pad 5 (bowed)", "Pad 6 (metallic)", "Pad 7 (halo)", "Pad 8 (sweep)",
        "FX 1 (rain)", "FX 2 (soundtrack)", "FX 3 (crystal)", "FX 4 (atmosphere)",
        "FX 5 (brightness)", "FX 6 (goblins)", "FX 7 (echoes)", "FX 8 (sci-fi)",
        "Sitar", "Banjo", "Shamisen", "Koto",
        "Kalimba", "Bagpipe", "Fiddle", "Shanai",
        "Tinkle Bell", "Agogo", "Steel Drums", "Woodblock",
        "Taiko Drum", "Melodic Tom", "Synth Drum", "Reverse Cymbal",
        "Guitar Fret Noise", "Breath Noise", "Seashore", "Bird Tweet",
        "Telephone Ring", "Helicopter", "Applause", "Gunshot"
    };

    private GUIStyle windowStyle;
    private GUIStyle buttonStyle;
    private GUIStyle selectedButtonStyle;
    private GUIStyle labelStyle;
    private GUIStyle textFieldStyle;
    private bool stylesInitialized;

    private Texture2D darkGrayTex;
    private Texture2D mediumGrayTex;
    private Texture2D lightGrayTex;
    private Texture2D activeTex;
    private Texture2D selectedBlueTex;

    private bool midiListExpanded = true;
    private bool channelListExpanded = false;
    private bool propertiesExpanded = false;

    void Start()
    {
        windowRect = new Rect(Screen.width * 0.1f, Screen.height * 0.1f,
                              Screen.width * 0.8f, Screen.height * 0.8f);
        songItemList = GameObject.FindGameObjectWithTag("SongItemList");
        songListHandler = songItemList.GetComponent<SongListHandler>();
        RefreshMidiList();
    }



    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = color;
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private void InitializeStyles()
    {
        // Create textures
        darkGrayTex = MakeTexture(2, 2, new Color(0.2f, 0.2f, 0.2f, 0.95f));
        mediumGrayTex = MakeTexture(2, 2, new Color(0.3f, 0.3f, 0.3f, 1));
        lightGrayTex = MakeTexture(2, 2, new Color(0.4f, 0.4f, 0.4f, 1));
        activeTex = MakeTexture(2, 2, new Color(0.5f, 0.5f, 0.5f, 1));
        selectedBlueTex = MakeTexture(2, 2, new Color(0.4f, 0.6f, 0.8f, 1));

        // Window style
        windowStyle = new GUIStyle(GUI.skin.window);
        windowStyle.normal.background = darkGrayTex;
        windowStyle.normal.textColor = Color.white;
        windowStyle.fontSize = 16;
        windowStyle.padding = new RectOffset(10, 10, 20, 10);

        // Button style
        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.normal.background = mediumGrayTex;
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.hover.background = lightGrayTex;
        buttonStyle.active.background = activeTex;
        buttonStyle.margin = new RectOffset(5, 5, 5, 5);
        buttonStyle.padding = new RectOffset(10, 10, 5, 5);

        // Selected button style
        selectedButtonStyle = new GUIStyle(buttonStyle);
        selectedButtonStyle.normal.background = selectedBlueTex;
        selectedButtonStyle.normal.textColor = Color.white;

        // Label style
        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontSize = 14;
        labelStyle.margin = new RectOffset(5, 5, 5, 5);

        // Text field style
        textFieldStyle = new GUIStyle(GUI.skin.textField);
        textFieldStyle.normal.background = darkGrayTex;
        textFieldStyle.normal.textColor = Color.white;
        textFieldStyle.padding = new RectOffset(5, 5, 5, 5);

        stylesInitialized = true;
    }

        void OnGUI()
    {
        if (!showWindow) return;

        if (!stylesInitialized)
            InitializeStyles();

        // Draw background overlay
        GUI.color = new Color(0, 0, 0, 0.8f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        windowRect = GUI.Window(0, windowRect, DrawWindow, "MIDI Test Creator", windowStyle);
    }

    //private void DrawWindow(int windowID)
    //{
    //    scrollPosition = GUILayout.BeginScrollView(scrollPosition);

    //    GUILayout.Label("MIDI Test Song Creator", labelStyle);

    //    if (GUILayout.Button("Refresh MIDI List", buttonStyle))
    //        RefreshMidiList();

    //    GUILayout.Space(10);

    //    // MIDI File Selection
    //    GUILayout.Label("1. Select MIDI File", labelStyle);
    //    if (midiFiles != null)
    //    {
    //        for (int i = 0; i < midiFiles.Length; i++)
    //        {
    //            bool isSelected = selectedMidiIndex == i;
    //            if (GUILayout.Button(midiFiles[i], isSelected ? selectedButtonStyle : buttonStyle))
    //            {
    //                selectedMidiIndex = i;
    //                LoadPreviewMidi(midiFiles[selectedMidiIndex]);
    //            }
    //        }
    //    }

    //    GUILayout.Space(10);

    //    // MIDI Info Display
    //    if (previewMidiLoad != null)
    //    {
    //        GUILayout.Label($"Duration: {previewMidiLoad.MPTK_Duration:mm\\:ss}", labelStyle);
    //        GUILayout.Label($"Tempo: {previewMidiLoad.MPTK_InitialTempo} BPM", labelStyle);
    //        GUILayout.Label($"Key Signature: {GetKeySignatureName(previewMidiLoad.MPTK_KeySigSharpsFlats, previewMidiLoad.MPTK_KeySigMajorMinor == 0 ? "Major" : "Minor")}", labelStyle);
    //    }

    //    GUILayout.Space(10);

    //    // Channel Selection
    //    GUILayout.Label("2. Select Channel", labelStyle);
    //    foreach (var channel in channelInstruments)
    //    {
    //        int noteCount = GetChannelNoteCount(channel.Key);
    //        if (noteCount > 0)
    //        {
    //            bool isSelected = selectedChannel == channel.Key;
    //            if (GUILayout.Button($"Channel {channel.Key}: {GetInstrumentName(channel.Value)} ({noteCount} notes)",
    //                isSelected ? selectedButtonStyle : buttonStyle))
    //            {
    //                selectedChannel = channel.Key;
    //            }
    //        }
    //    }

    //    GUILayout.Space(10);

    //    // Song Properties
    //    GUILayout.Label("3. Song Properties", labelStyle);
    //    GUILayout.BeginHorizontal();
    //    GUILayout.Label("Author:", labelStyle, GUILayout.Width(60));
    //    authorName = GUILayout.TextField(authorName, textFieldStyle);
    //    GUILayout.EndHorizontal();

    //    GUILayout.Label($"Difficulty: {difficulty}", labelStyle);
    //    difficulty = (int)GUILayout.HorizontalSlider(difficulty, 0, 3);

    //    GUILayout.Space(20);

    //    // Action Buttons
    //    if (GUILayout.Button("Create Test Song", buttonStyle))
    //        CreateSongListItem();

    //    if (GUILayout.Button("Close", buttonStyle))
    //        HideWindow();

    //    GUILayout.EndScrollView();
    //}

    private void DrawWindow(int windowID)
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        GUILayout.Space(10);
        GUILayout.Label("MIDI Song Creator", labelStyle);
        GUILayout.Space(20);

        // MIDI List Section
        midiListExpanded = GUILayout.Toggle(midiListExpanded, "MIDI Files", GUI.skin.button);
        if (midiListExpanded && midiFiles != null)
        {
            GUILayout.BeginVertical("box");
            for (int i = 0; i < midiFiles.Length; i++)
            {
                if (GUILayout.Button(midiFiles[i], selectedMidiIndex == i ? selectedButtonStyle : buttonStyle))
                {
                    selectedMidiIndex = i;
                    LoadPreviewMidi(midiFiles[selectedMidiIndex]);
                    midiListExpanded = false;
                    channelListExpanded = true;
                }
            }
            GUILayout.EndVertical();
        }
        GUILayout.Space(10);

        if (previewMidiLoad != null)
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label($"Duration: {previewMidiLoad.MPTK_Duration:mm\\:ss}", labelStyle);
            GUILayout.Label($"Tempo: {previewMidiLoad.MPTK_InitialTempo} BPM", labelStyle);
            GUILayout.Label($"Key Signature: {GetKeySignatureName(previewMidiLoad.MPTK_KeySigSharpsFlats, previewMidiLoad.MPTK_KeySigMajorMinor == 0 ? "Major" : "Minor")}", labelStyle);
            GUILayout.EndVertical();
            GUILayout.Space(10);
        }

        GUILayout.Space(10);

        // Channel List Section
        if (previewMidiLoad != null)
        {
            channelListExpanded = GUILayout.Toggle(channelListExpanded, "Available Channels", GUI.skin.button);
            if (channelListExpanded)
            {
                GUILayout.BeginVertical("box");
                foreach (var channel in channelInstruments)
                {
                    int noteCount = GetChannelNoteCount(channel.Key);
                    if (noteCount > 0)
                    {
                        if (GUILayout.Button($"Channel {channel.Key}: {GetInstrumentName(channel.Value)} ({noteCount} notes)",
                            selectedChannel == channel.Key ? selectedButtonStyle : buttonStyle))
                        {
                            selectedChannel = channel.Key;
                            channelListExpanded = false;
                            propertiesExpanded = true;
                        }
                    }
                }
                GUILayout.EndVertical();
            }
        }

        GUILayout.Space(10);

        // Properties Section
        if (selectedChannel != -1)
        {
            propertiesExpanded = GUILayout.Toggle(propertiesExpanded, "Song Properties", GUI.skin.button);
            if (propertiesExpanded)
            {
                GUILayout.BeginVertical("box");
                GUILayout.BeginHorizontal();
                GUILayout.Label("Author:", labelStyle, GUILayout.Width(60));
                authorName = GUILayout.TextField(authorName, textFieldStyle);
                GUILayout.EndHorizontal();

                GUILayout.Label($"Difficulty: {difficulty}", labelStyle);
                difficulty = (int)GUILayout.HorizontalSlider(difficulty, 0, 3);
                GUILayout.EndVertical();
            }
        }

        GUILayout.Space(20);

        if (selectedChannel != -1)
        {
            if (GUILayout.Button("Create Song", buttonStyle))
                CreateSongListItem();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Close", buttonStyle))
        {
            showWindow = false;
            songItemList.SetActive(true);
        }

        GUILayout.EndScrollView();
        //GUI.DragWindow(new Rect(0, 0, Screen.width, 20));
    }

    public void ShowWindow()
    {
        showWindow = true;
        songItemList.SetActive(false);
    }

    public void HideWindow()
    {
        showWindow = false;
        songItemList.SetActive(true);
    }


    private void RefreshMidiList()
    {
        if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null)
        {
            midiFiles = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.ToArray();
        }
        else
        {
            midiFiles = new string[0];
        }
    }

    private void LoadPreviewMidi(string midiName)
    {
        var loader = new MidiFileLoader();
        loader.MPTK_MidiName = midiName;

        if (loader.MPTK_Load())
        {
            previewMidiLoad = loader.MPTK_MidiLoaded;
            channelInstruments.Clear();

            foreach (var midiEvent in loader.MPTK_ReadMidiEvents())
            {
                if (midiEvent.Command == MPTKCommand.PatchChange)
                {
                    channelInstruments[midiEvent.Channel] = midiEvent.Value;
                }
            }
        }
    }

    private int GetChannelNoteCount(int channel)
    {
        if (previewMidiLoad == null) return 0;
        return previewMidiLoad.MPTK_MidiEvents.Count(evt =>
            evt.Channel == channel &&
            evt.Command == MPTKCommand.NoteOn &&
            evt.Velocity > 0);
    }

    private string GetInstrumentName(int programNumber)
    {
        // Special case for channel 9 (drums)
        if (programNumber == 9)
            return "Drums";

        // Standard instruments (0-127)
        if (programNumber >= 0 && programNumber < instrumentNames.Length)
            return instrumentNames[programNumber];

        return $"Unknown Instrument ({programNumber})";
    }

    private string GetKeySignatureName(int sharpsFlats, string majorMinor)
    {
        string[] keyNames = {
        "Cb", "Gb", "Db", "Ab", "Eb", "Bb", "F", "C", "G", "D", "A", "E", "B", "F#", "C#"
    };
        int index = sharpsFlats + 7; // 7 is the index for C
        if (index >= 0 && index < keyNames.Length)
        {
            return $"{keyNames[index]} {majorMinor}";
        }
        return "Unknown Key";
    }

    private void CreateSongListItem()
    {
        if (selectedMidiIndex < 0)
        {
            Debug.LogWarning("No MIDI file selected.");
            return;
        }

        string midiName = midiFiles[selectedMidiIndex];
        string instrumentName = GetInstrumentName(channelInstruments[selectedChannel]);

        // Create the SongItem
        SongItem newSong = MidiToSongConverter.CreateSongFromMidi(
            midiName, selectedRootNote, selectedChannel, instrumentName);

        if (newSong == null)
        {
            Debug.LogError("Failed to create SongItem from MIDI.");
            return;
        }

        // Update author and difficulty metadata
        newSong.author = authorName;
        if (newSong.metadata == null)
            newSong.metadata = new SongItem.MetadataList();

        var difficultyMetadata = newSong.metadata.values.Find(x => x.id == "difficulties");
        if (difficultyMetadata == null)
        {
            difficultyMetadata = new SongItem.Metadata
            {
                id = "difficulties",
                stringValue = ""
            };
            newSong.metadata.values.Add(difficultyMetadata);
        }
        difficultyMetadata.intValue = difficulty;

        // Add the SongItem to the list
        songListHandler.songItems.values.Add(newSong);

        // Refresh the UI to display the new song
        songListHandler.RefreshUI();

        Debug.Log($"Successfully added '{newSong.name}' to the Song List.");
    }
}
