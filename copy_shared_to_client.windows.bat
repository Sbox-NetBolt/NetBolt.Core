@ECHO OFF

DEL "NetBolt.Client/code/Shared" /S /Q
XCOPY "Shared" "NetBolt.Client/code/Shared" /D /E /I /Y