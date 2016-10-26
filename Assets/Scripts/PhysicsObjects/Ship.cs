﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Ship : PhysicsObject {

	public float force;

	public GameObject beam;
	public GameObject beamSpawnPos;

	//Thruster
    public Scrollbar healthBar;
    public GameObject thruster;
    private ParticleSystem particle;
    ParticleSystem.EmissionModule emmisions;

    private AudioSource shipAudio;
    public AudioClip beamPowerOn;

    //private AudioSource audio;
    public AudioClip laserShot;

    private float fireRateTimer = 0f;
    public float maxBeamPower = 3f;

    new protected void Start() {
        base.Start();
		UpdateHealthBar();

        particle = thruster.GetComponent<ParticleSystem>();
        emmisions = particle.emission;
        emmisions.enabled = false;
		shipAudio = GetComponent<AudioSource>();
	}

    // Update is called once per frame
    new protected void Update () {
		base.Update();
		setRotation();

		inputs();
   	}

	void inputs() {
		if (Input.GetButton("Fire1")) {
			fireRateTimer += Time.deltaTime;

			if (shipAudio.isPlaying == false)
				shipAudio.PlayOneShot(beamPowerOn);

			if (fireRateTimer > maxBeamPower)
				fire(maxBeamPower, 2);
		}

		if (Input.GetButtonUp("Fire1")) {
			shipAudio.Stop();

			if (fireRateTimer < 0.5f) {
				fire(1, 0);
			} else if (fireRateTimer < 1f) {
				fire(1, 1);
			} else {
				fire(fireRateTimer);
			}
			fireRateTimer = 0;
		}

		if (Input.GetKeyDown(KeyCode.W))
			emmisions.enabled = true;
		if (Input.GetKeyUp(KeyCode.W))
			emmisions.enabled = false;
	}

	void fire(float speedMultiplier = 1f, int power = 1) {
		GameObject proj = Instantiate(beam, beamSpawnPos.transform.position, transform.rotation) as GameObject;
		Rigidbody beamRb = proj.GetComponent<Rigidbody>();
		beamRb.velocity += rb.velocity;
		
		Beam beamScript = proj.GetComponent<Beam>();

		beamScript.velMult = speedMultiplier;
		beamScript.modVel = true;

		switch (power) {
			case 0:
				beamScript.densMult = 0.75f;
				beamRb.transform.localScale = beamRb.transform.localScale * 0.6f;
				break;
			case 1:
				beamScript.densMult = 1f;
				//beamRb.transform.localScale = beamRb.transform.localScale * 1f;
				break;
			case 2:
				beamScript.densMult = 1.25f;
				beamRb.transform.localScale = beamRb.transform.localScale * 1.5f;
				break;
			default:
				beamScript.densMult = 0.05f;
				break;
		}
		beamScript.modDens = true;

		fireRateTimer = 0;
		shipAudio.PlayOneShot(laserShot);
	}

	new protected void FixedUpdate() {
		base.FixedUpdate();
		if (Input.GetKey(KeyCode.W)) {
			rb.AddForce(transform.right * force - (rb.velocity / force*4), ForceMode.Force);
		}
		EnforceBoundaries();
		if (health < maxHealth) {
			health = health + 0.1f;
			UpdateHealthBar();
		}
	}
	
	override protected void OnCollisionEnter(Collision col) {
		base.OnCollisionEnter(col);
		UpdateHealthBar();
	}

	override public void takeDamage(float damage) {
		health -= damage;
		if (health <= 0)
			SceneManager.LoadScene(0);
	}

	void UpdateHealthBar() {
		healthBar.size = (health / maxHealth);
	}

	void setRotation() {
		Vector3 mousePos = Input.mousePosition;
		Vector3 objectPos = Camera.main.WorldToScreenPoint(transform.position);

		mousePos.x = mousePos.x - objectPos.x;
		mousePos.y = mousePos.y - objectPos.y;

		float playerRotationAngle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(new Vector3(0, 0, playerRotationAngle));
		Vector3 rot = transform.rotation.eulerAngles;
		rot = new Vector3(rot.x+180, rot.y, rot.z);
		transform.rotation = Quaternion.Euler(-rot);
	}

	override protected void EnforceBoundaries() {
		if (transform.position.x < Map.X)
			transform.position = new Vector3(Map.X, transform.position.y, transform.position.z);
		if (transform.position.x > Map.X + Map.W)
			transform.position = new Vector3(Map.X+Map.W, transform.position.y, transform.position.z);
		if (transform.position.y < Map.Y)
			transform.position = new Vector3(transform.position.x, Map.Y, transform.position.z);
		if (transform.position.y > Map.Y + Map.H)
			transform.position = new Vector3(transform.position.x, Map.Y+Map.H, transform.position.z);
	}
}
