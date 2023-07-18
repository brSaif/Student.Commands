FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["gRPCOnHttp3Client/gRPCOnHttp3Client.csproj", "gRPCOnHttp3Client/"]
RUN dotnet restore "gRPCOnHttp3Client/gRPCOnHttp3Client.csproj"
COPY . .
WORKDIR "/src/gRPCOnHttp3Client"
RUN dotnet build "gRPCOnHttp3Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "gRPCOnHttp3Client.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "gRPCOnHttp3Client.dll"]
