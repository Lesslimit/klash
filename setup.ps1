Set-ExecutionPolicy RemoteSigned

$tempFolder = $env:TMP;
$dotnetcoresdkPath = "$tempFolder\dotnetcore-sdk.exe"

Write-Host 'Installing Chocolatey package manager. Mazafaka!'  -ForegroundColor DarkGreen
iwr https://chocolatey.org/install.ps1 -UseBasicParsing | iex

Write-Host 'Downloading .NET Core SDK for Windows. Mazafaka!'  -ForegroundColor DarkGreen
iwr https://go.microsoft.com/fwlink/?LinkID=831453 -OutFile $dotnetcoresdkPath

Write-Host 'Installing .NET Core SDK for Windows. Mazafaka!'  -ForegroundColor DarkGreen
Start-Process $dotnetcoresdkPath -ArgumentList '/s', '/v', '/qn' -Wait

choco install git -y
choco install yarn -y

Write-Host 'Fucking Klash is ready to run. Mazafaka!' -ForegroundColor DarkGreen
Write-Host ''
Write-Host 'IMHO' -ForegroundColor Yellow