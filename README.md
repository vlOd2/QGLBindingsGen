# QGLBindingsGen
GLFW, OpenGL and OpenAL C# bindings generator for QuickGL

# How to use
- Run `generate.bat/.sh` and it will setup and run all generators

# GLFW: How to use
- Run `generate-glfw.bat/.sh` (needs curl/wget)

Alternatively you can:
1. Download the latest GLFW include header:<br>
`wget https://raw.githubusercontent.com/glfw/glfw/refs/heads/master/include/GLFW/glfw3.h` (use `curl` on windows)
2. Run the generator:<br>
`python3 GLFW/main.py` (`python` on Windows)

# OpenGL: How to use
- Run `generate-opengl.bat/.sh` (needs curl/wget)

Alternatively you can:
1. Download the latest OpenGL XML registry:<br>
`wget https://raw.githubusercontent.com/KhronosGroup/OpenGL-Registry/refs/heads/main/xml/gl.xml` (use `curl` on windows)
2. Run the generator:<br>
`python3 OpenGL/main.py` (`python` on Windows)

# OpenAL: How to use
- Run `generate-openal.bat/.sh` (needs curl/wget)

Alternatively you can:
1. Download the latest OpenAL include headers:<br>
`wget https://raw.githubusercontent.com/kcat/openal-soft/refs/heads/master/include/AL/al.h` (use `curl` on windows)
`wget https://raw.githubusercontent.com/kcat/openal-soft/refs/heads/master/include/AL/alc.h` (use `curl` on windows)
2. Run the generator:<br>
`python3 OpenAL/main.py` (`python` on Windows)

# ⚖ License
This project is licensed under the MIT license
<br>
You may refer to the "LICENSE" file for more information
