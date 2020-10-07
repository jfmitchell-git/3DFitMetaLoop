ECHO OFF
call "%CD%\Configuration\PublishBot.Config.cmd"



"%CD%\AutomationTools\bin\netcoreapp3.1\MetaLoop.DataImportTool.exe" importdb %EXCEL_PATH% -BaseUnityFolder:%UNITY3D_PATH%


pause