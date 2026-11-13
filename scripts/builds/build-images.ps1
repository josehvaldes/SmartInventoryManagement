# run from root  .\scripts\builds\build-images.ps1
param(
    [string]$Version = "1.0"
)

$ErrorActionPreference = "Stop"

docker build -t "smartinventory-api:$Version" `
    -f .\src\SmartInventory.API\Dockerfile `
    .\