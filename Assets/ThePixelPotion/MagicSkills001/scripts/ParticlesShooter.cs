using UnityEngine;
using System.Collections;

[ExecuteInEditMode()]

public class ParticlesShooter : MonoBehaviour {
	GameObject particle;
	public GameObject particleRotGO;
	int a = 0;
	public GameObject[] particles;
	GameObject[] spawned;
	public float particleOffset = 0.01f;
	public bool bullet = true;
	bool buttonPressed;
	public GUISkin particlesGui;
	public GUISkin tppGui;
	string currentParticleName;
	
	void Start () {

	}

	void DestroySpawned(){
		spawned = GameObject.FindGameObjectsWithTag("particle");
		if (spawned.Length <= 0){

		}
		else{
			for(int b = 0; b < spawned.Length ; b++){
				Destroy(spawned[b]);
			}
		}
	}

	void Spawn(){
		if(buttonPressed){
			buttonPressed=false;
		}
		else{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit=new RaycastHit();
			if(Physics.Raycast(ray,out hit))
			{
				if(hit.collider){
					int found = particles[a].name.IndexOf("Bullet");
					
					if (found > 0){
						GameObject newGO = new GameObject();
						newGO.name = "bullet";
						newGO.transform.position = hit.point + (hit.normal * (particleOffset + 1));
						newGO.transform.parent = particleRotGO.transform.parent;
						newGO.tag = "particle";
						
						particle = Instantiate(particles[a],newGO.transform.position,Quaternion.identity) as GameObject;
						particle.transform.parent = newGO.transform;
						particle.GetComponent<ParticleSystem>().Play();
						Destroy (newGO, 5.0f);
					}
					else{
						particle = Instantiate(particles[a],hit.point + (hit.normal * particleOffset),Quaternion.identity) as GameObject;
						particle.transform.parent = particleRotGO.transform.parent.parent;
						particle.GetComponent<ParticleSystem>().Play();
						Destroy (particle, 15.0f);
					}
				}else{
					
				}
				
			}
		}
	}

	void NextParticle(){
		a++;
		if(a >= particles.Length){
			a = 0;
		}
	}

	void PrevParticle(){
		a--;
		if(a < 0){
			a = particles.Length -1;
		}
	}

	void Update(){
		currentParticleName = particles[a].name.ToString();

		if(Input.GetMouseButtonUp(0)){
			Spawn();
		}
		if(Input.GetKeyDown(KeyCode.RightArrow)){
			NextParticle();
		}
		if(Input.GetKeyDown(KeyCode.LeftArrow)){
			PrevParticle();
		}
		if(Input.GetKeyDown(KeyCode.Escape)){
			DestroySpawned();
		}
	}

	void OnGUI ()
	{
		GUI.skin = particlesGui;

		if(GUI.Button(new Rect(Screen.width - 400,Screen.height - 40,200,40), "Next Particle (Right arrow)")){
			buttonPressed = true;
			NextParticle();
		}
		
		if(GUI.Button(new Rect(0,Screen.height - 40,200,40), "Previous Particle (Left arrow)")){
			buttonPressed = true;
			PrevParticle();
		}

		GUI.Label(new Rect(200,Screen.height - 40,Screen.width - 600,40), "# "+ (a+1) +"/25: " + currentParticleName);
		GUI.Label(new Rect((Screen.width/2) - ((Screen.width - 400)/2),0,Screen.width - 400,40), "Click on the ground to spawn particle.");

		if(GUI.Button(new Rect(Screen.width - 200,Screen.height - 40,200,40), "Clear Particles (Esc)")){
			buttonPressed = true;
			DestroySpawned();
		}

		GUI.skin = tppGui;

		if(GUI.Button(new Rect(Screen.width - 160,10,150,80), "thepixelpotion.tumblr.com")){
			Application.OpenURL("http://thepixelpotion.tumblr.com");
			buttonPressed = true;
		}

	}
}