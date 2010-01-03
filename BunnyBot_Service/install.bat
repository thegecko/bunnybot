@echo off
cd %~dp0
set ServiceName=BunnyBot
set /p ServiceName=Enter the service name if different from '%ServiceName%': 

"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727\installutil.exe" /ServiceName=%ServiceName% /i BunnyBot.exe
net start %ServiceName%
pause