using NUnit.Framework;

namespace TrainingVR.Maintenance.Tests
{
    public sealed class MaintenanceStateMachineTests
    {
        private MaintenanceStateMachine stateMachine;

        [SetUp]
        public void SetUp()
        {
            stateMachine = new MaintenanceStateMachine();
        }

        [Test]
        public void NewScenario_AwaitsPowerOff()
        {
            Assert.That(stateMachine.State, Is.EqualTo(ScenarioState.AwaitingPowerOff));
        }

        [Test]
        public void CorrectSequence_CompletesScenario()
        {
            Assert.That(stateMachine.TryAdvance(ScenarioAction.PowerOff), Is.True);
            Assert.That(stateMachine.TryAdvance(ScenarioAction.PartPickedUp), Is.True);
            Assert.That(stateMachine.TryAdvance(ScenarioAction.PartInstalled), Is.True);
            Assert.That(stateMachine.TryAdvance(ScenarioAction.ToolPickedUp), Is.True);
            Assert.That(stateMachine.TryAdvance(ScenarioAction.ToolUseCompleted), Is.True);
            Assert.That(stateMachine.TryAdvance(ScenarioAction.PowerOn), Is.True);
            Assert.That(stateMachine.State, Is.EqualTo(ScenarioState.Completed));
        }

        [TestCase(ScenarioAction.PartInstalled)]
        [TestCase(ScenarioAction.PartPickedUp)]
        [TestCase(ScenarioAction.ToolPickedUp)]
        [TestCase(ScenarioAction.ToolUseCompleted)]
        [TestCase(ScenarioAction.PowerOn)]
        public void ActionBeforePowerOff_IsRejected(ScenarioAction action)
        {
            Assert.That(stateMachine.TryAdvance(action), Is.False);
            Assert.That(stateMachine.State, Is.EqualTo(ScenarioState.AwaitingPowerOff));
        }

        [Test]
        public void ToolCompletion_AwaitsPowerOn()
        {
            stateMachine.TryAdvance(ScenarioAction.PowerOff);
            stateMachine.TryAdvance(ScenarioAction.PartPickedUp);
            stateMachine.TryAdvance(ScenarioAction.PartInstalled);
            stateMachine.TryAdvance(ScenarioAction.ToolPickedUp);
            stateMachine.TryAdvance(ScenarioAction.ToolUseCompleted);

            Assert.That(stateMachine.State, Is.EqualTo(ScenarioState.AwaitingPowerOn));
        }

        [Test]
        public void Reset_ReturnsToFirstStep()
        {
            stateMachine.TryAdvance(ScenarioAction.PowerOff);
            stateMachine.Reset();

            Assert.That(stateMachine.State, Is.EqualTo(ScenarioState.AwaitingPowerOff));
        }
    }
}
