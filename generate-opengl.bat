@echo off
curl "https://raw.githubusercontent.com/KhronosGroup/OpenGL-Registry/refs/heads/main/xml/gl.xml" -o "gl.xml"
python OpenGL\main.py