using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CustomGameEvent : UnityEvent<Component, object> {}

public class GameEventListener : MonoBehaviour
{

    [Tooltip("Event to register with.")]
    public GameEvent gameEvent;

    [Tooltip("Response to invoke when Event with GameData is raised.")]
    public CustomGameEvent response;

    private void OnEnable()
    {
        if (gameEvent != null)
        {
            gameEvent.RegisterListener(this);
            Debug.Log($"{gameObject.name} registered to event {gameEvent.name}");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: GameEvent reference is missing in OnEnable, skipping registration.");
        }
    }


    private void OnDisable()
    {
        if (gameEvent != null)
        {
            gameEvent.UnregisterListener(this);
            Debug.Log($"{gameObject.name} unregistered from event {gameEvent.name}");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: GameEvent reference is missing in OnDisable, skipping unregistration.");
        }
    }

    public void OnEventRaised(Component sender, object data) {
        response.Invoke(sender, data);
    }

}
