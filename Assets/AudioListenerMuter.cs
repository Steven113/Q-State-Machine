using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioListenerMuter : MonoBehaviour {

	public AudioListener audioListener;

	public bool muted;
	public bool expected;

	// Use this for initialization
	void Start () {
		expected = !muted;
	}
	
	// Update is called once per frame
	void Update () {
		if (expected != muted) {
			AudioListener.volume = muted?0f:1f;
			expected = muted;
		}
	}
}
