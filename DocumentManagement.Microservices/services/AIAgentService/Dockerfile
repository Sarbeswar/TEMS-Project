# Production Dockerfile for AIAgentService Web API.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY ./publish .
ENTRYPOINT ["dotnet", "AIAgent.API.dll"]
