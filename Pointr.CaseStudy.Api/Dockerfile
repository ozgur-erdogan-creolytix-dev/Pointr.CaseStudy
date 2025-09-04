FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Development

# ---- build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Pointr.CaseStudy.Api/Pointr.CaseStudy.Api.csproj", "Pointr.CaseStudy.Api/"]
COPY ["Pointr.CaseStudy.Application/Pointr.CaseStudy.Application.csproj", "Pointr.CaseStudy.Application/"]
COPY ["Pointr.CaseStudy.Infrastructure/Pointr.CaseStudy.Infrastructure.csproj", "Pointr.CaseStudy.Infrastructure/"]
COPY ["Pointr.CaseStudy.Domain/Pointr.CaseStudy.Domain.csproj", "Pointr.CaseStudy.Domain/"]
COPY ["Pointr.Base.Domain/Pointr.Base.Domain.csproj", "Pointr.Base.Domain/"]
COPY ["Pointr.Base.Application/Pointr.Base.Application.csproj", "Pointr.Base.Application/"]
COPY ["Pointr.Base.Infrastructure/Pointr.Base.Infrastructure.csproj", "Pointr.Base.Infrastructure/"]

RUN dotnet restore "Pointr.CaseStudy.Api/Pointr.CaseStudy.Api.csproj"

COPY . .
WORKDIR /src/Pointr.CaseStudy.Api

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ---- final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Pointr.CaseStudy.Api.dll"]