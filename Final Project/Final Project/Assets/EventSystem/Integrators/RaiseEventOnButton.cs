using UnityEngine;
using UnityEngine.UI;

public class RaiseGameEventOnButtonClick : MonoBehaviour
{
    [Tooltip("The GameEvent to raise when the button is clicked.")]
    public GameEvent gameEvent;

    // This method will be called by the button
    public void RaiseEvent()
    {
        if (gameEvent != null)
        {
            gameEvent.Raise(); // Raise the event, triggering all listeners
        }
    }
}
