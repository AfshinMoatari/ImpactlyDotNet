# Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env /app/out .

# Add cleanup and socket handling
RUN find /tmp -type s -delete 2>/dev/null || true && \
    rm -rf /tmp/* /var/tmp/* && \
    mkdir -p /tmp && \
    chmod 1777 /tmp

# Install curl for healthcheck and verify installation
RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/* && \
    curl --version

ENTRYPOINT ["dotnet", "API.dll"]