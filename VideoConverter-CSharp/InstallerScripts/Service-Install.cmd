@Echo Off
@Echo Install the Windows Service. This script must be run with admin rights.

SET SERVICE="D:\ccd\testable-services\example\VideoConverter\WindowsService\bin\Debug\VideoConverter.WindowsService.exe"
SET INSTALLUTIL=C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe

%INSTALLUTIL% %SERVICE%
