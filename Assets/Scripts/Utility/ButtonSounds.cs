using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonSounds : MonoBehaviour
{
    EventTrigger eventTrigger;

    // Start is called before the first frame update
    void Start()
    {
        eventTrigger = GetComponent<EventTrigger>();

        if (eventTrigger == null)
        {
            eventTrigger = gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry hoverEvent = new()
        {
            eventID = EventTriggerType.PointerEnter
        };
        hoverEvent.callback.AddListener((eventData) =>
        {
            AudioManager.Instance.PlaySFX("ButtonHover");
        });

        EventTrigger.Entry clickEvent = new()
        {
            eventID = EventTriggerType.PointerClick
        };
        clickEvent.callback.AddListener((eventData) =>
        {
            AudioManager.Instance.PlaySFX("ButtonClick");
        });

        eventTrigger.triggers.Add(hoverEvent);
        eventTrigger.triggers.Add(clickEvent);
    }
}
