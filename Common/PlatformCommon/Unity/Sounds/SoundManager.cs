#if !BACKOFFICE
using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Linq;
using System.Collections.Generic;
using System;

namespace MetaLoop.Common.PlatformCommon.Unity.Sounds
{

    [System.Serializable]
    public class SoundManager : MonoBehaviour
    {

        public DGSoundClip[] Sounds;

        private float volume = 1.0f;
        private float musicVolume = 1.0f;

        public int MaxEfxSoundSource = 5;
        private AudioSource[] efxSources;

        private AudioSource efxSource;                   //Drag a reference to the audio source which will play the sound effects.
        private AudioSource musicSource;                 //Drag a reference to the audio source which will play the music.

        private DGSoundClip currentMusic;

        public float lowPitchRange = .95f;              //The lowest a sound effect will be randomly pitched.
        public float highPitchRange = 1.05f;            //The highest a sound effect will be randomly pitched.


        private List<AudioSource> pausedSource;

        public static SoundManager Instance;
        public static SoundManager getInstance()
        {
           // if (instance == null)
               // instance = new SoundManager();

            return Instance;
        }

   

        void Awake()
        {
            
            Instance = this;

            pausedSource = new List<AudioSource>();

            efxSources = new AudioSource[MaxEfxSoundSource];
            for (int i = 0;i< MaxEfxSoundSource;i++)
            {
                efxSources[i] = gameObject.AddComponent<AudioSource>();
 
            }
           
            musicSource = gameObject.AddComponent<AudioSource>();

            foreach(var mySound in Sounds)
            {
                if(mySound.Name == "")
                {
                    mySound.Name = mySound.Audio.name;
                }
            }

        }


        public SoundManager()
        {


        }

        public void SetSfxVolume(float newVolume)
        {

            volume = newVolume;

            //update all volume
            efxSources.ToList().ForEach(p => p.volume = volume);


        }
        public void SetMusicVolume(float newVolume)
        {
            musicVolume = newVolume;



            //update  music
            musicSource.volume = currentMusic != null ? currentMusic.Volume*musicVolume : musicVolume;

        }

        private AudioSource GetAvailableEFXAudioSource()
        {

            //for testing purpose
          //  return gameObject.AddComponent<AudioSource>();

            for(int i = 0;i < efxSources.Length;i++) {
                AudioSource audioSource = efxSources[i];
              
                if (!audioSource.isPlaying && pausedSource.IndexOf(audioSource,0) == -1)
                {
                    return audioSource;
                }
            }
            return null;
        }

        public void PlaySound(DGSoundClip clip,AudioSource useAudioSource = null)
        {


            if (clip.Type == DGSoundType.Sound_Fx)
            {

                AudioSource useSource = GetAvailableEFXAudioSource();

                if (useAudioSource != null)
                    useSource = useAudioSource;

                if (useSource != null)
                {
                    useSource.DOKill();
                    clip.CurrentSource = useSource;

                    useSource.time = 0;

                    //Set the clip of our efxSource audio source to the clip passed in as a parameter.
                    useSource.clip = clip.Audio;
                    useSource.loop = clip.Loop;
                    useSource.volume = clip.Volume * volume;
                    useSource.Play();
                } else
                {
                    //Debug.Log(" no more sound !! wtf!");
                }

             

            }
            else
            {
                currentMusic = clip;

                //Set the clip of our efxSource audio source to the clip passed in as a parameter.
                musicSource.clip = clip.Audio;
                musicSource.loop = clip.Loop;
                musicSource.volume = clip.Volume * musicVolume;
                musicSource.Play();
            }

        }

        public void StopSound(DGSoundClip clip)
        {
            if (clip == null || clip.CurrentSource == null) return;

            //if not same clip, mean we pass the audiosource to another sound and we shouldnt stop it
            if (clip.Audio != clip.CurrentSource.clip) return;

            clip.CurrentSource.Stop();
            clip.currentPosition = 0f;
           
            //remove from paused sound
            pausedSource.Remove(clip.CurrentSource);

        }

        public void PauseMusic()
        {

            musicSource.Pause();

         
        }
        public void ResumeMusic()
        {
            musicSource.Play();
        }

        public void PauseSound(DGSoundClip clip)
        {
            //little hack
            /*if (clip != null && clip.CurrentSource != null && !clip.CurrentSource.isPlaying)
            {
                clip.currentPosition = -1f;
               
            }*/

            if (clip == null || clip.CurrentSource == null || clip.currentPosition == -1f) return;

            //add to paused sound
            pausedSource.Add(clip.CurrentSource);

            clip.currentPosition = clip.CurrentSource.time;
            clip.CurrentSource.Pause();
        }
        public void ResumeSound(DGSoundClip clip)
        {
            //Debug.Log("RESUME SOUND");
            if (clip == null || clip.CurrentSource == null || clip.currentPosition == -1f) return;
            

            if (clip.Audio.length < clip.currentPosition)
            {
                Debug.Log("ERROR HERE WTF HOW POSSIBLE");
            }
            else {
                clip.CurrentSource.time = clip.currentPosition;
            }

            //remove from paused sound
            pausedSource.Remove(clip.CurrentSource);

            clip.CurrentSource.Play();
            

        }

       

