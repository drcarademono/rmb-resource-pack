using System;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Utility;
using System.IO;
using FullSerializer;

namespace CustomRegionMaterials
{
    [Serializable]
    public class MaterialDefinition
    {
        public int archive;
        public int record;
        public int frame;
    }

    [Serializable]
    public class RegionMaterials
    {
        public string regionName;
        public MaterialDefinition[] defaultMaterials;
        public MaterialDefinition[] winterMaterials;
    }

    [Serializable]
    public class RegionMaterialSettings
    {
        public RegionMaterials[] regions;
    }

    [ImportedComponent]
    public class CustomRegionMaterials : MonoBehaviour
    {
        private RegionMaterialSettings regionMaterialSettings;
        private MeshRenderer meshRenderer;
        private static readonly fsSerializer _serializer = new fsSerializer();

        static Mod mod;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            Debug.Log("CustomRegionMaterialsMod: Init called.");
        }

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            Debug.Log($"[CustomRegionMaterials] Awake called for {gameObject.name}");
            LoadRegionMaterialSettings();
        }

        private void Start()
        {
            Debug.Log("[CustomRegionMaterials] Start called");
            UpdateMaterialBasedOnRegion();
        }

       private void LoadRegionMaterialSettings()
        {
            string cleanName = gameObject.name.Replace("(Clone)", "").Trim();
            Debug.Log($"[CustomRegionMaterials] Attempting to load JSON for '{cleanName}'");

            if (ModManager.Instance.TryGetAsset(cleanName + ".json", clone: false, out TextAsset jsonAsset))
            {
                string json = jsonAsset.text;
                Debug.Log($"[CustomRegionMaterials] JSON loaded successfully, contents: {json.Substring(0, Math.Min(json.Length, 500))}...");

                fsResult result = _serializer.TryDeserialize(fsJsonParser.Parse(json), ref regionMaterialSettings);
                if (!result.Succeeded)
                    Debug.LogError($"[CustomRegionMaterials] Deserialization failed: {result.FormattedMessages}");
                else
                    Debug.Log("[CustomRegionMaterials] Deserialization succeeded");
            }
            else
            {
                Debug.LogError("[CustomRegionMaterials] JSON file for material settings not found");
                regionMaterialSettings = new RegionMaterialSettings(); // Fallback to default
            }
        }

        private void UpdateMaterialBasedOnRegion()
        {
            bool isWinter = IsWinter(); // Determine if it's winter
            string currentRegionName = GameManager.Instance.PlayerGPS.CurrentRegionName;
            RegionMaterials regionMaterials = Array.Find(regionMaterialSettings.regions, r => r.regionName.Equals(currentRegionName, StringComparison.OrdinalIgnoreCase));

            MaterialDefinition[] definitions = isWinter ? regionMaterials?.winterMaterials : regionMaterials?.defaultMaterials;

            if (definitions == null || definitions.Length == 0)
            {
                Debug.LogError("[CustomRegionMaterials] No definitions found for the current region.");
                return;
            }

            Material[] selectedMaterials = LoadMaterialsFromDefinitions(definitions);

            if (selectedMaterials != null && selectedMaterials.Length > 0 && meshRenderer != null)
            {
                meshRenderer.materials = selectedMaterials;
            }
            else
            {
                Debug.LogError("[CustomRegionMaterials] No valid materials found for the current region.");
            }
        }

        private Material[] LoadMaterialsFromDefinitions(MaterialDefinition[] definitions)
        {
            if (definitions == null || definitions.Length == 0)
                return null;

            List<Material> materials = new List<Material>();
            foreach (var def in definitions)
            {
                Material loadedMaterial = null;
                Rect rectOut; // Required by GetMaterial but not necessarily used afterwards in this context

                // Get the MaterialReader instance from DaggerfallUnity
                MaterialReader materialReader = DaggerfallUnity.Instance.MaterialReader;

                // Attempt to use GetMaterial first
                loadedMaterial = materialReader.GetMaterial(def.archive, def.record, def.frame, 0, out rectOut, 0, false, false);

                if (loadedMaterial == null)
                {
                    // Fallback to TryImportMaterial if GetMaterial fails
                    if (TextureReplacement.TryImportMaterial(def.archive, def.record, def.frame, out loadedMaterial))
                    {
                        materials.Add(loadedMaterial);
                    }
                    else
                    {
                        Debug.LogWarning($"Could not load material for archive: {def.archive}, record: {def.record}, frame: {def.frame}");
                        materials.Add(null); // Optionally handle missing materials differently
                    }
                }
                else
                {
                    materials.Add(loadedMaterial); // Successfully loaded with GetMaterial
                }
            }
            return materials.ToArray();
        }

        private bool IsWinter()
        {
            DaggerfallDateTime now = DaggerfallUnity.Instance.WorldTime.Now;
            return now.SeasonValue == DaggerfallDateTime.Seasons.Winter;
        }
    }
}

