using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace TrainingVR.Maintenance
{
    public sealed class AddressableSceneLoader : MonoBehaviour
    {
        [SerializeField] private AssetReference sceneReference;
        [SerializeField] private GameObject loadingIndicator;
        [SerializeField] private RectTransform mouseClickArea;

        private InputActionManager[] disabledInputManagers;
        private bool isLoading;

        private void Update()
        {
            if (isLoading || mouseClickArea == null)
                return;

            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasReleasedThisFrame)
                return;

            if (EventSystem.current?.currentInputModule is not XRUIInputModule xrInputModule ||
                xrInputModule.enableMouseInput)
            {
                return;
            }

            var canvas = mouseClickArea.GetComponentInParent<Canvas>();
            var eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera != null ? canvas.worldCamera : Camera.main
                : null;

            if (RectTransformUtility.RectangleContainsScreenPoint(
                    mouseClickArea,
                    mouse.position.ReadValue(),
                    eventCamera))
            {
                Load();
            }
        }

        public void Load()
        {
            if (isLoading)
                return;

            if (sceneReference == null || !sceneReference.RuntimeKeyIsValid())
            {
                Debug.LogError("Addressable scene is not assigned.", this);
                return;
            }

            isLoading = true;
            if (loadingIndicator != null)
                loadingIndicator.SetActive(true);

            disabledInputManagers = SceneTransitionInputGuard.DisableManagersIn(gameObject.scene);
            SceneTransitionInputGuard.RebindSimulatorAfterNextSceneLoad();

            Debug.Log($"Loading addressable scene '{sceneReference.RuntimeKey}'...", this);

            var operation = Addressables.LoadSceneAsync(
                sceneReference,
                LoadSceneMode.Single,
                activateOnLoad: true);
            operation.Completed += OnLoadCompleted;
        }

        private void OnLoadCompleted(AsyncOperationHandle<SceneInstance> operation)
        {
            if (operation.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log($"Addressable scene '{sceneReference.RuntimeKey}' loaded successfully.", this);
                return;
            }

            isLoading = false;
            if (loadingIndicator != null)
                loadingIndicator.SetActive(false);

            SceneTransitionInputGuard.EnableManagers(disabledInputManagers);
            disabledInputManagers = null;
            SceneTransitionInputGuard.CancelPendingSimulatorRebind();

            Debug.LogException(operation.OperationException, this);
            Addressables.Release(operation);
        }
    }

    internal static class SceneTransitionInputGuard
    {
        public static InputActionManager[] DisableManagersIn(Scene scene)
        {
            var allManagers = Object.FindObjectsByType<InputActionManager>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);
            var managersInScene = new System.Collections.Generic.List<InputActionManager>();

            foreach (var manager in allManagers)
            {
                if (manager != null && manager.enabled && manager.gameObject.scene == scene)
                {
                    manager.enabled = false;
                    managersInScene.Add(manager);
                }
            }

            return managersInScene.ToArray();
        }

        public static void EnableManagers(InputActionManager[] managers)
        {
            if (managers == null)
                return;

            foreach (var manager in managers)
            {
                if (manager != null)
                    manager.enabled = true;
            }
        }

        public static void RebindSimulatorAfterNextSceneLoad()
        {
            SceneManager.sceneLoaded -= RebindSimulator;
            SceneManager.sceneLoaded += RebindSimulator;
        }

        public static void CancelPendingSimulatorRebind()
        {
            SceneManager.sceneLoaded -= RebindSimulator;
        }

        private static void RebindSimulator(Scene _, LoadSceneMode __)
        {
            SceneManager.sceneLoaded -= RebindSimulator;

            var simulators = Object.FindObjectsByType<XRInteractionSimulator>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            foreach (var simulator in simulators)
            {
                if (simulator == null)
                    continue;

                simulator.enabled = false;
                simulator.cameraTransform = null;
                simulator.leftControllerTransform = null;
                simulator.rightControllerTransform = null;
                simulator.leftHandAimTransform = null;
                simulator.rightHandAimTransform = null;
                simulator.enabled = true;
            }
        }
    }
}
