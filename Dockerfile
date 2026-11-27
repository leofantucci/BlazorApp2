FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore BlazorApp2/BlazorApp2.csproj

RUN dotnet publish BlazorApp2/BlazorApp2.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0

# Instala a biblioteca libgssapi_krb5.so.2 (deve rodar como root, antes do USER app)
RUN apt-get update && \
    apt-get install -y --no-install-recommends libgssapi-krb5-2 && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=build /app/publish .

# Render injeta a variável PORT; localmente usamos 10000 como padrão.
# O ASP.NET Core precisa escutar em 0.0.0.0 para receber tráfego do proxy.
ENV ASPNETCORE_URLS=http://0.0.0.0:10000

EXPOSE 10000

USER app

ENTRYPOINT ["/bin/sh", "-c", "export ASPNETCORE_URLS=\"http://0.0.0.0:${PORT:-10000}\"; exec dotnet BlazorApp2.dll"]
