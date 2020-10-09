rem This is configuration file for PublishBot.
rem PublishBot uses MSBUILD 2017 due to build issues with 2019. To be updated...

ECHO Loading PublishBot.Config...

set MSBUILD_PATH="C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"

set STAGING_PUBXML="%CD%\RESTApi\Properties\PublishProfiles\bioinredemptionmobile - Web Deploy.pubxml"
set PROD_PUBXML="%CD%\RESTApi\Properties\PublishProfiles\bioinredemptionmobile - Web Deploy.pubxml"

set PROJECT_PATH=..\BioIncRedemptionMobile
set UNITY3D_PATH=..\BioIncRedemptionMobile\UnityMobile
set EXCEL_PATH=BioIncRedemptionMetaLoop.xlsx
