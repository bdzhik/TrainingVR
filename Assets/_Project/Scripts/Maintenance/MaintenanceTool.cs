using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace TrainingVR.Maintenance
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public sealed class MaintenanceTool : MonoBehaviour
    {
        [SerializeField] private MaintenanceScenarioController scenario;
        [SerializeField, Min(0.1f)] private float activationDuration = 2f;

        private readonly HashSet<ToolWorkZone> workZones = new();
        private XRGrabInteractable interactable;
        private ToolWorkZone activeZone;
        private float elapsedTime;
        private bool activationHeld;

        private void Awake()
        {
            interactable = GetComponent<XRGrabInteractable>();
        }

        private void OnEnable()
        {
            interactable.activated.AddListener(OnActivated);
            interactable.deactivated.AddListener(OnDeactivated);
            if (scenario != null)
                scenario.Restarted += ResetTool;
        }

        private void OnDisable()
        {
            interactable.activated.RemoveListener(OnActivated);
            interactable.deactivated.RemoveListener(OnDeactivated);
            if (scenario != null)
                scenario.Restarted -= ResetTool;
            ResetProgress();
        }

        private void Update()
        {
            if (!activationHeld)
                return;

            if (activeZone == null ||
                !workZones.Contains(activeZone) ||
                !activeZone.IsAvailable ||
                scenario == null ||
                !scenario.CanPerform(ScenarioAction.ToolUseCompleted))
            {
                ResetProgress();
                return;
            }

            elapsedTime += Time.deltaTime;

            if (elapsedTime < activationDuration)
                return;

            var completedZone = activeZone;
            ResetProgress();
            completedZone.TryComplete();
        }

        public void EnterWorkZone(ToolWorkZone zone)
        {
            if (zone != null)
                workZones.Add(zone);
        }

        public void ExitWorkZone(ToolWorkZone zone)
        {
            workZones.Remove(zone);
            if (activeZone == zone)
                ResetProgress();
        }

        private void OnActivated(ActivateEventArgs _)
        {
            if (scenario == null)
                return;

            if (!scenario.CanPerform(ScenarioAction.ToolUseCompleted))
            {
                scenario.ReportRejected(ScenarioAction.ToolUseCompleted);
                return;
            }

            activeZone = FindAvailableZone();
            if (activeZone == null)
            {
                scenario.ReportFeedback(workZones.Count > 0
                    ? "Эта рабочая точка уже обработана. Перейдите к подсвеченной зоне."
                    : "Поместите наконечник отвёртки в подсвеченную рабочую зону.");
                return;
            }

            activationHeld = true;
        }

        private void OnDeactivated(DeactivateEventArgs _)
        {
            ResetProgress();
        }

        private void ResetTool()
        {
            workZones.Clear();
            ResetProgress();
        }

        private ToolWorkZone FindAvailableZone()
        {
            foreach (var zone in workZones)
            {
                if (zone != null && zone.IsAvailable)
                    return zone;
            }

            return null;
        }

        private void ResetProgress()
        {
            activationHeld = false;
            activeZone = null;
            elapsedTime = 0f;
        }
    }
}
