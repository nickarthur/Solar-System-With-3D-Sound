//=============================================================================
//
//                  Copyright (c) 2017 QUALCOMM Technologies Inc.
//                              All Rights Reserved.
//
//==============================================================================

#if UNITY_2017 || UNITY_2018 || UNITY_2019 || UNITY_2020 || UNITY_2021 || UNITY_2022 || UNITY_2023 || UNITY_2024 || UNITY_2025 || UNITY_2026 || UNITY_2027 || UNITY_2028 || UNITY_2029
#define Q3D_AMBISONIC_DECODING_SUPPORTED
#endif//#if UNITY_2017

using System.Collections.Generic;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.Audio;
using UnityEngine;

//#QualcommAudioMenu: keep menu name in sync, since C# doesn't allow string concatenation for its attributes
[AddComponentMenu("QualcommAudio/Q3DAudioSource")]
public class Q3DAudioSource : MonoBehaviour
{
    [HideInInspector]
    public AudioSource mAudioSource = null;

    //#Q3DAudioSourceInSync: keep minimum and maximum values in sync with Q3DAudioPlugin.cpp:InternalRegisterEffectDefinitionSoundShared()
    public const float GainMin=0.0f;
    public const float GainMax = 1.0f;
    [SerializeField]
    private float mGain = 1.0f;
    public float Gain
    {
        get
        {
            return mGain;
        }
        set
        {
            mGain = Mathf.Clamp(value, GainMin, GainMax);

            int channelsNum = GetClipChannelsNum();
            if (IsSoundObject(channelsNum))
            {
                mAudioSource.SetSpatializerFloat((int)Q3DAudioSource.SpatializerParameter.Gain, mGain);
            }
#if Q3D_AMBISONIC_DECODING_SUPPORTED
            else if (IsFirstOrSecondOrderAmbisonic(channelsNum))
            {
                mAudioSource.SetAmbisonicDecoderFloat((int)Q3DAudioSource.SpatializerParameter.Gain, mGain);
            }
#endif//#if Q3D_AMBISONIC_DECODING_SUPPORTED
        }
    }

    [SerializeField]
    private bool mDistanceRolloffOverrideAudioSourcesRange = false;
    public bool DistanceRolloffOverrideAudioSourcesRange
    {
        get
        {
            return mDistanceRolloffOverrideAudioSourcesRange;
        }
        set
        {
            mDistanceRolloffOverrideAudioSourcesRange = value;
            /*  #DistanceRolloffUpdate: Since we already have to check each frame if changes to the underlying 
             *  AudioSource's distance rolloff range should update Q3DAudioManager's distance rolloff parameters, just let
             *  that piece of code perform the update rather than duplicate logic */
        }
    }

    //see #Q3DAudioSourceInSync
    public const float kDistanceRolloffMinRange = 0.0f;
    public const float kDistanceRolloffMaxRange = float.MaxValue;
    private float mDistanceRolloffMinLast, mDistanceRolloffMaxLast;
    [SerializeField]
    private float mDistanceRolloffMin = 10.0f;
    public float DistanceRolloffMin
    {
        get
        {
            return mDistanceRolloffMin;
        }
        set
        {
            if(mDistanceRolloffOverrideAudioSourcesRange)
            {
                mDistanceRolloffMin = value;
                DistanceRolloffEnforceRange(ref mDistanceRolloffMin, ref mDistanceRolloffMax);
                //see #DistanceRolloffUpdate
            }
        }
    }

    [SerializeField]
    private float mDistanceRolloffMax = 50.0f;
    public float DistanceRolloffMax
    {
        get
        {
            return mDistanceRolloffMax;
        }
        set
        {
            mDistanceRolloffMax = value;
            DistanceRolloffEnforceRange(ref mDistanceRolloffMin, ref mDistanceRolloffMax);
            //see #DistanceRolloffUpdate
        }
    }

    public static void DistanceRolloffEnforceRange(ref float min, ref float max)
    {
        if (min < kDistanceRolloffMinRange)
        {
            min = kDistanceRolloffMinRange;
        }
        if(max > kDistanceRolloffMaxRange)
        {
            max = kDistanceRolloffMaxRange;
        }
        if(max < min)
        {
            max = min;
        }
    }

