import os
import shutil

# Set the path to the folder containing the prefab files and the source JSON file
target_folder = '.'
source_json_file = os.path.join(target_folder, '53000.json')

# List all prefab files in the target folder
all_files = os.listdir(target_folder)
prefab_files = [f for f in all_files if f.endswith('.prefab')]

# Copy and rename the source JSON file for each prefab file
for prefab_file in prefab_files:
    # Extract the base name without extension
    base_name = os.path.splitext(prefab_file)[0]
    # Set the target JSON file name
    target_json_file = os.path.join(target_folder, f'{base_name}.json')
    # Copy and rename the source JSON file
    shutil.copy(source_json_file, target_json_file)
    print(f'Copied and renamed JSON to: {target_json_file}')

