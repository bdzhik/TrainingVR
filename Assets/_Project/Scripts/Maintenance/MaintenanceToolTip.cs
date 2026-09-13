using UnityEngine;

namespace TrainingVR.Maintenance
{
    [RequireComponent(typeof(Collider))]
    public sealed class MaintenanceToolTip : MonoBehaviour
    {
        [SerializeField] private MaintenanceTool tool;

        public MaintenanceTool Tool => tool;

        private void Awake()
        {
            tool = tool != null ? tool : GetComponentInParent<MaintenanceTool>();
            if (tool == null)
                Debug.LogError("MaintenanceToolTip must be a child of MaintenanceTool.", this);
        }
    }
}
