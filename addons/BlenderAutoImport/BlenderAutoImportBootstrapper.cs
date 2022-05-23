using Godot;
using System;

namespace Qkmaxware.GodotAddons.Import {

[Tool]
public class BlenderAutoImportBootstrapper : EditorPlugin {
    private EditorImportPlugin plugin = new BlenderAutoImport();

	public override void _EnterTree() {
		base._EnterTree();
		this.AddImportPlugin(plugin);
	}

	public override void _ExitTree() {
		base._ExitTree();
		this.RemoveImportPlugin(plugin);
	}
}

}