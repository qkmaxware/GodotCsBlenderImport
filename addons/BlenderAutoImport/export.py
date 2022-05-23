import bpy
import sys

# https://docs.blender.org/api/current/bpy.ops.export_scene.html?highlight=bpy%20ops%20export_scene%20gltf#bpy.ops.export_scene.gltf
print("Exporting scene to " + sys.argv[-1] + "...")
bpy.ops.export_scene.gltf(filepath=sys.argv[-1], check_existing=False, export_format='GLB', export_apply=True)
print("done")