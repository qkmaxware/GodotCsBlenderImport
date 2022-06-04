using Godot;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Qkmaxware.GodotAddons.Import {

[Tool]
public class BlenderAutoImport : EditorImportPlugin {
	#region Identification
	public override string GetImporterName() => "qkmaxware.importers.blender";
	public override string GetVisibleName() => "Blender Scene";
	#endregion

	#region File Types
	public override Godot.Collections.Array GetRecognizedExtensions() {
		return new Godot.Collections.Array(new object[] {
			"blend"
		});
	}
	public string GetIntermediateExtension() => "glb";
	public override string GetSaveExtension() => "scn";
	public override string GetResourceType() => "PackedScene";
	#endregion

	#region Import Options
	public override int GetPresetCount() => 0;
	public override string GetPresetName(int index) => null;
	public override Godot.Collections.Array GetImportOptions(int preset) => new Godot.Collections.Array(new object[]{

	});
	public override bool GetOptionVisibility(string option, Godot.Collections.Dictionary options) => true;
	#endregion

	private static string BuildCommandLineArgs(params string[] args) {
		System.Text.StringBuilder sb = new System.Text.StringBuilder();

		foreach (string arg in args) {
			sb.Append(arg + " ");
		}

		return sb.ToString();
	}


	private bool Invoke (ProcessStartInfo pinfo, params string[] search_locations) {
		for (var i = 0; i < search_locations.Length; i++) {
			try {
				pinfo.FileName = search_locations[i];
				//GD.Print("Trying cmd | " + pinfo.FileName + " " + pinfo.Arguments);
				var process = Process.Start(pinfo);
				process.WaitForExit();
				var exit = process.ExitCode;
				return exit == 0;
			} catch {
				continue;
			}
		}
		return false;
	}

	public override int Import(string sourceFile, string savePath, Godot.Collections.Dictionary options, Godot.Collections.Array platformVariants, Godot.Collections.Array genFiles) {
		// Find blender install
		var exe_name = "blender" + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty);

		ProcessStartInfo info = new ProcessStartInfo();
		info.WorkingDirectory = System.Environment.CurrentDirectory;
		info.CreateNoWindow = false;
		info.UseShellExecute = true;
		info.FileName = exe_name;
		info.WindowStyle = ProcessWindowStyle.Hidden;
		info.Arguments = BuildCommandLineArgs(
			"\"" + sourceFile.Replace("res:/", System.Environment.CurrentDirectory) + "\"", 
			"--background", 
			"--python", "\"" + System.IO.Path.Combine(System.Environment.CurrentDirectory, "addons", "BlenderAutoImport", "export.py") + "\"", 
			"--", $"\"{savePath.Replace("res:/", System.Environment.CurrentDirectory)}.{GetIntermediateExtension()}\"");

		// On path
		bool didImport = Invoke(info, exe_name);

		// Plain installation
		if (!didImport) {
			// Install location of all blender versions
			var path = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\Program Files\Blender Foundation\" : @"/usr/lib/blender/";
			if (System.IO.Directory.Exists(path)) {
				// Get highest version of Blender
				var versionSpecificPath = System.IO.Directory.GetDirectories(path).Where(p => p.StartsWith("Blender") || p.StartsWith("blender")).OrderByDescending(p => p).FirstOrDefault();
				// Import using that version
				if (versionSpecificPath != null)
					didImport = Invoke(info, System.IO.Path.Combine(path, versionSpecificPath, exe_name));
			}
		}

		// Steam installation
		if (!didImport) {
			didImport = Invoke(
				info,
				// 64 and 32 bit paths
				RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\Program Files\Steam\steamapps\common\Blender\" + exe_name : @"~/.local/share/Steam/Blender/" + exe_name,
				RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\Program Files (x86)\Steam\steamapps\common\Blender\" + exe_name : @"~/.steam/steam/SteamApps/common/Blender/" + exe_name
			);
		}

		// Check to see if we did import anything
		if (!didImport) {
			return (int)Godot.Error.Unavailable;
		} else {
			var scene = new PackedSceneGLTF();
			var model = scene.ImportGltfScene(savePath + "." + GetIntermediateExtension());
			scene.Pack(model);
			ResourceSaver.Save(
				savePath + "." + this.GetSaveExtension(),
				scene
			);
			return (int)Godot.Error.Ok;
		}
	}
}

}
