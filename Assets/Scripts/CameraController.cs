using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class CameraController : MonoBehaviour
{
	private Vector3 centeredPosition;

	// Shake Stuff
	private static readonly float minShakeSpeed = 0.05f;
	private Queue<Vector3> shakeOffsets;
	private Vector3 offset;
	private Vector3 targetOffset;
	private float shakeStartTime;
	private float shakeDuration;
	private bool shakeDecreasingSpeed;
	private float shakeSpeed;	

	private GameObject PostProcessObjectsContainer { get; set; }
	public PostProcessVolume PostProcessVolume { get; set; }

	public Camera Camera { get; private set; }

    protected void Awake() {
		Camera = GetComponent<Camera>();
		Camera.depthTextureMode = DepthTextureMode.Depth;
		centeredPosition = transform.position;
    }

	protected virtual void Update() {
		float pctDuration = (Time.time - shakeStartTime) / shakeDuration;
		if (pctDuration >= 1) {
			targetOffset = Vector3.zero;
		}

		offset = Vector3.Lerp(offset, targetOffset, shakeSpeed);
		transform.position = centeredPosition + offset;

		if ( pctDuration < 1 && Vector2.Distance(offset, targetOffset) < 0.1f ) {
			if(shakeOffsets != null && shakeOffsets.Count > 0) {
				targetOffset = shakeOffsets.Dequeue();
			}
			else {
				targetOffset = Vector3.zero;
			}

			if(shakeDecreasingSpeed) {
				shakeSpeed = Mathf.Max(shakeSpeed * 0.9f, minShakeSpeed);
			}
		}   
    }

	public void Move(Vector3 p, bool isPosition) {
		if(isPosition) {
			centeredPosition = p;
		}
		else {
			centeredPosition += p;
		}
	}

	public void RandomShake(float duration, float amplitude, float speed, bool decreasingAmplitude, float minAngle = 0, float maxAngle = 360) {
		int positions = (int)(duration * speed * 50f);
		Vector3[] offsets = new Vector3[positions];
		for(int i = 0; i < positions; i++) {
			Vector3 dir = Utils.AngleToVector( UnityEngine.Random.Range(minAngle, maxAngle) );
			offsets[i] = dir * amplitude;
			if(decreasingAmplitude) {
				amplitude *= 0.95f;
			}
		}

		Shake(duration, speed, offsets);
		shakeDecreasingSpeed = false; // override
	}

	public void Shake(float maxDuration, float speed, float amplitude, bool decreasingAmplitude, params Vector3[] directions) {
		Vector3[] offsets = new Vector3[directions.Length];
		for (int i = 0; i < directions.Length; i++) {
			offsets[i] = directions[i] * amplitude;
			if (decreasingAmplitude) {
				amplitude *= 0.9f;
			}
		}

		Shake(maxDuration, speed, offsets);
	}

	public void Shake(float maxDuration, float speed, params Vector3[] offsets) {
		shakeSpeed = speed;
		shakeStartTime = Time.time;
		shakeDuration = maxDuration;
		shakeDecreasingSpeed = true;
		shakeOffsets = new Queue<Vector3>();

		foreach(Vector3 offset in offsets) {
			shakeOffsets.Enqueue(offset);
		}
		targetOffset = shakeOffsets.Dequeue();
		offset = Vector3.Lerp(offset, targetOffset, shakeSpeed);
	}

	public void EnablePostEffects(bool enable) {
		PostProcessObjectsContainer?.SetActive(enable);
	}

	public void RegisterPostProcessVolume(PostProcessVolume v) {
		PostProcessVolume = v;
		PostProcessObjectsContainer = v.transform.parent.gameObject;

		var childCamera = PostProcessObjectsContainer?.GetComponentInChildren<Camera>();
		if(childCamera != null) {
			childCamera.transform.position = Camera.transform.position;
		}
	}

	public T GetModifiablePostProcessSettings<T>() where T : PostProcessEffectSettings {
		if (PostProcessVolume != null && PostProcessVolume.gameObject != null) {
			PostProcessVolume.profile.TryGetSettings<T>(out T settings);
			return settings;
		}
		return null;
	}
}
