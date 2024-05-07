using System;
using BaseLightSwitch.prefab;
using BepInEx;
using FMOD;
using static OVRPlugin;


namespace BaseLightSwitch
{

    [BepInPlugin(GUID, MODNAME, VERSION)]
    [BepInDependency("com.snmodding.nautilus", BepInDependency.DependencyFlags.HardDependency)]

    public class Main : BaseUnityPlugin
    {
        #region[Declarations]
        public const string
            MODNAME = "BaseLightSwitch",
            AUTHOR = "Cookie",
            GUID = AUTHOR + "_" + MODNAME,
            VERSION = "1.0.0.0";
        #endregion

        public void Awake()
		{
           
			BaseLightSwitchPrefab baseLightSwitchPrefab = new BaseLightSwitchPrefab();
            baseLightSwitchPrefab.SetGameObject(baseLightSwitchPrefab.loadedPrefab);
            baseLightSwitchPrefab.Register();
		}
	}
}
