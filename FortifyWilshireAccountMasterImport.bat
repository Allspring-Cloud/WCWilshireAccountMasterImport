@echo off

set MYPATH=A:\data\projects\WCComposite\TFS
set DEBUG=false
set BUILDID=WilshireAccountMasterImport
set PROJECTROOT0="%MYPATH%\%BUILDID%"
set USEVS=true
set PROJECTNAME="WilshireAccountMasterImport"

IF EXIST %0 (
   set SCAScriptFile=%0
) ELSE (
  set SCAScriptFile=%0.bat
)

CALL "%MYPATH%\common.bat"
rem pause

REM ################Exclude###################################
REM ARGS -exclude "PROJECTROOT0_MARKER\**\bin\**\*"
REM ARGS -exclude "PROJECTROOT0_MARKER\**\lib\**\*"

REM ################To be scanned#############################
REM ARGS "PROJECTROOT0_MARKER\PROJECT_MARKER"
REM ###############Library Files##############################
REM ARGS -libdirs "PROJECTROOT0_MARKER\packages\WCMDependentUtils.1.0.22\lib\net462\log4net.dll"
REM ARGS -libdirs "PROJECTROOT0_MARKER\packages\WCMCommon.1.0.2\lib\net462\WCMCommon.dll"
REM ARGS -libdirs "PROJECTROOT0_MARKER\packages\WCMDatabase.1.0.2\lib\net462\WCMDatabase.dll"
REM ARGS -libdirs "PROJECTROOT0_MARKER\packages\WCMDependentUtils.1.0.22\lib\net462\WCMDependentUtils.dll"
REM ARGS -libdirs "PROJECTROOT0_MARKER\packages\WCMDependentUtils.1.0.22\lib\net462\### AS ###Services.dll"
REM ARGS -libdirs "PROJECTROOT0_MARKER\packages\WCMDependentUtils.1.0.22\lib\net462\### AS ###Utils.dll"
REM ARGS -libdirs "PROJECTROOT0_MARKER\packages\WCMEmailCommon.1.0.2\lib\net462\WCMEmailCommon.dll"
REM ARGS -libdirs "PROJECTROOT0_MARKER\packages\WCMImportAPI.1.0.14\lib\net462\WCMImportAPI.dll"
