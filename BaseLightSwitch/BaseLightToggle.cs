using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BaseLightSwitch
{
    public class BaseLightToggle : HandTarget, IHandTarget, IProtoEventListener
    {
        public bool IsLightsOn;
        private FieldInfo _isLightsOnField;
        private SubRoot _lightSwitchSubRoot;

        public BaseLightToggle()
        {
            // Initialize default light state (ON)
            this.IsLightsOn = true;
            // Get light state field
            this._isLightsOnField = typeof(SubRoot).GetField("subLightsOn", BindingFlags.Instance | BindingFlags.NonPublic);
            // Initialize temporary variable to null (needed by invoked function "RestoreLightState")
            this._lightSwitchSubRoot = null;
        }

        /// <summary>Returns the path to save folder.</summary>
        private string GetLightSwitchSavePath() => Path.Combine(Path.Combine(@"./SNAppData/SavedGames/", SaveLoadManager.main.GetCurrentSlot()), "BaseLightSwitch");

        /// <summary>Returns the SubRoot object (if light switch is in a base, the BaseRoot is casted into a SubRoot).</summary>
        public SubRoot GetSubRoot()
        {
            SubRoot subRoot = GetComponentInParent<SubRoot>(); // Try get SubRoot (if light switch is in a submarine)
            if (subRoot == null) subRoot = gameObject?.transform?.parent?.GetComponent<SubRoot>(); // Try get SubRoot from gameObject's parent (if light switch is in a submarine)
            if (subRoot == null) subRoot = GetComponentInParent<BaseRoot>(); // Try get SubRoot from BaseRoot (if light switch is in a base)
            if (subRoot == null) subRoot = gameObject?.transform?.parent?.GetComponent<BaseRoot>();  // Try get SubRoot from gameObject's parent BaseRoot (if light switch is in a base)
            return subRoot;
        }

        /// <summary>Gets called upon HandClick event.</summary>
        /// <param name="hand">The hand that triggered the click event.</param>
        public void OnHandClick(GUIHand hand)
        {
            if (!enabled) return;

            // Get light switch SubRoot
            var subRoot = GetSubRoot();
            if (subRoot == null) return; // Return if light switch is not in a base or in a submarine

            // Get light switch Constructable
            var constructable = GetComponent<Constructable>();
            if (constructable == null || !constructable.constructed) return; // Return if light switch has not been built

            // Get current light state
            var isLightsOn = (bool)this._isLightsOnField.GetValue(subRoot);

            // Set new light state
            this.IsLightsOn = !isLightsOn;
            subRoot.ForceLightingState(this.IsLightsOn);

            // Play sound (depending on new light state). Scraped from : https://github.com/K07H/DecorationsMod/blob/master/Subnautica_AudioAssets.txt
            if (this.IsLightsOn)
                FMODUWE.PlayOneShot(new FMODAsset() { id = "2103", path = "event:/sub/cyclops/lights_on", name = "5384ec29-f493-4ac1-9f74-2c0b14d61440", hideFlags = HideFlags.None }, MainCamera.camera.transform.position, 1f);
            else
                FMODUWE.PlayOneShot(new FMODAsset() { id = "2102", path = "event:/sub/cyclops/lights_off", name = "95b877e8-2ccd-451d-ab5f-fc654feab173", hideFlags = HideFlags.None }, MainCamera.camera.transform.position, 1f);
        }

        /// <summary>Gets called upon HandHover event.</summary>
        /// <param name="hand">The hand that triggered the hover event.</param>
        public void OnHandHover(GUIHand hand)
        {
            if (!enabled)
                return;

            var reticle = HandReticle.main;
            reticle.SetIcon(HandReticle.IconType.Hand, 1f);
            reticle.SetTextRaw(HandReticle.TextType.Hand, "ToggleLightsBase");
        }

        /// <summary>This function gets called by <see cref="OnProtoDeserialize(ProtobufSerializer)"/> when game loads. It restores light state of current base or submarine.</summary>
        public void RestoreLightState()
        {
            if (_lightSwitchSubRoot != null)
                _lightSwitchSubRoot.ForceLightingState(this.IsLightsOn);
        }

        /// <summary>Gets called when game loads.</summary>
        /// <param name="serializer">The Protobuf serializer.</param>
        public void OnProtoDeserialize(ProtobufSerializer serializer)
        {
            // Get light switch prefab ID
            var prefabId = GetComponent<PrefabIdentifier>();
            if (prefabId == null) return; // Return if we were not able to get unique prefab ID

            // Get save folder path
            string saveFolderPath = GetLightSwitchSavePath();
            if (!Directory.Exists(saveFolderPath)) return; // Return if save folder does not exist

            // Get save file path
            string saveFilePath = Path.Combine(saveFolderPath, "lightswitch_" + prefabId.Id + ".txt");
            if (!File.Exists(saveFilePath)) return; // Return if there's no data saved for this light switch

            // Read saved data
            string savedData = File.ReadAllText(saveFilePath, Encoding.UTF8);

            // Restore light state based on saved data
            if (!string.IsNullOrEmpty(savedData) && savedData == "0")
                this.IsLightsOn = false;
            else
                this.IsLightsOn = true;

            // Get light switch SubRoot
            _lightSwitchSubRoot = GetSubRoot();

            // If light switch SubRoot was found
            if (_lightSwitchSubRoot != null)
                Invoke("RestoreLightState", 5.0f); // Restore light state in 5 seconds (we add a small delay because the cyclops needs few frames to complete instantiation)
        }

        /// <summary>Gets called when game saves.</summary>
        /// <param name="serializer">The Protobuf serializer.</param>
        public void OnProtoSerialize(ProtobufSerializer serializer)
        {
            // Get light switch prefab ID (or add one if there isn't)
            var prefabId = GetComponent<PrefabIdentifier>();
            if (prefabId == null && gameObject != null)
            {
                prefabId = gameObject.AddComponent<PrefabIdentifier>();
                prefabId.ClassId = "LightSwitch";
            }
            if (prefabId == null) return; // Return if we were not able to get or set unique prefab ID

            // Prepare save path
            string saveFolderPath = GetLightSwitchSavePath();
            if (!Directory.Exists(saveFolderPath))
                Directory.CreateDirectory(saveFolderPath);

            // Save light switch state
            File.WriteAllText(Path.Combine(saveFolderPath, "lightswitch_" + prefabId.Id + ".txt"), IsLightsOn ? "1" : "0", Encoding.UTF8);
        }
    }
}