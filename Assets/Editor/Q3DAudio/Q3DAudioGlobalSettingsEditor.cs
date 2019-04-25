//=============================================================================
//
//                  Copyright (c) 2017 QUALCOMM Technologies Inc.
//                              All Rights Reserved.
//
//==============================================================================

#if UNITY_EDITOR

using System.IO;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Q3DAudioGlobalSettings))]
public class Q3DAudioGlobalSettingsEditor : Editor
{
    private string mWin32FilenameActive;
    private string mWin32FilenameDeactiveDebug;
    private string mWin32FilenameDeactiveRelease;

    private string mWin64FilenameActive;
    private string mWin64FilenameDeactiveDebug;
    private string mWin64FilenameDeactiveRelease;

    private string mAndroidFilenameActive;
    private string mAndroidFilenameDeactiveDebug;
    private string mAndroidFilenameDeactiveRelease;

    private bool mLogOnWin64Cpp;

    private string LogOnWin64Note
    {
        get
        {
            return  "Note that getting C++ level logging on Windows 64-bit requires closing the Editor, manually copying " + 
                    mWin64FilenameDeactiveDebug + " over " + mWin64FilenameActive + " and reopening the Editor.  This is because Unity loads " +
                    "the active plugin dll very early, and Windows does not allow copying over a loaded dll.  By default, " +
                    mWin64FilenameDeactiveRelease + " is in use, which provides no C++ logging";
        }
    }

    private static void BuildPluginFileNamesForPlatform(
        ref string filenameActive,
        ref string filenameDeactiveDebug,
        ref string filenameDeactiveRelease,
        /*const*/ string assetsDirFullPath,
        /*const*/ string pluginSubDir,
        /*const*/ string extensionActive,
        /*const*/ string extensionInactive,
        /*const*/ string activeFilenamePrefix)
    {
        const string q3dPluginName = "q3d";
        const string audioplugin = "audioplugin";
        const string libRelease = "_release";
        const string libDebug = "_debug";
        const string q3dAudio = "Q3DAudio/";

        /*const*/ string filenameDeactivated = q3dPluginName + "audio";
        filenameActive = assetsDirFullPath + "/Plugins/" + pluginSubDir + q3dAudio + activeFilenamePrefix + audioplugin + q3dPluginName + extensionActive;

        /*const*/ string sourceLibDir = assetsDirFullPath + "/" + q3dAudio + "PluginLibs/" + pluginSubDir;
        filenameDeactiveDebug = sourceLibDir + filenameDeactivated + libDebug + extensionInactive;
        filenameDeactiveRelease = sourceLibDir + filenameDeactivated + libRelease + extensionInactive;
    }
    public static void CopyAndroidLibrary(string sourceFullPath, string destinationFullPath)
    {
        Q3DAudioManager.DebugLogAlways("Copying " + sourceFullPath + " to " + destinationFullPath);

        File.SetAttributes(destinationFullPath, FileAttributes.Normal);
        File.Copy(sourceFullPath, destinationFullPath, true);
    }

    public void SetLogOnAndroid(bool logOnAndroid)
    {
        if (logOnAndroid)
        {
            CopyAndroidLibrary(mAndroidFilenameDeactiveDebug, mAndroidFilenameActive);
        }
        else
        {
            CopyAndroidLibrary(mAndroidFilenameDeactiveRelease, mAndroidFilenameActive);
        }
    }
    public void SetLogOnWin32(bool logOnWin32)
    {
        if (logOnWin32)
        {
            CopyAndroidLibrary(mWin32FilenameDeactiveDebug, mWin32FilenameActive);
        }
        else
        {
            CopyAndroidLibrary(mWin32FilenameDeactiveRelease, mWin32FilenameActive);
        }
    }
    //see LogOnWin64Note
    //public void SetLogOnWin64(bool logOnWin64)
    //{
    //    if (logOnWin64)
    //    {
    //        CopyAndroidLibrary(win64FilenameDeactiveDebug, win64FilenameActive);
    //    }
    //    else
    //    {
    //        CopyAndroidLibrary(win64FilenameDeactiveRelease, win64FilenameActive);
    //    }
    //}

    private bool FilesSameLength(string f0, string f1)
    {
        FileInfo f0Info = new FileInfo(f0);
        FileInfo f1Info = new FileInfo(f1);
        return f0Info.Length == f1Info.Length;
    }

