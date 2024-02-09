import bpy
import sys

# Function to clear all objects in the scene
def clear_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    for obj in bpy.data.objects:
        bpy.data.objects.remove(obj, do_unlink=True)

# Function to rescale UVs of all mesh objects in the current scene
def rescale_uvs(scale_factor=0.5):
    for obj in bpy.context.scene.objects:
        if obj.type == 'MESH':
            bpy.context.view_layer.objects.active = obj
            bpy.ops.object.select_all(action='DESELECT')
            obj.select_set(True)
            bpy.ops.object.mode_set(mode='EDIT')
            for uvmap in obj.data.uv_layers:
                for data in uvmap.data:
                    data.uv[0] *= scale_factor  # Scale U coordinate
                    data.uv[1] *= scale_factor  # Scale V coordinate
            bpy.ops.object.mode_set(mode='OBJECT')

# Main function to process the FBX file
def process_fbx(fbx_file_path, scale_factor=0.5):
    clear_scene()
    # Import the FBX file
    bpy.ops.import_scene.fbx(filepath=fbx_file_path)

    # Rescale UVs
    rescale_uvs(scale_factor=scale_factor)

    # Export the processed scene back to FBX
    output_file_path = fbx_file_path.replace('.fbx', '_rescaled.fbx')
    bpy.ops.export_scene.fbx(filepath=output_file_path, use_selection=False)

    print(f"Processed and saved as {output_file_path}")

# Assuming the last argument passed to the script is the FBX file path
if __name__ == "__main__":
    if len(sys.argv) > 1:
        fbx_file_path = sys.argv[-1]  # Get the FBX file path from the command-line argument
        process_fbx(fbx_file_path)
    else:
        print("Error: No FBX file path provided.")

