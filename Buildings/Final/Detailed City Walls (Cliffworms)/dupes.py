import os
import json
import hashlib

def file_hash(filepath):
    """Generate a hash value for the content of a file."""
    hasher = hashlib.sha256()
    with open(filepath, 'rb') as file:
        content = file.read()
        hasher.update(content)
    return hasher.hexdigest()

# Map to store file hash values and their corresponding filenames
hash_map = {}

# Loop through each file in the current directory
for filename in os.listdir('.'):
    if filename.endswith('.json'):
        # Calculate the file's hash
        filepath = os.path.join('.', filename)
        filehash = file_hash(filepath)
        
        # Check if the hash already exists in the map
        if filehash in hash_map:
            # This file is a duplicate, so delete it
            os.remove(filepath)
            print(f'Deleted duplicate file: {filename}')
        else:
            # This file is not a duplicate, so add its hash to the map
            hash_map[filehash] = filename

print('Duplicate files removed.')

