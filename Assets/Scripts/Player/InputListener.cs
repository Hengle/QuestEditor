using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputListener : MonoBehaviour {

	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Submit"))
        {
            FindObjectOfType<ChainPlayer>().ActivateWaitingInputState();
        }
	}
}
