@echo off
chcp 65001
title 安装Windows服务
echo ==============================
echo  正在安装服务...
echo ==============================
:: 自动取当前目录下的 exe（和bat同目录）
set "SERVICE_EXE=%~dp0SL.MLineDataPrecisionTracking.Service.exe"
%windir%\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe "%SERVICE_EXE%"
echo.
echo 安装完成！
pause