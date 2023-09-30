# Memodex

Memodex is a self-hosted web app optimized for flashcard learning. Create or
import decks to streamline your study sessions. The app features a
user-friendly
interface and a sleek dark theme, making it a simple and effective solution for
all your flashcard needs.

## How to run

### Docker

`docker run -v mdxvol:/app/data --name memodex -d -p 5000:80 -e
Media__Path=/app/data kamgru/memodex`

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
