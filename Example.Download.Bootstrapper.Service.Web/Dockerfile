FROM microsoft/dotnet:1.0-runtime

COPY bin/Debug/netcoreapp1.0/publish/ /root/
EXPOSE 5000/tcp

ENTRYPOINT dotnet /root/Example.Download.Bootstrapper.Service.Web.dll