    //keep in sync with unnamed C++ enum in Q3DAudioPlugin.cpp in the Spatializer namespace
    private enum SpatializerParameter
    {
        Gain,
        ChannelsNum,
        Id,
        IsPlaying,
        DistanceRolloffMin,
        DistanceRolloffMax,
        DistanceRolloffModel
    }
    //keep in sync with unnamed C++ enum in Q3DAudioPlugin.cpp in the AmbisonicSpatializer namespace
    private enum AmbisonicSpatializerParameter
    {
        Gain,
        ChannelsNum,
        Id,
        IsPlaying
    }

    ///keep in sync with C++ enum of the same name defined in public/common.h of the q3d_audio SAM code
    public enum vr_audio_distance_rolloff_model
    {
        VR_AUDIO_DISTANCE_ROLLOFF_MODEL_LOG = 0,
        VR_AUDIO_DISTANCE_ROLLOFF_MODEL_LINEAR,
        VR_AUDIO_DISTANCE_ROLLOFF_MODEL_NONE
    };
    [SerializeField]
    private vr_audio_distance_rolloff_model mDistanceRolloffModel = vr_audio_distance_rolloff_model.VR_AUDIO_DISTANCE_ROLLOFF_MODEL_LINEAR;
    public vr_audio_distance_rolloff_model DistanceRolloffModel
    {
        get
        {
            return mDistanceRolloffModel;
        }
        set
        {
            mDistanceRolloffModel = value;
            SetSpatializerFloatDistanceRolloff();
        }
    }

    //key is integer and value is that integer's equivalent 32bit IEEE float; this hash map ensures no id duplication
    static private Dictionary<int,float> smIdHashMap = new Dictionary<int,float>();
    static private System.Random smRandom = new System.Random(0);//minimize hash collisions; use seed for (unverified) deterministic id generation
    private float mId;

    const float kPlayingLastFrame = 1.0f;
    const float kNotPlayingLastFrame = 0.0f;
    private float mWasPlayingLastFrame = kNotPlayingLastFrame;
    private int mPreviousChannelsNum = 0;
    static public void LogIfQ3DAudioSourceNumIsNotCorrect(int q3dAudioSourcesLength, int audioSourcesLength, string gameObjectName)
    {
        if (q3dAudioSourcesLength != audioSourcesLength)
        {
            Q3DAudioManager.DebugLogAlways(
                "Authoring error: there should either be no Q3DAudioSource's on a GameObject, or there should be as many Q3DAudioSource's on the " +
                "GameObject as there are AudioSource's.  However, on " + gameObjectName + " there are " + audioSourcesLength +
                " AudioSource(s) and " + q3dAudioSourcesLength + " Q3DAudioSource(s).  Spatialization of these AudioSource's may be inconsistent");
        }
    }

    public bool InitializeAudioSource()
    {
        if(mAudioSource == null)
        {
            //assign one Q3DAudioSource to each AudioSource; if there aren't enough Q3DAudioSources, this is an authoring error
            Q3DAudioSource[] q3dAudioSources = GetComponents<Q3DAudioSource>();
            AudioSource[] audioSources = GetComponents<AudioSource>();
            for (int audioSourceIndex = 0; audioSourceIndex < audioSources.Length && audioSourceIndex < q3dAudioSources.Length; ++audioSourceIndex)
            {
                q3dAudioSources[audioSourceIndex].mAudioSource = audioSources[audioSourceIndex];
            }

            LogIfQ3DAudioSourceNumIsNotCorrect(q3dAudioSources.Length, audioSources.Length, this.gameObject.name);
        }

        if (mAudioSource == null)
        {
            DisableBecauseUninitialized();
            return false;
        }
        else
        {
            mDistanceRolloffMinLast = DistanceRolloffMin;
            mDistanceRolloffMaxLast = DistanceRolloffMax;
            return true;
        }
    }

    void OnEnable()
    {
        if(mAudioSource == null)
        {
            if(!InitializeAudioSource())
            {
                return;
            }
        }
    }

    private int GetClipChannelsNum()
    {
        return mAudioSource && mAudioSource.clip ? mAudioSource.clip.channels : 0;
    }

    private float IsPlayingSpatializerFloat()
    {
        if (mAudioSource && mAudioSource.isPlaying && IsSpatializedSound(GetClipChannelsNum()))
        {
            return 1.0f;//sound is "playing" if it is a valid q3d_audio signal channel format and is playing
        }
        else
        {
            return 0.0f;
        }
    }

    public static bool IsSoundObject(int channelsNum)
    {
        return channelsNum == 1;
    }
    public static bool IsFirstOrSecondOrderAmbisonic(int channelsNum)
    {
        return channelsNum == 4 || channelsNum == 9;
    }

