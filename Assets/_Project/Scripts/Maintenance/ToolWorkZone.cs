using System.Collections.Generic;
using UnityEngine;

namespace TrainingVR.Maintenance
{
    [RequireComponent(typeof(Collider))]
    public sealed class ToolWorkZone : MonoBehaviour
    {
        [SerializeField] private string displayName = "Крепление";
        [SerializeField] private GameObject guidanceVisual;

        private readonly Dictionary<MaintenanceTool, int> toolTipColliderCounts = new();
        private ToolWorkSequence sequence;

        public string DisplayName => displayName;
        public bool IsCompleted { get; private set; }
        public bool IsAvailable => sequence != null && sequence.IsWorkStepActive && !IsCompleted;

        internal void Initialize(ToolWorkSequence owner)
        {
            sequence = owner;
            ResetZone();
        }

        internal bool TryComplete()
        {
            if (!IsAvailable)
                return false;

            IsCompleted = true;
            SetGuidanceVisible(false);
            sequence.NotifyZoneCompleted(this);
            return true;
        }

        internal void ResetZone()
        {
            IsCompleted = false;
            RefreshGuidance();
        }

        internal void RefreshGuidance()
        {
            SetGuidanceVisible(IsAvailable);
        }

        private void OnTriggerEnter(Collider other)
        {
            var toolTip = other.GetComponentInParent<MaintenanceToolTip>();
            var tool = toolTip != null ? toolTip.Tool : null;
            if (tool == null)
                return;

            toolTipColliderCounts.TryGetValue(tool, out var count);
            toolTipColliderCounts[tool] = count + 1;
            if (count == 0)
                tool.EnterWorkZone(this);
        }

        private void OnTriggerExit(Collider other)
        {
            var toolTip = other.GetComponentInParent<MaintenanceToolTip>();
            var tool = toolTip != null ? toolTip.Tool : null;
            if (tool == null || !toolTipColliderCounts.TryGetValue(tool, out var count))
                return;

            if (count > 1)
            {
                toolTipColliderCounts[tool] = count - 1;
                return;
            }

            toolTipColliderCounts.Remove(tool);
            tool.ExitWorkZone(this);
        }

        private void OnDisable()
        {
            foreach (var tool in toolTipColliderCounts.Keys)
                tool.ExitWorkZone(this);

            toolTipColliderCounts.Clear();
        }

        private void SetGuidanceVisible(bool value)
        {
            if (guidanceVisual != null && guidanceVisual.activeSelf != value)
                guidanceVisual.SetActive(value);
        }
    }
}
