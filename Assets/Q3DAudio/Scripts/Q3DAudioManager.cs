//=============================================================================
//
//                  Copyright (c) 2017 QUALCOMM Technologies Inc.
//                              All Rights Reserved.
//
//==============================================================================

using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using UnityEngine;

public class Q3DAudioManager : MonoBehaviour
{
    static public void DebugLog(string debugLogStr)
    {
        bool log = false;
        Q3DAudioGlobalSettings globalSettings = Q3DAudioGlobalSettings.GetQ3DAudioGlobalSettings();
        if(globalSettings)
        {
#if UNITY_ANDROID
            log = globalSettings.mLogOnAndroid;
#elif UNITY_EDITOR
            log = globalSettings.mLogOnWin64CSharp;
#elif UNITY_STANDALONE_WIN
            if (IntPtr.Size == 8)
            {
                log = globalSettings.mLogOnWin64CSharp;
            }
            else if(IntPtr.Size == 4)
            {
                log = globalSettings.mLogOnWin32;
            }
#endif
        }

        if (log)
        {
            DebugLogAlways(debugLogStr);
        }
    }
    static public void DebugLogAlways(string debugLogStr)
    {
        Debug.Log("q3d_audio C#:" + debugLogStr);
    }

    static public Q3DAudioSource AddQ3DAudioSourceComponent(ref AudioSource audioSource)
    {
        return audioSource.gameObject.AddComponent(typeof(Q3DAudioSource)) as Q3DAudioSource;
    }

    public const string skPluginName = "audiopluginq3d";

    public static bool DisableIfUnsupportedPlatform(Behaviour behaviour)
    {
        if (UnsupportedPlatform)
        {
            behaviour.enabled = false;
            return true;
        }

        return false;
    }

    static public bool DisableIfQ3DNotActive(Behaviour behaviour)
    {
        //special-case unsupported platforms to minimize wasted startup processing
        bool disabledUnsupportedPlatform = DisableIfUnsupportedPlatform(behaviour);
        if (disabledUnsupportedPlatform)
        {
            return disabledUnsupportedPlatform;
        }

        bool supportedPlatformDisabled = false;
        Q3DAudioGlobalSettings globalSettings = Q3DAudioGlobalSettings.GetQ3DAudioGlobalSettings();
        if (globalSettings)
        {
#if UNITY_ANDROID
            supportedPlatformDisabled = globalSettings.mDisableOnAndroid;
#elif UNITY_EDITOR_WIN
            supportedPlatformDisabled = globalSettings.mDisableOnWin64Editor;
#elif UNITY_STANDALONE_WIN
            if (IntPtr.Size == 8)
            {
                supportedPlatformDisabled = globalSettings.mDisableOnWin64;
            }
            else if(IntPtr.Size == 4)
            {
                supportedPlatformDisabled = globalSettings.mDisableOnWin32;
            }
#endif
        }

        if(supportedPlatformDisabled)
        {
            behaviour.enabled = false;
            return true;
        }

        return false;
    }

    [DllImport(skPluginName)]
    public static extern void SetApplicationDataPath(string applicationDataPath, bool inEditor);

    public static bool UnsupportedPlatform
    {
        get
        {
#if !UNITY_ANDROID && !UNITY_STANDALONE_WIN && !UNITY_EDITOR_WIN
            return true;
#else
            return false;
#endif
        }
    }

    public static Type[] FindObjectsOfTypeAllWrapper<Type>()
    {
        return Resources.FindObjectsOfTypeAll(typeof(Type)) as Type[];
    }

