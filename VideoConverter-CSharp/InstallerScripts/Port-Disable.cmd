@Echo Off
@Echo Deletes permissions for Port/URL reservation. This script must be run with admin rights.

SET COMMAND=netsh http delete urlacl url=http://+:5050/

@Echo Command: %COMMAND%
%COMMAND%
