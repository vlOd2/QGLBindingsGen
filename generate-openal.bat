@echo off
curl "https://raw.githubusercontent.com/kcat/openal-soft/refs/heads/master/include/AL/al.h" -o "al.h"
curl "https://raw.githubusercontent.com/kcat/openal-soft/refs/heads/master/include/AL/alc.h" -o "alc.h"
python OpenAL\main.py