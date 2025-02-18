@echo off
dotnet test
IF %ERRORLEVEL% NEQ 0 (
    echo Tests fehlgeschlagen! Build wird nicht fortgesetzt.
    exit /b %ERRORLEVEL%
)
dotnet clean
dotnet build
