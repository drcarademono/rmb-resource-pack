import os
import json

# Function to generate a new filename with an incrementing number if needed
def generate_new_filename(base_filename, extension='.json'):
    counter = 1
    new_filename = f"{base_filename}{extension}"
    while os.path.exists(new_filename):
        new_filename = f"{base_filename}-{str(counter).zfill(2)}{extension}"
        counter += 1
    return new_filename

# Loop through each file in the current directory
for filename in os.listdir('.'):
    if filename.endswith('.json'):
        with open(filename, 'r') as file:
            data = json.load(file)
        
        try:
            model_id = data['RmbSubRecord']['Exterior']['Block3dObjectRecords'][0]['ModelId']
            base_new_filename = model_id
            new_filename = generate_new_filename(base_new_filename)
            
            # Renaming the file
            os.rename(filename, new_filename)
            print(f'Renamed {filename} to {new_filename}')
        except (KeyError, IndexError) as e:
            print(f'Error processing {filename}: {e}')

