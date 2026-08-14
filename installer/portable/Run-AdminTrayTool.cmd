@echo off
REM Lightweight launcher for portable AdminTrayTool
REM This script attempts to place a default config.json into ProgramData\AdminTrayTool if writable,
REM otherwise will leave the app to use per-user config under %%APPDATA%%. It then starts the exe.

setlocal enabledelayedexpansion
set EXE=AdminTrayTool.exe
set SRC_CONFIG=%~dp0config.json
set PROGDATA_DIR=%ProgramData%\AdminTrayTool

necho Starting AdminTrayTool (portable)

nif exist "%~dp0%EXE%" (
	echo Found %EXE% in folder.
) else (
	echo ERROR: %EXE% not found in this folder (%~dp0).
	pause
	exit /b 2
)

nrem Try to create ProgramData folder if possible and copy default config if missing
if not exist "%PROGDATA_DIR%" (
	echo Attempting to create %PROGDATA_DIR% ...
	md "%PROGDATA_DIR%" >nul 2>&1
)

nif exist "%PROGDATA_DIR%" (
	rem check write permission by creating a temp file
	>"%PROGDATA_DIR%\.writetest" echo test >nul 2>&1
	if exist "%PROGDATA_DIR%\.writetest" (
		del /f /q "%PROGDATA_DIR%\.writetest" >nul 2>&1
		echo ProgramData folder writable.
		if exist "%PROGDATA_DIR%\config.json" (
			echo Existing program-wide config detected; leaving it in place.
		) else (
			if exist "%SRC_CONFIG%" (
				copy /Y "%SRC_CONFIG%" "%PROGDATA_DIR%\config.json" >nul
				if %errorlevel%==0 echo Default config copied to %PROGDATA_DIR%\config.json
			)
		)
	) else (
		echo ProgramData not writable for this user; the app will fall back to per-user config under %%APPDATA%%.
	)
) else (
	echo Could not create ProgramData folder; the app will fall back to per-user config under %%APPDATA%%.
)

necho Launching %EXE% ...
start "" "%~dp0%EXE%"
endlocal
exit /b 0
