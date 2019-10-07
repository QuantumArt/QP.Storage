FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["QP.Storage.WebApp.csproj", ""]
RUN dotnet restore "./QP.Storage.WebApp.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "QP.Storage.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "QP.Storage.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QP.Storage.WebApp.dll"]