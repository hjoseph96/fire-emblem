using UnityEngine;
using System.Collections;

public class ParticlesPlayback : MonoBehaviour {
	public ParticleSystem particle1;
	public ParticleSystem particle2;
	public ParticleSystem particle3;
	public ParticleSystem particle4;
	public ParticleSystem particle5;

	ParticleSystem currentGO;
	bool play;

	// Use this for initialization
	void Start () {
		currentGO = particle1;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Alpha1)){
			currentGO.Stop();
			currentGO = particle1;
			currentGO.Play();
		}
		else if(Input.GetKeyDown(KeyCode.Alpha2)){
			currentGO.Stop();
			currentGO = particle2;
			currentGO.Play();
		}
		else if(Input.GetKeyDown(KeyCode.Alpha3)){
			currentGO.Stop();
			currentGO = particle3;
			currentGO.Play();
		}
		else if(Input.GetKeyDown(KeyCode.Alpha4)){
			currentGO.Stop();
			currentGO = particle4;
			currentGO.Play();
		}
		else if(Input.GetKeyDown(KeyCode.Alpha5)){
			currentGO.Stop();
			currentGO = particle5;
			currentGO.Play();
		}


	}
}