    private void Awake()
    {
        const string windowsPluginExtensionActive = ".dll";
        const string windowsPluginExtensionInactive = "._dll";
        const string windowsActiveFilenamePrefix = "";//windows doesn't have one
        string applicationDataPath = Application.dataPath;
        BuildPluginFileNamesForPlatform(ref mWin32FilenameActive, ref mWin32FilenameDeactiveDebug, ref mWin32FilenameDeactiveRelease, applicationDataPath, "x86/", windowsPluginExtensionActive, windowsPluginExtensionInactive, windowsActiveFilenamePrefix);
        BuildPluginFileNamesForPlatform(ref mWin64FilenameActive, ref mWin64FilenameDeactiveDebug, ref mWin64FilenameDeactiveRelease, applicationDataPath, "x86_64/", windowsPluginExtensionActive, windowsPluginExtensionInactive, windowsActiveFilenamePrefix);
        BuildPluginFileNamesForPlatform(ref mAndroidFilenameActive, ref mAndroidFilenameDeactiveDebug, ref mAndroidFilenameDeactiveRelease, applicationDataPath, "Android/libs/armeabi-v7a/", ".aar", "._aar", "lib");

        mLogOnWin64Cpp = FilesSameLength(mWin64FilenameActive, mWin64FilenameDeactiveDebug);
        return;
    }

    [MenuItem("GameObject/Audio/Q3DAudioGlobalSettings")]
    static void CreateQ3DAudioGlobalSettings()
    {
        string Q3DAudioGlobalSettingsString = "Q3DAudioGlobalSettings";
        GameObject go = new GameObject(Q3DAudioGlobalSettingsString);
        Undo.RegisterCreatedObjectUndo(go, "Created " + Q3DAudioGlobalSettingsString);//this must be done before adding the component for undo to work!
        go.AddComponent<Q3DAudioGlobalSettings>();
    }


    SerializedProperty  mAndroidReverbProcessorFirstChoiceSerializedProperty,
                        mAndroidReverbProcessorSecondChoiceSerializedProperty,
                        mLogOnAndroidSerializedProperty,
                        mLogOnWin32SerializedProperty,
                        mLogOnWin64CSharpSerializedProperty,
                        mDisableOnAndroidSerializedProperty,
                        mDisableOnWin32SerializedProperty,
                        mDisableOnWin64SerializedProperty,
                        mDisableOnWin64EditorSerializedProperty,
                        mOnUpdateSpatializeMonoAudioSourcesSerializedProperty,
                        mOnSceneLoadedSpatializeMonoAudioSourcesSerializedProperty,
                        mOnStartSpatializeMonoAudioSourcesSerializedProperty,
                        mLogSpatializeMonoAudioSourcesSerializedProperty;
    void OnEnable()
    {
        mAndroidReverbProcessorFirstChoiceSerializedProperty = serializedObject.FindProperty("mAndroidReverbProcessorFirstChoice");
        mAndroidReverbProcessorSecondChoiceSerializedProperty = serializedObject.FindProperty("mAndroidReverbProcessorSecondChoice");

        mLogOnAndroidSerializedProperty = serializedObject.FindProperty("mLogOnAndroid");
        mLogOnAndroidSerializedProperty.boolValue = FilesSameLength(mAndroidFilenameActive, mAndroidFilenameDeactiveDebug);

        mLogOnWin32SerializedProperty = serializedObject.FindProperty("mLogOnWin32");
        mLogOnWin32SerializedProperty.boolValue = FilesSameLength(mWin32FilenameActive, mWin32FilenameDeactiveDebug);

        serializedObject.ApplyModifiedProperties();

        mLogOnWin64CSharpSerializedProperty = serializedObject.FindProperty("mLogOnWin64CSharp");

        mDisableOnAndroidSerializedProperty = serializedObject.FindProperty("mDisableOnAndroid");
        mDisableOnWin32SerializedProperty = serializedObject.FindProperty("mDisableOnWin32");
        mDisableOnWin64SerializedProperty = serializedObject.FindProperty("mDisableOnWin64");
        mDisableOnWin64EditorSerializedProperty = serializedObject.FindProperty("mDisableOnWin64Editor");

        mOnUpdateSpatializeMonoAudioSourcesSerializedProperty = serializedObject.FindProperty("mOnUpdateSpatializeMonoAudioSources");
        mOnSceneLoadedSpatializeMonoAudioSourcesSerializedProperty = serializedObject.FindProperty("mOnSceneLoadedSpatializeMonoAudioSources");
        mOnStartSpatializeMonoAudioSourcesSerializedProperty = serializedObject.FindProperty("mOnStartSpatializeMonoAudioSources");
        mLogSpatializeMonoAudioSourcesSerializedProperty = serializedObject.FindProperty("mLogSpatializeMonoAudioSources");
    }

