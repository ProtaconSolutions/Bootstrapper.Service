FROM microsoft/dotnet:2.0-runtime

COPY bin/release/netcoreapp2.0/publish/ /root/
EXPOSE 5000

WORKDIR /root/
ENTRYPOINT dotnet /root/Example.Download.Bootstrapper.Service.Web.dll