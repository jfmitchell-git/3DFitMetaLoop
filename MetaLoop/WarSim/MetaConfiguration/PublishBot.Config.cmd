rem This is configuration file for PublishBot.
rem PublishBot uses MSBUILD 2017 due to build issues with 2019. To be updated...

ECHO Loading PublishBot.Config...
set MSBUILD_PATH="C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe"
set STAGING_PUBXML=%CD%\RESTfulApi\Properties\PublishProfiles\bioincbackofficewest2 - STAGING.pubxml
set PROD_PUBXML=%CD%\RESTfulApi\Properties\PublishProfiles\bioincbackofficewest2 - PROD.pubxml
set PROJECT_PATH=..\
set UNITY3D_PATH=..\WarSim_Unity