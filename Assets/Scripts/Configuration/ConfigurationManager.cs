using GoogleARCore;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ARConfigurator
{
    /// <summary>
    /// Manages and guides the product configuration process in accordance with the feature model.
    /// Responsible for placing product elements and maintaining the current configuration.
    /// </summary>
    public class ConfigurationManager : MonoBehaviour
    {
        public AssetManager AssetManager;
        public GUIController GUIController;
        public GameObject ManipulatorPrefab;
        private ModelInterpreter Interpreter;

        // These will be needed during element placement.
        private GameObject PlacementRoot;
        private Placement PlacementDirection;
        private TrackableHit FreePlacementHit;

        private double CurrentPrice = 0.0;
        private double PriceLimit = -1.0;

        private Anchor RootAnchor;
        private List<GameObject> PlacedElements;
        private Feature AppliedMaterial;


        /// <summary>
        /// Called by the AssetManager when the asset database has been successfully loaded.
        /// <param name="featureModelJson">The product's feature model in serialized JSON form.</param>
        /// </summary>
        public void OnDatabaseLoaded(string featureModelJson)
        {
            PlacedElements = new List<GameObject>();
            Interpreter = GetComponent<ModelInterpreter>();
            Interpreter.Init(featureModelJson);

            GUIController.ReadyGUI();
            GUIController.SetSelectableMaterials(Interpreter.GetAllMaterials());
        }

        /// <summary>
        /// Opens the feature selector to choose an element to be placed freely in the world.
        /// Called by PlaneTapListener whenever user taps a detected AR plane.
        /// </summary>
        public void PlaceFree(TrackableHit placementHit)
        {
            // Allow only the first element to be placed freely.
            // All other elements should be added by using the plus buttons.
            if (PlacedElements.Count > 0) { return; }

            FreePlacementHit = placementHit;
            PlacementRoot = null;

            GUIController.SetSelectableFeatures(Interpreter.GetAllPlaceable());
            GUIController.OpenFeatureSelector();
        }

        /// <summary>
        /// Opens the feature selector to choose an element to be placed to the left.
        /// </summary>
        public void PlaceLeft(GameObject rootElement)
        {
            PlacementRoot = rootElement;
            PlacementDirection = Placement.Left;

            var selectableFeatures = Interpreter.GetAllowedLeft(rootElement.GetComponent<ElementMetadata>().FeatureId);
            GUIController.SetSelectableFeatures(selectableFeatures);
            GUIController.OpenFeatureSelector();
        }

        /// <summary>
        /// Opens the feature selector to choose an element to be placed to the right.
        /// </summary>
        public void PlaceRight(GameObject rootElement)
        {
            PlacementRoot = rootElement;
            PlacementDirection = Placement.Right;

            var selectableFeatures = Interpreter.GetAllowedRight(rootElement.GetComponent<ElementMetadata>().FeatureId);
            GUIController.SetSelectableFeatures(selectableFeatures);
            GUIController.OpenFeatureSelector();
        }

        /// <summary>
        /// Opens the feature selector to choose an element to be placed above.
        /// </summary>
        public void PlaceAbove(GameObject rootElement)
        {
            PlacementRoot = rootElement;
            PlacementDirection = Placement.Upper;

            var selectableFeatures = Interpreter.GetAllowedAbove(rootElement.GetComponent<ElementMetadata>().FeatureId);
            GUIController.SetSelectableFeatures(selectableFeatures);
            GUIController.OpenFeatureSelector();
        }

        /// <summary>
        /// Called by the GUI Controller whenever the user selects a feature from the feature list.
        /// This is the second step of placing elements in the world. At this point, one of the <c>PlaceX</c> methods
        /// has set up either which element the new prefab will be placed next to, or saved a <see cref="TrackableHit"/> object
        /// which references where the user tapped an AR plane in the world.
        /// </summary>
        public void OnFeatureSelected(long featureId)
        {
            GameObject prefab = AssetManager.GetPrefabForFeature(featureId);
            GameObject placedElement = Instantiate(prefab, Vector3.zero, Quaternion.identity);

            // Position and rotation of the placed element. Will be decided based on placement strategy.
            var elementPosition = Vector3.zero;
            var elementRotation = Quaternion.identity;

            // If placement root is not set, we are placing the new element freely.
            // In this case position and rotation are those of the initial tap.
            if (PlacementRoot == null)
            {
                elementPosition = FreePlacementHit.Pose.position;
                elementRotation = FreePlacementHit.Pose.rotation;
                placedElement.transform.SetPositionAndRotation(elementPosition, elementRotation);
            }
            // If placement root is set, we are placing the new element next to it.
            else
            {
                var rootCollider = PlacementRoot.GetComponent<MeshCollider>();
                var elementCollider = placedElement.GetComponent<MeshCollider>();

                // Element rotation affects bounding box calculations,
                // we need to set it before computing the position.
                elementRotation = rootCollider.transform.rotation;
                placedElement.transform.rotation = elementRotation;

                switch (PlacementDirection)
                {
                    case Placement.Left:
                        elementPosition = rootCollider.transform.TransformPoint(Vector3.right *
                            (rootCollider.bounds.extents.x + elementCollider.bounds.extents.x));
                        break;
                    case Placement.Right:
                        elementPosition = rootCollider.transform.TransformPoint(Vector3.left *
                            (rootCollider.bounds.extents.x + elementCollider.bounds.extents.x));
                        break;
                    case Placement.Upper:
                        elementPosition = rootCollider.transform.TransformPoint(Vector3.up *
                            (rootCollider.bounds.size.y + 0.5f));
                        break;
                }

                placedElement.transform.position = elementPosition;
            }

            // Place the manipulator and set it as parent.
            var manipulator = Instantiate(ManipulatorPrefab, elementPosition, elementRotation);
            placedElement.transform.SetParent(manipulator.transform);

            // Post-processing.
            // First placed element needs additional rotation. Anchor must also be created.
            if (PlacementRoot == null)
            {
                // Compensate for the hitPose rotation facing away from the raycast (i.e. camera).
                placedElement.transform.Rotate(0, 180.0f, 0, Space.Self);

                if (RootAnchor == null)
                {
                    RootAnchor = FreePlacementHit.Trackable.CreateAnchor(FreePlacementHit.Pose);
                }
            }
            // Relatively positioned elements need to have their relations set.
            else
            {
                var rootController = PlacementRoot.GetComponent<ElementController>();
                var placedController = placedElement.GetComponent<ElementController>();
                switch (PlacementDirection)
                {
                    case Placement.Left:
                        rootController.CurrentLeftFeatureId = featureId;
                        placedController.CurrentRightFeatureId = rootController.GetFeatureId();
                        break;
                    case Placement.Right:
                        rootController.CurrentRightFeatureId = featureId;
                        placedController.CurrentLeftFeatureId = rootController.GetFeatureId();
                        break;
                    case Placement.Upper:
                        rootController.CurrentUpperFeatureId = featureId;
                        break;
                }
                PlacementRoot.GetComponentInParent<ElementSelectionManipulator>().Deselect();
            }

            manipulator.transform.parent = RootAnchor.transform;
            PlacedElements.Add(placedElement);

            CurrentPrice += prefab.GetComponent<ElementMetadata>().Price;

            PlacementRoot = null;
        }

        /// <summary>
        /// Called by the GUI Controller whenever the user selects a material from the list.
        /// Currently simply applies the material to every placed element in the scene.
        /// </summary>
        public void OnMaterialSelected(long materialFeatureId)
        {
            ElementController[] elementControllers = FindObjectsOfType<ElementController>();

            // Value of -1 represents "no texture". Reset all textures in the scene.
            if (materialFeatureId == -1)
            {
                foreach (ElementController controller in elementControllers)
                {
                    controller.RestoreTextures();
                    if (AppliedMaterial != null) CurrentPrice -= AppliedMaterial.Material.Price;
                }
                AppliedMaterial = null;
                return;
            }

            // Apply the texture to every placed element in the scene.
            Feature newMaterial = Interpreter.GetFeature(materialFeatureId);
            Texture2D texture = AssetManager.GetTextureByName(newMaterial.Material.TextureFilename);
            foreach (ElementController controller in elementControllers)
            {
                if (AppliedMaterial != null) CurrentPrice -= AppliedMaterial.Material.Price;
                CurrentPrice += newMaterial.Material.Price;
                controller.SetTexture(texture);
            }
            AppliedMaterial = newMaterial;
        }

        public double GetPriceLimit()
        {
            return PriceLimit;
        }

        public void SetPriceLimit(double limit)
        {
            PriceLimit = limit;
        }

        /// <summary>
        /// Validates the current product configuration and returns a human-readable report to be shown in the GUI.
        /// </summary>
        public string ValidateConfiguration()
        {
            string result = $"Validation Results{Environment.NewLine}{Environment.NewLine}";
            bool valid = true;
            List<long> placedFeatureIds = PlacedElements.ConvertAll(
                element => element.GetComponent<ElementMetadata>().FeatureId
                );

            // Check mandatory features.
            List<Feature> missingMandatoryFeatures = Interpreter.GetMandatoryFeatures();
            foreach (Feature mandatoryFeature in Interpreter.GetMandatoryFeatures())
            {
                foreach (Feature placeableDescendant in Interpreter.GetPhysicalSubfeatures(mandatoryFeature.Id))
                {
                    if (placedFeatureIds.Contains(placeableDescendant.Id))
                    {
                        missingMandatoryFeatures.Remove(mandatoryFeature);
                        break;
                    }
                }
            }
            if (missingMandatoryFeatures.Count > 0)
            {
                valid = false;
                foreach (Feature missingFeature in missingMandatoryFeatures)
                {
                    result += $"• Mandatory feature '{missingFeature.Name}' is not represented.{Environment.NewLine}";
                }
            }

            // Check XOR features.
            foreach (Feature xorFeature in Interpreter.GetXorFeatures())
            {
                var representedSubtrees = new List<List<Feature>>();
                var treeNames = new List<string>();
                foreach (Feature childFeature in xorFeature.Features)
                {
                    var placedDescendants = Interpreter.GetPhysicalSubfeatures(childFeature.Id)
                        .FindAll(feature => placedFeatureIds.Contains(feature.Id));
                    if (placedDescendants.Count > 0)
                    {
                        representedSubtrees.Add(placedDescendants);
                        treeNames.Add(childFeature.Name);
                    }
                }

                if (representedSubtrees.Count > 1)
                {
                    valid = false;
                    result += $"• {treeNames[0]} cannot be selected together with {treeNames[1]}.{Environment.NewLine}";

                    PlacedElements.Find(element => element.GetComponent<ElementMetadata>().FeatureId == representedSubtrees[0][0].Id)
                        .GetComponent<ElementController>().MarkAsInvalid();
                    PlacedElements.Find(element => element.GetComponent<ElementMetadata>().FeatureId == representedSubtrees[1][0].Id)
                        .GetComponent<ElementController>().MarkAsInvalid();
                }
            }

            // Check 'Requires'
            foreach (long placedId in placedFeatureIds)
            {
                Feature feature = Interpreter.GetFeature(placedId);
                if (feature.RequiringDependencyTo == null) continue;
                foreach (long requiredId in feature.RequiringDependencyTo)
                {
                    if (!placedFeatureIds.Contains(requiredId))
                    {
                        valid = false;
                        result += $"• {feature.Name} requires {Interpreter.GetFeature(requiredId).Name}.{Environment.NewLine}";
                        PlacedElements.Find(element => element.GetComponent<ElementMetadata>().FeatureId == placedId)
                            .GetComponent<ElementController>().MarkAsInvalid();
                    }
                }
            }

            // Check 'Excludes'
            var checkedIds = new List<long>();
            foreach (long placedId in placedFeatureIds)
            {
                Feature feature = Interpreter.GetFeature(placedId);
                checkedIds.Add(placedId);
                if (feature.ExcludingDependency == null) continue;
                foreach (long excludedId in feature.ExcludingDependency)
                {
                    if (placedFeatureIds.Contains(excludedId) && !checkedIds.Contains(excludedId))
                    {
                        valid = false;
                        result += $"• {feature.Name} and {Interpreter.GetFeature(excludedId).Name} are mutually exclusive.{Environment.NewLine}";

                        PlacedElements.Find(element => element.GetComponent<ElementMetadata>().FeatureId == placedId)
                            .GetComponent<ElementController>().MarkAsInvalid();
                        PlacedElements.Find(element => element.GetComponent<ElementMetadata>().FeatureId == excludedId)
                            .GetComponent<ElementController>().MarkAsInvalid();
                    }
                }
            }

            // Check price.
            if (PriceLimit > -1.0 && CurrentPrice > PriceLimit)
            {
                valid = false;
                result += $"• Current product price is {(CurrentPrice - PriceLimit)} above the set limit.{Environment.NewLine}";
            }


            if (valid)
            {
                result += "Your configuration is valid.";
            }

            return result;
        }
    }
}
