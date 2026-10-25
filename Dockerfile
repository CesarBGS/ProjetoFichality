# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia todos os arquivos do repositório
COPY . .

# Restaura as dependências apontando direto para o repositório
RUN dotnet restore

# Compila o projeto em modo Release
RUN dotnet publish -c Release -o /app/out

# Estágio de Execução (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/out .

# Configura a porta do Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ProjetoFichality.dll"]