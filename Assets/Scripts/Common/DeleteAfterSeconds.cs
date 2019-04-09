using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DeleteAfterSeconds : NetworkBehaviour {

    [SerializeField]
    float _seconds = 1f;

	// Use this for initialization
	void Start () {
        if (!isServer) return;
        StartCoroutine(WaitForSeconds());
	}

    IEnumerator WaitForSeconds()
    {
        yield return new WaitForSeconds(_seconds);
        Destroy(gameObject);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
