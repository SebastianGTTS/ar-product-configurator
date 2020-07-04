using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace ARConfigurator
{
    /// <summary>
    /// Loads all the assets (prefabs, textures, etc.) as a <see cref="AssetBundle"/> via a web request.
    /// After loading, keeps loaded assets in memory for later instantiation.
    /// Also initializes the ConfigurationManager with the loaded feature model.
    /// </summary>
    public class AssetManager : MonoBehaviour
    {
        public string DatabaseLocation;

        private AssetBundle DatabaseAssetBundle;
        private Dictionary<long, GameObject> ElementPrefabs;
        private Dictionary<string, Texture2D> Textures;

        void Start()
        {
            StartCoroutine(LoadAssetBundle());
        }

        private IEnumerator LoadAssetBundle()
        {
            // Clear any cached versions.
            bool cacheCleared = Caching.ClearCache();
            if (!cacheCleared)
            {
                Debug.Log("Failed to clear cache. AssetBundle may not have been updated.");
            }

            // Fetch AssetBundle.
            using (var request = UnityWebRequestAssetBundle.GetAssetBundle(DatabaseLocation, 0))
            {
                yield return request.SendWebRequest();
                if (request.isHttpError || request.isNetworkError)
                {
                    Debug.Log("Web request failed.");
                }
                else
                {
                    DatabaseAssetBundle = DownloadHandlerAssetBundle.GetContent(request);
                }
            }

            // Downloading finished. Process AssetBundle.
            if (DatabaseAssetBundle == null)
            {
                Debug.Log("Failed to load database.");
                yield break;
            }
            else
            {
                // Load element prefabs.
                ElementPrefabs = new Dictionary<long, GameObject>();
                foreach (GameObject asset in DatabaseAssetBundle.LoadAllAssets<GameObject>())
                {
                    var elementMetadata = asset.GetComponent<ElementMetadata>();
                    if (elementMetadata != null)
                    {
                        ElementPrefabs.Add(elementMetadata.FeatureId, asset);
                    }
                }

                // Load materials.
                Textures = new Dictionary<string, Texture2D>();
                foreach (Texture2D texture in DatabaseAssetBundle.LoadAllAssets<Texture2D>())
                {
                    Textures.Add(texture.name, texture);
                }

                // Set up the Configuration Manager.
                var featureModelAsset = DatabaseAssetBundle.LoadAsset<TextAsset>("featuremodel.asset");
                FindObjectOfType<ConfigurationManager>().OnDatabaseLoaded(featureModelAsset.text);
            }
        }

        /// <summary>
        /// Returns a ready-to-place prefab for a particular physical feature.
        /// <param name="featureId">ID of the associated feature.</param>
        /// </summary>
        public GameObject GetPrefabForFeature(long featureId)
        {
            return ElementPrefabs[featureId];
        }

        /// <summary>
        /// Returns the texture by its name.
        /// <param name="textureName">Name of the texture as specified in the TextureFilename property of the material.</param>
        /// </summary>
        public Texture2D GetTextureByName(string textureName)
        {
            return Textures[textureName];
        }
    }
}
