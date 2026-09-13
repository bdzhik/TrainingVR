using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace TrainingVR.Maintenance
{
    [RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
    public sealed class ResettableGrabObject : MonoBehaviour
    {
        [SerializeField] private MaintenanceScenarioController scenario;

        private XRGrabInteractable interactable;
        private Rigidbody body;
        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private void Awake()
        {
            interactable = GetComponent<XRGrabInteractable>();
            body = GetComponent<Rigidbody>();
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        private void OnEnable()
        {
            if (scenario != null)
                scenario.Restarted += ResetPose;
        }

        private void OnDisable()
        {
            if (scenario != null)
                scenario.Restarted -= ResetPose;
        }

        private void ResetPose()
        {
            if (interactable.interactionManager != null)
                interactable.interactionManager.CancelInteractableSelection(
                    (UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable)interactable);

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.position = initialPosition;
            body.rotation = initialRotation;
        }
    }
}
