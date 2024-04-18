@echo off
setlocal

set "SLNDIR=%~dp0src"
dotnet test "%SLNDIR%\api-fingerprint.sln" --nologo
