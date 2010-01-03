@echo off
cd %~dp0
set /p ServiceName=Enter the service name if different from 'BunnyBot': 

"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\installutil.exe" /ServiceName=%ServiceName% /u BunnyBot.exe
pause