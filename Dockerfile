# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore
COPY ["CollegeHallBooking.csproj", "./"]
RUN dotnet restore "CollegeHallBooking.csproj"

# Copy remaining source code and publish
COPY . .
RUN dotnet publish "CollegeHallBooking.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render exposes PORT environment variable dynamically (defaults to 8080 or process.env.PORT)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "CollegeHallBooking.dll"]
