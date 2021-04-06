$InformationPreference = 'continue'

Get-ChildItem -Filter "Start*Test.ps1" | % {
    Write-Information "Executing script : $_"
    & $_
}