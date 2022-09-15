using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CollieMollie.Audio
{
    public class AudioManager : MonoBehaviour
    {
        #region Variable Field
        [Header("Event")]
        [SerializeField] private AudioEventChannel _audioEventChannel = null;

        [Header("Mixer")]
        [SerializeField] private AudioMixer _mixer = null;

        [Header("Pool")]
        [SerializeField] private AudioPlayer _audioPlayerRef = null;
        [SerializeField] private Transform _poolHolder = null;
        [SerializeField] private IObjectPool<AudioPlayer> _pool = null;
        #endregion

        private void Start()
        {
            _pool = new ObjectPool<AudioPlayer>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject);
        }

        private void OnEnable()
        {
            _audioEventChannel.OnPlayAudioRequest += PlayAudio;
        }

        private void OnDisable()
        {
            _audioEventChannel.OnPlayAudioRequest -= PlayAudio;
        }

        #region Subscribers
        private void PlayAudio(AudioPreset preset)
        {
            AudioPlayer audioPlayer = _pool.Get();
            AudioPlayer.Data audioPlayerData = new AudioPlayer.Data
            {
                clip = preset.GetAudioClip(),
                group = preset.MixerGroup
            };
            audioPlayer.Play(audioPlayerData);
        }
        #endregion

        #region Pool
        private AudioPlayer CreatePooledItem()
        {
            AudioPlayer audioPlayer = Instantiate(_audioPlayerRef, _poolHolder);
            audioPlayer.Pool = _pool;
            return audioPlayer;
        }

        private void OnTakeFromPool(AudioPlayer audioPlayer)
        {
            audioPlayer.gameObject.SetActive(true);
        }

        private void OnReturnedToPool(AudioPlayer audioPlayer)
        {
            audioPlayer.gameObject.SetActive(false);
        }

        private void OnDestroyPoolObject(AudioPlayer audioPlayer)
        {
            Addressables.Release(audioPlayer);
            Destroy(audioPlayer.gameObject);
        }
        #endregion
    }
}