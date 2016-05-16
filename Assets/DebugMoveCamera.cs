using UnityEngine;
using System.Collections;

public class DebugMoveCamera : MonoBehaviour {

    public float xMovement = 1f;
    public float yMovement = 1f;
    
	void Update () {
        transform.position += new Vector3(xMovement * Time.deltaTime, yMovement * Time.deltaTime, 0);
	}
}
