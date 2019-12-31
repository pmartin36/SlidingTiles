using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashScreen : MonoBehaviour
{
	public bool AnimationComplete;
	public ParticleSystem parentParticleSystem;

    void Start() {
		Camera camera = CameraManager.Instance.Camera;
		float y = camera.orthographicSize * 2;
		float x = y * camera.aspect;
		var psSize = new Vector3(x, y, 1) * 1.1f;

		Vector3 position = new Vector3(camera.transform.position.x, camera.transform.position.y, 0);

		List <ParticleSystem> ps = new List<ParticleSystem>() { parentParticleSystem };
		ps.Add(parentParticleSystem.transform.GetChild(0).GetComponent<ParticleSystem>());
		foreach(ParticleSystem p in ps) {
			p.transform.position = position;

			var shapeModule = p.shape;
			if(shapeModule.scale.x < psSize.x || shapeModule.scale.y < psSize.y) 
				shapeModule.scale = psSize;
		}

		GameManager.Instance.AsyncLoadScene(
			SceneHelpers.MenuBuildIndex, 
			StartCoroutine(WaitForAssetLoad()),
			null,
			() => StartCoroutine(WaitForHideComplete()),
			false
		);
    }

	public void UpdateAnimationState() {
		AnimationComplete = true;
	}

    private IEnumerator WaitForAssetLoad() {
		yield return new WaitUntil(() => AnimationComplete);
	}

	private IEnumerator WaitForHideComplete() {
		AnimationComplete = false;
		GetComponent<Animator>().Play("SplashHide");
		yield return new WaitUntil(() => AnimationComplete);
		GameManager.Instance.UnloadScene(SceneHelpers.SplashBuildIndex);
	}
}
