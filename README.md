# Service bootstrapper
Problem: Customer own computer requires running service on it with self update support. This isn't service logic itself but bootstrapper
for service executable which is downloaded and updated automatically from internet.

```
Create publishable docker image of web server where this service can be downloaded and configured.

.\CreateDockerImage.Web.ps1 -Tag mystuff/bootstrapper-service:v1.0.0

docker run -p 5000:5000 -it mystuff/bootstrapper-service:v1.0.0

Invoke-WebRequest "http://localhost:5000/bootstrapperservicecore/?startupFile=console1.exe&remoteServicePackageFile=http://sourcefileserver.fi/yourownservice" 
```

## Bootstrapper configuration
Bootstrapper is configured in config.json file.
```javascript
{
  "StartupFile": "Baja.Client.Application.exe",
  "StartupFileArguments": "--LogPath={startupLocation}\\log\\",
  "RemoteServicePackageFile": "http://localhost:5000/bootstrapperservice",
  "UpdaterInterval": 30,
  "RemoteServiceHeaders": {
    "Version": "1.0",
	"CustomHeader": "Value"
  }
}
```

| Attribute | Description |
| --- | --- |
| `StartupFile`                 | Executable file which bootstrapper starts |
| `StartupFileArguments`        | Arguments for the startup file. `{startupLocation}` is the base directory for the bootstrapper. |
| `RemoteServicePackageFile`    | Remote server address where service version is fetched. Remote server must return version number with GET request. Bootstrapper downloads service from same endpoint by adding /"version" to end of url. |
| `UpdaterInterval`             | Interval in seconds how often bootstrapper tries to update service. |
| `RemoteServiceHeaders`        | Extra headers used with remote calls. |



