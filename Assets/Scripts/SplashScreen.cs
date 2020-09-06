using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SplashScreen : MonoBehaviour
{
	public bool AnimationComplete;
	public ParticleSystem parentParticleSystem;
	public RawImage Black;

	private List<ParticleSystem> ps;

	void Start() {
		Camera camera = CameraManager.Instance.Camera;
		float y = camera.orthographicSize * 2;
		float x = y * camera.aspect;
		var psSize = new Vector3(x, y, 1) * 1.1f;

		Vector3 position = new Vector3(camera.transform.position.x, camera.transform.position.y, 0);
		GetComponent<AudioSource>().volume = 0.8f * (GameManager.Instance.SaveData?.FxVolume ?? 1f);

		ps = new List<ParticleSystem>() { parentParticleSystem };
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

		var particles = new Dictionary<ParticleSystem, ParticleSystem.Particle[]>();
		foreach (ParticleSystem p in ps) {
			p.Stop(true, ParticleSystemStopBehavior.StopEmitting);
			ParticleSystem.Particle[] m_Particles = new ParticleSystem.Particle[p.particleCount];
			p.GetParticles(m_Particles);
			particles.Add(p, m_Particles);
		}

		GetComponent<Animator>().Play("SplashHide");
		yield return new WaitUntil(() => AnimationComplete);

		float t = 0;
		const float jTime = 0.5f;
		int lpw = Mathf.Max(1, GameManager.Instance.SaveData?.LastPlayedWorld ?? 1);
		MusicManager.Instance.LoadMusicForWorldAndChangeTrack(lpw, jTime / 2f, 0.8f);

		while (t < jTime) {
			Black.color = Color.Lerp(Color.black, Color.clear, t / jTime);

			Color pColor = Color.Lerp(Color.white, Color.clear, t / jTime);
			foreach (var p in ps) {
				for (int i = 0; i < particles[p].Length; i++) {
					var particle = particles[p][i];
					particle.startColor = pColor;
					particles[p][i] = particle;
				}
				p.SetParticles(particles[p], particles[p].Length);
			}

			t += Time.deltaTime;
			yield return null;
		}
		GameManager.Instance.UnloadScene(SceneHelpers.SplashBuildIndex);
	}
}
