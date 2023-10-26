import re
import json
import sys

# Check if the JSON file name is provided as a command-line argument
if len(sys.argv) < 2:
    print("Please provide the JSON file name as a command-line argument.")
    sys.exit(1)

# Read the JSON data from the file
json_file = sys.argv[1]
with open(json_file, 'r') as f:
    data = json.load(f)

# Extracting the file paths
files = data['Files']

# Function to extract the numeric part of the string
def extract_numeric(file_path):
    return int(re.search(r'\d+', file_path).group())

# Sorting the files first by the alphanumeric part and then by the numeric part
files.sort(key=lambda x: (re.split(r'(\d+)', x.lower()), extract_numeric(x)))

# Writing the rearranged files back to the JSON
data['Files'] = files

# Writing the updated data back to the JSON file
with open(json_file, 'w') as f:
    json.dump(data, f, indent=4)

print(f"Files in {json_file} have been rearranged in alphabetical and numeric order.")

