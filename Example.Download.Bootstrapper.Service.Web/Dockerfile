FROM microsoft/dotnet:1.0-runtime

COPY bin/release/netcoreapp1.0/publish/ /root/
EXPOSE 5000

WORKDIR /root/

ENTRYPOINT dotnet /root/Example.Download.Bootstrapper.Service.Web.dll