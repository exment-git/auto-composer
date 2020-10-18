Copy-Item ExmentAutoUpdate\ExmentAutoUpdate\bin\Release\ExmentAutoUpdate.exe dist\ -Force
Copy-Item ExmentAutoUpdate\ExmentAutoUpdate\bin\Release\ExmentAutoUpdate.exe.config dist\ -Force
Copy-Item ExmentAutoUpdate\ExmentAutoUpdate\bin\Release\Newtonsoft.Json.dll dist\ -Force

Remove-Item -Path dist\ExmentAutoUpdateWindows.zip -Force

Compress-Archive -Path dist\* -DestinationPath dist\ExmentAutoUpdateWindows.zip
