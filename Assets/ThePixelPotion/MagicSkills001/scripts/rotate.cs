using UnityEngine;
using System.Collections;

public class rotate : MonoBehaviour {
	public float speedX = 10f;
	public float speedY = 10f;
	public float speedZ = 10f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		transform.Rotate(speedX * Time.deltaTime, speedY * Time.deltaTime, speedZ * Time.deltaTime);
	}
}
