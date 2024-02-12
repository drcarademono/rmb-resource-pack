using System;
using UnityEngine;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallConnect.Arena2;using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility;

[Serializable]
public class ClimateMaterials
{
    public Material[] defaultMaterials;
    public Material[] winterMaterials;
}

[Serializable]
public class ClimateMaterialSettings
{
    [Tooltip("Fallback: Woodlands")]
    public ClimateMaterials ocean = new ClimateMaterials();

    [Tooltip("Fallback: Woodlands")]
    public ClimateMaterials desert = new ClimateMaterials();

    [Tooltip("Fallback: Desert")]
    public ClimateMaterials desert2 = new ClimateMaterials();

    [Tooltip("Fallback: Woodlands")]
    public ClimateMaterials mountain = new ClimateMaterials();

    [Tooltip("Fallback: Woodlands")]
    public ClimateMaterials rainforest = new ClimateMaterials();

    [Tooltip("Fallback: Rainforest")]
    public ClimateMaterials swamp = new ClimateMaterials();

    [Tooltip("Fallback: Desert")]
    public ClimateMaterials subtropical = new ClimateMaterials();

    [Tooltip("Fallback: Woodlands")]
    public ClimateMaterials mountainWoods = new ClimateMaterials();

    [Tooltip("Fallback: Woodlands")]
    public ClimateMaterials woodlands = new ClimateMaterials();

    [Tooltip("Fallback: Woodlands")]
    public ClimateMaterials hauntedWoodlands = new ClimateMaterials();
}


public class CustomRuntimeMaterials : MonoBehaviour
{
    [SerializeField]
    private ClimateMaterialSettings climateMaterialSettings = new ClimateMaterialSettings();

    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        UpdateMaterialBasedOnClimateAndSeason();
    }

    private void UpdateMaterialBasedOnClimateAndSeason()
    {
        MapsFile.Climates currentClimate = GetCurrentClimate();
        ClimateMaterials materialsForClimate = GetMaterialsForClimate(currentClimate);
        bool isWinter = IsWinter();

        Material[] selectedMaterials = isWinter ? materialsForClimate.winterMaterials : materialsForClimate.defaultMaterials;

        // Ensure the array matches the MeshRenderer's materials array length
        if (selectedMaterials != null && meshRenderer != null)
        {
            Material[] materialsToApply = new Material[meshRenderer.materials.Length];
            for (int i = 0; i < materialsToApply.Length; i++)
            {
                materialsToApply[i] = i < selectedMaterials.Length ? selectedMaterials[i] : null; // Use null or a default material as filler
            }
            meshRenderer.materials = materialsToApply;
        }
    }

    private ClimateMaterials GetMaterialsForClimate(MapsFile.Climates climate)
    {
        // Implement fallback logic here as previously discussed
        // This example directly returns the materials for simplicity
        return climateMaterialSettings.woodlands; // Placeholder for actual logic
    }

    private MapsFile.Climates GetCurrentClimate()
    {
        return (MapsFile.Climates)GameManager.Instance.PlayerGPS.CurrentClimateIndex;
    }

    private bool IsWinter()
    {
        DaggerfallDateTime now = DaggerfallUnity.Instance.WorldTime.Now;
        return now.SeasonValue == DaggerfallDateTime.Seasons.Winter;
    }
}

