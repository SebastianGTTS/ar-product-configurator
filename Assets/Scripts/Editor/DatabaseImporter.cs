using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ARConfigurator
{
    /// <summary>
    /// Converts the reusable assets from the modeling stage (feature model, 3D models) into a <see cref="AssetBundle"/>
    /// that can be uploaded to a remote location and downloaded by the application at runtime as a single asset database.
    /// Only works inside Unity Editor.
    /// </summary>
    public class DatabaseImporter
    {
        [MenuItem("Product Configurator/Create Database Bundle")]
        static void ConvertToAssetBundle()
        {
            ImportDatabase(false);
        }

        [MenuItem("Product Configurator/Generate Assets Only")]
        static void ConvertToPrefabAssets()
        {
            ImportDatabase(true);
        }

        static void ImportDatabase(bool assetsOnly)
        {
            string inputDirectory = "Assets/Input";
            string bundleOutputDirectory = "Assets/Bundles";
            string prefabTempDirectory = "Assets/Prefabs/FromImporter";
            string bundleName = "elements.db";
            int errorCount = 0;

            if (!Directory.Exists(inputDirectory))
            {
                EditorUtility.DisplayDialog("Input Not Found", $"Input directory “{inputDirectory}” not found.", "Close");
                return;
            }

            // Load JSON file to process.
            var jsonAssets = new List<TextAsset>();
            foreach (string guid in AssetDatabase.FindAssets("t:TextAsset", new[] { inputDirectory }))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetExtension(assetPath).ToLower() == ".json")
                {
                    jsonAssets.Add(AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath));
                }
            }
            if (jsonAssets.Count == 0)
            {
                EditorUtility.DisplayDialog("No Files", $"No JSON files were found in “{inputDirectory}”.", "Close");
                return;
            }
            if (jsonAssets.Count > 1)
            {
                EditorUtility.DisplayDialog("Too Many Files", $"More than one JSON file found in “{inputDirectory}”.", "Close");
                return;
            }

            // Parse the database from JSON.
            var fm = FeatureModel.FromJson(jsonAssets[0].text);

            // Generate prefab assets that will represent placeable elements at runtime.
            if (!Directory.Exists(prefabTempDirectory))
            {
                Directory.CreateDirectory(prefabTempDirectory);
            }
            var generatedAssetPaths = new List<string>();
            foreach (Feature feature in fm.FeatureMap.Values)
            {
                // Skip features that cannot be turned into prefabs.
                if (!feature.IsPhysical || feature.Metadata == null)
                {
                    continue;
                }

                var prefab = new GameObject(feature.Name);
                var metadata = prefab.AddComponent<ElementMetadata>();
                metadata.Init(feature);

                // Attach 3D model.
                string[] modelFileGUIDs = AssetDatabase.FindAssets(metadata.ModelFilename, new[] { inputDirectory });
                if (modelFileGUIDs.Length == 0)
                {
                    Debug.Log($"Could not find 3D model with name: {metadata.ModelFilename}");
                    Object.DestroyImmediate(prefab);
                    errorCount++;
                    continue;
                }

                var modelAsset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(modelFileGUIDs[0])) as GameObject;
                if (modelAsset == null)
                {
                    Debug.Log($"Model {metadata.ModelFilename} could not be loaded.");
                    Object.DestroyImmediate(prefab);
                    errorCount++;
                    continue;
                }

                var model = Object.Instantiate(modelAsset, prefab.transform, false);
                if (model == null)
                {
                    Debug.Log($"Model {metadata.ModelFilename} could not be instantiated.");
                    Object.DestroyImmediate(prefab);
                    errorCount++;
                    continue;
                }
                model.name = "3D Model";

                // Generate a collider mesh.
                var submeshes = model.GetComponentsInChildren<MeshFilter>();
                var combine = new CombineInstance[submeshes.Length];
                for (int i = 0; i < submeshes.Length; i++)
                {
                    combine[i].mesh = submeshes[i].sharedMesh;
                    combine[i].transform = submeshes[i].transform.localToWorldMatrix;
                }
                var combinedMesh = new Mesh();
                combinedMesh.CombineMeshes(combine);
                prefab.AddComponent<ElementController>().SaveColliderMesh(combinedMesh);

                // Set AssetBundle label and save the prefab.
                bool saveSuccess;
                string prefabAssetPath = AssetDatabase.GenerateUniqueAssetPath($"{prefabTempDirectory}/{prefab.name}.prefab");
                GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(prefab, prefabAssetPath, out saveSuccess);
                if (prefabAsset == null || !saveSuccess)
                {
                    Debug.Log($"Prefab {prefab.name} could not be saved as prefab asset.");
                    Object.DestroyImmediate(prefab);
                    errorCount++;
                    continue;
                }
                AssetImporter.GetAtPath(prefabAssetPath).SetAssetBundleNameAndVariant(bundleName, "");
                generatedAssetPaths.Add(prefabAssetPath);

                Object.DestroyImmediate(prefab);
            }

            // Mark linked textures to be put into the bundle.
            foreach (Feature feature in fm.FeatureMap.Values)
            {
                if (!feature.IsMaterial || feature.Material == null)
                {
                    continue;
                }

                var textureFileGUIDs = AssetDatabase.FindAssets(feature.Material.TextureFilename, new[] { inputDirectory });
                if (textureFileGUIDs.Length == 0)
                {
                    Debug.Log($"Could not find texture with name: {feature.Material.TextureFilename}");
                    errorCount++;
                    continue;
                }

                var assetPath = AssetDatabase.GUIDToAssetPath(textureFileGUIDs[0]);
                AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(bundleName, "");
            }

            // Save the feature model as JSON asset.
            var featureModelJson = new TextAsset(jsonAssets[0].text);
            var featureModelSavePath = $"{prefabTempDirectory}/featuremodel.asset";
            AssetDatabase.CreateAsset(featureModelJson, featureModelSavePath);
            AssetImporter.GetAtPath(featureModelSavePath).SetAssetBundleNameAndVariant(bundleName, "");
            generatedAssetPaths.Add(featureModelSavePath);


            // If only prefabs were requested, we are done.
            if (assetsOnly) { return; }


            // Build the bundle.
            if (!Directory.Exists(bundleOutputDirectory))
            {
                Directory.CreateDirectory(bundleOutputDirectory);
            }
            BuildPipeline.BuildAssetBundles(bundleOutputDirectory, BuildAssetBundleOptions.None, BuildTarget.Android);

            // AssetBundle is now saved. Delete the generated content.
            foreach (string path in generatedAssetPaths)
            {
                bool deleteSuccess = AssetDatabase.DeleteAsset(path);
                if (!deleteSuccess)
                {
                    Debug.Log($"Failed to delete generated prefab asset at: {path}");
                    errorCount++;
                }
            }
            Directory.Delete(prefabTempDirectory, true);
            AssetDatabase.Refresh();

            if (errorCount > 0)
            {
                EditorUtility.DisplayDialog("Import Errors",
                                            (errorCount == 1 ? "There was an error" : $"There were {errorCount} errors") + " during import." +
                                            "\nSee console log for more information.",
                                            "Close");
            }
            else
            {
                EditorUtility.DisplayDialog("Import Complete",
                                            "Database was imported with no errors." +
                                            $"\nResults were saved to {bundleOutputDirectory}/{bundleName}",
                                            "Close");
            }
        }
    }
}
