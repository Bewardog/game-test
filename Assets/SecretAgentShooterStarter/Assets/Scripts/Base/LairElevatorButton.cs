using UnityEngine;

/// <summary>
/// A button for the lair elevator: a floor button inside the car, or a call
/// button on a landing that brings the car to that floor.
/// </summary>
public class LairElevatorButton : MonoBehaviour, IInteractable
{
    public LairElevator elevator;
    public int stopIndex;
    public bool isCallButton;

    public string InteractionPrompt
    {
        get
        {
            if (elevator == null) return string.Empty;
            if (isCallButton)
            {
                return elevator.CurrentStop == stopIndex && !elevator.IsBusy
                    ? "Open elevator"
                    : "Call elevator";
            }

            return $"Go to {elevator.GetStopName(stopIndex)}";
        }
    }

    public void Interact(SpyPlayerController player)
    {
        if (elevator == null) return;
        elevator.RequestStop(stopIndex, player);
    }
}
