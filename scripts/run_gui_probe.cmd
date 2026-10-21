@echo off
cd /d C:\Users\ft8132
if not exist C:\Users\ft8132\ft8132-gui mkdir C:\Users\ft8132\ft8132-gui
del /q C:\Users\ft8132\ft8132-gui\probe_result.json 2>nul
C:\Users\ft8132\.local\bin\uv.exe run --python 3.12 --with pywinauto --with pillow python C:\Users\ft8132\win_gui_probe.py %* > C:\Users\ft8132\ft8132-gui\probe_stdout.txt 2>&1
echo EXIT=%ERRORLEVEL%>> C:\Users\ft8132\ft8132-gui\probe_stdout.txt
