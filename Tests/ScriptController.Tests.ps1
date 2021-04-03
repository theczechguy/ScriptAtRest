param (
    $ApplicationFolder,
    $Username,
    $UserPassword,
    $apiUrl,
    $ScriptName
)
BeforeAll {
    $exeFile = Join-Path -Path $ApplicationFolder -ChildPath "ScriptAtRestServer.exe"
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $Username,$UserPassword)))
    
    $process = Start-Process -FilePath $exeFile -WorkingDirectory $ApplicationFolder -PassThru
    Start-Sleep -Seconds 5

    $body = @{
        "firstname" = "Test"
        "lastname" = "Test"
        "username" = $Username
        "password" = $UserPassword
    } | ConvertTo-Json

    $response = Invoke-RestMethod `
        -Method Post `
        -Uri "$apiUrl/users/register" `
        -Body $body `
        -ContentType "application/json"
}

Describe "Script controller tests" {
    It "Register a new script" {
        $script = Get-Content .\TestScript.ps1

        $Bytes = [System.Text.Encoding]::UTF8.GetBytes($script)
        $encoded =[Convert]::ToBase64String($Bytes)

        $body = @{
            "name" = $ScriptName
            "EncodedContent" = $encoded
            "type" = 1
        }

        $response = Invoke-RestMethod `
            -Method Post `
            -Uri "$apiUrl/scripts/register" `
            -Body $body `
            -ContentType "application/json"
        
        $response.name | should -BeExactly $ScriptName
    }
}