ECHO OFF

call "%CD%\MetaConfiguration\PublishBot.Config.cmd"

mklink /D "%UNITY3D_PATH%\Assets\MetaCommon" "%CD%\MetaCommon\Common"

rem path is relative from itself hence double back
mklink /D "%CD%\GameLogic\Shared" "..\%UNITY3D_PATH%\Assets\Scripts\DryGinStudios\WarSim\Meta"

pause

