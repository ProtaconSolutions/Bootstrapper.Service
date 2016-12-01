# Service bootstrapper
Problem: Customer own computer requires running service on it with self update support. This isn't service logig itself but bootstrapper
for service executable which is downloaded and updated automatically from internet.

```
Create publishable docker image of web server where this service can be downloaded and configured.

.\CreateDockerImage.Web.ps1 -Tag mystuff/bootstrapper-service:v1.0.0

docker run -p 5000:5000 -it mystuff/bootstrapper-service:v1.0.0

Invoke-WebRequest "http://localhost:5000/bootstrapperservice/?startupFile=console1.exe&remoteServicePackageFile=http://sourcefileserver.fi/yourownservice.zip" 
```