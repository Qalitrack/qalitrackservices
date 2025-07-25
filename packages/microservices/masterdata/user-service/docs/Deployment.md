
# Deployment Documentation

This section outlines how to deploy the **User Service Microservice** application, which is designed to be containerized using **Docker**.

## Deployment Overview

The application is packaged using **Docker** to ensure a consistent, portable, and isolated environment for running the service across different environments, including development, testing, staging, and production.

### Key Steps for Deployment

1. **Dockerfile**: The application is built into a container image using a Dockerfile, which contains all the instructions for setting up the environment and running the application.

2. **Docker Image Build**: The **Dockerfile** defines the instructions to build the Docker image that contains the application and its dependencies.

3. **Docker Compose (Optional)**: For multi-container setups or when the application needs to interact with other services (like a database or message broker), **Docker Compose** can be used to manage the containers.

---

## Dockerfile

The Dockerfile defines the environment needed to run the **User Service Microservice** API. Here's an overview of the Dockerfile contents:

```dockerfile
# Use official .NET Core SDK image as a base image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

# Use official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["UserService.Api/UserService.Api.csproj", "UserService.Api/"]
RUN dotnet restore "UserService.Api/UserService.Api.csproj"
COPY . .
WORKDIR "/src/UserService.Api"
RUN dotnet build "UserService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserService.Api.csproj" -c Release -o /app/publish

# Copy the build output to the base image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserService.Api.dll"]
```

### Explanation of the Dockerfile:

* **Base Image**: The `FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base` command uses the official **.NET Core Runtime** image, which contains the runtime environment needed to run the application.

* **Build Stage**: In the `FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build` section, we use the **.NET SDK** image to compile and build the application. The application is restored and built using the **dotnet restore** and **dotnet build** commands.

* **Publish Stage**: In the `RUN dotnet publish` command, the application is compiled and prepared for release, and output is placed in the `/app/publish` directory.

* **Final Image**: The final image is created by copying the application from the **publish** stage to the base image, where it is ready to run.

---

## Docker Compose (Optional)

If you're using multiple services (like a **PostgreSQL** database, a **message queue**, or other microservices), you can define and run your application using **Docker Compose**.

### Example `docker-compose.yml`:

```yaml
version: '3.8'

services:
  userservice:
    image: userservice:latest
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "80:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Username=postgres;Password=example;Database=userdb
    depends_on:
      - postgres

  postgres:
    image: postgres:13
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: example
      POSTGRES_DB: userdb
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

volumes:
  pgdata:
```

### Explanation of `docker-compose.yml`:

* **Userservice**: This is the main service for your **User Service Microservice**. It is built from the **Dockerfile**, and the application will be accessible on port `80`.

* **Postgres**: This container is the database service, which uses the official **PostgreSQL** image. It is connected to the `userservice` through an environment variable specifying the connection string.

* **Volumes**: The `pgdata` volume ensures that the database data persists even when the container is stopped or recreated.

---

## Deployment to Cloud Platforms

You can deploy the Dockerized application to a cloud platform, such as **AWS**, **Azure**, or **Google Cloud**, using their respective container services:

* **AWS**: Use **Amazon ECS** or **Elastic Beanstalk** to deploy Docker containers.
* **Azure**: Use **Azure App Service** or **Azure Kubernetes Service (AKS)** for containerized deployments.
* **Google Cloud**: Use **Google Kubernetes Engine (GKE)** or **Cloud Run** for containerized applications.

Each platform offers tools and configurations to manage the deployment of containers, including scaling, load balancing, and monitoring.

---

## Building and Running the Docker Image

To build and run the Docker container locally, follow these steps:

### Step 1: Build the Docker Image

From the root directory of your project (where the Dockerfile is located), run:

```bash
docker build -t userservice .
```

### Step 2: Run the Docker Container

Once the image is built, you can run it with:

```bash
docker run -p 8080:80 userservice
```

This will start the application on **[http://localhost:8080](http://localhost:8080)**.

---

## Continuous Deployment (CI/CD)

To automate the build and deployment of the Docker image, you can integrate it into a **CI/CD pipeline** using tools like **GitHub Actions**, **Jenkins**, or **GitLab CI**.

Example of a basic **GitHub Actions** configuration for Docker:

```yaml
name: Docker Build and Push

on:
  push:
    branches:
      - main

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout code
        uses: actions/checkout@v2

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v1

      - name: Build and push Docker image
        uses: docker/build-push-action@v2
        with:
          context: .
          file: Dockerfile
          push: true
          tags: username/userservice:latest
```

This GitHub Action will trigger on pushes to the `main` branch, build the Docker image, and push it to Docker Hub.

---

