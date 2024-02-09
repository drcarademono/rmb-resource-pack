#!/bin/bash

# Loop through all FBX files in the current directory
for f in *.fbx; do
  echo "Processing $f..."
  # Use Blender in background mode, open a blank file, run your script, and then quit
  blender -b --python uv_rescale.py -- "$f"
  # Optionally, you can add commands to export the processed file to FBX again
  # This step may require adjusting your rescale_uv.py script to include export functionality
done
