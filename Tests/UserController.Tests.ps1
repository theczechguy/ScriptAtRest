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
Describe "New user registration tests" {
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

    It "Fail registering the same user again" {
        $body = @{
            "firstname" = "Test"
            "lastname" = "Test"
            "username" = $Username
            "password" = $UserPassword
        } | ConvertTo-Json

        try {
            $err
            $response = Invoke-RestMethod `
                -Method Post `
                -Uri "$apiUrl/users/register" `
                -Body $body `
                -ContentType "application/json"
        }
        catch {
            $err = $_
        }
        ($err.ErrorDetails.Message | ConvertFrom-Json).message | Should -BeExactly ("Username : $Username is already taken")
        
    }
}

Describe "Get users tests" {
    It "Get all users" {

        $response = Invoke-RestMethod `
            -Method Get `
            -Uri "$apiUrl/users" `
            -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        
        $response.Count | Should -BeGreaterOrEqual 1
    }

    It "Fail getting all users with wrong credentials" {
        $base64AuthInfo = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $Username,'wrong_password')))
        $err
        try {            
            $response = Invoke-RestMethod `
                -Method Get `
                -Uri "$apiUrl/users" `
                -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        }
        catch {
            $err = $_
        }

        $err.Exception.Response.StatusCode | Should -BeExactly 'Unauthorized'
    }
}

Describe "User deletion tests" {
    It "Fail deleting non-existing user" {
        $err
        try {
            Invoke-RestMethod `
                -Method Delete `
                -Uri "$apiUrl/users/99" `
                -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        }
        catch {
          $err = $_
        }

        ($err.ErrorDetails.Message | ConvertFrom-Json).message | Should -BeExactly 'User with requested id not found'
    }

    It "Delete user created in first test" {
        $response = Invoke-RestMethod `
            -Method Get `
            -Uri "$apiUrl/users" `
            -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
        $user = $response | where username -eq $Username
        $user | Should -not -BeNullOrEmpty

        Invoke-RestMethod `
            -Method Delete `
            -Uri "$apiUrl/users/$($user.id)" `
            -Headers @{Authorization=("Basic {0}" -f $base64AuthInfo)}
    }
}
    
AfterAll{
    Start-Sleep -Seconds 5
    Stop-Process -InputObject $process -Force
}