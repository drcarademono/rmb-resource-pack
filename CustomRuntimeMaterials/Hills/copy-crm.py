import os
import shutil

# Set the path to the folder containing the prefab files and the source JSON file
target_folder = '.'
source_json_file = os.path.join(target_folder, '52003.json')
source_json_basename = os.path.splitext(os.path.basename(source_json_file))[0]

# Function to check if a file name (without extension) ends with 1, 4, or 7
def should_copy(filename):
    return filename[-1] in ['3', '6', '9'] and filename != source_json_basename

# List all files in the target folder
all_files = os.listdir(target_folder)

# Filter prefab files ending with 1, 4, or 7
prefab_files = [f for f in all_files if f.endswith('.prefab') and should_copy(os.path.splitext(f)[0])]

# Copy and rename the source JSON file for each matching prefab file
for prefab_file in prefab_files:
    # Extract the base name without extension
    base_name = os.path.splitext(prefab_file)[0]
    # Set the target JSON file name
    target_json_file = os.path.join(target_folder, f'{base_name}.json')
    # Skip copying if the source and target are the same
    if source_json_file == target_json_file:
        continue
    # Copy and rename the source JSON file
    shutil.copy(source_json_file, target_json_file)
    print(f'Copied and renamed JSON to: {target_json_file}')

