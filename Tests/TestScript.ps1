param($prvni,$druhy)
#region setup
    $ErrorActionPreference = 'stop'
    $InformationPreference = 'continue'
#endregion

#region log some stuff
    Write-Information "Tohle je prvni parameter : $prvni"
    Write-Information "Tohle je druhy parameter : $druhy"
#endregion

#region do some magic
    $arrayHodnot = $prvni.split(",")
    $arrayHodnot | % {
        Write-Information "Tohle je hodnota : $_"
    }
#endregion"