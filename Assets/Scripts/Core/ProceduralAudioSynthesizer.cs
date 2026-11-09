using System;
using System.Collections.Generic;
using UnityEngine;

namespace FNaFSisterLocation.Core
{
    public enum VoiceProfileId
    {
        HandUnit,
        AngstyTeen,
        CircusBaby,
        ElizabethAfton,
        WilliamAfton,
        BoardMember,
        MichaelAfton,
        FuntimeFreddy,
        BonBon,
        Vlad,
        Clara,
        Announcer,
        Technician
    }

    public struct SubtitleLine
    {
        public string Speaker;
        public string Text;
        public VoiceProfileId Profile;
        public float Duration;
    }

    /// <summary>
    /// 100% Procedural Audio Synthesis & Voice Formant Engine.
    /// Zero external audio files required; generates AudioClips mathematically.
    /// </summary>
    public sealed class ProceduralAudioSynthesizer
    {
        private const int SAMPLE_RATE = 22050;
        private readonly AudioSource _sfxSource;
        private readonly AudioSource _ambientSource;
        private readonly AudioSource _musicBoxSource;
        private readonly AudioSource _voiceSource;

        private readonly Dictionary<string, AudioClip> _cachedClips = new Dictionary<string, AudioClip>();
        private readonly Queue<SubtitleLine> _dialogueQueue = new Queue<SubtitleLine>();

        public string ActiveSpeaker { get; private set; } = "";
        public string ActiveSubtitle { get; private set; } = "";
        private float _subtitleTimer = 0f;

        // Ballora's 4-stage "Crumbling Dreams" melody notes (Hz)
        private static readonly float[] CrumblingDreamsNotes = new float[]
        {
            440.0f, 523.25f, 659.25f, 587.33f, 523.25f, 493.88f, 440.0f, 392.0f,
            440.0f, 523.25f, 659.25f, 783.99f, 659.25f, 587.33f, 523.25f, 493.88f
        };

        public ProceduralAudioSynthesizer(GameObject host)
        {
            _sfxSource = host.AddComponent<AudioSource>();
            _ambientSource = host.AddComponent<AudioSource>();
            _ambientSource.loop = true;
            _musicBoxSource = host.AddComponent<AudioSource>();
            _musicBoxSource.loop = true;
            _voiceSource = host.AddComponent<AudioSource>();

            PrewarmCommonClips();
        }

        private void PrewarmCommonClips()
        {
            _cachedClips["shock"] = GenerateShockClip();
            _cachedClips["honk"] = GenerateHonkClip();
            _cachedClips["vent_crawl"] = GenerateVentCrawlClip();
            _cachedClips["flash_beacon"] = GenerateFlashBeaconClip();
            _cachedClips["springlock_wind"] = GenerateSpringlockClickClip();
            _cachedClips["door_slam"] = GenerateDoorSlamClip();
            _cachedClips["jumpscare"] = GenerateJumpscareScreechClip();
            _cachedClips["baby_cry"] = GenerateBabyCryClip();
            _cachedClips["chime"] = GenerateChimeClip();
            _cachedClips["crumbling_dreams"] = GenerateCrumblingDreamsLoop();
            _cachedClips["casual_bongos"] = GenerateCasualBongosLoop();
            _cachedClips["elevator_hum"] = GenerateElevatorHumLoop();
        }

        public void Update(float deltaTime)
        {
            if (_subtitleTimer > 0f)
            {
                _subtitleTimer -= deltaTime;
                if (_subtitleTimer <= 0f)
                {
                    if (_dialogueQueue.Count > 0)
                    {
                        PlayNextQueuedLine();
                    }
                    else
                    {
                        ActiveSpeaker = "";
                        ActiveSubtitle = "";
                    }
                }
            }
            else if (_dialogueQueue.Count > 0)
            {
                PlayNextQueuedLine();
            }
        }

        public void ClearDialogue()
        {
            _dialogueQueue.Clear();
            _subtitleTimer = 0f;
            ActiveSpeaker = "";
            ActiveSubtitle = "";
            if (_voiceSource != null) _voiceSource.Stop();
        }

        public void Speak(string speaker, string text, VoiceProfileId profile, float duration = 4.5f, bool interrupt = false)
        {
            if (interrupt)
            {
                ClearDialogue();
            }
            _dialogueQueue.Enqueue(new SubtitleLine
            {
                Speaker = speaker,
                Text = text,
                Profile = profile,
                Duration = Mathf.Max(1.8f, duration)
            });
            if (_subtitleTimer <= 0f)
            {
                PlayNextQueuedLine();
            }
        }

