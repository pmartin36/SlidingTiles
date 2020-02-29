using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gust : MonoBehaviour
{
	private Wind wind;
	private float CurveTime;

	private float SpeedModifier = 1;
	private float AmplitudeModifier;

	private AnimationCurve XCurve;
	private Vector3 StartPosition;

	private TrailRenderer tr;
	private Quaternion WindDirection;

	private bool active = false;

    void Start() {
		wind = GetComponentInParent<Wind>();
		tr = GetComponent<TrailRenderer>();

		CurveTime = tr.time + Random.value;
	}

    void Update() {
		if(active) {
			tr.enabled = true;

			// get latest from Wind
			float t = CurveTime * SpeedModifier;
			if(t > 1.5f + tr.time*SpeedModifier) {
				active = false;
				wind.GustCompleted();
			}

			Vector3 movement = WindDirection * new Vector3(XCurve.Evaluate(t) * 2 * wind.Radius, wind.Y.Evaluate(t) * AmplitudeModifier, 0);
			transform.position = StartPosition + movement;	
		}

		CurveTime += Time.deltaTime;
	}

	public void SetProperties(float offsetTime) {
		Vector2 d = Utils.AngleToVector(wind.Direction);
		WindDirection = Quaternion.Euler(0,0,wind.Direction);
		SpeedModifier = wind.Strength;
		AmplitudeModifier = (20f + 10f * Random.value) * Mathf.Sign(Random.value - 0.5f);

		tr.enabled = false;
		float absX = Mathf.Abs(d.x);
		float absY = Mathf.Abs(d.y);
		if (absX > absY) {
			StartPosition = wind.CameraBounds.center + new Vector3(
				wind.Radius * Mathf.Sign(-d.x), 
				(Random.value * 0.7f + absY) * wind.CameraBounds.size.y * Mathf.Sign(-d.y)
			);
		}
		else {
			StartPosition = wind.CameraBounds.center + new Vector3(
				(Random.value * 0.7f + absX) * wind.CameraBounds.size.x * Mathf.Sign(-d.x), 
				wind.Radius * Mathf.Sign(-d.y)
			);
		}
		transform.position = StartPosition;
		tr.Clear();

		XCurve = wind.X;
		//XCurve = new AnimationCurve(Wind.X.keys);
		//Keyframe f1 = XCurve[1];
		//f1.time = 0.2f + 0.4f * Random.value;

		//Keyframe f2 = XCurve[2];
		//f2.time = f1.time + (0.15f + 0.1f * Random.value);

		//XCurve.MoveKey(2, f2);
		//XCurve.MoveKey(1, f1);

		CurveTime = -offsetTime;
		active = true;

		//Debug.Log(d);
		//Debug.Log(SpeedModifier);
		//Debug.Log(AmplitudeModifier);
		//Debug.Log(StartPosition);
	}
}
