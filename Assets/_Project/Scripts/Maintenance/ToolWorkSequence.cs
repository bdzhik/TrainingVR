using System;
using UnityEngine;

namespace TrainingVR.Maintenance
{
    public sealed class ToolWorkSequence : MonoBehaviour
    {
        [SerializeField] private MaintenanceScenarioController scenario;
        [SerializeField] private ToolWorkZone[] zones = Array.Empty<ToolWorkZone>();

        public bool IsWorkStepActive =>
            scenario != null && scenario.CurrentState == ScenarioState.AwaitingToolUse;

        private void Awake()
        {
            foreach (var zone in zones)
            {
                if (zone != null)
                    zone.Initialize(this);
            }
        }

        private void OnEnable()
        {
            if (scenario == null)
                return;

            scenario.StateChanged += OnStateChanged;
            scenario.Restarted += ResetZones;
        }

        private void Start()
        {
            RefreshZones();
        }

        private void OnDisable()
        {
            if (scenario == null)
                return;

            scenario.StateChanged -= OnStateChanged;
            scenario.Restarted -= ResetZones;
        }

        internal void NotifyZoneCompleted(ToolWorkZone completedZone)
        {
            var completedCount = 0;
            var validZoneCount = 0;

            foreach (var zone in zones)
            {
                if (zone == null)
                    continue;

                validZoneCount++;
                if (zone.IsCompleted)
                    completedCount++;
            }

            if (validZoneCount > 0 && completedCount == validZoneCount)
            {
                scenario.TryPerform(ScenarioAction.ToolUseCompleted);
                return;
            }

            scenario.ReportFeedback(
                $"{completedZone.DisplayName} обработано. Осталось зон: {validZoneCount - completedCount}.");
        }

        private void OnStateChanged(ScenarioState _)
        {
            RefreshZones();
        }

        private void ResetZones()
        {
            foreach (var zone in zones)
            {
                if (zone != null)
                    zone.ResetZone();
            }
        }

        private void RefreshZones()
        {
            foreach (var zone in zones)
            {
                if (zone != null)
                    zone.RefreshGuidance();
            }
        }
    }
}
