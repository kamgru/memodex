# Memodex

Memodex is a self-hosted web app optimized for flashcard learning. Create or
import decks to streamline your study sessions. The app features a
user-friendly
interface and a sleek dark theme, making it a simple and effective solution for
all your flashcard needs.

## How to run

### Docker

```bash
docker run -v mdxvol:/app/data --name memodex -d -p 5000:80 -e Media__Path=/app/data kamgru/memodex
```

### Docker Compose

```yaml
services:
  
  app:
    build: .
    ports:
      - "5000:80"
    environment:
      - Media__Path=/app/data
    volumes:
      - memodex:/app/data

volumes:
  memodex:
```

## How to build

### Build Docker Image from source

```bash
git clone https://github.com/kamgru/memodex.git
cd memodex
docker build -t memodex .
docker run -d -p 5000:80 -v mdxvol:/app/data -e Media__Path=/app/data --name 
memodex memodex
```

### Run Docker Compose from source

```bash
docker compose up -d
```

### Build and run from source

In order to build the source you will need:

- dotnet sdk 7.0
- nodejs 18
- npm

```bash
git clone https://github.com/kamgru/memodex.git
cd memodex/src/Memodex.WebApp
npm install
npm run build
dotnet run
```

### Build and publish production from source

```bash
git clone https://github.com/kamgru/memodex.git
cd memodex/src/Memodex.WebApp
npm install
npm run build
mkdir -p publish/media
cp ../../data/img/avatars
dotnet publish -c Release -o publish
cd publish
./Memodex.WebApp
```

## Test

### Integration Tests

```bash
cd src
dotnet test --filter Integration
```

### End-to-End Tests

```bash
cd src
dotnet run --project Memodex.WebApp &
dotnet test --settings Memodex.Tests.E2e/.runsettings --filter E2e
```

### Running End-to-End tests within Rider

<pre>
|-- File
    |-- Settings
        |-- Build, Execution, Deployment 
            |-- Unit Testing 
                |-- Test Runner
</pre>
Add path to the `src/Memodex.Tests.E2e/.runsettings` file to `Test Settings`

## How to host behind Nginx reverse proxy

The bare bones configuration for hosting behind Nginx reverse proxy, assuming
the app is running on port 5000 and the subpath is `/memodex`.

```nginx
http {
        map $http_connection $connection_upgrade {
                "~*Upgrade" $http_connection;
                default keep-alive;
        }

        server {
                listen 80;
                server_name example.com;
                location /memodex {
                        proxy_pass              http://127.0.0.1:5000;
                        proxy_http_version      1.1;
                        proxy_set_header        Upgrade $http_upgrade;
                        proxy_set_header        Connection $connection_upgrade;
                        proxy_set_header        Host $host;
                        proxy_cache_bypass      $http_upgrade;
                        proxy_set_header        X-Forwarded-For $proxy_add_x_forwarded_for;
                        proxy_set_header        X-Forwarded-Proto $scheme;
                }
        }
```

In order to run the app on a subpath, set the `PathBase` 
environment variable to the subpath, depending on the hosting method:
- Docker: `-e PathBase=/memodex`
- Docker Compose: `environment: - PathBase=/memodex`
- `appsettings.json`: `"PathBase": "/memodex"
