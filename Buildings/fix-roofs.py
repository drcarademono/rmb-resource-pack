import os
import json
import re

def is_in_specified_range(model_id):
    specific_numbers = {2003, 2005, 2010, 2011, 2013, 2104, 2105}
    ranges = [(2015, 2035), (2109, 2115), (2117, 2135)]

    if model_id in specific_numbers:
        return False
    for (start, end) in ranges:
        if start <= model_id <= end:
            return False
    return True

def update_model_id(obj):
    model_id = obj.get('ModelId')
    model_id_num = obj.get('ModelIdNum')

    if isinstance(model_id_num, int) and 2000 <= model_id_num < 3000:
        if is_in_specified_range(model_id_num):
            obj['YPos'] += 128
            obj['ZPos'] -= 128

        obj['ModelIdNum'] = int('28' + str(model_id_num)[2:])

def process_block3dobjectrecords(section):
    if 'Block3dObjectRecords' in section:
        for record in section['Block3dObjectRecords']:
            update_model_id(record)

def find_replace_in_json(file_path):
    try:
        with open(file_path, 'r', encoding='utf-8') as file:
            data = json.load(file)

        modified = False
        if 'RmbSubRecord' in data and 'Exterior' in data['RmbSubRecord']:
            process_block3dobjectrecords(data['RmbSubRecord']['Exterior'])
            modified = True

        if modified:
            with open(file_path, 'w', encoding='utf-8') as file:
                json.dump(data, file, indent=4)
            print(f"Updated file: {file_path}")

    except Exception as e:
        print(f"Error processing file {file_path}: {e}")

def find_json_files(directory):
    for root, dirs, files in os.walk(directory):
        for file in files:
            if file.endswith('.json'):
                yield os.path.join(root, file)

def main():
    for json_file in find_json_files('.'):
        find_replace_in_json(json_file)

if __name__ == "__main__":
    main()

