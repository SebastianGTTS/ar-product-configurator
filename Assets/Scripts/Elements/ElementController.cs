using GoogleARCore;
using System.Collections.Generic;
using UnityEngine;

namespace ARConfigurator
{
    /// <summary>
    /// Controls the behaviour of a single placed element at runtime, such as applying/resetting colors and textures.
    /// Also constructs a mesh collider upon instantiation.
    /// </summary>
    public class ElementController : MonoBehaviour
    {
        // These have to be public because they need to be serialized.
        // Only used internally to reconstruct the collider mesh on instantiation.
        [HideInInspector] public Vector2[] ColliderMeshUV;
        [HideInInspector] public Vector3[] ColliderMeshVertices;
        [HideInInspector] public int[] ColliderMeshTriangles;

        private ElementMetadata ElementMetadata;
        public long CurrentLeftFeatureId = -1;
        public long CurrentRightFeatureId = -1;
        public long CurrentUpperFeatureId = -1;

        private Stack<Color> ColorHistory;
        private Renderer[] ElementRenderers;
        private Dictionary<int, Color> OriginalColors;
        private Dictionary<int, Texture> OriginalTextures;

        private bool IsColliding = false;

        // Note: initialization logic HAS to happen inside Awake instead of Start
        // Otherwise, the collider won't have enough time to instantiate when element is placed.
        void Awake()
        {
            ElementMetadata = GetComponent<ElementMetadata>();

            ElementRenderers = GetComponentsInChildren<Renderer>();
            ColorHistory = new Stack<Color>();
            OriginalColors = new Dictionary<int, Color>();
            OriginalTextures = new Dictionary<int, Texture>();
            foreach (Renderer renderer in ElementRenderers)
            {
                foreach (Material material in renderer.materials)
                {
                    OriginalColors.Add(material.GetInstanceID(), material.color);
                    OriginalTextures.Add(material.GetInstanceID(), material.mainTexture);
                }
            }

            // Rebuild the collider mesh.
            var colliderMesh = new Mesh();
            colliderMesh.vertices = ColliderMeshVertices;
            colliderMesh.triangles = ColliderMeshTriangles;
            colliderMesh.uv = ColliderMeshUV;
            colliderMesh.RecalculateNormals();
            colliderMesh.RecalculateBounds();

            // Add a mesh collider to the prefab.
            var collider = gameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = colliderMesh;
            collider.convex = true;
            collider.isTrigger = true;

            // Add physics to the element
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        public long GetFeatureId()
        {
            return ElementMetadata.FeatureId;
        }

        public void Colorize(Color color)
        {
            foreach (Renderer renderer in ElementRenderers)
            {
                foreach (Material material in renderer.materials)
                {
                    material.color = color;
                }
            }
            ColorHistory.Push(color);
        }

        public void RestorePreviousColor()
        {
            if (ColorHistory.Count > 1)
            {
                ColorHistory.Pop();
                Color lastColor = ColorHistory.Peek();
                foreach (Renderer renderer in ElementRenderers)
                {
                    foreach (Material material in renderer.materials)
                    {
                        material.color = lastColor;
                    }
                }
            }
            else
            {
                if (ColorHistory.Count == 1) ColorHistory.Pop();
                foreach (Renderer renderer in ElementRenderers)
                {
                    foreach (Material material in renderer.materials)
                    {
                        material.color = OriginalColors[material.GetInstanceID()];
                    }
                }
            }

        }

        public void SetTexture(Texture2D texture)
        {
            foreach (Renderer renderer in ElementRenderers)
            {
                foreach (Material material in renderer.materials)
                {
                    material.mainTexture = texture;
                }
            }
        }

        public void RestoreTextures()
        {
            foreach (Renderer renderer in ElementRenderers)
            {
                foreach (Material material in renderer.materials)
                {
                    material.mainTexture = OriginalTextures[material.GetInstanceID()];
                }
            }
        }

        public void MarkAsInvalid()
        {
            Colorize(Color.red);
        }

        public bool IsLeftSlotOccupied()
        {
            return CurrentLeftFeatureId > -1;
        }

        public bool IsRightSlotOccupied()
        {
            return CurrentRightFeatureId > -1;
        }

        public bool IsUpperSlotOccupied()
        {
            return CurrentUpperFeatureId > -1;
        }

        private void OnTriggerEnter(Collider otherCollider)
        {
            var visualizer = otherCollider.gameObject.GetComponent<SolidPlaneVisualizer>();
            if (!IsColliding && visualizer != null && visualizer.GetPlaneType() == DetectedPlaneType.Vertical)
            {
                Colorize(Color.yellow);
                IsColliding = true;
            }
        }

        private void OnTriggerExit(Collider otherCollider)
        {
            var visualizer = otherCollider.gameObject.GetComponent<SolidPlaneVisualizer>();
            if (IsColliding && visualizer != null && visualizer.GetPlaneType() == DetectedPlaneType.Vertical)
            {
                RestorePreviousColor();
                IsColliding = false;
            }
        }

#if UNITY_EDITOR
        // This method is only used during database import.
        public void SaveColliderMesh(Mesh colliderMesh)
        {
            ColliderMeshVertices = colliderMesh.vertices;
            ColliderMeshTriangles = colliderMesh.triangles;
            ColliderMeshUV = colliderMesh.uv;
        }
#endif
    }
}
