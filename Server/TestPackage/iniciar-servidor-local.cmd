@echo off
title Remotely Server - Teste local
cd /d "%~dp0"

set ASPNETCORE_URLS=http://0.0.0.0:5000
set ASPNETCORE_ENVIRONMENT=Development

echo.
echo  Remotely Server - teste local
echo  =============================
echo.
echo  Painel neste PC: http://localhost:5000
echo  Outros PCs:      http://IP-DESTE-PC:5000
echo.
echo  Mantenha esta janela aberta durante o teste.
echo  Para encerrar, pressione Ctrl+C.
echo.

Remotely_Server.exe

echo.
echo O servidor foi encerrado.
pause
