using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VoxelBusters.NativePlugins;

public class AndroidCommunicator : PhoneCommunicator {
	public override void GoToStore() {
		Application.OpenURL("market://details?id=com.MadeByMoonlight.The16Spaces");
	}
}
