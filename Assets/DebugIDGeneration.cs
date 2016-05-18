using UnityEngine;
using System.Collections;

public class DebugIDGeneration : MonoBehaviour {

	void Start () {
        int x = -1;
        int y = 0;

        long id = ((long)x << 32) + y;
        Debug.Log("ID: " + id);

        long id2 = ((long)y << 32) + x;
        Debug.Log("ID2: " + id2);

        long id3 = ((long)x << 32) + x;
        Debug.Log("ID2: " + id3);

    }
}
