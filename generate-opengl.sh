#!/bin/sh
wget "https://raw.githubusercontent.com/KhronosGroup/OpenGL-Registry/refs/heads/main/xml/gl.xml" -O "gl.xml"
python3 OpenGL/main.py