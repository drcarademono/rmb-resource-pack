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

# Extracting the list of items from the JSON
items = data["_list"]

# Function to extract the numeric part of the string
def extract_numeric(item):
    match = re.search(r'(\d+\.\d+)', item['Label'])
    return float(match.group()) if match else None

# Sorting the items by the alphanumeric part and then by the numeric part
items.sort(key=lambda x: [int(s) if s.isdigit() else s for s in re.split(r'(\d+)', x['Label'].lower())])

# Writing the rearranged items back to the JSON
data["_list"] = items

# Writing the updated data back to the JSON file
with open(json_file, 'w') as f:
    json.dump(data, f, indent=4)

print(f"Items in {json_file} have been rearranged in alphabetical and numeric order based on the 'Label' field.")

