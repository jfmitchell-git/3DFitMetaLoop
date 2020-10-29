ECHO OFF
call "%CD%\Configuration\PublishBot.Config.cmd"


ECHO --------------------------------------------------
ECHO Building.... 
ECHO --------------------------------------------------

%MSBUILD_PATH% /p:Configuration=Release /p:DefineConstants="BACKOFFICE;STAGING;DISABLE_PLAYFABCLIENT_API" %CD%\MetaLoop.sln /t:Clean,Build /p:DeployOnBuild=false /p:PublishProfile=%STAGING_PUBXML% /verbosity:quiet /p:WarningLevel=0


"%CD%\AutomationTools\bin\netcoreapp3.1\MetaLoop.DataImportTool.exe" importdb %EXCEL_PATH% -BaseUnityFolder:%UNITY3D_PATH%


pause