#!/bin/bash

for file in *.blend; do
    blender -b "$file" -P convert_to_fbx.py
done

