using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Nautilus.Assets;
using Nautilus.Crafting;
using Nautilus.Utility;
using UnityEngine;
using System.Diagnostics.CodeAnalysis;
using static CraftData;
using Nautilus.Assets.Gadgets;
using static RootMotion.FinalIK.RagdollUtility;

namespace BaseLightSwitch.prefab
{
    public class BaseLightSwitchPrefab : CustomPrefab
    {
        private AssetBundle modAB;
        public GameObject loadedPrefab;
        [SetsRequiredMembers]
        public BaseLightSwitchPrefab() : base("LightSwitch", "Light Switch", "A light switch that toggles the base's lighting state.", RamuneLib.Utils.ImageUtils.GetSprite("LightIcon"))
        {
            this.LoadAssetBundle();
            this.SetPdaGroupCategory(TechGroup.Miscellaneous, TechCategory.Misc).SetBuildable();
            this.SetRecipe(new(new Ingredient(TechType.Titanium, 1)));
            this.SetUnlock(TechType.Peeper);
            SetGameObject(loadedPrefab);
            Register();
        }

        public void LoadAssetBundle()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uriBuilder = new UriBuilder(codeBase);
            string text = Path.Combine(Path.GetDirectoryName(Uri.UnescapeDataString(uriBuilder.Path)), "lightswitch.assets");
            this.modAB = AssetBundle.LoadFromFile(text);
            if (this.modAB == null)
            {
                throw new Exception("Light Switch AssetBundle not found! Path: " + text);
            }
            GameObject gameObject = this.modAB.LoadAsset<GameObject>("LightSwitch");
            if (gameObject != null)
            {
                Nautilus.Utility.PrefabUtils.AddBasicComponents(gameObject, "LightSwitch", TechType.None, LargeWorldEntity.CellLevel.Near);
                Nautilus.Utility.MaterialUtils.ApplySNShaders(gameObject);

                Constructable constructable = gameObject.AddComponent<Constructable>();
                constructable.allowedOnWall = true;
                constructable.allowedOnGround = false;
                constructable.allowedInSub = true;
                constructable.allowedInBase = true;
                constructable.allowedOnCeiling = false;
                constructable.allowedOutside = false;
                constructable.techType = base.Info.TechType;

                Transform modelTransform = gameObject.transform.Find("model");
                if (modelTransform != null)
                {
                    Transform lightSwitchTransform = modelTransform.Find("LIGHTSWITCH");
                    if (lightSwitchTransform != null)
                    {
                        constructable.model = lightSwitchTransform.gameObject;
                    }
                }

                ConstructableBounds constructableBounds = gameObject.AddComponent<ConstructableBounds>();
                TechTag techTag = gameObject.AddComponent<TechTag>();
                techTag.type = base.Info.TechType;

                BoxCollider collider = gameObject.EnsureComponent<BoxCollider>();
                collider.size = Vector3.one;

                BoxCollider mainModelCollider = gameObject.EnsureComponent<BoxCollider>();
                mainModelCollider.size = new Vector3(0.1f, 0.1f, 0.1f);

                Rigidbody component = gameObject.GetComponent<Rigidbody>();
                UnityEngine.Object.DestroyImmediate(component);

                gameObject.AddComponent<BaseModuleLighting>();
                gameObject.AddComponent<BaseLightToggle>();

                this.loadedPrefab = gameObject;
            }
            else
            {
                throw new Exception("Light Switch GameObject not found in the AssetBundle!");
            }

            this.modAB.Unload(false);
        }
    }
}
