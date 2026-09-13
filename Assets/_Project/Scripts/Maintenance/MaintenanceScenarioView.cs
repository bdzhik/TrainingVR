using System.Collections;
using TMPro;
using UnityEngine;

namespace TrainingVR.Maintenance
{
    public sealed class MaintenanceScenarioView : MonoBehaviour
    {
        [SerializeField] private MaintenanceScenarioController scenario;
        [SerializeField] private TMP_Text currentTaskText;
        [SerializeField] private TMP_Text stepsText;
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField, Min(0f)] private float transientMessageDuration = 3f;

        private Coroutine messageRoutine;

        private void OnEnable()
        {
            if (scenario == null)
                return;

            scenario.StateChanged += OnStateChanged;
            scenario.FeedbackRaised += ShowMessage;
            OnStateChanged(scenario.CurrentState);
        }

        private void OnDisable()
        {
            if (scenario == null)
                return;

            scenario.StateChanged -= OnStateChanged;
            scenario.FeedbackRaised -= ShowMessage;
        }

        private void OnStateChanged(ScenarioState state)
        {
            if (currentTaskText != null)
                currentTaskText.text = GetStepMessage(state);

            if (stepsText != null)
                stepsText.text = BuildChecklist(state);

            if (feedbackText != null)
                feedbackText.text = string.Empty;
        }

        private void ShowMessage(string message)
        {
            var targetText = feedbackText != null ? feedbackText : currentTaskText;
            if (targetText == null)
                return;

            if (messageRoutine != null)
                StopCoroutine(messageRoutine);

            targetText.text = message;
            messageRoutine = StartCoroutine(KeepMessageVisible(targetText, message));
        }

        private IEnumerator KeepMessageVisible(TMP_Text targetText, string message)
        {
            yield return new WaitForSecondsRealtime(transientMessageDuration);

            if (targetText != null && targetText.text == message)
            {
                targetText.text = targetText == currentTaskText
                    ? GetStepMessage(scenario.CurrentState)
                    : string.Empty;
            }

            messageRoutine = null;
        }

        private static string GetStepMessage(ScenarioState state)
        {
            return state switch
            {
                ScenarioState.AwaitingPowerOff => "Отключите установку рубильником.",
                ScenarioState.AwaitingPartPickup => "Возьмите сменный автомат.",
                ScenarioState.AwaitingPartInstallation => "Установите автомат в крепление.",
                ScenarioState.AwaitingToolPickup => "Возьмите отвёртку.",
                ScenarioState.AwaitingToolUse =>
                    "Поместите наконечник отвёртки в сферу и удерживайте кнопку активации (T) 2 секунды.",
                ScenarioState.AwaitingPowerOn => "Включите установку рубильником.",
                ScenarioState.Completed => "Тренировка пройдена!",
                _ => string.Empty
            };
        }

        private static string BuildChecklist(ScenarioState state)
        {
            var currentStep = state switch
            {
                ScenarioState.AwaitingPowerOff => 0,
                ScenarioState.AwaitingPartPickup => 1,
                ScenarioState.AwaitingPartInstallation => 2,
                ScenarioState.AwaitingToolPickup => 3,
                ScenarioState.AwaitingToolUse => 4,
                ScenarioState.AwaitingPowerOn => 5,
                ScenarioState.Completed => 6,
                _ => 0
            };

            return string.Join("\n",
                FormatStep(0, currentStep, "Отключить питание"),
                FormatStep(1, currentStep, "Взять автомат"),
                FormatStep(2, currentStep, "Установить автомат"),
                FormatStep(3, currentStep, "Взять отвёртку"),
                FormatStep(4, currentStep, "Обработать два крепления"),
                FormatStep(5, currentStep, "Включить питание"));
        }

        private static string FormatStep(int step, int currentStep, string label)
        {
            if (step < currentStep)
                return $"<color=#59D98E>✓  {label}</color>";
            if (step == currentStep)
                return $"<color=#FFC857>●  {label}</color>";
            return $"<color=#9AA4B2>○  {label}</color>";
        }
    }
}
