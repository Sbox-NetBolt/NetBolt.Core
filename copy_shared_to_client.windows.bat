@ECHO OFF

DEL "NetBolt.Client\code\Shared" /S /Q
XCOPY "NetBolt.Shared" "NetBolt.Client\code\Shared" /D /E /I /Y /Exclude:copy_exclusion.txt