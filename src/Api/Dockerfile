FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY Couture.sln .
COPY Directory.Packages.props .
COPY Directory.Build.props .
COPY SharedKernel/*.csproj SharedKernel/
COPY Api/*.csproj Api/
COPY Modules/Identity/Identity/*.csproj Modules/Identity/Identity/
COPY Modules/Identity/Identity.Contracts/*.csproj Modules/Identity/Identity.Contracts/
COPY Modules/Orders/Orders/*.csproj Modules/Orders/Orders/
COPY Modules/Orders/Orders.Contracts/*.csproj Modules/Orders/Orders.Contracts/
COPY Modules/Clients/Clients/*.csproj Modules/Clients/Clients/
COPY Modules/Clients/Clients.Contracts/*.csproj Modules/Clients/Clients.Contracts/
COPY Modules/Finance/Finance/*.csproj Modules/Finance/Finance/
COPY Modules/Finance/Finance.Contracts/*.csproj Modules/Finance/Finance.Contracts/
COPY Modules/Dashboard/Dashboard/*.csproj Modules/Dashboard/Dashboard/
COPY Modules/Dashboard/Dashboard.Contracts/*.csproj Modules/Dashboard/Dashboard.Contracts/
COPY Modules/Notifications/Notifications/*.csproj Modules/Notifications/Notifications/
COPY Modules/Notifications/Notifications.Contracts/*.csproj Modules/Notifications/Notifications.Contracts/
COPY Modules/Catalog/Catalog/*.csproj Modules/Catalog/Catalog/
COPY Modules/Catalog/Catalog.Contracts/*.csproj Modules/Catalog/Catalog.Contracts/
COPY Tests/Domain.Tests/*.csproj Tests/Domain.Tests/
COPY Tests/Orders.Tests/*.csproj Tests/Orders.Tests/
COPY Tests/Clients.Tests/*.csproj Tests/Clients.Tests/
COPY Tests/Finance.Tests/*.csproj Tests/Finance.Tests/
COPY Tests/Dashboard.Tests/*.csproj Tests/Dashboard.Tests/
COPY Tests/Notifications.Tests/*.csproj Tests/Notifications.Tests/
COPY Tests/Integration.Tests/*.csproj Tests/Integration.Tests/

RUN dotnet restore Couture.sln

# Copy everything and publish
COPY . .
RUN dotnet publish Api/Couture.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

# Create uploads directory
RUN mkdir -p wwwroot/uploads

EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Couture.Api.dll"]
