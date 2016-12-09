[CmdletBinding()]
Param(
    [Parameter(Mandatory)]$Tag
)

dotnet build -c release $PSScriptRoot\Bootstapper.Service.Core\
Compress-Archive -Path $PSScriptRoot\Bootstapper.Service.Core\bin\release\net452 -DestinationPath $PSScriptRoot\Example.Download.Bootstrapper.Service.Web\Package\current.zip -Force

dotnet build -c release $PSScriptRoot\Bootstapper.Service.Core\
dotnet publish -c release $PSScriptRoot\Example.Download.Bootstrapper.Service.Web\
docker build -t $Tag $PSScriptRoot\Example.Download.Bootstrapper.Service.Web\