    public override void OnInspectorGUI()
    {
        Q3DAudioGlobalSettings q3dAudioGlobalSettings = (Q3DAudioGlobalSettings)target;
        string spatializeMonoAudioSourcesTooltipString = 
                "all AudioSources are gathered, and any AudioSource that has a mono(1 channel) clip and does not have a " +
                "Q3DAudioSource and Spatialized=true will get both, thereby turning that AudioSource into a Q3DAudio sound object.  If " +
                "LogSpatializeMonoAudioSources is checked, a log message is emitted with the name of the mono clip that was transformed into a sound object.  " +
                "Depending on the complexity of the scene, this might be an expensive operation, so you might want to use this checkbox as a conversion " +
                "tool to discover which AudioSources should be set this way, set them yourself and then uncheck this checkbox.";
        string negligibleProcessingTimeTooltipString = 
            "If checked, all Q3DAudio scripts and plugin effects will consume negligible processing time on startup and shutdown, and even more " +
            "negligible processing time on a typical frame on the ";
        string logTooltipString = 
            "Enables Q3DAudio logging, which is very slow and will often cause audio dropouts, but gives much insight into what Q3DAudio is doing";

        if(Application.isPlaying)
        {
            GUI.enabled = false;
        }

        string androidReverbProcessorTooltipString = 
            "On Android platforms running on some Snapdragon hardware, reverb processing can be offloaded to separate processors for better performance and/or battery usage.  If neither the first nor second choice is available, then reverb will run on the ARM CPU.  ARM_CPU is guaranteed to be available";
        mAndroidReverbProcessorFirstChoiceSerializedProperty.enumValueIndex = 
            (int)(Q3DAudioGlobalSettings.vr_audio_shoebox_mode)EditorGUILayout.EnumPopup(
                new GUIContent
                (
                    "1st Choice Reverb Processor (Android,Snapdragon)",
                    androidReverbProcessorTooltipString
                ), 
                q3dAudioGlobalSettings.mAndroidReverbProcessorFirstChoice
        );
        mAndroidReverbProcessorSecondChoiceSerializedProperty.enumValueIndex = 
            (int)(Q3DAudioGlobalSettings.vr_audio_shoebox_mode)EditorGUILayout.EnumPopup(
                new GUIContent
                (
                    "2nd Choice Reverb Processor (Android,Snapdragon)",
                    androidReverbProcessorTooltipString
                ),
                q3dAudioGlobalSettings.mAndroidReverbProcessorSecondChoice
        );

        EditorGUILayout.Separator();

        bool logOnWin32OldValue = mLogOnWin32SerializedProperty.boolValue;
        mLogOnWin32SerializedProperty.boolValue = EditorGUILayout.Toggle(
            new GUIContent
            (
                "Log on Win32",
                logTooltipString
            ),
            q3dAudioGlobalSettings.mLogOnWin32
        );
        if(logOnWin32OldValue != mLogOnWin32SerializedProperty.boolValue)
        {
            SetLogOnWin32(mLogOnWin32SerializedProperty.boolValue);
        }

        mLogOnWin64CSharpSerializedProperty.boolValue = EditorGUILayout.Toggle(
            new GUIContent
            (
                "Log on Win64 (C# only)",
                logTooltipString + ".  " + LogOnWin64Note
            ),
            q3dAudioGlobalSettings.mLogOnWin64CSharp
        );

        bool oldGUIEnabled = GUI.enabled;
        GUI.enabled = false;
        EditorGUILayout.Toggle(
            new GUIContent
            (
                "Log on Win64 (C++)",
                logTooltipString + ".  " + LogOnWin64Note
            ),
            mLogOnWin64Cpp
        );
        GUI.enabled = oldGUIEnabled;

        bool logOnAndroidOldValue = mLogOnAndroidSerializedProperty.boolValue;
        mLogOnAndroidSerializedProperty.boolValue = EditorGUILayout.Toggle(
            new GUIContent
            (
                "Log on Android",
                logTooltipString
            ),
            q3dAudioGlobalSettings.mLogOnAndroid
        );
        if (logOnAndroidOldValue != mLogOnAndroidSerializedProperty.boolValue)
        {
            SetLogOnAndroid(mLogOnAndroidSerializedProperty.boolValue);
        }

        EditorGUILayout.Separator();

        mDisableOnWin32SerializedProperty.boolValue = EditorGUILayout.Toggle(
            new GUIContent
            (
                "Disable on Win32",
                negligibleProcessingTimeTooltipString + "Win32 (x86) platform."
            ),
            q3dAudioGlobalSettings.mDisableOnWin32
        );
        mDisableOnWin64SerializedProperty.boolValue = EditorGUILayout.Toggle(
            new GUIContent
            (
                "Disable on Win64",
                negligibleProcessingTimeTooltipString + "Win64 (x64) platform."
            ),
            q3dAudioGlobalSettings.mDisableOnWin64
        );
        mDisableOnWin64EditorSerializedProperty.boolValue = EditorGUILayout.Toggle(
            new GUIContent
            (
                "Disable on Win64 Editor",
                negligibleProcessingTimeTooltipString + "Win64 (x64) Editor platform.  Note that if you disable Q3DAudio in the Editor but have played the game in the Editor with one more Q3DAudio mixer effects, those effects will continue to use more-than-minimal per-frame processing resources until the Editor is restarted"
            ),
            q3dAudioGlobalSettings.mDisableOnWin64Editor
        );
        mDisableOnAndroidSerializedProperty.boolValue = EditorGUILayout.Toggle(
            new GUIContent
            (
                "Disable on Android",
                negligibleProcessingTimeTooltipString + "Android platform."
            ),
            q3dAudioGlobalSettings.mDisableOnAndroid
        );
        if (Application.isPlaying)
        {
            GUI.enabled = true;
        }

        EditorGUILayout.Separator();

        mOnUpdateSpatializeMonoAudioSourcesSerializedProperty.boolValue = EditorGUILayout.Toggle(
            new GUIContent
            (
                "OnUpdateSpatializeMonoAudioSources",
                "If checked, once every frame " + spatializeMonoAudioSourcesTooltipString
            ),
            q3dAudioGlobalSettings.mOnUpdateSpatializeMonoAudioSources
        );
        mOnSceneLoadedSpatializeMonoAudioSourcesSerializedProperty.boolValue = EditorGUILayout.Toggle(
            new GUIContent
            (
                "OnSceneLoadedSpatializeMonoAudioSources",
                "If checked, once every wave of OnSceneLoaded() calls " + spatializeMonoAudioSourcesTooltipString
            ),
            q3dAudioGlobalSettings.mOnSceneLoadedSpatializeMonoAudioSources
        );
        mOnStartSpatializeMonoAudioSourcesSerializedProperty.boolValue = EditorGUILayout.Toggle(
            new GUIContent
            (
                "OnStartSpatializeMonoAudioSources",
                "If checked, once every wave of OnStart() calls " + spatializeMonoAudioSourcesTooltipString
            ),
            q3dAudioGlobalSettings.mOnStartSpatializeMonoAudioSources
        );

        bool allowDebugLoggingForSpatializedMonoAudioSources =
            mOnUpdateSpatializeMonoAudioSourcesSerializedProperty.boolValue ||
            mOnSceneLoadedSpatializeMonoAudioSourcesSerializedProperty.boolValue ||
            mOnStartSpatializeMonoAudioSourcesSerializedProperty.boolValue;
        if(!allowDebugLoggingForSpatializedMonoAudioSources)
        {
            GUI.enabled = false;
        }
        mLogSpatializeMonoAudioSourcesSerializedProperty.boolValue = EditorGUILayout.Toggle(
            new GUIContent
            (
                "LogSpatializeMonoAudioSources",
                "If checked, every time one of the above 'SpatializeMonoAudioSources' checkboxes transforms an AudioSource into a sound object, a log " + 
                "with the name of the AudioSource's mono clip is emitted."
            ),
            q3dAudioGlobalSettings.mLogSpatializeMonoAudioSources
        );
        if (!allowDebugLoggingForSpatializedMonoAudioSources)
        {
            GUI.enabled = true;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif//#if UNITY_EDITOR