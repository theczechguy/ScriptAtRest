param (
    $ApplicationFolder,
    $Username,
    $UserPassword,
    $apiUrl
)

Describe "User Controller Tests" {
    BeforeAll{
        $exeFile = Join-Path -Path $ApplicationFolder -ChildPath "ScriptAtRestServer.exe"
        
        $process = Start-Process -FilePath $exeFile -WorkingDirectory $ApplicationFolder -PassThru
        Start-Sleep -Seconds 5
    }

    It "Register new user" {
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
        
        $response.message | Should -Be "User Registered"
    }

    It "Get all users" {

        $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $Username,$UserPassword)))

        $response = Invoke-RestMethod `
            -Method Get `
            -Uri "$apiUrl/users" `
            -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        
        $response.Count | Should -BeGreaterOrEqual 1
    }

    
    AfterAll{
        Start-Sleep -Seconds 5
        Stop-Process -InputObject $process -Force
    }
}