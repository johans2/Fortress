using UnityEngine;
using System.Collections;

public class DemoManBehaviour : MonoBehaviour {

    SpriteRenderer[] renderers;



    void Awake() {
        renderers = GetComponentsInChildren<SpriteRenderer>();

    }

	void Start () {
	
	}
	
	void LateUpdate () {
        for (int i = 0; i < renderers.Length; i++)
        {


        }


	}
}
