@echo off
echo Running Unit Tests...
dotnet test --logger "console;verbosity=detailed"

echo Cleaning project...
dotnet clean

echo Building project...
dotnet build
