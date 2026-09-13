using UnityEngine;
using UnityEngine.SceneManagement;

namespace TrainingVR.Maintenance
{
    public sealed class ScenarioNavigation : MonoBehaviour
    {
        [SerializeField] private MaintenanceScenarioController scenario;
        [SerializeField] private string menuSceneName = "Menu";

        public void Restart()
        {
            scenario?.RestartScenario();
        }

        public void ReturnToMenu()
        {
            if (Application.CanStreamedLevelBeLoaded(menuSceneName))
            {
                SceneTransitionInputGuard.DisableManagersIn(gameObject.scene);
                SceneTransitionInputGuard.RebindSimulatorAfterNextSceneLoad();
                SceneManager.LoadScene(menuSceneName);
                return;
            }

            scenario?.ReportFeedback($"Сцена '{menuSceneName}' не добавлена в Player Scene List.");
        }
    }
}
