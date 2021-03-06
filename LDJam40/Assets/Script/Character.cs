﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface CharacterListener {
	void onKill(Character character);
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
public class Character : MonoBehaviour {

	public static class MoveDirection {
		public const int left = -1;
		public const int right = 1;
		public const int none = 0;
	}

	float maxSpeed = 3f;
	float moveSpeed = 20f;
	float dampAmount = 0.20f;
	float jumpForce = 350f;
	int direction = MoveDirection.none;

	private Rigidbody2D rb;
	public bool isGrounded = true;
	public ParticleSystem deathPS;
	private bool isAlive = true;

	int numberOfRays = 5;
	float defaultXScale;
	BoxCollider2D bc;

    Animator am;
	AudioSource asource;
	public AudioClip jumpSound;
	public AudioClip deathSound;
	List<CharacterListener> listeners = new List<CharacterListener>();

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D> ();
		bc = GetComponent<BoxCollider2D>();
		am = GetComponent<Animator>();
		asource = GetComponent<AudioSource>();
		defaultXScale = transform.localScale.x;
	}

	// Update is called once per frame
	void Update () {
		CheckIfGrounded();
		if(direction == MoveDirection.left) {
			transform.localScale = new Vector3(defaultXScale * -1, transform.localScale.y, transform.localScale.z);
		}
		if(direction == MoveDirection.right) {
			transform.localScale = new Vector3(defaultXScale, transform.localScale.y, transform.localScale.z);
		}

        am.SetFloat("HorizontalVelocity", Mathf.Abs(rb.velocity.x));
        am.SetFloat("VerticalVelocity", isGrounded ? 0 : Mathf.Max(-rb.velocity.y,0));
	}

	public void ChangeDirection(int direction) {
		this.direction = direction;
	}

	public int GetDirection() {
		return this.direction;
	}

	public void Move(int direction) {
		if(direction != MoveDirection.none) {
			float defaultMaxSpeed = maxSpeed;

			if(!GetComponent<PlayerController>()) {
				maxSpeed += Random.Range(-0.25f, 0.25f);
			}

			if(direction * rb.velocity.x < maxSpeed) {
				rb.AddForce(Vector2.right * direction * moveSpeed);
			}

			if(Mathf.Abs(rb.velocity.x) > maxSpeed) {
				rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
			}

			maxSpeed = defaultMaxSpeed;
		} else {
			if(isGrounded) {
				rb.velocity = new Vector2(rb.velocity.x * (1 - dampAmount), rb.velocity.y);
			}
		}
	}

	public void Jump() {
		if(isGrounded) {
			float defaultJumpForce = jumpForce;
			if(!GetComponent<PlayerController>()) {
				jumpForce += Random.Range(-20f, 20f);
			}
			rb.AddForce(Vector2.up * jumpForce);
			jumpForce = defaultJumpForce;
            am.SetTrigger("Jump");
			asource.pitch = Random.Range(0.9f,1.1f);
			asource.PlayOneShot(jumpSound);
		}
	}

	public void Kill() {
		if(!isAlive) {
			return;
		}
		Debug.Log("Got killed!");
		isAlive = false;

		// Disable collider and sprite renderer. Probably better to do this another way...
		bc.enabled = false;
		GetComponent<SpriteRenderer>().enabled = false;

		ParticleSystem initPS = Instantiate(deathPS, transform.position, Quaternion.identity);
		Destroy(initPS.gameObject, initPS.main.duration);
		asource.pitch = Random.Range(0.9f,1.1f);
		asource.PlayOneShot(deathSound);
		foreach (CharacterListener l in listeners) {
			l.onKill(this);
		}
		if(GetComponent<PlayerController>()) {
			GameManager.instance.gameOver = true;	
		}
	}

	public bool IsAlive {
		get { return this.isAlive; }
	}

	public void AddListener(CharacterListener listener) {
		listeners.Add(listener);
	}

	void CheckIfGrounded(){
		isGrounded = false;
		Bounds bounds = bc.bounds;
		float raySpacing = (bounds.max.x - bounds.min.x) / (numberOfRays - 1);
		Vector3 bottomLeft = bounds.min;

		for(int i = 0; i < numberOfRays; i++){
			Vector3 startCastFrom = new Vector3(bottomLeft.x + raySpacing * i, bottomLeft.y);
			Vector3 endCastAt = new Vector3(bottomLeft.x + raySpacing * i, bottomLeft.y - 0.05f);

			Debug.DrawLine(startCastFrom, endCastAt, Color.red);

			if(Physics2D.Linecast(startCastFrom, endCastAt, 1 << LayerMask.NameToLayer("Ground"))){
				isGrounded = true;
			}
		}
	}

	void FixedUpdate() {
		Move(direction);
	}
}
