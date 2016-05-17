using UnityEngine;
using System.Collections;

public class DebugCameraTeleport : MonoBehaviour {

    public float delay = 2f;
    public Vector3 newPosition;

	IEnumerator Start () {
        yield return new WaitForSeconds(delay);
        transform.position = newPosition;
	}
}
