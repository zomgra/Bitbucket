# Installing and Running Program

Running MSSQL using docker

```docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrongPassword123" -e "MSSQL_PID=Developer" -p 1433:1433  --name sql1 --hostname sql1 -d mcr.microsoft.com/mssql/server:2022-preview-ubuntu-22.04```

Next you should run dotnet app

Variant 1 for user, who install _dotnet_

* Clone git project
* Change direction to _/Bitbucket/_ 
* Use in console ```dotnet run```
* Go to _http://localhost:5074/swagger/index.html_



Variant 2

* Clone git project
* Create docker container ```docker build -t my-bitbucket-app .```
* Use in console ```docker run -p 5074:80 -e ASPNETCORE_ENVIRONMENT=Development -e ASPNETCORE_URLS=http://localhost:5074 -e ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS=true -e DOTNET_USE_POLLING_FILE_WATCHER=1 -e NUGET_PACKAGES=/root/.nuget/fallbackpackages -e NUGET_FALLBACK_PACKAGES=/root/.nuget/fallbackpackages -e PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin -e ASPNETCORE_URLS=http://+:80 -e DOTNET_RUNNING_IN_CONTAINER=true -e DOTNET_VERSION=6.0.24 -e ASPNET_VERSION=6.0.24  my-bitbucket-app```
* Go to _http://localhost:5074/swagger/index.html_


For watching metrics:

_http://localhost:5074/metrics_
