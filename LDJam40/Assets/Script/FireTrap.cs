﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTrap : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

	void OnTriggerEnter2D(Collider2D other) {
		Character character = other.gameObject.GetComponent<Character>();
		character.Kill();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}