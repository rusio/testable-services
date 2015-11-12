@Echo Off
@Echo Grants permissions for Port/URL reservation. This script must be run with admin rights.

SET COMMAND=netsh http add urlacl url=http://+:5050/ user=%USERDOMAIN%\%USERNAME%

@Echo Command: %COMMAND%
%COMMAND%
