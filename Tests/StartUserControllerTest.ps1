$pesterContainer = New-PesterContainer -Path .\UserController.Tests.ps1 -Data @{
    ApplicationFolder = 'c:\Users\alien\OneDrive\Dokumenty\GitHub\ScriptAtRest\ScriptAtRestServer\bin\Release\netcoreapp3.1\'
    Username = "Test{0}" -f (Get-Random -Minimum 1 -Maximum 10)
    UserPassword = 'test'
    ApiUrl = 'http://localhost:5000'
}

Invoke-Pester -Container $pesterContainer -Output Detailed