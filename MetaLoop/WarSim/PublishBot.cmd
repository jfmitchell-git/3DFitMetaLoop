ECHO OFF
call "%CD%\MetaConfiguration\PublishBot.Config.cmd"
GOTO ASK
:ASK
ECHO --------------------------------------------------
ECHO Wich environement should we build? (staging/prod)
ECHO --------------------------------------------------


set /P INPUT=Type environement: %=%
set /P TIMEOUT=Timeout Kick:%=%
set /P TIMEOUT_CACHE=Timeout AFTER:%=%
set /P VERSIONID=MAJOR VERSION:%=%

set /P CDNONLY=CDN Sync Only? (y/*): %=%



If "%INPUT%"=="staging" goto STAGING
If "%INPUT%"=="prod" goto PROD

ECHO Invalid input option
GOTO ASK

:STAGING
ECHO --------------------------------------------------
ECHO Building.... STAGING
ECHO --------------------------------------------------
%MSBUILD_PATH% /p:Configuration=Release /p:DefineConstants="BACKOFFICE;STAGING;DISABLE_PLAYFABCLIENT_API" %CD%\MetaBackend.sln /t:Clean,Build /p:DeployOnBuild=false /p:PublishProfile=%STAGING_PUBXML% /verbosity:quiet /p:WarningLevel=0

IF NOT %ERRORLEVEL%==0 GOTO ERROR
If "%CDNONLY%"=="y" goto CDNONLY_PUSH

ECHO --------------------------------------------------
ECHO Preparing Release, Shutting down servers....
ECHO --------------------------------------------------
"%CD%\PlayFabCdnManager\bin\Release\PlayFabCdnManager.exe" -status offline %TIMEOUT%



ECHO --------------------------------------------------
ECHO Deploying to staging....
ECHO --------------------------------------------------

%MSBUILD_PATH% /p:Configuration=Release /p:DefineConstants="BACKOFFICE;STAGING;DISABLE_PLAYFABCLIENT_API" %CD%\MetaBackend.sln /t:Clean,Build /p:DeployOnBuild=true /p:PublishProfile=%STAGING_PUBXML% /verbosity:quiet /p:WarningLevel=0


GOTO STEP2

:PROD
ECHO --------------------------------------------------
ECHO Writing App Version... PRODUCTION
ECHO --------------------------------------------------

"%CD%\PlayFabCdnManager\bin\Release\PlayFabCdnManager.exe" -appversion %VERSIONID% "%CD%\RESTApi\AppVersion.txt


ECHO --------------------------------------------------
ECHO Building... PRODUCTION
ECHO --------------------------------------------------
%MSBUILD_PATH% /p:Configuration=Release /p:DefineConstants="BACKOFFICE;DISABLE_PLAYFABCLIENT_API" %CD%\MetaBackend.sln /t:Clean,Build /p:DeployOnBuild=false /p:PublishProfile=%PROD_PUBXML% /verbosity:quiet /p:WarningLevel=0

IF NOT %ERRORLEVEL%==0 GOTO ERROR
If "%CDNONLY%"=="y" goto CDNONLY_PUSH


ECHO --------------------------------------------------
ECHO Preparing Release, Shutting down servers....
ECHO --------------------------------------------------
"%CD%\PlayFabCdnManager\bin\Release\PlayFabCdnManager.exe" -status offline %TIMEOUT%


ECHO --------------------------------------------------
ECHO Deployging to production
ECHO --------------------------------------------------
%MSBUILD_PATH% /p:Configuration=Release /p:DefineConstants="BACKOFFICE;DISABLE_PLAYFABCLIENT_API" %CD%\MetaBackend.sln /t:Clean,Build /p:DeployOnBuild=true /p:PublishProfile=%PROD_PUBXML% /verbosity:quiet /p:WarningLevel=0

GOTO STEP2

:STEP2
IF NOT %ERRORLEVEL%==0 GOTO ERROR
GOTO DONE

:ERROR
ECHO --------------------------------------------------
ECHO ERROR HAS OCCURED, BUILD FAILED!
ECHO --------------------------------------------------
pause
GOTO END

:DONE
ECHO --------------------------------------------------
ECHO "Build and publish to AZURE COMPLETED... Running PlayFabCdnManager..."
ECHO --------------------------------------------------
"%CD%\PlayFabCdnManager\bin\Release\PlayFabCdnManager.exe" "-path:%PROJECT_PATH%"


ECHO(
ECHO --------------------------------------------------
ECHO PATCH PROCESS COMPLETED....
ECHO --------------------------------------------------

ECHO --------------------------------------------------
ECHO Finishing Release, Setting servers Online....
ECHO --------------------------------------------------

If "%INPUT%"=="staging" goto STAGING_finish
If "%INPUT%"=="prod" goto PROD_finish

:STAGING_finish
"%CD%\PlayFabCdnManager\bin\Release\PlayFabCdnManager.exe" -status online %TIMEOUT_CACHE% %VERSIONID%
GOTO COMPLETE

:PROD_finish
"%CD%\PlayFabCdnManager\bin\Release\PlayFabCdnManager.exe" -status online %TIMEOUT_CACHE% %VERSIONID%
GOTO COMPLETE

:CDNONLY_PUSH
ECHO --------------------------------------------------
ECHO "Running PlayFabCdnManager... (CDN ONLY MODE)"
ECHO --------------------------------------------------
"%CD%\PlayFabCdnManager\bin\Release\PlayFabCdnManager.exe" -cdnonly "-path:%PROJECT_PATH%"
GOTO COMPLETE

:COMPLETE
pause
:END