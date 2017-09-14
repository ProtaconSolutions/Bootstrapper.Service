[CmdletBinding()]
Param(
    [Parameter(Mandatory)]$Tag
)

dotnet build -c release $PSScriptRoot\Bootstrapper.Service.Core\
Compress-Archive -Path $PSScriptRoot\Bootstrapper.Service.Core\bin\release\net461 -DestinationPath $PSScriptRoot\Example.Download.Bootstrapper.Service.Web\Package\current.zip -Force

dotnet build -c release $PSScriptRoot\Bootstrapper.Service.Core\
dotnet publish -c release $PSScriptRoot\Example.Download.Bootstrapper.Service.Web\
docker build -t $Tag $PSScriptRoot\Example.Download.Bootstrapper.Service.Web\