# --------------------------------
# Stage 1: Build the application
# --------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy remaining source code
COPY . .

# Publish application for production
RUN dotnet publish -c Release -o /app/publish


# --------------------------------
# Stage 2: Runtime image
# --------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Configure ASP.NET to listen on port 5106
ENV ASPNETCORE_URLS=http://+:5106

# Expose API port
EXPOSE 5106

# Start the Web API
ENTRYPOINT ["dotnet", "WebApplication1.dll"]