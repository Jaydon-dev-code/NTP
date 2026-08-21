@echo off
chcp 65001
title 安装Windows服务
echo ==============================
echo  正在安装服务...
echo ==============================
:: 自动取当前目录下的 exe（和bat同目录）
set "SERVICE_EXE=%~dp0SL.MLineDataPrecisionTracking.Service.exe"
set "SERVICE_NAME=SL.MLineDataPrecisionTracking.Service"

if not exist "%SERVICE_EXE%" (
    echo 错误：找不到服务程序 %SERVICE_EXE%
    pause
    exit /b
)

:: 安装服务
%windir%\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe "%SERVICE_EXE%"
if %errorlevel% neq 0 (
    echo 服务安装失败！
    pause
    exit /b
)

echo.
echo ==============================
echo  配置故障重启规则：间隔60秒、最多重试3次、24小时重置计数
echo ==============================
:: reset=86400  失败计数24小时(86400秒)自动清零
:: actions=三次重启，每次延迟60000毫秒=60秒
sc failure "%SERVICE_NAME%" reset=86400 actions=restart/60000/restart/120000/restart/180000
:: 设置自动启动
sc config "%SERVICE_NAME%" start=auto

echo.
echo ==============================
echo  正在启动服务...
echo ==============================
net start "%SERVICE_NAME%"

echo.
echo 服务安装+配置完成！
pause