# Use the official .NET SDK image as the first stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory to /app
WORKDIR /app

# Copy the project files and restore dependencies
COPY *.sln .
COPY ./src/*.csproj ./src/
RUN dotnet restore

# Copy the rest of the application files and build the project
COPY ./src/. ./src/
RUN dotnet publish -c Release -o out

# Use the official ASP.NET Core runtime image as the second stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set the working directory to /app
WORKDIR /app

# Copy the build output from the previous stage
COPY --from=build /app/src/out .

# Expose the port on which the app will run
EXPOSE 80

# Start the application
ENTRYPOINT [ "dotnet", "YourApplication.dll" ]
