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
    Get-ChildItem -Path $ApplicationFolder -Filter "LocalDatabase.db" | Remove-Item -Force
    
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
            "Name" = $ScriptName
            "EncodedContent" = $encoded
            "Type" = 1
        } | ConvertTo-Json

        $response = Invoke-RestMethod `
            -Method Post `
            -Uri "$apiUrl/scripts/register" `
            -Body $body `
            -ContentType "application/json" `
            -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        
        $response.name | should -BeExactly $ScriptName
    }

    It "Get the script by id '1'" {
        $response = Invoke-RestMethod `
            -Method Get `
            -Uri "$apiUrl/scripts/1" `
            -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        
        $response.id | should -BeExactly 1
        $response.name | should -BeExactly $ScriptName
    }

    It "Get all scripts - should retrieve at least 1" {
        $response = Invoke-RestMethod `
        -Method Get `
        -Uri "$apiUrl/scripts" `
        -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        
        $response.count | Should -BeGreaterOrEqual 1
    }

    It "Execute test script with parameters" {

            $param1 = "prvni"

            $Bytes = [System.Text.Encoding]::UTF8.GetBytes($param1)
            $param1 =[Convert]::ToBase64String($Bytes)

            $param2 = "druhy"
            $Bytes = [System.Text.Encoding]::UTF8.GetBytes($param2)
            $param2 =[Convert]::ToBase64String($Bytes)


        $body = @{
            "Parameters" = @(
                @{
                    "Name"= 'prvni'
                    'EncodedValue' = $param1

                },
                @{
                    "Name"= 'druhy'
                    'EncodedValue' = $param2
                }
            )
        } | ConvertTo-Json

        $response = Invoke-RestMethod `
            -Method Post `
            -Uri "$apiUrl/scripts/run/1" `
            -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        
        $response.exitcode | should -BeExactly 0
        $response.errorOutput | should -BeNullOrEmpty
        $response.output | should -BeLike "*Tohle je druhy parameter : $param1"
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

    Start-Sleep -Seconds 10
    Stop-Process -InputObject $process -Force
}