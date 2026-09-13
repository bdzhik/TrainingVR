using UnityEngine;

namespace TrainingVR.Maintenance
{
    public sealed class EquipmentIndicator : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int LegacyColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private MaintenanceScenarioController scenario;
        [SerializeField] private Renderer indicatorRenderer;
        [SerializeField, ColorUsage(false, true)] private Color poweredColor = new(1f, 0.03f, 0.03f, 1f);
        [SerializeField] private Color powerOffColor = new(0.25f, 0.25f, 0.25f, 1f);
        [SerializeField, ColorUsage(false, true)] private Color completedColor = new(0.05f, 1f, 0.2f, 1f);
        [SerializeField, Min(0f)] private float emissionIntensity = 2f;

        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            indicatorRenderer = indicatorRenderer != null ? indicatorRenderer : GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            if (scenario == null)
                return;

            scenario.StateChanged += ApplyState;
            ApplyState(scenario.CurrentState);
        }

        private void OnDisable()
        {
            if (scenario != null)
                scenario.StateChanged -= ApplyState;
        }

        private void ApplyState(ScenarioState state)
        {
            if (indicatorRenderer == null)
                return;

            var color = state switch
            {
                ScenarioState.AwaitingPowerOff => poweredColor,
                ScenarioState.Completed => completedColor,
                _ => powerOffColor
            };

            var emission = state == ScenarioState.AwaitingPowerOff || state == ScenarioState.Completed
                ? color * emissionIntensity
                : Color.black;

            indicatorRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(LegacyColorId, color);
            propertyBlock.SetColor(EmissionColorId, emission);
            indicatorRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
