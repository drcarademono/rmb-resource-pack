import os
import json
import re

def update_model_id(obj):
    # Update ModelId if it's a string and matches the pattern 204xx
    if 'ModelId' in obj and re.match(r'204\d\d$', obj['ModelId']):
        obj['ModelId'] = '201' + obj['ModelId'][3:]

    # Update ModelIdNum if it's an integer in the range 20400-20499
    if 'ModelIdNum' in obj and isinstance(obj['ModelIdNum'], int):
        if 20400 <= obj['ModelIdNum'] <= 20499:
            obj['ModelIdNum'] = int('201' + str(obj['ModelIdNum'])[3:])

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

