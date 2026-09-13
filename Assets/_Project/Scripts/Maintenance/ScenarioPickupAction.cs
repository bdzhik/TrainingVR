using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace TrainingVR.Maintenance
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public sealed class ScenarioPickupAction : MonoBehaviour
    {
        [SerializeField] private MaintenanceScenarioController scenario;
        [SerializeField] private ScenarioAction pickupAction;

        private XRGrabInteractable interactable;

        private void Awake()
        {
            interactable = GetComponent<XRGrabInteractable>();
        }

        private void OnEnable()
        {
            interactable.selectEntered.AddListener(OnPickedUp);
        }

        private void OnDisable()
        {
            interactable.selectEntered.RemoveListener(OnPickedUp);
        }

        private void OnPickedUp(SelectEnterEventArgs _)
        {
            if (scenario == null)
                return;

            if (pickupAction != ScenarioAction.PartPickedUp &&
                pickupAction != ScenarioAction.ToolPickedUp)
            {
                Debug.LogError($"{nameof(ScenarioPickupAction)} supports pickup actions only.", this);
                return;
            }

            scenario.TryPerform(pickupAction);
        }
    }
}
