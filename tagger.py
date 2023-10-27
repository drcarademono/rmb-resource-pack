import json
import sys

def update_tags(json_file):
    with open(json_file, 'r') as f:
        data = json.load(f)

    for item in data["_list"]:
        if item["ID"].endswith("1"):
            item["Tags"] = "dirt"
        elif item["ID"].endswith("2"):
            item["Tags"] = "grass"
        elif item["ID"].endswith("3"):
            item["Tags"] = "rock"

    with open(json_file, 'w') as f:
        json.dump(data, f, separators=(',', ':'))


if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python json_tag_updater.py <json_file>")
        sys.exit(1)

    json_file = sys.argv[1]
    update_tags(json_file)
    print(f"Tags updated in {json_file} successfully.")