        private void PlayNextQueuedLine()
        {
            if (_dialogueQueue.Count == 0) return;
            var line = _dialogueQueue.Dequeue();
            ActiveSpeaker = line.Speaker;
            ActiveSubtitle = line.Text;
            _subtitleTimer = line.Duration;

            AudioClip cadence = GenerateVoiceCadenceClip(line.Text, line.Profile, Mathf.Min(line.Duration, 3.5f));
            _voiceSource.clip = cadence;
            _voiceSource.volume = 0.45f;
            _voiceSource.Play();
        }

        public void PlaySfx(string key, float volume = 0.8f, float panStereo = 0f)
        {
            if (_cachedClips.TryGetValue(key, out AudioClip clip))
            {
                _sfxSource.panStereo = panStereo;
                _sfxSource.PlayOneShot(clip, volume);
            }
        }

        public void SetMusicBoxStage(int stage, float panStereo = 0f)
        {
            // stage 0 = off, 1 = distant, 2 = medium, 3 = loud danger
            if (stage <= 0)
            {
                if (_musicBoxSource.isPlaying) _musicBoxSource.Stop();
                return;
            }
            if (_musicBoxSource.clip != _cachedClips["crumbling_dreams"])
            {
                _musicBoxSource.clip = _cachedClips["crumbling_dreams"];
            }
            if (!_musicBoxSource.isPlaying)
            {
                _musicBoxSource.Play();
            }
            _musicBoxSource.panStereo = panStereo;
            _musicBoxSource.volume = stage == 1 ? 0.16f : (stage == 2 ? 0.45f : 0.92f);
            _musicBoxSource.pitch = stage == 3 ? 1.04f : 1.0f;
        }

        public void StartAmbientLoop(string loopKey, float volume = 0.28f)
        {
            if (_cachedClips.TryGetValue(loopKey, out AudioClip clip))
            {
                if (_ambientSource.clip == clip && _ambientSource.isPlaying)
                {
                    _ambientSource.volume = volume;
                    return;
                }
                _ambientSource.clip = clip;
                _ambientSource.volume = volume;
                _ambientSource.Play();
            }
        }

        public void StopAmbientLoop()
        {
            if (_ambientSource != null && _ambientSource.isPlaying)
            {
                _ambientSource.Stop();
            }
        }

