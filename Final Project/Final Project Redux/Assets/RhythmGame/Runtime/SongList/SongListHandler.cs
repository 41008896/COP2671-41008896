using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RhythmGameStarter
{
    public class SongListHandler : MonoBehaviour
    {
        // [Comment("Handling note recording target switching")]
        public Transform container;

        [Tooltip("Must contain Button & TextMeshProUGUI component")]
        public GameObject itemPrefab;

        [ReorderableDisplay("Item")]
        public SongItemList songItems;

        [Comment("Events")]
        [CollapsedEvent]
        public SongItemEvent onItemSelect;

        [Serializable] public class SongItemList : ReorderableList<SongItem> { }

#if UNITY_EDITOR
        private readonly string savePath = Path.Combine(Application.dataPath, "Songs", "songlist.json");
#else
        private readonly string savePath = Path.Combine(Application.persistentDataPath, "songlist.json");
#endif

        private void Start()
        {
            EnsureJsonFileExists(); // Ensure the file exists at start
            LoadSongListFromFile();   // Load the file after ensuring it exists
            RefreshUI();              // Refresh the UI with loaded data
        }

        public void RefreshUI()
        {
            foreach (Transform child in container)
                GameObject.Destroy(child.gameObject);

            int i = 0;
            foreach (var target in songItems)
            {
                if (!target) continue;

                i++;

                var itemName = target.name;

                var item = Instantiate(itemPrefab, container);
                item.transform.localScale = Vector3.one;
                item.name = itemName;

                var songListItem = item.GetComponent<SongListItem>();

                songListItem.targetSongItem = target;
                songListItem.label.text = target.name;
                songListItem.authorLabel.text = target.author;
                songListItem.indexLabel.text = i.ToString();
                if (songListItem.coverArtImage)
                    songListItem.coverArtImage.sprite = target.coverArt;
                if (target.TryGetMetadata("difficulties", out var difficulties))
                {
                    songListItem.difficultiesFill.fillAmount = difficulties.intValue / 3f;
                }
                else
                {
                    songListItem.difficultiesFill.gameObject.SetActive(false);
                }
                songListItem.button.onClick.AddListener(() =>
                {
                    onItemSelect.Invoke(target);
                });
                // Subscribe to the delete event
                songListItem.onItemDelete.AddListener(RemoveSong);

                songListItem.onItemSetup.Invoke();
            }
        }

        public void AddSong(SongItem newSong)
        {
            if (!songItems.values.Contains(newSong))
            {
                songItems.values.Add(newSong);
                // Save the updated list to the JSON file
                SaveSongListToFile();
                // Refresh UI to reflect changes
                RefreshUI();
            }
        }

        public void RemoveSong(SongItem songToRemove)
        {
            if (songItems.values.Contains(songToRemove))
            {
                songItems.values.Remove(songToRemove);
                // Save the updated list to the JSON file
                SaveSongListToFile();
                // Refresh UI to reflect changes
                RefreshUI();
            }
        }


        private void LoadSongListFromFile()
        {
            try
            {
                string json = File.ReadAllText(savePath);
                Debug.Log($"Loaded JSON: {json}");

                if (string.IsNullOrWhiteSpace(json))
                {
                    songItems.values = new List<SongItem>();
                    return;
                }

                var wrapper = JsonUtility.FromJson<SongItemListWrapper>(json);
                Debug.Log($"Wrapper items count: {wrapper?.items?.Count ?? 0}");

                songItems.values = wrapper?.items?
                    .Where(serializedItem => serializedItem != null)
                    .Select(serializedItem => serializedItem.ToSongItem())
                    .ToList() ?? new List<SongItem>();

                Debug.Log($"Loaded {songItems.values.Count} songs");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                songItems.values = new List<SongItem>();
            }
        }

        private void EnsureJsonFileExists()
        {
            if (!File.Exists(savePath))
            {
                // Create proper JSON structure with empty items array
                string json = "{\"items\":[]}";
                File.WriteAllText(savePath, json);
                Debug.Log($"Created new song list file at {savePath}");
            }
        }

        //private void SaveSongListToFile()
        //{
        //    try
        //    {
        //        // Take first song only for testing
        //        var testSong = songItems.values.FirstOrDefault();
        //        if (testSong == null) return;

        //        var serializedSong = new SerializableSongItem(testSong);
        //        string directJson = JsonUtility.ToJson(serializedSong, true);
        //        Debug.Log($"Direct serialization test: {directJson}");

        //        // Now try with wrapper
        //        var wrapper = new SongItemListWrapper
        //        {
        //            items = new List<SerializableSongItem> { serializedSong }
        //        };
        //        string wrapperJson = JsonUtility.ToJson(wrapper, true);
        //        Debug.Log($"Wrapper serialization test: {wrapperJson}");

        //        File.WriteAllText(savePath, wrapperJson);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.LogException(ex);
        //    }
        //}

        private void SaveSongListToFile()
        {
            try
            {
                var testSong = songItems.values[0];
                var serializedSong = new SerializableSongItem(testSong);
                string json = JsonUtility.ToJson(serializedSong, true);
                Debug.Log($"Single song JSON: {json}");
                File.WriteAllText(savePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save song: {ex}");
            }
        }


        [System.Serializable]
        private class SongItemListWrapper
        {
            public List<SerializableSongItem> items;
        }
    }
}