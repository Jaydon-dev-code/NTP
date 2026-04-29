@echo off
chcp 65001
echo ======================================
echo    以管理员权限彻底卸载服务
echo    无需重启，服务立即消失
echo ======================================

:: 第一步：标准卸载
%windir%\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe /u "%~dp0SL.MLineDataPrecisionTracking.Service.exe"

:: 第二步：强制删除服务（彻底消失，不需要重启）
sc delete "SLMLineDataPrecisionTracking.Service"

echo.
echo 服务已彻底卸载并删除！
echo 刷新服务列表即可看到服务已消失！
echo.
pause