    static public void SpatializeMonoAudioSources(bool log)
    {
        AudioSource[] audioSources = FindObjectsOfTypeAllWrapper<AudioSource>();

        foreach (AudioSource audioSource in audioSources)
        {
            AudioClip clip = audioSource.clip;
            if (clip && clip.channels == 1 && audioSource.GetComponent<Q3DAudioSource>() == null)
            {
                audioSource.gameObject.AddComponent(typeof(Q3DAudioSource));
                audioSource.spatialize = true;
                if(log)
                {
                    Q3DAudioManager.DebugLogAlways("AudioSource " + audioSource.gameObject.name + " with mono clip " + clip.name + " found without a Q3DAudioSource; adding Q3DAudioSource and setting Spatialize=true");
                }
            }
        }
    }

    [DllImport(Q3DAudioManager.skPluginName)]
    public static extern void GetDefaultReverb(
        out float gainAdjust,
        out float brightAdjust,
        out float timeAdjust,
        out int wetMix,
        out float roomDimensionsX_meters,
        out float roomDimensionsY_meters,
        out float roomDimensionsZ_meters,
        out /*vr_audio_room_material_type*/int roomMatWalls,
        out /*vr_audio_room_material_type*/int roomMatCeiling,
        out /*vr_audio_room_material_type*/int roomMatFloor);

    public struct Q3DAudioRoomSettings
    {
        public float gainAdjust;
        public float brightAdjust;
        public float timeAdjust;
        public int wetMix;
        public float roomDimensionsX_meters;
        public float roomDimensionsY_meters;
        public float roomDimensionsZ_meters;
        public Q3DAudioRoom.vr_audio_room_material_type roomMaterialWalls;
        public Q3DAudioRoom.vr_audio_room_material_type roomMaterialCeiling;
        public Q3DAudioRoom.vr_audio_room_material_type roomMaterialFloor;

        public bool BinaryEquivalent(Q3DAudioRoomSettings q3dAudioRoomSettings)//because we don't need all the HashCode(), etc C# machinery
        {
            return gainAdjust == q3dAudioRoomSettings.gainAdjust &&
                    brightAdjust == q3dAudioRoomSettings.brightAdjust &&
                    timeAdjust == q3dAudioRoomSettings.timeAdjust &&
                    wetMix == q3dAudioRoomSettings.wetMix &&
                    roomDimensionsX_meters == q3dAudioRoomSettings.roomDimensionsX_meters &&
                    roomDimensionsY_meters == q3dAudioRoomSettings.roomDimensionsY_meters &&
                    roomDimensionsZ_meters == q3dAudioRoomSettings.roomDimensionsZ_meters &&
                    roomMaterialWalls == q3dAudioRoomSettings.roomMaterialWalls &&
                    roomMaterialCeiling == q3dAudioRoomSettings.roomMaterialCeiling &&
                    roomMaterialFloor == q3dAudioRoomSettings.roomMaterialFloor;
        }
    };
    public Q3DAudioRoomSettings mQ3DAudioRoomSettingsDefault;
    public Q3DAudioRoomSettings mQ3DAudioRoomSettingsLast;

    [DllImport(Q3DAudioManager.skPluginName)]
    public static extern void SetReverbProcessorPreferences(int firstChoiceReverbProcessor,int secondChoiceReverbProcessor);
    static private bool CommandLineParameterMatch(
        string commandArgName,
        string commandLineArg, 
        bool caseInsensitive)
    {
        return commandArgName.IndexOf(commandArgName) == 0;
    }