        public void FadeOut(string name, float duration = 0.5f)
        {
            List<AudioSource> source = efxSources.Where(p => p.clip != null && (p.clip.name == name || p.clip.name == name + "(Clone)")).ToList();

            if(musicSource.clip != null && (musicSource.clip.name == name || musicSource.clip.name == name + "(Clone)"))
                source.Add(musicSource);

            source.ForEach(p => { p.DOKill(); p.DOFade(0, duration).OnComplete(p.Stop); });
        }

        public void FadeOut(DGSoundClip clip,float duration = 0.5f)
        {
            clip.CurrentSource.DOKill();
            clip.CurrentSource.DOFade(0, duration).OnComplete(()=> clip.CurrentSource.Stop());
        }

        public DGSoundClip GetSoundByName(string soundName)
        {

            foreach (DGSoundClip sound in Sounds)
            {
                if (sound.Name == soundName)
                {
                    return sound;
                }
            }


            return null;

        }


        /// <summary>
        /// Play a sound by name (need to be in the Sound list)
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public DGSoundClip PlaySoundByName(string Name,float volume = -1,bool unique = false,AudioSource useAudioSource = null)
        {
 
            foreach(DGSoundClip sound in Sounds)
            {
                if(sound.Name == Name)
                {
                    
                    if(unique)
                    {
                        //check if the sound is already playing, if yes, we don't play it
                        if(efxSources.Where(p => p.clip == sound.Audio && p.isPlaying && p.time < 0.2f).ToList().Count > 0)
                        {
                            return null;
                        }
                    }

                    //before
                    //PlaySound(sound);
                    //return sound;

                    //should create a copy i think
                    DGSoundClip newSound = new DGSoundClip();
                    newSound.Loop = sound.Loop;
                    newSound.Name = sound.Name;
                    newSound.Type = sound.Type;
                    newSound.Volume = volume != -1 && volume != 0 ? volume : sound.Volume;
                    newSound.Audio = sound.Audio;
                    newSound.currentPosition = 0f;
                   

                    PlaySound(newSound, useAudioSource);
                    return newSound;
                }
            }
            return null;
        }


        public void PlaySoundFx(AudioClip clip, float volume = -1f)
        {

            AudioSource useSource = GetAvailableEFXAudioSource();

            if (useSource != null)
            {
                //Set the clip of our efxSource audio source to the clip passed in as a parameter.
                useSource.clip = clip;
                useSource.loop = false;
                useSource.volume = volume == -1 ? this.volume : volume;
                useSource.Play();
            }

               

        }


        public DGSoundClip PlaySoundByResources(string path, float volume = 1f, DGSoundType type = DGSoundType.Sound_Fx, bool loop = false)
        {


            

            DGSoundClip sound = new DGSoundClip();
            sound.Volume = volume;
            sound.Type = type;
            sound.Loop = loop;
            StartCoroutine(LoadSoundByResources(sound, path));

            return sound;

        }

        IEnumerator LoadSoundByResources(DGSoundClip sound,string path)
        {
            var request = Resources.LoadAsync(path);
            yield return request;

            sound.Audio = request.asset as AudioClip;

            PlaySound(sound);

        }

        public IEnumerator PlaySoundByPath(string path, DGSoundType type)
        {

            WWW request = new WWW(path);

            // Wait for download to complete
            yield return request;

            // use request.audio 
            AudioClip loadedMp3 = request.GetAudioClip(false, false);

            DGSoundClip newSound = new DGSoundClip();
            newSound.Audio = loadedMp3;
            newSound.Type = type;

            PlaySound(newSound);


        }

        public void PlayRandom(params AudioClip[] clips)
        {

            //Generate a random number between 0 and the length of our array of clips passed in.
            int randomIndex = UnityEngine.Random.Range(0, clips.Length);

            //Choose a random pitch to play back our clip at between our high and low pitch ranges.
            float randomPitch = UnityEngine.Random.Range(lowPitchRange, highPitchRange);

            //Set the pitch of the audio source to the randomly chosen pitch.
            efxSource.pitch = randomPitch;

            //Set the clip to the clip at our randomly chosen index.
            efxSource.clip = clips[randomIndex];

            //Play the clip.
            efxSource.Play();

        }


    }
}
#endif