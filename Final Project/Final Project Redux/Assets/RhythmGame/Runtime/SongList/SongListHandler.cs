﻿using System;
using System.Collections;
using System.Collections.Generic;
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

        private void Start()
        {
            RefreshUI();
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

        private void RemoveSong(SongItem songToRemove)
        {
            Debug.Log($"Removing song: {songToRemove.name}");
            songItems.values.Remove(songToRemove); // Remove the song from the list
            RefreshUI(); // Refresh the UI
        }
    }
}