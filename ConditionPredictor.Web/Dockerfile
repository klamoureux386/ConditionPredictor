# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src

COPY ["ConditionPredictor.Web/ConditionPredictor.Web.csproj", "ConditionPredictor.Web/"]
RUN dotnet restore "ConditionPredictor.Web/ConditionPredictor.Web.csproj"

COPY . .
WORKDIR "/src/ConditionPredictor.Web"
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "ConditionPredictor.Web.dll"]
