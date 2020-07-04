using GoogleARCore;
using GoogleARCore.Examples.ObjectManipulation;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ARConfigurator
{
    public class ElementSelectionManipulator : Manipulator
    {
        public Color SelectedElementColor;
        public GameObject PlusButtonPrefab;

        private Camera MainCamera;
        private GameObject LeftPlusButton;
        private GameObject RightPlusButton;
        private GameObject UpperPlusButton;

        private ConfigurationManager Configurator;
        private ElementController ElementController;
        private ElementMetadata ElementMetadata;

        void Start()
        {
            Configurator = FindObjectOfType<ConfigurationManager>();
            ElementController = GetComponentInChildren<ElementController>();
            ElementMetadata = GetComponentInChildren<ElementMetadata>();

            // Set up buttons.
            MainCamera = Camera.main;

            if (!ElementMetadata.LeftSlot.Contains(-1))
            {
                LeftPlusButton = Instantiate(PlusButtonPrefab);
                LeftPlusButton.transform.parent = transform;
                LeftPlusButton.name = "Left Plus Button";
                LeftPlusButton.GetComponentInChildren<Canvas>().worldCamera = MainCamera;
                LeftPlusButton.GetComponentInChildren<Button>().onClick.AddListener(() => ButtonClicked(Placement.Left));
                LeftPlusButton.SetActive(false);
            }

            if (!ElementMetadata.RightSlot.Contains(-1))
            {
                RightPlusButton = Instantiate(PlusButtonPrefab);
                RightPlusButton.transform.parent = transform;
                RightPlusButton.name = "Right Plus Button";
                RightPlusButton.GetComponentInChildren<Canvas>().worldCamera = MainCamera;
                RightPlusButton.GetComponentInChildren<Button>().onClick.AddListener(() => ButtonClicked(Placement.Right));
                RightPlusButton.SetActive(false);
            }

            if (!ElementMetadata.UpperSlot.Contains(-1))
            {
                UpperPlusButton = Instantiate(PlusButtonPrefab);
                UpperPlusButton.transform.parent = transform;
                UpperPlusButton.name = "Upper Plus Button";
                UpperPlusButton.GetComponentInChildren<Canvas>().worldCamera = MainCamera;
                UpperPlusButton.GetComponentInChildren<Button>().onClick.AddListener(() => ButtonClicked(Placement.Upper));
                UpperPlusButton.SetActive(false);
            }
        }

        protected override bool CanStartManipulationForGesture(TapGesture gesture)
        {
            // Don't start selection gesture if the user tapped a UI element.
            if (EventSystem.current.IsPointerOverGameObject(gesture.FingerId))
            {
                return false;
            }

            return true;
        }

        protected override void OnEndManipulation(TapGesture gesture)
        {
            if (gesture.WasCancelled)
            {
                return;
            }

            if (ManipulationSystem.Instance == null)
            {
                return;
            }

            if (gesture.TargetObject == gameObject)
            {
                Select();
            }

            // Raycast against the location the player touched to search for planes.
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon;
            if (!Frame.Raycast(gesture.StartPosition.x, gesture.StartPosition.y, raycastFilter, out hit))
            {
                Deselect();
            }
        }

        protected override void OnSelected()
        {
            // Colorize the selected element.
            ElementController.Colorize(SelectedElementColor);

            // Display the plus buttons around the element.
            // Element prefabs are rotated 180° upon placement, so we need to swap left and right vectors.
            var collider = GetComponentInChildren<MeshCollider>();

            if (LeftPlusButton != null && !ElementController.IsLeftSlotOccupied())
            {
                LeftPlusButton.transform.position = collider.transform.TransformPoint(
                    Vector3.right * (collider.bounds.extents.x + 0.25f) +
                    Vector3.up * collider.bounds.extents.y);
                LeftPlusButton.transform.rotation = collider.transform.rotation;
                LeftPlusButton.SetActive(true);
            }

            if (RightPlusButton != null && !ElementController.IsRightSlotOccupied())
            {
                RightPlusButton.transform.position = collider.transform.TransformPoint(
                    Vector3.left * (collider.bounds.extents.x + 0.25f) +
                    Vector3.up * collider.bounds.extents.y);
                RightPlusButton.transform.rotation = collider.transform.rotation;
                RightPlusButton.SetActive(true);
            }

            if (UpperPlusButton != null && !ElementController.IsUpperSlotOccupied())
            {
                UpperPlusButton.transform.position = collider.transform.TransformPoint(Vector3.up * (collider.bounds.size.y + 0.3f));
                UpperPlusButton.transform.rotation = collider.transform.rotation;
                UpperPlusButton.SetActive(true);
            }
        }

        protected override void OnDeselected()
        {
            // Restore original color of the element
            ElementController.RestorePreviousColor();

            // Disable the buttons
            if (LeftPlusButton != null) LeftPlusButton.SetActive(false);
            if (RightPlusButton != null) RightPlusButton.SetActive(false);
            if (UpperPlusButton != null) UpperPlusButton.SetActive(false);
        }

        private void ButtonClicked(Placement position)
        {
            switch (position)
            {
                case Placement.Left:
                    Configurator.PlaceLeft(ElementController.gameObject);
                    break;
                case Placement.Right:
                    Configurator.PlaceRight(ElementController.gameObject);
                    break;
                case Placement.Upper:
                    Configurator.PlaceAbove(ElementController.gameObject);
                    break;
            }
        }
    }
}
