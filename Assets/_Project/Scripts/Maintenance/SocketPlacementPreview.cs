using System;
using System.Collections.Generic;
using UnityEngine;

namespace TrainingVR.Maintenance
{
    public sealed class SocketPlacementPreview : MonoBehaviour
    {
        [SerializeField] private MaintenanceScenarioController scenario;
        [SerializeField] private Transform placementAnchor;
        [SerializeField] private Transform sourceRoot;
        [SerializeField] private Renderer[] sourceRenderers = Array.Empty<Renderer>();
        [SerializeField] private Material previewMaterial;

        [Header("Preview pose correction")]
        [SerializeField] private Vector3 localPositionOffset;
        [SerializeField] private Vector3 localRotationOffset;
        [SerializeField] private Vector3 localScaleMultiplier = Vector3.one;

        private readonly List<Mesh> generatedMeshes = new();
        private GameObject previewRoot;

        private void Awake()
        {
            placementAnchor = placementAnchor != null ? placementAnchor : transform;
            CreatePreview();
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

            if (previewRoot != null)
                previewRoot.SetActive(false);
        }

        private void OnDestroy()
        {
            foreach (var generatedMesh in generatedMeshes)
            {
                if (generatedMesh != null)
                    Destroy(generatedMesh);
            }
        }

        private void ApplyState(ScenarioState state)
        {
            if (previewRoot != null)
                previewRoot.SetActive(state == ScenarioState.AwaitingPartInstallation);
        }

        private void CreatePreview()
        {
            if (previewMaterial == null || sourceRoot == null)
            {
                Debug.LogWarning("Socket placement preview is not configured.", this);
                return;
            }

            if (sourceRenderers.Length == 0)
                sourceRenderers = sourceRoot.GetComponentsInChildren<Renderer>(true);

            previewRoot = new GameObject("CircuitBreakerPlacementPreview");
            previewRoot.transform.SetParent(placementAnchor, false);
            previewRoot.transform.localPosition = localPositionOffset;
            previewRoot.transform.localRotation = Quaternion.Euler(localRotationOffset);
            previewRoot.transform.localScale = localScaleMultiplier;

            foreach (var sourceRenderer in sourceRenderers)
                CreateRendererCopy(sourceRenderer);

            previewRoot.SetActive(false);
        }

        private void CreateRendererCopy(Renderer sourceRenderer)
        {
            if (sourceRenderer == null)
                return;

            Mesh mesh;
            if (sourceRenderer is SkinnedMeshRenderer skinnedRenderer)
            {
                mesh = new Mesh { name = $"{skinnedRenderer.name}_PlacementPreview" };
                skinnedRenderer.BakeMesh(mesh);
                generatedMeshes.Add(mesh);
            }
            else
            {
                var sourceFilter = sourceRenderer.GetComponent<MeshFilter>();
                if (sourceFilter == null || sourceFilter.sharedMesh == null)
                    return;

                mesh = sourceFilter.sharedMesh;
            }

            var rendererObject = new GameObject($"{sourceRenderer.name}_Preview");
            var rendererTransform = rendererObject.transform;
            rendererTransform.SetParent(previewRoot.transform, false);
            CopyRelativeTransform(sourceRenderer.transform, rendererTransform);

            rendererObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            var rendererCopy = rendererObject.AddComponent<MeshRenderer>();
            rendererCopy.sharedMaterials = CreateMaterialArray(mesh.subMeshCount);
            rendererCopy.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rendererCopy.receiveShadows = false;
        }

        private void CopyRelativeTransform(Transform source, Transform destination)
        {
            destination.localPosition = sourceRoot.InverseTransformPoint(source.position);
            destination.localRotation = Quaternion.Inverse(sourceRoot.rotation) * source.rotation;
            destination.localScale = Divide(source.lossyScale, sourceRoot.lossyScale);
        }

        private Material[] CreateMaterialArray(int count)
        {
            var materials = new Material[Mathf.Max(1, count)];
            for (var i = 0; i < materials.Length; i++)
                materials[i] = previewMaterial;
            return materials;
        }

        private static Vector3 Divide(Vector3 value, Vector3 divisor)
        {
            return new Vector3(
                SafeDivide(value.x, divisor.x),
                SafeDivide(value.y, divisor.y),
                SafeDivide(value.z, divisor.z));
        }

        private static float SafeDivide(float value, float divisor)
        {
            return Mathf.Abs(divisor) > Mathf.Epsilon ? value / divisor : value;
        }
    }
}
