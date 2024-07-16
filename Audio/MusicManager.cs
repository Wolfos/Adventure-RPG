using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Models;
using OpenWorld;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;
using World;
using Random = UnityEngine.Random;

namespace Audio
{
    public class MusicManager : MonoBehaviour
    {
        [Serializable]
        private struct Music
        {
            [FormerlySerializedAs("music")] public AudioClip clip;
            public TimeStamp minStartTime;
            public TimeStamp maxStartTime;
            public WorldSpace worldSpace;
            public bool locationSpecific;
            public Location location;
        }

        [SerializeField] private List<Music> allMusic;
        [SerializeField] private List<AudioClip> nonSpecificMusic;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private int minSilenceDuration = 10;
        [SerializeField] private int maxSilenceDuration = 50;

        public static bool MusicDisabled = false;

        private void Start()
        {
            StartCoroutine(MusicRoutine());
        }
        

        private IEnumerator MusicRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                allMusic.Shuffle();
                var musicSelected = false;
                foreach (var music in allMusic)
                {
                    if (music.worldSpace != WorldStreamer.CurrentWorldSpace) continue;
                    if (music.locationSpecific)
                    {
                        if (OverworldLocations.CurrentSmallLocation != null &&
                            OverworldLocations.CurrentSmallLocation.location != music.location) continue;
                        if (OverworldLocations.CurrentBigLocation != null &&
                            OverworldLocations.CurrentBigLocation.location != music.location) continue;

                        if (OverworldLocations.CurrentSmallLocation == null &&
                            OverworldLocations.CurrentBigLocation == null) continue;
                    }

                    if (TimeManager.IsBetween(music.minStartTime, music.maxStartTime) == false) continue;

                    audioSource.clip = music.clip;
                    audioSource.Play();
                    musicSelected = true;
                    break;
                }

                if (musicSelected == false)
                {
                    var random = Random.Range(0, nonSpecificMusic.Count);
                    audioSource.clip = nonSpecificMusic[random];
                    audioSource.Play();
                }

                yield return new WaitForSecondsRealtime(audioSource.clip.length);

                var silenceDuration = Random.Range(minSilenceDuration, maxSilenceDuration);
                yield return new WaitForSecondsRealtime(silenceDuration);
            }
        }

        private void Update()
        {
            audioSource.volume = MusicDisabled ? 0 : 1;
        }
    }
}