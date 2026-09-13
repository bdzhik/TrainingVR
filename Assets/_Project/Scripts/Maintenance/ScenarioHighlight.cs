using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrainingVR.Maintenance
{
    public sealed class ScenarioHighlight : MonoBehaviour
    {
        [SerializeField] private MaintenanceScenarioController scenario;
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private Renderer[] targetRenderers = Array.Empty<Renderer>();
        [SerializeField] private ScenarioState[] activeStates = Array.Empty<ScenarioState>();

        private readonly Dictionary<Renderer, Material[]> originalMaterials = new();
        private bool isHighlighted;

        private void Awake()
        {
            if (targetRenderers.Length == 0)
                targetRenderers = GetComponentsInChildren<Renderer>(true);

            foreach (var targetRenderer in targetRenderers)
            {
                if (targetRenderer != null)
                    originalMaterials[targetRenderer] = targetRenderer.sharedMaterials;
            }
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

            SetHighlighted(false);
        }

        private void ApplyState(ScenarioState state)
        {
            for (var i = 0; i < activeStates.Length; i++)
            {
                if (activeStates[i] == state)
                {
                    SetHighlighted(true);
                    return;
                }
            }

            SetHighlighted(false);
        }

        private void SetHighlighted(bool value)
        {
            if (isHighlighted == value || outlineMaterial == null)
                return;

            isHighlighted = value;
            foreach (var pair in originalMaterials)
            {
                if (pair.Key == null)
                    continue;

                if (!value)
                {
                    pair.Key.sharedMaterials = pair.Value;
                    continue;
                }

                var highlightedMaterials = new Material[pair.Value.Length + 1];
                Array.Copy(pair.Value, highlightedMaterials, pair.Value.Length);
                highlightedMaterials[^1] = outlineMaterial;
                pair.Key.sharedMaterials = highlightedMaterials;
            }
        }
    }
}
