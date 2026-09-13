using System;
using UnityEngine;

namespace TrainingVR.Maintenance
{
    public sealed class MaintenanceScenarioController : MonoBehaviour
    {
        private readonly MaintenanceStateMachine stateMachine = new();

        public ScenarioState CurrentState => stateMachine.State;

        public event Action<ScenarioState> StateChanged;
        public event Action<string> FeedbackRaised;
        public event Action Restarted;

        public bool CanPerform(ScenarioAction action)
        {
            return stateMachine.CanPerform(action);
        }

        public bool TryPerform(ScenarioAction action)
        {
            if (!CanPerform(action))
            {
                ReportRejected(action);
                return false;
            }

            stateMachine.TryAdvance(action);
            StateChanged?.Invoke(CurrentState);
            return true;
        }

        public void ReportRejected(ScenarioAction action)
        {
            FeedbackRaised?.Invoke(GetRejectionMessage(action));
        }

        public void ReportFeedback(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
                FeedbackRaised?.Invoke(message);
        }

        public void RestartScenario()
        {
            stateMachine.Reset();
            Restarted?.Invoke();
            StateChanged?.Invoke(CurrentState);
        }

        private string GetRejectionMessage(ScenarioAction attemptedAction)
        {
            if (CurrentState == ScenarioState.Completed)
                return "Сценарий уже завершён. Нажмите «Перезапустить», чтобы пройти его снова.";

            return CurrentState switch
            {
                ScenarioState.AwaitingPowerOff => "Сначала отключите установку рубильником.",
                ScenarioState.AwaitingPartPickup => "Возьмите сменный автомат.",
                ScenarioState.AwaitingPartInstallation when attemptedAction == ScenarioAction.PowerOff =>
                    "Питание уже отключено. Теперь установите сменную деталь.",
                ScenarioState.AwaitingPartInstallation => "Сначала установите сменную деталь.",
                ScenarioState.AwaitingToolPickup => "Возьмите отвёртку.",
                ScenarioState.AwaitingToolUse =>
                    "Поместите наконечник отвёртки в сферу и удерживайте кнопку активации 2 секунды.",
                ScenarioState.AwaitingPowerOn => "Работа завершена. Включите установку рубильником.",
                _ => "Действие выполнено в неправильном порядке."
            };
        }
    }
}
