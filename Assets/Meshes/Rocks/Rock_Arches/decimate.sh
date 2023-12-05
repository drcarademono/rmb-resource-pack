#!/bin/bash

# Check if Blender is installed
if ! command -v blender &> /dev/null; then
    echo "Blender is not installed. Please install Blender and make sure it's in your PATH."
    exit 1
fi

# Check if the decimate.py script exists
if [ ! -f "decimate.py" ]; then
    echo "The decimate.py script is not found in the current directory."
    exit 1
fi

# Loop through all .blend files in the current directory
for blend_file in *.blend; do
    # Run Blender with the decimate.py script
    blender --background --python decimate.py "$blend_file"
done

echo "Decimation process completed for all .blend files in the current directory."

