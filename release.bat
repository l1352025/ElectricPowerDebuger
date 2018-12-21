@echo off

rem :::::::::::::  需配置 工程文件名projName，目标文件名objName 其他默认 :::::::::::::::::

set projName=ElectricPowerDebuger

set objName=电力调试助手

set srcPath=E:\code\%projName%
 
set dstPath=G:\release\%objName%


rem :::::::::::::  拷贝发布文件  ::::::::::::::

del /q /f %dstPath%\%objName%\* 

xcopy  %srcPath%\%projName%\bin\Release\%projName%.exe  %dstPath%\%objName%\ /y /d


rem :::::::::::::  打包发布文件  ::::::::::::::

for /f "eol=-" %%i in (ReleaseLog.txt) do (
set execName=%%~nxi
goto pack
)
:pack
	move %dstPath%\%objName%\%projName%.exe %dstPath%\%objName%\%execName%.exe 
	rar a -ep1 %dstPath%\%execName%.rar %dstPath%\%objName%


rem :::::::::::::  拷贝到工作目录  ::::::::::::::

set workPath=E:\调试工具
xcopy  %dstPath%\%objName%  %workPath%\%objName%\ /s /y


rem :::::::::::::  创建桌面快捷方式  ::::::::::::::

set fileName=%workPath%\%objName%\%execName%.exe
set shortName=%execName%.exe

:makeLink
	set TargetPath="%fileName%"
	set ShortcutPath="%userprofile%\desktop\%shortName%.lnk"
	set IconLocation="%fileName%,0"
	set HotKey=""

	echo Set WshShell=WScript.CreateObject("WScript.Shell") >tmp.vbs
	echo Set Shortcut=WshShell.CreateShortCut(%ShortcutPath%) >>tmp.vbs
	echo Shortcut.Hotkey = %HotKey% >>tmp.vbs
	echo Shortcut.IconLocation=%IconLocation% >>tmp.vbs
	echo Shortcut.TargetPath=%TargetPath% >>tmp.vbs
	echo Shortcut.Save >>tmp.vbs
	
	call tmp.vbs
	del /f /s /q tmp.vbs

rem :::::::::::::  Release 完成 ::::::::::::::

pause

start %dstPath%


