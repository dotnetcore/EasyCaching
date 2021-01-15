@echo off
echo Starting Single: 
pushd %~dp0\Basic
echo   Master: 6380
@start "Redis (Master): 6380" /min ..\3.0.503\redis-server.exe master-6380.conf
popd