        private AudioClip GenerateVoiceCadenceClip(string text, VoiceProfileId profile, float durationSec)
        {
            int samples = Mathf.Max(2048, Mathf.RoundToInt(SAMPLE_RATE * durationSec));
            float[] data = new float[samples];

            float baseFreq = 165f;
            float vibrato = 5f;
            bool robotic = false;

            switch (profile)
            {
                case VoiceProfileId.HandUnit:
                    baseFreq = 155f; robotic = true; break;
                case VoiceProfileId.AngstyTeen:
                    baseFreq = 195f; robotic = true; break;
                case VoiceProfileId.CircusBaby:
                    baseFreq = 265f; vibrato = 3.5f; break;
                case VoiceProfileId.ElizabethAfton:
                    baseFreq = 340f; vibrato = 6f; break;
                case VoiceProfileId.WilliamAfton:
                    baseFreq = 110f; vibrato = 2f; break;
                case VoiceProfileId.BoardMember:
                    baseFreq = 135f; vibrato = 2.5f; break;
                case VoiceProfileId.MichaelAfton:
                    baseFreq = 122f; vibrato = 3f; break;
                case VoiceProfileId.FuntimeFreddy:
                    baseFreq = 220f; vibrato = 14f; robotic = true; break;
                case VoiceProfileId.BonBon:
                    baseFreq = 370f; vibrato = 8f; break;
                case VoiceProfileId.Vlad:
                    baseFreq = 145f; vibrato = 4.5f; break;
                case VoiceProfileId.Clara:
                    baseFreq = 290f; vibrato = 7f; break;
            }

            int syllableCount = Mathf.Clamp(text.Length / 4, 3, 28);
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SAMPLE_RATE;
                int sylIdx = Mathf.FloorToInt((i / (float)samples) * syllableCount);
                int charSeed = text.Length > 0 ? text[sylIdx % text.Length] : 65;
                bool pause = (charSeed % 5 == 0);
                if (pause)
                {
                    data[i] = 0f;
                    continue;
                }

                float pitchMod = 1f + ((charSeed % 7) - 3) * 0.04f + Mathf.Sin(t * vibrato * Mathf.PI * 2f) * 0.02f;
                float f = baseFreq * pitchMod;
                float s = Mathf.Sin(2f * Mathf.PI * f * t) * 0.6f + Mathf.Sin(4f * Mathf.PI * f * t) * 0.25f;
                if (robotic)
                {
                    s *= Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 55f * t));
                }
                float env = Mathf.Sin(((i % (samples / syllableCount)) / (float)(samples / syllableCount)) * Mathf.PI);
                data[i] = s * env * 0.35f;
            }

            AudioClip clip = AudioClip.Create("VoiceCadence", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateShockClip()
        {
            int samples = Mathf.RoundToInt(SAMPLE_RATE * 0.85f);
            float[] data = new float[samples];
            System.Random rng = new System.Random(101);
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SAMPLE_RATE;
                float buzz = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 60f * t)) * 0.55f +
                             Mathf.Sin(2f * Mathf.PI * 180f * t) * 0.3f;
                float crackle = ((float)rng.NextDouble() * 2f - 1f) * 0.65f;
                float env = 1f - (i / (float)samples) * 0.5f;
                data[i] = Mathf.Clamp((buzz + crackle) * env, -1f, 1f);
            }
            AudioClip clip = AudioClip.Create("Shock", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateHonkClip()
        {
            int samples = Mathf.RoundToInt(SAMPLE_RATE * 0.22f);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SAMPLE_RATE;
                float f = i < samples / 2 ? 580f : 490f;
                float s = Mathf.Sin(2f * Mathf.PI * f * t) + 0.4f * Mathf.Sin(4f * Mathf.PI * f * t);
                float env = Mathf.Sin((i / (float)samples) * Mathf.PI);
                data[i] = s * env * 0.5f;
            }
            AudioClip clip = AudioClip.Create("Honk", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateVentCrawlClip()
        {
            int samples = Mathf.RoundToInt(SAMPLE_RATE * 0.18f);
            float[] data = new float[samples];
            System.Random rng = new System.Random(202);
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SAMPLE_RATE;
                float clang = Mathf.Sin(2f * Mathf.PI * 140f * t) * Mathf.Exp(-t * 18f);
                float noise = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 25f) * 0.5f;
                data[i] = (clang + noise) * 0.7f;
            }
            AudioClip clip = AudioClip.Create("VentCrawl", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateFlashBeaconClip()
        {
            int samples = Mathf.RoundToInt(SAMPLE_RATE * 0.45f);
            float[] data = new float[samples];
            System.Random rng = new System.Random(303);
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SAMPLE_RATE;
                float snap = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 35f);
                float whineFreq = Mathf.Lerp(1200f, 3800f, t / 0.45f);
                float whine = Mathf.Sin(2f * Mathf.PI * whineFreq * t) * 0.25f;
                data[i] = Mathf.Clamp(snap + whine, -1f, 1f);
            }
            AudioClip clip = AudioClip.Create("FlashBeacon", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateSpringlockClickClip()
        {
            int samples = Mathf.RoundToInt(SAMPLE_RATE * 0.06f);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SAMPLE_RATE;
                data[i] = Mathf.Sin(2f * Mathf.PI * 950f * t) * Mathf.Exp(-t * 60f) * 0.6f;
            }
            AudioClip clip = AudioClip.Create("SpringlockClick", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateDoorSlamClip()
        {
            int samples = Mathf.RoundToInt(SAMPLE_RATE * 0.32f);
            float[] data = new float[samples];
            System.Random rng = new System.Random(404);
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SAMPLE_RATE;
                float thud = Mathf.Sin(2f * Mathf.PI * 68f * t) * Mathf.Exp(-t * 12f);
                float clang = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 20f) * 0.45f;
                data[i] = Mathf.Clamp((thud + clang) * 0.85f, -1f, 1f);
            }
            AudioClip clip = AudioClip.Create("DoorSlam", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateJumpscareScreechClip()
        {
            int samples = Mathf.RoundToInt(SAMPLE_RATE * 1.35f);
            float[] data = new float[samples];
            System.Random rng = new System.Random(505);
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SAMPLE_RATE;
                float fm = Mathf.Sin(2f * Mathf.PI * 32f * t) * 180f;
                float s1 = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * (420f + fm) * t)) * 0.35f;
                float s2 = Mathf.Sign(Mathf.Sin(2f * Mathf.PI * (890f - fm * 0.7f) * t)) * 0.35f;
                float s3 = ((float)rng.NextDouble() * 2f - 1f) * 0.4f;
                float env = Mathf.Clamp01(1.2f - (i / (float)samples));
                data[i] = Mathf.Clamp((s1 + s2 + s3) * env, -1f, 1f);
            }
            AudioClip clip = AudioClip.Create("Jumpscare", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateBabyCryClip()
        {
            int samples = Mathf.RoundToInt(SAMPLE_RATE * 0.55f);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SAMPLE_RATE;
                float f = Mathf.Lerp(680f, 420f, t / 0.55f) + Mathf.Sin(t * 35f) * 25f;
                data[i] = Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Sin((i / (float)samples) * Mathf.PI) * 0.45f;
            }
            AudioClip clip = AudioClip.Create("BabyCry", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateChimeClip()
        {
            int samples = Mathf.RoundToInt(SAMPLE_RATE * 0.4f);
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)SAMPLE_RATE;
                data[i] = (Mathf.Sin(2f * Mathf.PI * 880f * t) + 0.5f * Mathf.Sin(2f * Mathf.PI * 1320f * t)) * Mathf.Exp(-t * 7f) * 0.4f;
            }
            AudioClip clip = AudioClip.Create("Chime", samples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateCrumblingDreamsLoop()
        {
            float noteDur = 0.42f;
            int totalSamples = Mathf.RoundToInt(SAMPLE_RATE * noteDur * CrumblingDreamsNotes.Length);
            float[] data = new float[totalSamples];
            int samplesPerNote = Mathf.RoundToInt(SAMPLE_RATE * noteDur);

            for (int n = 0; n < CrumblingDreamsNotes.Length; n++)
            {
                float freq = CrumblingDreamsNotes[n];
                int start = n * samplesPerNote;
                for (int i = 0; i < samplesPerNote && (start + i) < totalSamples; i++)
                {
                    float t = i / (float)SAMPLE_RATE;
                    float bell = (Mathf.Sin(2f * Mathf.PI * freq * t) + 0.35f * Mathf.Sin(4f * Mathf.PI * freq * t)) * Mathf.Exp(-t * 5.2f);
                    data[start + i] = bell * 0.45f;
                }
            }
            AudioClip clip = AudioClip.Create("CrumblingDreams", totalSamples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateCasualBongosLoop()
        {
            float loopDur = 1.6f;
            int totalSamples = Mathf.RoundToInt(SAMPLE_RATE * loopDur);
            float[] data = new float[totalSamples];
            float[] hitTimes = new float[] { 0.0f, 0.2f, 0.4f, 0.7f, 0.9f, 1.1f, 1.3f, 1.45f };
            float[] hitFreqs = new float[] { 210f, 280f, 210f, 310f, 210f, 280f, 320f, 240f };

            for (int h = 0; h < hitTimes.Length; h++)
            {
                int start = Mathf.RoundToInt(hitTimes[h] * SAMPLE_RATE);
                int len = Mathf.RoundToInt(0.14f * SAMPLE_RATE);
                for (int i = 0; i < len && (start + i) < totalSamples; i++)
                {
                    float t = i / (float)SAMPLE_RATE;
                    float f = hitFreqs[h] * Mathf.Exp(-t * 8f);
                    data[start + i] += Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Exp(-t * 22f) * 0.55f;
                }
            }
            AudioClip clip = AudioClip.Create("CasualBongos", totalSamples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateElevatorHumLoop()
        {
            int totalSamples = SAMPLE_RATE * 2;
            float[] data = new float[totalSamples];
            for (int i = 0; i < totalSamples; i++)
            {
                float t = i / (float)SAMPLE_RATE;
                float hum = Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.25f + Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.12f;
                float rattle = Mathf.Sin(2f * Mathf.PI * 6f * t) * 0.08f;
                data[i] = hum + rattle;
            }
            AudioClip clip = AudioClip.Create("ElevatorHum", totalSamples, 1, SAMPLE_RATE, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