    private static string[] GetCommandLineArgumentValueStrings()
    {
        return Enum.GetNames(typeof(Q3DAudioGlobalSettings.vr_audio_shoebox_mode));
    }
    private static Q3DAudioGlobalSettings.vr_audio_shoebox_mode ParseVRAudioShoeboxModeEnumFromString(string reverbProcessorName)
    {
        return (Q3DAudioGlobalSettings.vr_audio_shoebox_mode)Enum.Parse(typeof(Q3DAudioGlobalSettings.vr_audio_shoebox_mode), reverbProcessorName);
    }
    private static bool StringIsInString(string substr, string str)
    {
        return str.IndexOf(substr) >= 0;
    }
    private static void ParseAndroidReverbProcessorIfItExists(ref Q3DAudioGlobalSettings.vr_audio_shoebox_mode androidReverbProcessor, string c)
    {
        string commandLineArg = c.ToLower();
        string[] commandLineArgumentValueStrings = GetCommandLineArgumentValueStrings();
        foreach(string commandLineArgumentValueString in commandLineArgumentValueStrings)
        {
            if (StringIsInString(commandLineArgumentValueString.ToLower(), commandLineArg))
            {
                androidReverbProcessor = ParseVRAudioShoeboxModeEnumFromString(commandLineArgumentValueString);
                return;
            }
        }
    }
#if UNITY_ANDROID
    private bool HasExtra(string firstChoiceReverbProcessorKey, AndroidJavaObject intent)
    {
        return intent.Call<bool>("hasExtra", firstChoiceReverbProcessorKey);
    }
    private string GetExtraValue(string key, AndroidJavaObject intent)
    {
        AndroidJavaObject extras = intent.Call<AndroidJavaObject>("getExtras");
        return extras.Call<string>("getString", key);
    }
    private void ParseAndroidReverbProcessorIfItExists(
        ref Q3DAudioGlobalSettings.vr_audio_shoebox_mode reverbProcessor,
        string reverbProcessorKey, 
        AndroidJavaObject intent)
    {
        if (HasExtra(reverbProcessorKey, intent))
        {
            ParseAndroidReverbProcessorIfItExists(ref reverbProcessor, GetExtraValue(reverbProcessorKey, intent));
        }
    }
#endif//#if UNITY_ANDROID
    void Awake()
    {
        //Q3DAudioManager should never be constructed by Q3DAudioListener if we are running a platform Q3DAudio does not support or the user has disabled
        //allow C++ plugin can emit to Unity's debug logs -- call as soon as possible!
        DebugLogCallbackDelegate callbackDelegate = new DebugLogCallbackDelegate(DebugLogCallBackFunction);
        System.IntPtr intptrDelegate = Marshal.GetFunctionPointerForDelegate(callbackDelegate);//convert callback_delegate into a function pointer that can be used in unmanaged code
        SetDebugLogCallback(intptrDelegate);// Call the API passing along the function pointer

        Q3DAudioGlobalSettings.vr_audio_shoebox_mode firstChoiceReverbProcessor = Q3DAudioGlobalSettings.vr_audio_shoebox_mode.COMPUTE_DSP;
        Q3DAudioGlobalSettings.vr_audio_shoebox_mode secondChoiceReverbProcessor = Q3DAudioGlobalSettings.vr_audio_shoebox_mode.HEXAGON_ADSP;
        Q3DAudioGlobalSettings globalSettings = Q3DAudioGlobalSettings.GetQ3DAudioGlobalSettings();
        if (globalSettings)
        {
            firstChoiceReverbProcessor = globalSettings.mAndroidReverbProcessorFirstChoice;
            secondChoiceReverbProcessor = globalSettings.mAndroidReverbProcessorSecondChoice;
        }

        /*  * command line arguments (Windows) and intents (Android) override global settings
            * command line arguments are case-insensitive and are expected to be formatted: NAME_OF_EXE Q3DAudio1stChoiceReverbProcessor=COMPUTE_DSP
            * the equivalent Android intent has a case-sensitive key (due to Android's requirements) and a case-insensitive value: 
              adb shell am start -n NAME_OF_PACKAGE/com.unity3d.player.UnityPlayerActivity -e Q3DAudio1stChoiceReverbProcessor COMPUTE_CPU
        */
        string firstChoiceReverbProcessorString = "1stChoiceReverbProcessor";
        string secondChoiceReverbProcessorString = "2ndChoiceReverbProcessor";
        string q3dAudioString = "Q3DAudio";

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        //Windows is case-insensitive for convenience, because (unlike some other platforms) we can make it so
        q3dAudioString = q3dAudioString.ToLower();
        firstChoiceReverbProcessorString = firstChoiceReverbProcessorString.ToLower();
        secondChoiceReverbProcessorString = secondChoiceReverbProcessorString.ToLower();

        string[] commandLineArgs = Environment.GetCommandLineArgs();
        foreach (string c in commandLineArgs)
        {
            string commandLineArg = c.ToLower();
            if (StringIsInString(q3dAudioString, commandLineArg))
            {
                if (StringIsInString(firstChoiceReverbProcessorString, commandLineArg))
                {
                    ParseAndroidReverbProcessorIfItExists(ref firstChoiceReverbProcessor, commandLineArg);
                }
                else if (StringIsInString(secondChoiceReverbProcessorString, commandLineArg))
                {
                    ParseAndroidReverbProcessorIfItExists(ref secondChoiceReverbProcessor, commandLineArg);
                }
            }
        }
#elif UNITY_ANDROID
        AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        AndroidJavaObject intent = currentActivity.Call<AndroidJavaObject>("getIntent");
        ParseAndroidReverbProcessorIfItExists(ref firstChoiceReverbProcessor, q3dAudioString + firstChoiceReverbProcessorString, intent);
        ParseAndroidReverbProcessorIfItExists(ref secondChoiceReverbProcessor, q3dAudioString + secondChoiceReverbProcessorString, intent);
#endif//#if UNITY_STANDALONE_WIN #elif UNITY_ANDROID

        Q3DAudioManager.DebugLog("Reverb: 1stChoiceProcessor=" + firstChoiceReverbProcessor + ";2ndChoiceProcessor=" + secondChoiceReverbProcessor);
        SetReverbProcessorPreferences((int)firstChoiceReverbProcessor, (int)secondChoiceReverbProcessor);

        //point C++ plugin to the directory where q3d_audio dll's are deployed
        Q3DAudioManager.SetApplicationDataPath(Application.dataPath, InEditor());

        //the user is not allowed to set the default parameters on the mixer effect at runtime, so this can be done here once
        int roomMatWalls, roomMatCeiling, roomMatFloor;
        GetDefaultReverb(
            out mQ3DAudioRoomSettingsDefault.gainAdjust,
            out mQ3DAudioRoomSettingsDefault.brightAdjust,
            out mQ3DAudioRoomSettingsDefault.timeAdjust,
            out mQ3DAudioRoomSettingsDefault.wetMix,
            out mQ3DAudioRoomSettingsDefault.roomDimensionsX_meters,
            out mQ3DAudioRoomSettingsDefault.roomDimensionsY_meters,
            out mQ3DAudioRoomSettingsDefault.roomDimensionsZ_meters,
            out roomMatWalls,
            out roomMatCeiling,
            out roomMatFloor);
        mQ3DAudioRoomSettingsDefault.roomMaterialWalls = (Q3DAudioRoom.vr_audio_room_material_type)roomMatWalls;
        mQ3DAudioRoomSettingsDefault.roomMaterialCeiling = (Q3DAudioRoom.vr_audio_room_material_type)roomMatCeiling;
        mQ3DAudioRoomSettingsDefault.roomMaterialFloor = (Q3DAudioRoom.vr_audio_room_material_type)roomMatFloor;

#if UNITY_EDITOR
        //after the first "play", the editor doesn't recreate the mixer effects, so the default settings are not necessarily what q3d_audio is using
        SetAudioRoomSettings(mQ3DAudioRoomSettingsDefault);//q3d_audio uses default settings
#else
        //for standalone players, the default settings are by definition what is set immediately after creating the mixer effects
        mQ3DAudioRoomSettingsLast = mQ3DAudioRoomSettingsDefault;
#endif//#if UNITY_EDITOR

        DontDestroyOnLoad(this);//persist until the game is shut down

        //BEG_CONFIG
        /*  as of Unity 2017.1.0f3 this cannot be done.  Executing the code below appears to cause Unity to destroy all 
         *  mixer effects, and the spatializer effect and ambisonic decoder effect (correct), and then recreate all of 
         *  these except for the ambisonic decoder effect (wrong) and then proceeds to play silence on Android (wrong) */

        ///@todo: warn if overriding values
        //AudioConfiguration audioConfiguration = AudioSettings.GetConfiguration(); 
        //audioConfiguration.dspBufferSize = 192;
        ////audioConfiguration.sampleRate = 48000;
        ////audioConfiguration.speakerMode = AudioSpeakerMode.Stereo;
        ////AudioSettings.Reset(audioConfiguration);///@todo: if false, then log epic failure
        //END_CONFIG
    }
    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        Q3DAudioGlobalSettings globalSettings = Q3DAudioGlobalSettings.GetQ3DAudioGlobalSettings();
        if (globalSettings && globalSettings.mOnStartSpatializeMonoAudioSources)
        {
            SpatializeMonoAudioSources(globalSettings.mLogSpatializeMonoAudioSources);
        }
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Q3DAudioGlobalSettings globalSettings = Q3DAudioGlobalSettings.GetQ3DAudioGlobalSettings();
        if (globalSettings && globalSettings.mOnSceneLoadedSpatializeMonoAudioSources)
        {
            SpatializeMonoAudioSources(globalSettings.mLogSpatializeMonoAudioSources);
        }
    }
    void Update()
    {
        Q3DAudioGlobalSettings globalSettings = Q3DAudioGlobalSettings.GetQ3DAudioGlobalSettings();
        if (globalSettings && globalSettings.mOnUpdateSpatializeMonoAudioSources)
        {
            SpatializeMonoAudioSources(globalSettings.mLogSpatializeMonoAudioSources);
        }
    }
    void OnApplicationQuit()
    {
        Q3DAudioManager.DebugLog("OnApplicationQuit called!");
        OnApplicationQuit(InEditor());
    }

    public void SetAudioRoomSettings(Q3DAudioRoomSettings currentSettings)
    {
        if (!currentSettings.BinaryEquivalent(mQ3DAudioRoomSettingsLast))
        {
            mQ3DAudioRoomSettingsLast = currentSettings;

            Q3DAudioManager.DebugLog("Q3DAudioRoom:Set=["
                + currentSettings.gainAdjust + ";"
                + currentSettings.brightAdjust + ";"
                + currentSettings.timeAdjust + ";"
                + currentSettings.wetMix + ";"
                + currentSettings.roomDimensionsX_meters + ";"
                + currentSettings.roomDimensionsY_meters + ";"
                + currentSettings.roomDimensionsZ_meters + ";"
                + currentSettings.roomMaterialWalls + ";"
                + currentSettings.roomMaterialCeiling + ";"
                + currentSettings.roomMaterialFloor + "]");

            Q3DAudioRoom.SetReverb(
                currentSettings.gainAdjust,
                currentSettings.brightAdjust,
                currentSettings.timeAdjust,
                currentSettings.wetMix,
                currentSettings.roomDimensionsX_meters,
                currentSettings.roomDimensionsY_meters,
                currentSettings.roomDimensionsZ_meters,
                (int)currentSettings.roomMaterialWalls,
                (int)currentSettings.roomMaterialCeiling,
                (int)currentSettings.roomMaterialFloor);
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void DebugLogCallbackDelegate(string str);
    static void DebugLogCallBackFunction(string str)
    {
        Debug.Log(str);
    }

    [DllImport(Q3DAudioManager.skPluginName)]
    public static extern void SetDebugLogCallback(System.IntPtr fp);

    [DllImport(Q3DAudioManager.skPluginName)]
    public static extern void OnApplicationQuit(bool inEditor);

    private bool InEditor()
    {
#if UNITY_EDITOR
        return true;
#else
        return false;
#endif//#if UNITY_EDITOR
    }
}
