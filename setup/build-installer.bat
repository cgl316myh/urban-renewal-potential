@echo off
chcp 65001 >nul
setlocal
set "ISCC=%ProgramFiles%\Inno Setup 7\ISCC.exe"
if not exist "%ISCC%" set "ISCC=%ProgramFiles(x86)%\Inno Setup 7\ISCC.exe"
if not exist "%ISCC%" (
  echo 未找到 Inno Setup 7：ISCC.exe
  echo 请确认已安装到 Program Files\Inno Setup 7
  exit /b 1
)

set "ROOT=%~dp0"
set "ISS=%ROOT%UrbanRenewal.iss"
set "DBG=%ROOT%..\src\UrbanRenewal.Host\bin\Debug\UrbanRenewal.Host.exe"
set "ICO=%ROOT%UrbanRenewal.ico"

if not exist "%DBG%" (
  echo 未找到 Debug 主程序：
  echo   %DBG%
  echo 请先用 Visual Studio / MSBuild 生成 Configuration=Debug 解决方案。
  exit /b 1
)

if not exist "%ICO%" (
  echo 未找到安装包图标：
  echo   %ICO%
  exit /b 1
)

echo 使用: "%ISCC%"
echo 脚本: "%ISS%"
echo 源目录: Debug
"%ISCC%" "%ISS%"
if errorlevel 1 exit /b 1
echo.
echo 安装包已生成到: %ROOT%output\
dir /b "%ROOT%output\*.exe"
endlocal
