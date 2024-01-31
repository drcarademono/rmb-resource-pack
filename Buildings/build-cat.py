import json
import os

# Initialize data structures
output_data = {
    "list": [],
    "templates": {}
}

# Function to extract details from filename
def extract_details_from_filename(filename):
    # Extract parts of the filename without extension
    parts = os.path.splitext(os.path.basename(filename))[0].split('-')
    return parts if len(parts) == 3 else None

# Function to process the JSON files
def process_json_file(json_file, id_counter, subcategory_counts):
    details = extract_details_from_filename(json_file)
    if not details:
        return id_counter  # Skip files with incorrect naming format

    category, subcategory, label = details

    # Update the subcategory count
    subcategory_key = (category, subcategory)
    subcategory_counts[subcategory_key] = subcategory_counts.get(subcategory_key, 0) + 1

    id_label = f"{id_counter:04}"

    # Append the data to the output list
    output_data["list"].append({
        "ID": id_label,
        "Label": label,
        "Category": category,
        "Subcategory": subcategory,
        "Filename": json_file  # Temporarily store filename for later processing
    })

    # Read and add template data
    with open(json_file, "r") as f:
        data = json.load(f)
        output_data["templates"][id_label] = data

    return id_counter + 1

# Function to recursively find JSON files
def find_json_files_recursively(start_path):
    json_files = []
    for root, dirs, files in os.walk(start_path):
        for file in files:
            if file.endswith(".json"):
                json_files.append(os.path.join(root, file))
    return json_files

# Process all JSON files found recursively in the current directory
id_counter = 3000
subcategory_counts = {}
json_files = find_json_files_recursively(".")

for file in json_files:
    id_counter = process_json_file(file, id_counter, subcategory_counts)

# Add the bracketed count to each Subcategory
for item in output_data["list"]:
    cat_subcat_key = (item["Category"], item["Subcategory"])
    count = subcategory_counts[cat_subcat_key]
    item["Subcategory"] = f"{item['Subcategory']} [{count}]"

# Remove the temporary filename field
for item in output_data["list"]:
    del item["Filename"]

# Sort the list alphabetically by Category, then by Subcategory, and finally by Label
output_data["list"].sort(key=lambda x: (x['Category'], x['Subcategory'], x['Label']))

# Output the final data to a JSON file
output_file = "output.json"
with open(output_file, "w") as f:
    json.dump(output_data, f, indent=4)

print(f"Output written to {output_file}")

