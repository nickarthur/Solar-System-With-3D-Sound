# Solar-System-With-3D-Sound

Basic Information:

Maintainer: Kou Zhiwu、YongRong、YanShiqin、FangBo.

Start date: April 24th, 2019.

Function description:

Import the Qualcomm 3D audio plugin into Unity and add planetary sound effects to create a solar system with 3D sound.

Document introduction:

===>Editor/Q3DAudio
    |
    |--Q3DAudioGlobalSettingsEditor.cs：mainly to add Q3DAudioGlobalSettings's attributes in Unity interface.
    |--Q3DAudioRoomEditor.cs:mainly to add Q3DAudioRoom's attributes in Unity interface.
    |--Q3DAudioSourceEditor.cs：mainly to add Q3DAudioSource's attributes in Unity interface.
    
===>Music:some Audio clips.

===>Materials:some textures used in planet models.

===>Plugins:some dynamic libraries used in different Architecture, such armeabi-v7a、x86、x86_64.

===>Q3DAudio
    |--PluginLibs/ ：some dynamic libraries too.
    |--Scripts 
         |--Q3DAudioGlobalSettings.cs: get Q3DAudioGlobalSettings attribute values from inspector and handle it accordingly.
         |--Q3DAudioListener.cs:       get Q3DAudioListener attribute values from inspector and handle it accordingly.
         |--Q3DAudioManager.cs:        get Q3DAudioManager attribute values from inspector and handle it accordingly.
         |--Q3DAudioRoom.cs:           get Q3DAudioRoom values from inspector and handle it accordingly.
         |--Q3DAudioSource.cs:         get Q3DAudioSource attribute values from inspector and handle it accordingly.

===>Scripts
     |--PlanetMove.cs:                 mainly to make planets achieve rotation　and revolution.
     |--SwitchToMain.cs:               switch to main Scene.
     |--SwitchToNepture.cs:            switch to Nepture Scene.
     |--SwitchToUp.cs:                 switch to looking up Scene.
     |--ZoomInAndout.cs:               mainly to make gameobjects zoom in and out.

===>AudioMixer.mixer:an AudioMixer.

===>LookUpScene.unity: a Scene Perspective.

===>MainScene.unity:a Scene Perspective.

===>Mat.mat: material file.

===>NeptureScene.unity:a Scene Perspective.

Usage Instructions:

1、Download code from github according to the repository from　“https://github.com/ThunderSoft-XA/Solar-System-With-3D-Sound.git”.
2、install Unity, and any version of Unity before Unity 5 will not work with this product.
3、douuble click *.unity to open the demo.
4、compile them into APK and install it to android device.
5、open the APK, and you will enjoy it.
6、if you care about some performance indicators, you can use Snapdragon Profiler to view it.

