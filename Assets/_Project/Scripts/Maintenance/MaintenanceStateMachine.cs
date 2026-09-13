namespace TrainingVR.Maintenance
{
    public sealed class MaintenanceStateMachine
    {
        public ScenarioState State { get; private set; } = ScenarioState.AwaitingPowerOff;

        public bool CanPerform(ScenarioAction action)
        {
            return State switch
            {
                ScenarioState.AwaitingPowerOff => action == ScenarioAction.PowerOff,
                ScenarioState.AwaitingPartPickup => action == ScenarioAction.PartPickedUp,
                ScenarioState.AwaitingPartInstallation => action == ScenarioAction.PartInstalled,
                ScenarioState.AwaitingToolPickup => action == ScenarioAction.ToolPickedUp,
                ScenarioState.AwaitingToolUse => action == ScenarioAction.ToolUseCompleted,
                ScenarioState.AwaitingPowerOn => action == ScenarioAction.PowerOn,
                _ => false
            };
        }

        public bool TryAdvance(ScenarioAction action)
        {
            if (!CanPerform(action))
                return false;

            State = State switch
            {
                ScenarioState.AwaitingPowerOff => ScenarioState.AwaitingPartPickup,
                ScenarioState.AwaitingPartPickup => ScenarioState.AwaitingPartInstallation,
                ScenarioState.AwaitingPartInstallation => ScenarioState.AwaitingToolPickup,
                ScenarioState.AwaitingToolPickup => ScenarioState.AwaitingToolUse,
                ScenarioState.AwaitingToolUse => ScenarioState.AwaitingPowerOn,
                ScenarioState.AwaitingPowerOn => ScenarioState.Completed,
                _ => State
            };

            return true;
        }

        public void Reset()
        {
            State = ScenarioState.AwaitingPowerOff;
        }
    }
}
