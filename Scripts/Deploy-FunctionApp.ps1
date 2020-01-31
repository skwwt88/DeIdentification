

$PackageFile = 'Package.zip'

function Build-Package() {
    $SrcFoler = '..\DeIdentification'
    $OutputFolder = '.\Package'

    dotnet build -c release -o $OutputFolder $SrcFoler

    Compress-Archive -Update -Path "$OutputFolder\*" -DestinationPath $PackageFile
}

Build-Package
az functionapp deployment source config-zip -g functiontest -n test2-skwwt --src $PackageFile 
