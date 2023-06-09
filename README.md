# docker-ravendb-webapi

```powershell
dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\Passaro.API.pfx -p passaro.api
dotnet dev-certs https --trust
```

```powershell
docker compose up -d
```

Banco de Dados: ```http://localhost:8081/```

API: ```http://localhost:8000/swagger/index.html```
