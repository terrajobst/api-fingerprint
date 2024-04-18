@echo off
setlocal

set "SLNDIR=%~dp0src"
dotnet build "%SLNDIR%\api-fingerprint.sln" --nologo
