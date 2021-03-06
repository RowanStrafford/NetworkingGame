﻿using UnityEngine;
using System.Collections;

public abstract class PhysicsObject : MonoBehaviour {

	protected Rigidbody rb;//accessing it publicly does not work
	protected float maxHealth;
	protected float health;
	public float healthMult;

	public float dmgMult = 1;

	public float maxVel = 20;

	//Physics fun
	public float dens = 1;

	public float densMult = 1;
	public bool modDens = false;

	public float velMult = 1;
	public bool modVel = false;

	public int team = 0;

	protected int immune = 0;


	// Use this for initialization
	virtual protected void Start() {
		//enabled = false;
		//Time.timeScale = 0;
		
		rb = GetComponent<Rigidbody>();
		rb.SetDensity(dens);
		maxHealth = Mathf.Sqrt(rb.mass) * healthMult;
		health = maxHealth;
	}

	// Update is called once per frame
	virtual protected void Update() {
		EnforceBoundaries();
	}

	virtual protected void FixedUpdate() {
		setPos();
		updatePhysVars();
		
		//Cap Velocity
		if (rb.velocity.magnitude > maxVel)
			rb.velocity = rb.velocity.normalized * maxVel;

		immune++;
	}

	private void setPos() {
		//sometimes vector forward pos is off center, fix later
		Vector3 pos = transform.position;
		pos.z = 0;
		transform.position = pos;
	}

	private void updatePhysVars() {
		if (modVel) {
			rb.velocity *= velMult;
			modVel = false;
		}
		if (modDens) {
			dens *= densMult;
			rb.SetDensity(dens);
			modDens = false;
			maxHealth = rb.mass * healthMult;
			ResetHealth();
		}
	}

	virtual protected void OnCollisionEnter(Collision col) {
		if (immune < 0)
			return;

		PhysicsObject physicsObject = col.gameObject.GetComponent<PhysicsObject>();

		if (physicsObject.immune < 0)
			return;

		if (team == physicsObject.team&&team==1)
			return;

		float collisionForce = (col.relativeVelocity.magnitude* col.relativeVelocity.magnitude/2) * maxHealth;

		physicsObject.takeDamage(Mathf.Sqrt(collisionForce/4)*dmgMult);
	}

	virtual public void takeDamage(float damage) {
		health -= damage;
		if (health <= 0)
			Die(damage);
	}

	virtual protected void Die(float damage) {
		Destroy(gameObject);
	}

	public float GetHealth() {
		return health;
	}

	public void ResetHealth() {
		health = maxHealth;
	}

	virtual protected void EnforceBoundaries() {
		if (transform.position.x < Map.X || transform.position.x > Map.X + Map.W || transform.position.y < Map.Y || transform.position.y > Map.Y + Map.H)
			Destroy(gameObject, 1f);
	}
}
