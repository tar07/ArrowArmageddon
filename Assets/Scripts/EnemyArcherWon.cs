﻿/*
Author: Trevor Richardson
PlayerControllerScript.cs
04-07-2015

	Script for controlling the enemy. Mirrors the player controls for many functions.
	
 */

using UnityEngine;
using System.Collections;

public class EnemyArcherWon : MonoBehaviour {
	
	// movement
	public bool right = true;
	float hSpeed = 0;
	
	// arrow projectile
	public GameObject arrowPrefab;
	float arrowSpeed = 10.0f;
	
	// detect ground contact
	bool onGround = false;
	public Transform groundCircle;
	float groundRadius = 0.05f;
	public LayerMask GroundMask;
	
	Animator anim;
	
	int health = 1;
	bool KOed = false;
	private bool inAction = false;
	
	public AudioSource damagedAudio;
	public AudioSource throwAudio;
	public AudioSource KOAudio;
	
	
	// Ignore collision with other enemies, get animator
	void Start () {
		anim = GetComponent<Animator>();
		Flip ();
	}
	
	// Movement & patrol controls
	void FixedUpdate () {
		
		// detect ground contact for anims
		onGround = Physics2D.OverlapCircle(groundCircle.position, groundRadius, GroundMask);
		anim.SetBool("Grounded", onGround);
		// Update vertical speed for fall anim
		anim.SetFloat ("vSpeed", rigidbody2D.velocity.y);
		
		// update horizontal speed in animator and move enemy
		anim.SetFloat("hSpeed", Mathf.Abs(hSpeed));
		rigidbody2D.velocity = new Vector2(hSpeed, rigidbody2D.velocity.y);
		
		// Same as player control to determine if a flip is needed
		if (hSpeed < 0 && right)
			Flip ();
		else if ( hSpeed > 0 && !right)
			Flip ();
		
	}
	
	IEnumerator Shoot() {
		anim.SetTrigger("ThrowTrigger");
		yield return new WaitForSeconds(.5f);
		if (!KOed) {
			// spawn arrow and send it in the facing direction
			GameObject enemyArrow = (GameObject)Instantiate(arrowPrefab, transform.position, Quaternion.identity);
			if (right)
				enemyArrow.rigidbody2D.velocity = new Vector2(arrowSpeed, 0);
			else {
				enemyArrow.transform.localScale = new Vector3(-enemyArrow.transform.localScale.x, enemyArrow.transform.localScale.y, enemyArrow.transform.localScale.z);
				enemyArrow.rigidbody2D.velocity = new Vector2(-arrowSpeed, 0);
			}
			// play throw fx and destroy brick after 1s
			throwAudio.Play();
			Destroy (enemyArrow, 7.0f);
		}
	}
	
	// Remove enemy collision boxes on death
	void Update() {
	}
	
	IEnumerator StationaryBehavior() {
		inAction = true;
		StartCoroutine(Shoot());
		yield return new WaitForSeconds(1f);
		inAction = false;
	}
	
	// Flip sprite horizontally
	void Flip() {
		right = !right;
		transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}
	
	// If the enemy collides with a brick, destroy the brick, play enemy KO anim, and set KOed status to true
	void OnTriggerEnter2D (Collider2D col)
	{
		if (col.gameObject.tag == "Arrow") {
			Hit ();
		}
	}
	
	// Send KO signal to animator and wait 3 seconds to respawn
	IEnumerator KO() {
		anim.SetBool("GameOver", true);
		KOed = true;
		yield return new WaitForSeconds(1.0f);
		GetComponent<CircleCollider2D>().enabled = false;
		GetComponent<BoxCollider2D>().enabled = false;
		Destroy (gameObject, 2.0f);
	}
	
	void Hit() {
		--health;
		damagedAudio.Play();
		//anim.SetTrigger("HitTrigger");
		switch (health) {
		case 2:
			anim.SetTrigger("HitTrigger");
			break;
		case 1:
			anim.SetTrigger("HitTrigger");
			break;
		case 0:
			StartCoroutine(KO());
			break;
		}
	}
}