    ///keep in sync with C++ enum of the same name in Q3DAudioPlugin.cpp
    enum IsPlayingChangedFlags {IsPlaying = 1 << 0,
                                IsSoundObject = 1 << 1,
                                IsSoundField = 1 << 2 };
    //see comments in C++ function of the same name in Q3DAudioPlugin.cpp
    [DllImport(Q3DAudioManager.skPluginName)]
    public static extern void IsPlayingChanged(float id, int flags);

    private void IsPlayingChangedSoundObject(int isPlayingFlags)
    {
        isPlayingFlags |= (int)IsPlayingChangedFlags.IsSoundObject;
        IsPlayingChanged(mId, isPlayingFlags);
    }

    private void IsPlayingChangedSoundField(int isPlayingFlags)
    {
        isPlayingFlags |= (int)IsPlayingChangedFlags.IsSoundField;
        IsPlayingChanged(mId, isPlayingFlags);
    }

    private void UpdatePlayingThisFrame()
    {
        //see #IsPlaying in Q3DAudioPlugin.cpp for an explanation of this function
        int channelsNum = GetClipChannelsNum();
        bool isSoundObject = IsSoundObject(channelsNum);
        bool isFirstOrSecondOrderAmbisonic = IsFirstOrSecondOrderAmbisonic(channelsNum);
        float isPlayingThisFrame = IsPlayingSpatializerFloat();

#if Q3D_AMBISONIC_DECODING_SUPPORTED
        bool wasSoundObject = IsSoundObject(mPreviousChannelsNum);
        bool wasFirstOrderAmbisonic = IsFirstOrSecondOrderAmbisonic(mPreviousChannelsNum);
#endif//#if Q3D_AMBISONIC_DECODING_SUPPORTED
        bool switchedFromOneSoundFormatToAnother =
            IsSpatializedSound(mPreviousChannelsNum) && !IsSpatializedSound(channelsNum)
#if Q3D_AMBISONIC_DECODING_SUPPORTED
            || ((wasSoundObject && isFirstOrSecondOrderAmbisonic)
            || (wasFirstOrderAmbisonic && isSoundObject))
#endif//#if Q3D_AMBISONIC_DECODING_SUPPORTED
            ;
        if ((mWasPlayingLastFrame != isPlayingThisFrame) || switchedFromOneSoundFormatToAnother)
        {
            mWasPlayingLastFrame = isPlayingThisFrame;
            int isPlayingFlags = isPlayingThisFrame != 0.0f ? (int)IsPlayingChangedFlags.IsPlaying : 0;
            if (isSoundObject)
            {
                IsPlayingChangedSoundObject(isPlayingFlags);
                mAudioSource.SetSpatializerFloat((int)Q3DAudioSource.SpatializerParameter.IsPlaying, isPlayingThisFrame);

                if (isPlayingThisFrame != 0.0f)
                {
                    //sound has started playing; synchronize spatializer plugin effect with this class's datafields
                    mAudioSource.SetSpatializerFloat((int)Q3DAudioSource.SpatializerParameter.Gain, mGain);
                    SetSpatializerFloatDistanceRolloff();
                    mAudioSource.SetSpatializerFloat((int)Q3DAudioSource.SpatializerParameter.ChannelsNum, channelsNum);
                    mAudioSource.SetSpatializerFloat((int)Q3DAudioSource.SpatializerParameter.Id, mId);
                }
            }
#if Q3D_AMBISONIC_DECODING_SUPPORTED
            else  if (isFirstOrSecondOrderAmbisonic)
            {
                IsPlayingChangedSoundField(isPlayingFlags);
                mAudioSource.SetAmbisonicDecoderFloat((int)Q3DAudioSource.SpatializerParameter.IsPlaying, isPlayingThisFrame);

                if (isPlayingThisFrame != 0.0f)
                {
                    //sound has started playing; synchronize spatializer plugin effect with this class's datafields
                    mAudioSource.SetAmbisonicDecoderFloat((int)Q3DAudioSource.SpatializerParameter.Gain, mGain);
                    mAudioSource.SetAmbisonicDecoderFloat((int)Q3DAudioSource.SpatializerParameter.ChannelsNum, channelsNum);
                    mAudioSource.SetAmbisonicDecoderFloat((int)Q3DAudioSource.SpatializerParameter.Id, mId);
                }
            }
#endif//#if Q3D_AMBISONIC_DECODING_SUPPORTED
            if (switchedFromOneSoundFormatToAnother)
            {
#if Q3D_AMBISONIC_DECODING_SUPPORTED
                if (wasSoundObject)
#endif//#if Q3D_AMBISONIC_DECODING_SUPPORTED
                {
                    IsPlayingChangedSoundObject(0);//free previous sound object
                }
#if Q3D_AMBISONIC_DECODING_SUPPORTED
                else if (wasFirstOrderAmbisonic)
                {
                    IsPlayingChangedSoundField(0);//free previous soundfield
                }
#endif//#if Q3D_AMBISONIC_DECODING_SUPPORTED
            }
        }

#if Q3D_AMBISONIC_DECODING_SUPPORTED
        /*  ensure that if a Q3DAudioSource's underlying AudioSource/AudioClip is switched to a 
         *  non-spatialized audio format (like stereo) or switched from one spatialized sound format to another, 
         *  that the resources associated with the previous spatialized sound are freed */
        mPreviousChannelsNum = channelsNum;
#endif//#if Q3D_AMBISONIC_DECODING_SUPPORTED
    }

