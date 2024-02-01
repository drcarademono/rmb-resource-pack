import os
import json
import re

def update_model_id(obj):
    if 'ModelId' in obj and re.match(r'201\d\d$', obj['ModelId']):
        obj['ModelId'] = '198' + obj['ModelId'][3:]

    if 'ModelIdNum' in obj and isinstance(obj['ModelIdNum'], int) and 19100 <= obj['ModelIdNum'] <= 19199:
        obj['ModelIdNum'] = int('208' + str(obj['ModelIdNum'])[3:])

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

