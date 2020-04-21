@echo off

for /f "tokens=*" %%s in ('findstr /C:"public const string Version = @" %~dp0\AutoSplitVideo.WPF\Service\UpdateChecker.cs') do (
    set version=%%s
)
set version=%version:~32,-2%

echo Version: %version%

echo package .NET Core SelfContained x64
7z a -mx9 AutoSplitVideo-Win64-%version%.7z %~dp0\AutoSplitVideo.WPF\bin\Release\netcoreapp3.1\win-x64\publish\
7z rn AutoSplitVideo-Win64-%version%.7z publish AutoSplitVideo
