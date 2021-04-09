param (
    $ApplicationFolder,
    $Username,
    $UserPassword,
    $apiUrl,
    $ScriptName
)
BeforeAll {
    $InformationPreference = 'continue'

    #region script type tests
        $scriptTypeName = 'Test Type'
        $scriptTypeRunner = "pwsh.exe"
        $scriptTypeFileExtension = 'ps.1'
        $scriptTypeArgument = '-f'
    #endregion

    #region remove local database
        Write-Information "Deleting localdatabase.db"
        Get-ChildItem -Path $ApplicationFolder -Filter "LocalDatabase.db" | Remove-Item -Force
    #endregion

    #region auth
        $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $Username,$UserPassword)))
    #endregion

    #region start api
        Write-Information "Starting api"
        $exeFile = Join-Path -Path $ApplicationFolder -ChildPath "ScriptAtRestServer.exe"
        $process = Start-Process -FilePath $exeFile -WorkingDirectory $ApplicationFolder -PassThru
        Start-Sleep -Seconds 5
    #endregion

    #region register test user
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
    #endregion
}

Describe "Script type tests" {
    It "Register a new script type" {
        $body = @{
            "Name" = $scriptTypeName
            "Runner" = $scriptTypeRunner
            "FileExtension" = $scriptTypeFileExtension
            "ScriptArgument" = $scriptTypeArgument
        } | ConvertTo-Json

        $response = Invoke-RestMethod `
            -Method Post `
            -Uri "$apiUrl/scripts/type" `
            -Body $body `
            -ContentType "application/json" `
            -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        
        $response.name | should -BeExactly $scriptTypeName
        $response.id | should -not -BeNullOrEmpty
        $response.FileExtension | should -be $scriptTypeFileExtension
        $response.ScriptArgument | should -be $scriptTypeArgument
    }

    It "Register new script type that will be used in next tests" {
        $body = @{
            "Name" = "$scriptTypeName-test"
            "Runner" = $scriptTypeRunner
            "FileExtension" = $scriptTypeFileExtension
            "ScriptArgument" = $scriptTypeArgument
        } | ConvertTo-Json

        $response = Invoke-RestMethod `
            -Method Post `
            -Uri "$apiUrl/scripts/type" `
            -Body $body `
            -ContentType "application/json" `
            -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        
        $response.name | should -BeExactly "$scriptTypeName-test"
        $response.id | should -not -BeNullOrEmpty
        $response.FileExtension | should -be $scriptTypeFileExtension
        $response.ScriptArgument | should -be $scriptTypeArgument
    }

    It "Update script name"{
        $body = @{
            "Name" = "$scriptTypeName-updated"
            "Runner" = $scriptTypeRunner
            "FileExtension" = $scriptTypeFileExtension
            "ScriptArgument" = $scriptTypeArgument
        } | ConvertTo-Json

        $response = Invoke-RestMethod `
            -Method Put `
            -Uri "$apiUrl/scripts/type/1" `
            -Body $body `
            -ContentType "application/json" `
            -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        
        $response.name | Should -be "$scriptTypeName-updated"
        $response.id | should -Be 1
        $response.FileExtension | should -be $scriptTypeFileExtension
        $response.ScriptArgument | should -be $scriptTypeArgument
    }

    It "Fail updating name which already exists" {
        $body = @{
            "Name" = "$scriptTypeName-test"
            "Runner" = $scriptTypeRunner
            "FileExtension" = $scriptTypeFileExtension
            "ScriptArgument" = $scriptTypeArgument
        } | ConvertTo-Json

        $err
        try {
            $response = Invoke-RestMethod `
                -Method Put `
                -Uri "$apiUrl/scripts/type/1" `
                -Body $body `
                -ContentType "application/json" `
                -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}   
        }
        catch {
            $err = $_
        }
        $err | should -not -BeNullOrEmpty
        ($err.ErrorDetails.Message | ConvertFrom-Json).message | should -be "New script type name is already taken !"
    }

    It "Get all script types - should be at least 2"{
        $response = Invoke-RestMethod `
            -Method Get `
            -Uri "$apiUrl/scripts/type" `
            -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        
        $response.count | Should -BeGreaterOrEqual 2
    }
    It "Get script type by ID" {
        $response = Invoke-RestMethod `
            -Method Get `
            -Uri "$apiUrl/scripts/type/1" `
            -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        
        $response.id | should -BeExactly 1
        $response.name | should -be $scriptTypeName
    }

    It "Delete script type with id 1" {
        $err
        try {
            $response = Invoke-RestMethod `
                -Method Delete `
                -Uri "$apiUrl/scripts/type/1" `
                -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}   
        }
        catch {
            $err = $_
        }
        $err | Should -BeNullOrEmpty
    }
}

Describe "Script operation tests" {
    It "Register a new script" {
        $script = Get-Content .\TestScript.ps1 -Encoding utf8 -Raw

        $Bytes = [System.Text.Encoding]::UTF8.GetBytes($script)
        $encoded =[Convert]::ToBase64String($Bytes)

        $body = @{
            "Name" = $ScriptName
            "EncodedContent" = $encoded
            "Type" = 2
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
            $param1Encoded =[Convert]::ToBase64String($Bytes)

            $param2 = "druhy"
            $Bytes = [System.Text.Encoding]::UTF8.GetBytes($param2)
            $param2Encoded =[Convert]::ToBase64String($Bytes)


        $body = @{
            "Parameters" = @(
                @{
                    "Name"= 'prvni'
                    'EncodedValue' = $param1Encoded

                },
                @{
                    "Name"= 'druhy'
                    'EncodedValue' = $param2Encoded
                }
            )
        } | ConvertTo-Json

        $response = Invoke-RestMethod `
            -Method Post `
            -Uri "$apiUrl/scripts/run/1" `
            -ContentType "application/json" `
            -Body $body `
            -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        
        $response.exitcode | should -BeExactly 0
        $response.errorOutput | should -BeNullOrEmpty
        $response.output | should -BeLike "*Tohle je prvni parameter : $param1*"
    }

    It "Delete the script by id '1'" {
        $err
        try {
            $response = Invoke-RestMethod `
                -Method Delete `
                -Uri "$apiUrl/scripts/1" `
                -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}   
        }
        catch {
            $err = $_
        }
        $err | should -BeNullOrEmpty
    }
}

AfterAll {
    Start-Sleep -Seconds 5
    Stop-Process -InputObject $process -Force
}