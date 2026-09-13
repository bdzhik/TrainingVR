# Maintenance scenario architecture

The scenario uses a small finite-state machine because the assignment has a strict, linear workflow.
Adding a separate State class for every step would add ceremony without improving extensibility here.

## Responsibilities

- `MaintenanceStateMachine` contains the pure domain rules and has no Unity dependency.
- `MaintenanceScenarioController` coordinates the scenario and publishes state, feedback, progress, and reset events.
- `PowerLever`, `ReplacementPartSocket`, and `MaintenanceTool` adapt XR Interaction Toolkit events into scenario actions.
- `ToolWorkZone` only detects whether a tool is inside the valid work area.
- `MaintenanceScenarioView` observes the controller and updates presentation.
- `ScenarioHighlight` displays an outline only for objects relevant to the current state.
- `EquipmentIndicator` maps scenario power states to red, gray, and green indication.
- `ScenarioNavigation` owns restart and Addressables-based menu navigation.
- `ResettableGrabObject` restores movable scene objects after restart.

This keeps domain rules independent from XR input and UI (SRP/DIP), uses Observer for presentation and reset notifications,
and uses a finite-state machine to reject invalid action order.

## Scene setup

Components are assigned manually in the Inspector. This keeps the scene configuration explicit and avoids editor code
that depends on object names.
