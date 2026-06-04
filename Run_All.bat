@echo off
title Executar Tudo - ProjectUploader
echo Subindo API...
start cmd /c "Run_API.bat"

echo Aguardando 5 segundos para a API subir completamente e rodar os scripts de DB...
timeout /t 5 /nobreak >nul

echo Subindo ServerApp (DownloadApp)...
start cmd /c "Run_ServerApp.bat"

echo Subindo ClientApp (UploadApp)...
start cmd /c "Run_ClientApp.bat"

echo Tudo em execucao!
pause
