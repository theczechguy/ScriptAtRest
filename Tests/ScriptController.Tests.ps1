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

    $testUserResponse = Invoke-RestMethod `
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
            -ContentType "application/json" `
            -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        
        $response.name | should -BeExactly $ScriptName
    }
}

AfterAll {
    $response = Invoke-RestMethod `
        -Method Get `
        -Uri "$apiUrl/users" `
        -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
    $user = $response | where username -eq $Username

Invoke-RestMethod `
    -Method Delete `
    -Uri "$apiUrl/users/$($user.id)" `
    -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}

    Stop-Process -InputObject $process -Force
}