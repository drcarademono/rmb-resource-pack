import csv
import os
import shutil

# Create the output directory
output_directory = 'out'
if not os.path.exists(output_directory):
    os.makedirs(output_directory)

# Load the pattern.csv file
lookup_table = {}
with open('pattern.csv', mode='r') as file:
    reader = csv.reader(file)
    for row in reader:
        if len(row) == 2:
            lookup_table[row[0]] = row[1]

# Rename files in the current directory
for filename in os.listdir('.'):
    if filename != 'pattern.csv' and os.path.isfile(filename):
        for key in lookup_table:
            if key in filename:
                new_filename = filename.replace(key, lookup_table[key])
                shutil.copy(filename, os.path.join(output_directory, new_filename))

print("Files have been renamed and exported to the 'out' directory.")

