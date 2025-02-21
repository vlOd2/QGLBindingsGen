#!/bin/sh
wget "https://raw.githubusercontent.com/kcat/openal-soft/refs/heads/master/include/AL/al.h" -O "al.h"
wget "https://raw.githubusercontent.com/kcat/openal-soft/refs/heads/master/include/AL/alc.h" -O "alc.h"
python3 OpenAL/main.py