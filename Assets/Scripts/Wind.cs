using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Wind : MonoBehaviour
{
	public AnimationCurve X;
	public AnimationCurve Y;

	public float Direction { get; set; }
	public float Strength { get; set; }

	public float Radius { get; set; }
	public Bounds CameraBounds { get; set; }

	private int GustsCompleted = 0;
	private int NextDirectionChangeGustAmount = 0;

	public Gust[] Gusts { get; set; }

	private Background background;

    void Start() {
		Camera c = CameraManager.Instance.Camera;
		Radius = c.orthographicSize * 1.2f;
		if(c.aspect > 1) {
			Radius *= c.aspect;
		}

		Gusts = GetComponentsInChildren<Gust>();
		CameraBounds = new Bounds(new Vector3(c.transform.position.x, c.transform.position.y, 0), new Vector3(c.orthographicSize * c.aspect, c.orthographicSize));

		background = GameManager.FindObjectsOfType<Background>().First(g => g.gameObject.scene == this.gameObject.scene);

		SetProperties();
		GenerateNewGusts(true);
	}

	public void SetStrength(float strength, bool clampbottom = true) {
		float val = strength;
		val = Mathf.Clamp(strength, clampbottom ? 0.1f : 0.0f, 0.2f);
		foreach (var m in background.AllMaterials) {
			m.SetFloat("_WindStrength", val);
		}
	}

	private void SetProperties() {
		Direction = Random.value * 360f;
		Strength = 0.15f + Random.value * 0.05f;

		foreach(var m in background.AllMaterials) {
			m.SetFloat("_WindDirectionAngle", ((180f + Direction) % 360f) * Mathf.Deg2Rad);
		}
		SetStrength(Strength);
	}

	public bool GustCompleted() {
		GustsCompleted++;
		if(GustsCompleted % Gusts.Length == 0) {
			return GenerateNewGusts();
		}
		return false;
	}

	public bool GenerateNewGusts(bool skipChange = false) {
		if (GustsCompleted >= NextDirectionChangeGustAmount) {
			NextDirectionChangeGustAmount += 2 * Gusts.Length;
			if(skipChange) {
				SetProperties();
				SetGusts();
			}
			else {
				StartCoroutine(ChangeWindDirection());
			}
			return true;
		}
		return false;
	}

	private void SetGusts() {
		for(int i = 0; i < Gusts.Length; i++) {
			Gust g = Gusts[i];
			float offset = (float)i / Gusts.Length;
			float gustOffset = offset * i + Random.value * offset * 0.8f;
			g.SetProperties(i);
		}
	}

	private IEnumerator ChangeWindDirection() {
		float timeBetweenWind = 1f + Random.value * 2f;
		float time = 0;
		while(time < timeBetweenWind) {
			float str = Mathf.Lerp(Strength, 0, time / timeBetweenWind);
			SetStrength(str, false);
			time += Time.deltaTime;
			yield return null;
		}

		SetProperties();
		SetGusts();

		time = 0;
		while (time < 1f) {
			float str = Mathf.Lerp(0, Strength, time);
			SetStrength(str, false);
			time += Time.deltaTime;
			yield return null;
		}
		SetStrength(Strength);
	}
}
