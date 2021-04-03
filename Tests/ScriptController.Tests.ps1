param (
    $ApplicationFolder,
    $Username,
    $UserPassword,
    $apiUrl
)
BeforeAll {
    $exeFile = Join-Path -Path $ApplicationFolder -ChildPath "ScriptAtRestServer.exe"
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $Username,$UserPassword)))
    
    $process = Start-Process -FilePath $exeFile -WorkingDirectory $ApplicationFolder -PassThru
    Start-Sleep -Seconds 5
}