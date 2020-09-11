using UnityEditor;
using System.IO;
using UnityEditor.Build;
using UnityEngine;
using UnityEditor.Build.Reporting;
using System.Diagnostics;

class BuildProcessor : IPostprocessBuildWithReport {
	public int callbackOrder { get { return 0; } }

	public void OnPostprocessBuild(BuildReport report) {
		var path = Directory.GetParent(Application.dataPath).FullName + "\\copy-assets-to-private-repo.ps1";
		UnityEngine.Debug.Log(path);

		ProcessStartInfo processInfo = new ProcessStartInfo("powershell.exe", path);
		processInfo.CreateNoWindow = true;
		processInfo.UseShellExecute = false;

		var process = Process.Start(processInfo);
		process.EnableRaisingEvents = true;
		process.Exited += (sender, args) => process.Dispose();
	}
}
