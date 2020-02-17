using UnityEngine;
using System.Collections;

[ExecuteInEditMode()]

public class ShowFps : MonoBehaviour {
	int frames = 0;
	float timeNow;
	float updateInterval = 0.25f;
	float lastInterval;
	float fps;
	string fpsText;

	void Start () {
		lastInterval = Time.realtimeSinceStartup;
		frames = 0;
	}

	void Update () {
		frames++;
		timeNow = Time.realtimeSinceStartup;

		if (timeNow > lastInterval + updateInterval)
		{
			//fps = (int)Mathf.Round(frames / (timeNow - lastInterval));
			fps = frames / (timeNow - lastInterval);
			float ms = 1000.0f / Mathf.Max (fps, 0.00001f);
			fpsText = ms.ToString("0.####") + " ms\n" + fps.ToString("0.#") + " fps";
			frames = 0;
			lastInterval = timeNow;
		}
	}
	void OnGUI(){
		GUIStyle labelLeft = new GUIStyle("label");
		labelLeft.alignment = TextAnchor.MiddleLeft;
		GUI.Label(new Rect(10,10,400,40), fpsText);
	}
}
