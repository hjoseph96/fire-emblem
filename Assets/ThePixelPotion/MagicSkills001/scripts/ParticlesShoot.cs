using UnityEngine;
using System.Collections;

public class ParticlesShoot : MonoBehaviour {
	public GameObject bulletGO;
	public float speed;
	int a = 0;

	public GameObject[] particles;

	// Use this for initialization
	void Start () {
		/*for(int i = 0; i < particles.Length; i++)
		{
			Debug.Log(i);
		}*/
		Debug.Log("długość tablicy " + particles.Length);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Space)){
			print(particles[a].name);
			GameObject arrow = Instantiate (particles[a], transform.position, transform.rotation) as GameObject;
			arrow.GetComponent<ParticleSystem>().Play();
			a++;
			if(a >= particles.Length){
				a = 0;
			}
		}
	}
	void OnGUI() {
		GUI.Label(new Rect(10, 10, 150, 150), particles[a].ToString());
	}
}
