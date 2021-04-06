param (
    $ApplicationFolder
)
BeforeAll{
    $exeFile = Join-Path -Path $ApplicationFolder -ChildPath "ScriptAtRestServer.exe"
    $apiUrl = 'http://localhost:5000/'
    
    Start-Process -FilePath $exeFile -WorkingDirectory $ApplicationFolder
    $username = "StressTest{0}" -f (Get-Random -Minimum 1 -Maximum 10)


    $body = @{
        "firstname" = "Stress"
        "lastname" = "Test"
        "username" = 'stressuser'
        "password" = "test"
    } | ConvertTo-Json

    Invoke-RestMethod `
        -Method Post `
        -Uri "$apiUrl/users/register" `
        -Body $body `
        -ContentType "application/json"



    $body = @{
        "name" = "stress"
        "content" = "start-sleep -seconds 5 ; write-host 'ahoj kamaradi'"
        "type" = 1
    }

    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f 'stressuser','test')))

    $scriptObject = Invoke-RestMethod `
        -Method Post `
        -Uri "$apiUrl/scripts/register" `
        -Body $body `
        -ContentType 'application/json' `
        -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
}

Describe "Puts pressure on api" {
    It "Request 20 script executions at the same time" {
        1..20 | ForEach-Object -ThrottleLimit 20 -Parallel {
            $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f 'stressuser','test')))
            Invoke-RestMethod -Method Post `
                -Uri "http://localhost:5000/scripts/run/$($using:scriptobject.id)" `
                -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        }
    }
}





<#

Describe "Run multiple scripts execute requests stress test" {
    It "Should return output from 20 script executions" {
        
    }
}

$base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f 'testuser1','nejvetsisecret')))


for ($i = 0; $i -lt 10; $i++) {
    Invoke-RestMethod -Method Post `
        -Uri 'http://localhost:5000/scripts/run/6' `
        -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}   
}

1..100 | ForEach-Object -ThrottleLimit 50 -Parallel {
    $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f 'testuser1','nejvetsisecret')))
    Invoke-RestMethod -Method Post `
        -Uri 'http://localhost:5000/scripts/run/6' `
        -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
}
#>