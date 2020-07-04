using GoogleARCore;
using GoogleARCore.Examples.ObjectManipulation;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ARConfigurator
{
    /// <summary>
    /// Listens for user's screen tap gestures and checks if the tap was against a detected plane.
    /// When a tap against a plane is detected, tells ConfigurationManager to try to place an element in the world.
    /// </summary>
    public class PlaneTapListener : Manipulator
    {
        public Camera FirstPersonCamera;

        protected override bool CanStartManipulationForGesture(TapGesture gesture)
        {
            if (gesture.TargetObject == null && !EventSystem.current.IsPointerOverGameObject(gesture.FingerId))
            {
                return true;
            }

            return false;
        }

        protected override void OnEndManipulation(TapGesture gesture)
        {
            if (gesture.WasCancelled)
            {
                return;
            }

            // If gesture is targeting an existing object we are done.
            if (gesture.TargetObject != null)
            {
                return;
            }

            // Raycast against the location the player touched to search for planes.
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon;

            if (Frame.Raycast(gesture.StartPosition.x, gesture.StartPosition.y, raycastFilter, out hit))
            {
                // Use hit pose and camera pose to check if hittest is from the back of the plane.
                if (hit.Trackable is DetectedPlane &&
                    Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position, hit.Pose.rotation * Vector3.up) < 0)
                {
                    Debug.Log("Hit at back of the current DetectedPlane");
                }
                // Only use upward horizontal planes.
                else if (hit.Trackable is DetectedPlane &&
                    ((DetectedPlane)hit.Trackable).PlaneType == DetectedPlaneType.HorizontalUpwardFacing)
                {
                    FindObjectOfType<ConfigurationManager>().PlaceFree(hit);
                }
                else
                {
                    Debug.Log("Hit on an unsupported plane.");
                }
            }
        }
    }
}
