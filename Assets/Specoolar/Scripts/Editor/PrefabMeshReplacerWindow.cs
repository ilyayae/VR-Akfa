// PrefabMeshReplacerWindow.cs
//
// Place this file inside a folder named "Editor" anywhere in your Assets
// (e.g. Assets/Editor/PrefabMeshReplacerWindow.cs).
//
// Usage:
//   1. Tools > Prefab Mesh Replacer
//   2. Assign the "Target Material" (the material used to find objects)
//   3. Assign the "Replacement Mesh"
//   4. Select one or more prefab assets in the Project window
//   5. Click "Process Selected Prefabs"
//
// For each selected prefab, the tool loads it via PrefabUtility.LoadPrefabContents
// (an isolated in-memory instance of the prefab), scans every MeshRenderer and
// SkinnedMeshRenderer in the hierarchy, and if any of their materials matches the
// target material, replaces the mesh on that renderer. Changes are saved back to
// the prefab asset and the temporary instance is unloaded.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PrefabMeshReplacerWindow : EditorWindow
{
    private Material targetMaterial;
    private Mesh replacementMesh;
    private bool includeInactive = true;

    [MenuItem("Tools/Prefab Mesh Replacer")]
    public static void ShowWindow()
    {
        var window = GetWindow<PrefabMeshReplacerWindow>("Prefab Mesh Replacer");
        window.minSize = new Vector2(360, 200);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Replace mesh on renderers that use a given material", EditorStyles.wordWrappedLabel);
        EditorGUILayout.Space();

        targetMaterial = (Material)EditorGUILayout.ObjectField(
            new GUIContent("Target Material", "Renderers using this material will have their mesh replaced."),
            targetMaterial, typeof(Material), false);

        replacementMesh = (Mesh)EditorGUILayout.ObjectField(
            new GUIContent("Replacement Mesh", "The mesh to assign to matching renderers."),
            replacementMesh, typeof(Mesh), false);

        includeInactive = EditorGUILayout.Toggle(
            new GUIContent("Include Inactive Objects", "Also search inactive children in the prefab hierarchy."),
            includeInactive);

        EditorGUILayout.Space();

        var selectedPrefabPaths = GetSelectedPrefabPaths();
        EditorGUILayout.LabelField($"Selected prefab assets: {selectedPrefabPaths.Count}");

        if (selectedPrefabPaths.Count > 0)
        {
            using (var scroll = new EditorGUILayout.ScrollViewScope(Vector2.zero, GUILayout.MaxHeight(100)))
            {
                foreach (var p in selectedPrefabPaths)
                    EditorGUILayout.LabelField(p, EditorStyles.miniLabel);
            }
        }

        EditorGUILayout.Space();

        bool canRun = targetMaterial != null && replacementMesh != null && selectedPrefabPaths.Count > 0;
        using (new EditorGUI.DisabledScope(!canRun))
        {
            if (GUILayout.Button("Process Selected Prefabs", GUILayout.Height(30)))
            {
                ProcessPrefabs(selectedPrefabPaths);
            }
        }

        if (!canRun)
        {
            EditorGUILayout.HelpBox(
                "Assign a target material, a replacement mesh, and select one or more prefab assets in the Project window.",
                MessageType.Info);
        }
    }

    private List<string> GetSelectedPrefabPaths()
    {
        var paths = new List<string>();
        foreach (var obj in Selection.objects)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && path.EndsWith(".prefab"))
                paths.Add(path);
        }
        return paths;
    }

    private void ProcessPrefabs(List<string> prefabPaths)
    {
        int prefabsModified = 0;
        int renderersModified = 0;

        try
        {
            for (int i = 0; i < prefabPaths.Count; i++)
            {
                string path = prefabPaths[i];
                bool cancelled = EditorUtility.DisplayCancelableProgressBar(
                    "Processing Prefabs", path, (float)i / prefabPaths.Count);
                if (cancelled) break;

                // Load the prefab into an isolated in-memory scene ("open it" programmatically).
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                bool changedThisPrefab = false;

                try
                {
                    var meshRenderers = root.GetComponentsInChildren<MeshRenderer>(includeInactive);
                    foreach (var mr in meshRenderers)
                    {
                        if (TryReplaceMesh(mr))
                        {
                            changedThisPrefab = true;
                            renderersModified++;
                        }
                    }

                    if (changedThisPrefab)
                    {
                        PrefabUtility.SaveAsPrefabAsset(root, path);
                        prefabsModified++;
                    }
                }
                finally
                {
                    // Always unload, whether or not we saved, to avoid leaking the temp scene.
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[Prefab Mesh Replacer] Modified {renderersModified} renderer(s) across {prefabsModified} prefab(s).");
    }

    private bool TryReplaceMesh(MeshRenderer meshRenderer)
    {
        if (!UsesTargetMaterial(meshRenderer.sharedMaterials)) return false;

        var bound = meshRenderer.localBounds;

        var meshFilter = meshRenderer.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == replacementMesh) return false;

        meshFilter.sharedMesh = replacementMesh;
        meshRenderer.transform.localScale = bound.extents * 2;
        meshRenderer.transform.localPosition += bound.center;
        EditorUtility.SetDirty(meshFilter);
        return true;
    }

    private bool UsesTargetMaterial(Material[] materials)
    {
        if (materials == null) return false;
        foreach (var mat in materials)
        {
            if (mat == targetMaterial) return true;
        }
        return false;
    }
}
