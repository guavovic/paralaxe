using System;
using UnityEngine;

namespace Guavovic.Parallax.Samples
{
    /// <summary>Um trecho do mapa com a sua música, o seu ambiente e o tipo de chão (para os passos).</summary>
    [Serializable]
    public sealed class DemoAudioZone
    {
        public string name;
        [Tooltip("A zona vale da câmera neste X em diante, até a próxima zona.")]
        public float fromX = -10000f;
        public AudioClip[] music = new AudioClip[0];
        public AudioClip ambience;
        public DemoSurface surface;
    }

    public enum DemoSurface
    {
        Grass,
        Stone,
        Rock
    }

    /// <summary>
    /// Música e ambiente por zona do mapa, trocando com fade cruzado. As músicas da zona tocam uma depois da outra,
    /// sem repetir a mesma em seguida. Sobrevive à troca de cena: a cena nova só passa as suas zonas, e a música segue.
    /// </summary>
    public sealed class DemoAudio : MonoBehaviour
    {
        [SerializeField] private DemoAudioZone[] zones = new DemoAudioZone[0];
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.45f;
        [SerializeField, Range(0f, 1f)] private float ambienceVolume = 0.5f;
        [SerializeField, Min(0.1f)] private float crossfadeSeconds = 4f;

        private static DemoAudio _instance;

        private readonly AudioSource[] _music = new AudioSource[2];
        private readonly AudioSource[] _ambience = new AudioSource[2];
        private int _musicSlot;
        private int _ambienceSlot;
        private int _zone = -1;
        private int _lastTrack = -1;

        public static DemoAudio Current => _instance;
        public DemoAudioZone CurrentZone => _zone >= 0 && _zone < zones.Length ? zones[_zone] : null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlay()
        {
            _instance = null;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                _instance.Adopt(zones);
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            for (int i = 0; i < 2; i++)
            {
                _music[i] = NewSource(false);
                _ambience[i] = NewSource(true);
            }
        }

        private AudioSource NewSource(bool loop)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.volume = 0f;
            source.spatialBlend = 0f;
            return source;
        }

        /// <summary>Cena nova com outras zonas: troca as zonas e deixa a música seguir se a zona continuar igual.</summary>
        private void Adopt(DemoAudioZone[] newZones)
        {
            var previous = CurrentZone;
            zones = newZones;
            _zone = -1;
            int index = ZoneAt(CameraX());
            if (previous != null && index >= 0 && SameMusic(previous, zones[index]))
                _zone = index;
        }

        private static bool SameMusic(DemoAudioZone a, DemoAudioZone b)
        {
            return a.ambience == b.ambience && a.music.Length == b.music.Length && (a.music.Length == 0 || a.music[0] == b.music[0]);
        }

        private void Update()
        {
            int index = ZoneAt(CameraX());
            if (index != _zone && index >= 0)
            {
                _zone = index;
                _lastTrack = -1;
                PlayNextTrack();
                Swap(_ambience, ref _ambienceSlot, zones[index].ambience, restart: false);
            }

            // Perto do fim da música, entra a próxima com o mesmo fade.
            var playing = _music[_musicSlot];
            if (playing.clip != null && playing.isPlaying && playing.clip.length > crossfadeSeconds * 2f
                && playing.time >= playing.clip.length - crossfadeSeconds)
                PlayNextTrack();

            float step = Time.unscaledDeltaTime / crossfadeSeconds;
            Fade(_music, _musicSlot, musicVolume, step);
            Fade(_ambience, _ambienceSlot, ambienceVolume, step);
        }

        private void PlayNextTrack()
        {
            var zone = CurrentZone;
            if (zone == null || zone.music.Length == 0)
            {
                Swap(_music, ref _musicSlot, null, restart: true);
                return;
            }

            int next = UnityEngine.Random.Range(0, zone.music.Length);
            if (zone.music.Length > 1 && next == _lastTrack)
                next = (next + 1) % zone.music.Length;
            _lastTrack = next;
            Swap(_music, ref _musicSlot, zone.music[next], restart: true);
        }

        /// <summary>Troca com fade cruzado. Sem restart, o mesmo som que já toca continua (ambiente da zona igual).</summary>
        private static void Swap(AudioSource[] pair, ref int slot, AudioClip clip, bool restart)
        {
            if (!restart && pair[slot].clip == clip && pair[slot].isPlaying)
                return;

            slot = 1 - slot;
            pair[slot].clip = clip;
            pair[slot].volume = 0f;
            if (clip != null)
                pair[slot].Play();
            else
                pair[slot].Stop();
        }

        private static void Fade(AudioSource[] pair, int slot, float volume, float step)
        {
            for (int i = 0; i < 2; i++)
            {
                float target = i == slot ? volume : 0f;
                pair[i].volume = Mathf.MoveTowards(pair[i].volume, target, step * Mathf.Max(volume, 0.01f));
                if (i != slot && pair[i].isPlaying && pair[i].volume <= 0f)
                    pair[i].Stop();
            }
        }

        private int ZoneAt(float x)
        {
            int found = -1;
            for (int i = 0; i < zones.Length; i++)
            {
                if (x >= zones[i].fromX)
                    found = i;
            }
            return found;
        }

        private static float CameraX()
        {
            var camera = Camera.main;
            return camera != null ? camera.transform.position.x : 0f;
        }
    }
}
