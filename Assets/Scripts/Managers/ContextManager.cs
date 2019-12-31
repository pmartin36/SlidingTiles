using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

public abstract class ContextManager : MonoBehaviour
{
	public bool AcceptingInputs { get; set; } = true;
	protected InputController InputController;

	public bool ResourcesLoaded { get; set; } = false;

	public virtual void Awake() {
		
	}

	public virtual void Start() {
		InputController = InputController.CreateInputController();
		StartCoroutine(WaitUntilLoaded());
	}

	public virtual void Update() {
		HandleInput(
			AcceptingInputs 
			? InputController.GetInput()
			: InputPackage.Empty
		);
	}

	private IEnumerator WaitUntilLoaded() {
		List<IRequireResources> resources = GameObject.FindObjectsOfType<MonoBehaviour>().OfType<IRequireResources>().ToList();
		yield return new WaitUntil(() => resources.All(r => r.Loaded));
		ResourcesLoaded = true;
	}

	public abstract void HandleInput(InputPackage p);
}
