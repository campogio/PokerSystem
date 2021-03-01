echo Build type: %1

cd /d %~dp0 
xcopy bin\%1\netcoreapp3.1\en-GB ..\..\runtime\en-GB /e /i /Y
xcopy bin\%1\netcoreapp3.1\es-ES ..\..\runtime\es-ES /e /i /Y
xcopy bin\%1\netcoreapp3.1\pt-BR ..\..\runtime\pt-BR /e /i /Y
EXIT /B 0
