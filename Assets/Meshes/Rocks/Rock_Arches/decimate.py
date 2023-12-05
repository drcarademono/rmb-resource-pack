import bpy
import os
import sys

# Check if a command-line argument is provided
if len(sys.argv) < 4:
    print("Usage: blender --background --python script.py -- <input_file>")
    sys.exit(1)

# Get the input file path from the command-line argument
blend_file_path = sys.argv[4]

# Load the Blender file
bpy.ops.wm.open_mainfile(filepath=blend_file_path)

# Set the decimation ratio (0.5 for 50% reduction)
decimation_ratio = 0.6

# Iterate through all objects in the scene and apply the decimation modifier
for obj in bpy.context.scene.objects:
    bpy.context.view_layer.objects.active = obj
    bpy.ops.object.modifier_add(type='DECIMATE')
    bpy.context.object.modifiers["Decimate"].ratio = decimation_ratio
    bpy.ops.object.modifier_apply({"object": obj}, modifier="Decimate")

# Get the filename without extension
filename = os.path.splitext(os.path.basename(blend_file_path))[0]

# Save the result to the current directory with a new filename
output_file_path = os.path.join(os.path.dirname(blend_file_path), f"{filename}_decimated.blend")
bpy.ops.wm.save_as_mainfile(filepath=output_file_path)

