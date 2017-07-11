using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffAfterSeconds : MonoBehaviour {
    [SerializeField]
    float secondsUntilOff = 0.1f;

    float onTime;
    bool canDisableSelf, setNewTime;

    private void Start()
    {
        onTime = Time.time;
        canDisableSelf = true;
    }

    private void Update()
    {
        if (setNewTime && Time.time - onTime >= secondsUntilOff)
        {
            setNewTime = false;
            gameObject.SetActive(false);
        }
        else if (canDisableSelf)
        {
            canDisableSelf = false;
            onTime = Time.time;
            setNewTime = true;
        }

    }

    private void OnDisable()
    {
        canDisableSelf = true;
    }
}
