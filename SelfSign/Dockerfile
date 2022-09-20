#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["SelfSign/SelfSign.csproj", "SelfSign/"]
RUN dotnet restore "SelfSign/SelfSign.csproj"
COPY . .
WORKDIR "/src/SelfSign"
RUN dotnet build "SelfSign.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SelfSign.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SelfSign.dll"]