using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace TrainingVR.Maintenance
{
    [RequireComponent(typeof(XRSimpleInteractable))]
    public sealed class PowerLever : MonoBehaviour
    {
        [SerializeField] private MaintenanceScenarioController scenario;
        [SerializeField] private Transform handle;
        [SerializeField] private Vector3 offRotationOffset = new(0f, 0f, -55f);

        private XRSimpleInteractable interactable;
        private Quaternion initialRotation;
        private bool isOff;

        private void Awake()
        {
            interactable = GetComponent<XRSimpleInteractable>();
            handle = handle != null ? handle : transform;
            initialRotation = handle.localRotation;
        }

        private void OnEnable()
        {
            interactable.selectEntered.AddListener(OnSelected);
            if (scenario != null)
                scenario.Restarted += ResetLever;
        }

        private void OnDisable()
        {
            interactable.selectEntered.RemoveListener(OnSelected);
            if (scenario != null)
                scenario.Restarted -= ResetLever;
        }

        private void OnSelected(SelectEnterEventArgs _)
        {
            if (scenario == null)
                return;

            var action = isOff ? ScenarioAction.PowerOn : ScenarioAction.PowerOff;
            if (!scenario.TryPerform(action))
                return;

            isOff = !isOff;
            handle.localRotation = isOff
                ? initialRotation * Quaternion.Euler(offRotationOffset)
                : initialRotation;
        }

        private void ResetLever()
        {
            isOff = false;
            handle.localRotation = initialRotation;
        }
    }
}
