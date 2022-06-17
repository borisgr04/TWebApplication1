#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

RUN ["apt-get", "update"]
RUN ["apt-get", "-y", "install", "libgdiplus"]

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["TWebApplication1/TWebApplication1.csproj", "TWebApplication1/"]
RUN dotnet restore "TWebApplication1/TWebApplication1.csproj"
COPY . .
WORKDIR "/src/TWebApplication1"
RUN dotnet build "TWebApplication1.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TWebApplication1.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TWebApplication1.dll"]