    private bool IsSpatializedSound(int channelsNum)
    {
        return IsSoundObject(channelsNum)
#if Q3D_AMBISONIC_DECODING_SUPPORTED
	             || IsFirstOrSecondOrderAmbisonic(channelsNum)
#endif//#if Q3D_AMBISONIC_DECODING_SUPPORTED
                ;
    }

    private void SetSpatializerFloatDistanceRolloff()
    {
        mAudioSource.SetSpatializerFloat((int)Q3DAudioSource.SpatializerParameter.DistanceRolloffMin, DistanceRolloffMin);
        mAudioSource.SetSpatializerFloat((int)Q3DAudioSource.SpatializerParameter.DistanceRolloffMax, DistanceRolloffMax);
        mAudioSource.SetSpatializerFloat((int)Q3DAudioSource.SpatializerParameter.DistanceRolloffModel, (float)mDistanceRolloffModel);
    }

    private void DisableBecauseUninitialized()
    {
        Q3DAudioManager.DebugLog("Q3DAudioSource " + gameObject.name + " found no AudioSource on its GameObject -- disabling.  (Q3DAudioSource must have an AudioSource that references an AudioClip of 1 or 4 channels to work as a sound object or First Order Ambisonics soundfield, respectively).  Soundfields are supported by Unity 2017.1.0f3 and later versions");
        enabled = false;
    }

    void Awake()
    {
        if (Q3DAudioManager.DisableIfQ3DNotActive(this))
        {
            return;
        }

        int idInt;
        do
        {
            //Unity's audio interface only passes floats; use 23-bit mantissa of IEEE 32bit float to keep things simple with non-fractional float ids
            idInt = smRandom.Next(2 << 23);
        } while (smIdHashMap.ContainsKey(idInt));
        mId = (float)idInt;
        smIdHashMap.Add(idInt, mId);

        if (!InitializeAudioSource())
        {
            return;
        }
    }

    void OnDestroy()
    {
        if (!Q3DAudioManager.DisableIfQ3DNotActive(this))
        {
            UpdatePlayingThisFrame();//release q3d_audio resources
        }
    }

    private bool UpdateDistanceRolloffIfNecessary(
        ref float distanceRolloffLast, 
        float distanceRolloffCurrent, 
        bool updateDistanceRolloff)
    {
        if (distanceRolloffLast != distanceRolloffCurrent)
        {
            distanceRolloffLast = distanceRolloffCurrent;
            updateDistanceRolloff = true;
        }
        return updateDistanceRolloff;
    }

    void Update()
    {
        UpdatePlayingThisFrame();

        //see #DistanceRolloffUpdate
        if(!DistanceRolloffOverrideAudioSourcesRange)
        {
            mDistanceRolloffMin = mAudioSource.minDistance;
            mDistanceRolloffMax = mAudioSource.maxDistance;
            DistanceRolloffEnforceRange(ref mDistanceRolloffMin, ref mDistanceRolloffMax);
        }
        bool updateDistanceRolloff = false;
        updateDistanceRolloff = UpdateDistanceRolloffIfNecessary(ref mDistanceRolloffMinLast, DistanceRolloffMin, updateDistanceRolloff);
        updateDistanceRolloff = UpdateDistanceRolloffIfNecessary(ref mDistanceRolloffMaxLast, DistanceRolloffMax, updateDistanceRolloff);
        if(updateDistanceRolloff)
        {
            SetSpatializerFloatDistanceRolloff();
        }
    }
}
