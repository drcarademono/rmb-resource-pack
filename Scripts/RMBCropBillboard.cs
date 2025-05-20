using UnityEngine;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Utility;

[ImportedComponent]
public class RMBCropBillboard : MonoBehaviour
{
    private Camera mainCamera;
    static Mod mod;
    public bool FaceY = false; // Set this based on whether you want the object to rotate around Y axis only
    private MeshRenderer meshRenderer;
    private static GameObject modGameObject;
    static bool snowlessModEnabled;

    [Invoke(StateManager.StateTypes.Start, 0)]
    public static void Init(InitParams initParams)
    {
        mod = initParams.Mod;
        modGameObject = new GameObject(mod.Title);
        modGameObject.AddComponent<RMBCropBillboard>();
        //Debug.Log("RMBCropBillboard: Init called and component added to game object.");

        var snowlessMod1 = ModManager.Instance.GetModFromGUID("4f7f8aa1-7bd8-4f33-bd02-bbb5ac758a5d");
        var snowlessMod2 = ModManager.Instance.GetModFromGUID("510e24c8-8fc4-44c0-8927-8786b5bd0fe4");
        snowlessModEnabled = (snowlessMod1 != null && snowlessMod1.Enabled)
                          || (snowlessMod2 != null && snowlessMod2.Enabled);
    }

    void Awake()
    {
        if (gameObject.name == "RMB Resource Pack")
        {
            return; // Skip loading materials for this game object.
        }
        meshRenderer = GetComponent<MeshRenderer>();
        if (mainCamera == null)
        {
            mainCamera = Camera.main; // Automatically find the main camera if not assigned
        }
        ApplyMaterialBasedOnName();
    }

    void Update()
    {
        if (mainCamera && Application.isPlaying && meshRenderer.enabled)
        {
            float y = FaceY ? mainCamera.transform.forward.y : 0;
            Vector3 viewDirection = -new Vector3(mainCamera.transform.forward.x, y, mainCamera.transform.forward.z);
            transform.LookAt(transform.position + viewDirection);
        }
    }

    void OnDestroy()
    {
        // Clean up resources when the game object is destroyed
        if (modGameObject != null)
        {
            Destroy(modGameObject);
            modGameObject = null;
        }
    }

    private void ApplyMaterialBasedOnName()
    {
        string name = gameObject.name;
        //Debug.Log($"Checking object with name: {name}");
        if (name.StartsWith("DaggerfallBillboard"))
        {
            //Debug.Log("Object is a Daggerfall billboard.");
            string[] parts = name.Split(new[] { "__" }, System.StringSplitOptions.None);
            if (parts.Length >= 3)
            {
                MeshFilter meshFilter = GetComponent<MeshFilter>();
                if (meshFilter == null)
                {
                    Debug.LogError("MeshFilter component not found on the billboard object.");
                    return;
                }

                // Get the original height before applying new mesh
                float originalHeight = meshFilter.mesh ? meshFilter.mesh.bounds.size.y : 0;

                int archive;
                int record;
                if (IsWinter())
                {
                    archive = 511; // Use winter archive
                    record = 22;  // Use winter record
                }
                else
                {
                    archive = 301; // Default archive
                    int initialRecord = int.Parse(parts[2].Split('_')[1]);
                    record = AdjustRecordBasedOnClimate(initialRecord);
                }
                //Debug.Log($"Using archive: {archive}, record: {record} based on season and climate.");

                MaterialReader materialReader = DaggerfallUnity.Instance.MaterialReader;
                MeshReader meshReader = DaggerfallUnity.Instance.MeshReader;
                Rect rectOut;
                Vector2 sizeOut;
                Material material = materialReader.GetMaterial(archive, record, 0, 0, out rectOut, 4, true, true);
                Mesh mesh = meshReader.GetBillboardMesh(rectOut, archive, record, out sizeOut);

                if (material != null && mesh != null)
                {
                    Renderer renderer = GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        // Ensure alpha transparency settings
                        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.SetInt("_ZWrite", 0);
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.EnableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

                        // Force the renderer to use the transparent shader
                        material.shader = Shader.Find("Transparent/Diffuse");

                        renderer.material = material;
                        meshFilter.mesh = mesh;
                        //Debug.Log("Material and mesh successfully applied to renderer.");

                        // Force the renderer to update
                        renderer.enabled = false;
                        renderer.enabled = true;

                        // Calculate new height and adjust position to align the base
                        float newHeight = mesh.bounds.size.y;
                        float heightDifference = newHeight - originalHeight;
                        AlignToBase(transform.position.y + heightDifference / 2);
                    }
                    else
                    {
                        Debug.LogError("Renderer component not found on the billboard object.");
                    }
                }
                else
                {
                    Debug.LogError($"Failed to load material or mesh for archive: {archive}, record: {record}");
                }
            }
            else
            {
                Debug.LogError("Object name does not have enough parts to extract material info.");
            }
        }
        else
        {
            Debug.Log("Object name does not start with 'DaggerfallBillboard' and will not be processed.");
        }
    }

    private int AdjustRecordBasedOnClimate(int initialRecord)
    {
        MapsFile.Climates currentClimate = GetCurrentClimate();
        switch (currentClimate)
        {
            case MapsFile.Climates.Mountain:
                return initialRecord == 0 ? 0 : 1;
            case MapsFile.Climates.Desert:
                return 2;
            case MapsFile.Climates.Desert2:
                return 20;
            case MapsFile.Climates.Subtropical:
                return initialRecord == 0 ? 3 : 4;
            case MapsFile.Climates.Rainforest:
            case MapsFile.Climates.Swamp:
                return initialRecord == 0 ? 7 : 8;
            default:
                return initialRecord == 0 ? 19 : 21; // No change for Temperate, MountainWoods, HauntedWoodlands
        }
    }

    private void AlignToBase(float newYPosition)
    {
        Vector3 newPosition = transform.position;
        newPosition.y = newYPosition; // Update to align base properly
        transform.position = newPosition;
        //Debug.Log("Billboard aligned to maintain base position.");
    }

    private MapsFile.Climates GetCurrentClimate()
    {
        if (GameManager.Instance == null || GameManager.Instance.PlayerGPS == null || GameManager.Instance.PlayerObject == null)
        {
            return (MapsFile.Climates)231; // Return default climate as 231
        }
        return (MapsFile.Climates)GameManager.Instance.PlayerGPS.CurrentClimateIndex;
    }

    private bool IsWinter()
    {
        if (GameManager.Instance?.PlayerGPS == null) return false;
        var now = DaggerfallUnity.Instance.WorldTime.Now;
        int c = GameManager.Instance.PlayerGPS.CurrentClimateIndex;

        return now.SeasonValue == DaggerfallDateTime.Seasons.Winter
            && c != (int)MapsFile.Climates.Desert
            && c != (int)MapsFile.Climates.Desert2
            && c != (int)MapsFile.Climates.Subtropical
            && (!snowlessModEnabled 
                || (c != (int)MapsFile.Climates.Rainforest 
                 && c != (int)MapsFile.Climates.Swamp));
    }
}

