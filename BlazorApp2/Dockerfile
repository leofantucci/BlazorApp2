FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia tudo da raiz da solução
COPY . .

# Restaura dependências
RUN dotnet restore

# Compila o projeto BlazorApp2
RUN dotnet publish BlazorApp2/BlazorApp2.csproj -c Release -o /app/publish

# Imagem de execução final do ASP.NET 9.0
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BlazorApp2.dll"]