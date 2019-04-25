//=============================================================================
//
//                  Copyright (c) 2017 QUALCOMM Technologies Inc.
//                              All Rights Reserved.
//
//==============================================================================

using UnityEngine;

public class Q3DAudioGlobalSettings : MonoBehaviour
{
    //keep in sync with common.h's enum of the same name
    public enum vr_audio_shoebox_mode
    {
        ARM_CPU,        //VR_AUDIO_SHOEBOX_MODE_APPS
        HEXAGON_ADSP,   //VR_AUDIO_SHOEBOX_MODE_ADSP
        COMPUTE_DSP     //VR_AUDIO_SHOEBOX_MODE_CDSP;
    }
                                        
    public vr_audio_shoebox_mode mAndroidReverbProcessorFirstChoice = vr_audio_shoebox_mode.COMPUTE_DSP;
    public vr_audio_shoebox_mode mAndroidReverbProcessorSecondChoice = vr_audio_shoebox_mode.HEXAGON_ADSP;

    public bool mLogOnAndroid;
    public bool mLogOnWin32;
    public bool mLogOnWin64CSharp;

    public bool mDisableOnAndroid = false;
    public bool mDisableOnWin32 = false;
    public bool mDisableOnWin64 = false;
    public bool mDisableOnWin64Editor = false;

    public bool mOnUpdateSpatializeMonoAudioSources = false;
    public bool mOnSceneLoadedSpatializeMonoAudioSources = false;
    public bool mOnStartSpatializeMonoAudioSources = false;
    public bool mLogSpatializeMonoAudioSources = true;

    public static Q3DAudioGlobalSettings GetQ3DAudioGlobalSettings() { return smQ3DAudioGlobalSettings; }
    static Q3DAudioGlobalSettings smQ3DAudioGlobalSettings;///note that this value will remain resident even if the underlying GameObject is destroyed, so the last set global settings will persist forever

    void Awake()
    {
        if (Q3DAudioManager.DisableIfUnsupportedPlatform(this))
        {
            return;
        }
        
        /*  setup the global settings even if the user has disabled Q3DAudio on this supported platform; this allows the rest of Q3DAudio to disable 
         *  with minimal startup processing */
        if (smQ3DAudioGlobalSettings)
        {
            Q3DAudioManager.DebugLog(  "There is already another Q3DAudioGlobalSettings; this Q3DAudioGlobalSettings replaces and destroys it.  Multiple Q3DAudioGlobalSetting's in one scene is not recommended practice, since " +
                        "this original Q3DAudioGlobalSettings will not be restored, even if this Q3DAudioGlobalSettings is destroyed.  We recommend you " +
                        "instantiate one Q3DAudioGlobalSettings at startup; it will remain resident for the entire lifetime of the application, even if its " +
                        "GameObject is destroyed when a new scene is loaded.  If you place one Q3DAudioGlobalSettings object in each scene, then each time " +
                        "a new scene is loaded the latest Q3DAudioGlobalSettings will replace the previous one without emitting this message.");
            GameObject.Destroy(smQ3DAudioGlobalSettings.gameObject);
        }
        smQ3DAudioGlobalSettings = this;
    }
}
