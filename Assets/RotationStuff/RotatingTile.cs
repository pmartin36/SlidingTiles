using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RotatingTile : Tile
{
	private static RotationInfo RotationInfo;

	public float Rotation0to90 => Rotation % 90;
	public float RotationPercent => Mathf.Abs(Rotation0to90) / 90;

	public float Rotation;
	public SpriteRenderer Box;
	public SpriteRenderer BoxBottom;
	private SpriteRenderer BoxSide;

	public int RotationDirection;

	static RotatingTile() {
		
	}

	protected override void Start() {
		base.Start();
		Addressables.LoadAssetAsync<RotationInfo>("RotationInfo").Completed +=
			(obj) => {
				RotationInfo = obj.Result;
			};
	}

    public override void Update()
    {
		UpdateMaterialProperties();
		UpdateRotations();
	}

	private void UpdateMaterialProperties() {
		float r = RotationPercent;
		bool positive = RotationDirection > 0;
		if(RotationInfo != null) {
			float rx = RotationInfo.rightPoint_x.Evaluate(r);
			float lx = RotationInfo.leftPoint_x.Evaluate(r);

			BoxBottom.sharedMaterial.SetFloat("_Y", RotationInfo.Y.Evaluate(r));
			BoxBottom.sharedMaterial.SetFloat("_LeftX", positive ? rx : lx);
			BoxBottom.sharedMaterial.SetFloat("_RightX", positive ? lx : rx);

			if(BoxSide != null) {
				rx = RotationInfo.rightPoint_x.Evaluate(1 - r);
				lx = RotationInfo.leftPoint_x.Evaluate(1 - r);

				BoxSide.sharedMaterial.SetFloat("_Y", RotationInfo.Y.Evaluate(1 - r));
				BoxSide.sharedMaterial.SetFloat("_RightX", positive ? rx : lx);
				BoxSide.sharedMaterial.SetFloat("_LeftX", positive ? lx : rx);
			}
		}
	}

	private void UpdateRotations() {
		Box.transform.localRotation = Quaternion.Euler(0, 0, Rotation);
		float diff = Rotation0to90;
		float bbr = BoxBottom.transform.eulerAngles.z;
		while (bbr - diff > 360) bbr -= 360;
		while (diff - bbr > 360) bbr += 360;
		diff -= bbr;
		BoxBottom.transform.RotateAround(Box.transform.position, Vector3.forward, diff);
		if(BoxSide != null) {
			BoxSide.transform.RotateAround(Box.transform.position, Vector3.forward, diff);
		}

		foreach (PlatformController p in childPlatforms) {
			var blocker = p.GetCurrentBlocker();
			if (blocker) {
				blocker.collider.GetComponent<IPlatformMoveBlocker>().MoveFromRotation(diff, blocker.point, this.transform.position);
				return;
			}
		}
	}

	public void BeginRotation(int direction) {
		RotationDirection = direction;
		SetTemporaryUnmovable(true);
		if (BoxSide == null) {
			CreateBoxSide();
		}

		BoxSide.sortingOrder += 10;
		BoxBottom.sortingOrder += 10;
		Box.sortingOrder += 10;

		var bbPos = BoxBottom.transform.localPosition;
		var boxPos = Box.transform.localPosition;
		var diff = boxPos.y - bbPos.y;
		if (direction > 0) {
			BoxSide.transform.localPosition = new Vector3(-diff, boxPos.y, bbPos.z);
			BoxSide.transform.localEulerAngles = new Vector3(0, 0, -90);
		}
		else {
			BoxSide.transform.localPosition = new Vector3(diff, boxPos.y, bbPos.z);
			BoxSide.transform.localEulerAngles = new Vector3(0, 0, 90);
		}
	}

	public void EndRotation() {
		SetTemporaryUnmovable(false);
		RotationComplete();
	}

	private void RotationComplete() {
		UpdateMaterialProperties();

		Box.transform.localRotation = Quaternion.Euler(0, 0, Rotation);
		BoxBottom.transform.RotateAround(Box.transform.position, Vector3.forward, -BoxBottom.transform.eulerAngles.z);
		BoxSide.transform.RotateAround(Box.transform.position, Vector3.forward, -90-BoxSide.transform.eulerAngles.z);

		BoxSide.sortingOrder -= 10;
		BoxBottom.sortingOrder -= 10;
		Box.sortingOrder -= 10;
	}

	private void CreateBoxSide() {
		var go = GameObject.Instantiate(BoxBottom.gameObject);
		BoxSide = go.GetComponent<SpriteRenderer>();
		BoxSide.material = new Material(BoxBottom.material);
		BoxSide.sortingOrder = BoxBottom.sortingOrder + 1;

		BoxSide.transform.parent = this.transform;
		BoxSide.transform.localScale = Vector3.one;

		UpdateMaterialProperties();
	}
	
	private float Convert0To90(float r) {
		while (r < 0) r += 360;
		return (r % 90);
	}
}
