# Installing and Running Program

Variant 1

* Clone git project
* Change direction to _/Bitbucket/_ 
* Use in console ```dotnet run```
* Go to _http://localhost:5074/swagger/index.html_

Variant 2

* Clone git project
* Change direction to _/Bitbucket/_
* Use in console ```docker run -p 5074:80 -e ASPNETCORE_ENVIRONMENT=Development -e ASPNETCORE_URLS=http://localhost:5074 -e ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS=true -e DOTNET_USE_POLLING_FILE_WATCHER=1 -e NUGET_PACKAGES=/root/.nuget/fallbackpackages -e NUGET_FALLBACK_PACKAGES=/root/.nuget/fallbackpackages -e PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin -e ASPNETCORE_URLS=http://+:80 -e DOTNET_RUNNING_IN_CONTAINER=true -e DOTNET_VERSION=6.0.24 -e ASPNET_VERSION=6.0.24  my-bitbucket-app```
* Go to _http://localhost:5074/swagger/index.html_