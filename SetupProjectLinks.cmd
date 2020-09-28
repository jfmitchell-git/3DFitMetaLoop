ECHO OFF

call "%CD%\Configuration\PublishBot.Config.cmd"

mklink /D "%UNITY3D_PATH%\Assets\MetaLoop.PlatformCommon" "%CD%\Common\PlatformCommon"

mklink /D "%CD%\GameLogic\Shared" "..\%UNITY3D_PATH%\Assets\DryGinStudios\WarSim\Meta"

pause

