using UnityEngine;
using UnityEditor;

public class MeshSimplifierTool
{
    [MenuItem("Assets/Simplify Selected Mesh")]
    public static void SimplifySelectedMesh()
    {
        Mesh sourceMesh = Selection.activeObject as Mesh;
        if (sourceMesh == null)
        {
            Debug.LogError("Select a Mesh asset in the Project window first (not the GameObject, the actual mesh sub-asset).");
            return;
        }

        float quality = 0.01f; // keep ~1% of original triangles - adjust if needed

        var simplifier = new UnityMeshSimplifier.MeshSimplifier();
        simplifier.Initialize(sourceMesh);
        simplifier.SimplifyMesh(quality);

        Mesh simplified = simplifier.ToMesh();
        simplified.name = sourceMesh.name + "_Simplified";

        string path = "Assets/" + simplified.name + ".asset";
        AssetDatabase.CreateAsset(simplified, path);
        AssetDatabase.SaveAssets();

        Debug.Log("Simplified mesh saved to " + path + " — original tris: "
            + (sourceMesh.triangles.Length / 3) + ", new tris: "
            + (simplified.triangles.Length / 3));
    }
}