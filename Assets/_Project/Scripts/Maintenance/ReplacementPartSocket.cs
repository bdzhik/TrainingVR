using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace TrainingVR.Maintenance
{
    [RequireComponent(typeof(XRSocketInteractor))]
    public sealed class ReplacementPartSocket : MonoBehaviour
    {
        [SerializeField] private MaintenanceScenarioController scenario;
        [SerializeField] private XRGrabInteractable replacementPart;
        [SerializeField, Min(0.1f)] private float feedbackCooldown = 1f;

        private XRSocketInteractor socket;
        private XRSelectFilterDelegate selectionFilter;
        private XRHoverFilterDelegate hoverFilter;
        private float nextFeedbackTime;
        private bool isInstalled;

        private void Awake()
        {
            socket = GetComponent<XRSocketInteractor>();
            selectionFilter = new XRSelectFilterDelegate(CanSelect);
            hoverFilter = new XRHoverFilterDelegate(CanHover);
        }

        private void OnEnable()
        {
            socket.hoverFilters.Add(hoverFilter);
            socket.selectFilters.Add(selectionFilter);
            socket.selectEntered.AddListener(OnPartSelected);

            if (scenario != null)
                scenario.Restarted += OnScenarioRestarted;
        }

        private void OnDisable()
        {
            socket.selectEntered.RemoveListener(OnPartSelected);
            socket.selectFilters.Remove(selectionFilter);
            socket.hoverFilters.Remove(hoverFilter);

            if (scenario != null)
                scenario.Restarted -= OnScenarioRestarted;
        }

        private bool CanHover(IXRHoverInteractor _, IXRHoverInteractable interactable)
        {
            return IsReplacementPart(interactable) &&
                   (isInstalled ||
                    scenario != null && scenario.CanPerform(ScenarioAction.PartInstalled));
        }

        private bool CanSelect(IXRSelectInteractor _, IXRSelectInteractable interactable)
        {
            if (!IsReplacementPart(interactable))
                return false;

            // Selection filters can be evaluated again after selectEntered. Keep the
            // installed part valid after the scenario advances to the next state.
            if (isInstalled)
                return true;

            if (scenario != null && scenario.CanPerform(ScenarioAction.PartInstalled))
                return true;

            if (scenario != null && Time.unscaledTime >= nextFeedbackTime)
            {
                nextFeedbackTime = Time.unscaledTime + feedbackCooldown;
                scenario.ReportRejected(ScenarioAction.PartInstalled);
            }

            return false;
        }

        private void OnPartSelected(SelectEnterEventArgs args)
        {
            if (!IsReplacementPart(args.interactableObject))
                return;

            isInstalled = true;
            if (scenario == null || !scenario.TryPerform(ScenarioAction.PartInstalled))
                isInstalled = false;
        }

        private void OnScenarioRestarted()
        {
            isInstalled = false;
        }

        private bool IsReplacementPart(IXRInteractable interactable)
        {
            return replacementPart != null &&
                   interactable != null &&
                   interactable.transform == replacementPart.transform;
        }
    }
}
