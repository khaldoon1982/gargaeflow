# GarageFlow — Deployment Guide

## Overzicht

Dit document beschrijft hoe je GarageFlow volledig deployt:

- De **API server** met PostgreSQL (cloud)
- De **desktop app** als installeerbare Windows applicatie
- De **sync** tussen meerdere apparaten
- Het aanmaken van **gebruikers** per werkstation

---

## Architectuur

```
Werkstation 1 (receptie)          Werkstation 2 (werkplaats)
┌─────────────────────┐          ┌─────────────────────┐
│  GarageFlow.exe     │          │  GarageFlow.exe     │
│  SQLite (lokaal)    │          │  SQLite (lokaal)    │
│  .env.local         │          │  .env.local         │
└────────┬────────────┘          └────────┬────────────┘
         │                                │
         └───────────┬────────────────────┘
                     │ HTTPS
                     ▼
          ┌─────────────────────┐
          │  GarageFlow API     │
          │  api.jouwdomein.nl  │
          │  PostgreSQL         │
          └─────────────────────┘
```

---

## Deel 1: API Server Deployen

### Optie A: Coolify (aanbevolen)

#### Stap 1: VPS voorbereiden

Je hebt nodig:
- Een VPS (bijv. Hetzner, DigitalOcean, Contabo)
- Minimaal: 2 CPU, 2 GB RAM, 20 GB opslag
- Ubuntu 22.04+ of Debian 12+
- Coolify geïnstalleerd (https://coolify.io)

#### Stap 2: PostgreSQL aanmaken in Coolify

1. Open Coolify dashboard
2. Ga naar **Resources** → **New** → **Database** → **PostgreSQL**
3. Configureer:
   - Database naam: `garageflow_prod`
   - Gebruikersnaam: `garageflow_user`
   - Wachtwoord: **sterk wachtwoord genereren**
   - Poort: `5432` (intern, niet publiek)
4. Klik **Deploy**
5. Noteer de interne hostname (bijv. `garageflow-postgres`)

#### Stap 3: API deployen in Coolify

1. Ga naar **Resources** → **New** → **Application**
2. Koppel je Git repository (of gebruik Docker image)
3. Selecteer de `Dockerfile` in de root van het project
4. Configureer environment variables:

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:8080
ConnectionStrings__Default=Host=garageflow-postgres;Port=5432;Database=garageflow_prod;Username=garageflow_user;Password=JOUW_STERKE_WACHTWOORD
GARAGEFLOW_API_KEY=JOUW_API_SLEUTEL
GARAGEFLOW_LOG_LEVEL=Information
```

5. Stel het domein in: `api.jouwdomein.nl`
6. Schakel HTTPS in (Let's Encrypt)
7. Klik **Deploy**

#### Stap 4: Controleren

Open in je browser:
```
https://api.jouwdomein.nl/health
```

Verwacht antwoord:
```json
{"status":"healthy","timestamp":"2026-04-03T..."}
```

Test de sync status:
```
https://api.jouwdomein.nl/api/sync/status
```
(vereist header: `X-Api-Key: JOUW_API_SLEUTEL`)

### Optie B: Docker Compose (lokaal testen)

```bash
# Start API + PostgreSQL lokaal
docker-compose up -d

# Controleer status
curl http://localhost:5001/health
```

**Let op**: docker-compose.yml is alleen voor lokale ontwikkeling, niet voor productie.

---

## Deel 2: Desktop App Publiceren

### Stap 1: Publiceren als standalone applicatie

```bash
# Vanuit de solution root
dotnet publish GarageFlow.Wpf -c Release -r win-x64 --self-contained true -o ./publish
```

Dit maakt een map `publish/` met:
- `GarageFlow.exe` — de applicatie
- Alle .dll bestanden
- .NET runtime (self-contained, geen installatie nodig op doelcomputer)

### Stap 2: .env.local voorbereiden

Maak in de `publish/` map een `.env.local` bestand aan:

```env
GARAGEFLOW_SYNC_ENABLED=true
GARAGEFLOW_SYNC_INTERVAL_SECONDS=60
GARAGEFLOW_API_URL=https://api.jouwdomein.nl
GARAGEFLOW_API_KEY=JOUW_API_SLEUTEL
GARAGEFLOW_DEVICE_ID=auto
GARAGEFLOW_SQLITE_PATH=garageflow.db
GARAGEFLOW_LOG_LEVEL=Information
```

**Belangrijk**: Gebruik dezelfde `GARAGEFLOW_API_KEY` als op de server.

### Stap 3: Mapstructuur controleren

Na publiceren en configureren:

```
publish/
├── GarageFlow.exe          ← start de app
├── .env.local              ← configuratie
├── garageflow.db           ← wordt automatisch aangemaakt bij eerste start
├── logs/                   ← wordt automatisch aangemaakt
└── (alle .dll bestanden)
```

### Stap 4: Testen

1. Dubbelklik op `GarageFlow.exe`
2. Inloggen met standaard account: `admin` / `admin123`
3. **Wijzig het wachtwoord direct** na eerste login

---

## Deel 3: Installer Maken (optioneel)

### Optie A: Inno Setup (gratis, aanbevolen)

1. Download Inno Setup: https://jrsoftware.org/isinfo.php
2. Maak een `installer.iss` script:

```iss
[Setup]
AppName=GarageFlow
AppVersion=1.0.0
DefaultDirName={autopf}\GarageFlow
DefaultGroupName=GarageFlow
OutputDir=installer_output
OutputBaseFilename=GarageFlow-Setup-1.0.0
Compression=lzma
SolidCompression=yes

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\GarageFlow"; Filename: "{app}\GarageFlow.exe"
Name: "{commondesktop}\GarageFlow"; Filename: "{app}\GarageFlow.exe"

[Run]
Filename: "{app}\GarageFlow.exe"; Description: "GarageFlow starten"; Flags: nowait postinstall
```

3. Compileer met Inno Setup → levert `GarageFlow-Setup-1.0.0.exe`

### Optie B: ZIP distributie

Pak de `publish/` map als ZIP:
```bash
Compress-Archive -Path ./publish/* -DestinationPath GarageFlow-1.0.0.zip
```

Verstuur de ZIP naar de klant, uitpakken op het bureaublad, klaar.

---

## Deel 4: Installatie op een Nieuw Werkstation

### Vereisten

- Windows 10 of 11
- Geen extra software nodig (self-contained publicatie)
- Internettoegang voor sync (optioneel, app werkt ook offline)

### Installatiestappen

1. **Installer uitvoeren** (of ZIP uitpakken)
2. **.env.local aanpassen** per werkstation:
   - `GARAGEFLOW_DEVICE_ID=auto` (wordt automatisch uniek per PC)
   - `GARAGEFLOW_API_URL` en `GARAGEFLOW_API_KEY` moeten hetzelfde zijn op alle werkstations
3. **App starten** → GarageFlow.exe
4. **Eerste keer**: database wordt automatisch aangemaakt + seed data geladen
5. **Inloggen** met admin account

### Per werkstation

Elke installatie krijgt automatisch:
- Een eigen lokale SQLite database
- Een uniek `DeviceId` (opgeslagen in `%LocalAppData%\GarageFlow\device-id`)
- Een eigen set log bestanden

---

## Deel 5: Gebruikersbeheer

### Standaard gebruikers (seed data)

| Gebruiker | Wachtwoord | Rol | Rechten |
|---|---|---|---|
| admin | admin123 | Admin | Alles |
| technicus | tech123 | Technician | Onderhoud, keuringen |
| receptie | receptie123 | Receptionist | Klanten, voertuigen, afspraken |

### Rollen

| Rol | Beschrijving |
|---|---|
| Admin | Volledige toegang, instellingen, back-ups, gebruikersbeheer |
| Technician | Onderhoud, keuringen, voertuigen |
| Receptionist | Klanten, voertuigen, herinneringen |
| Viewer | Alleen-lezen |

### Gebruikers per werkstation

De huidige versie (MVP) heeft **lokale gebruikers per apparaat**. Dit betekent:

- Gebruikers worden aangemaakt in de lokale database
- Elke PC heeft zijn eigen set gebruikers
- Seed data maakt de standaard 3 gebruikers aan op eerste start

### Aanbeveling voor meerdere werkstations

1. Op het **eerste werkstation**: log in als admin, wijzig wachtwoorden
2. Op elk **volgend werkstation**: de seed data maakt automatisch dezelfde standaard gebruikers aan
3. Wijzig wachtwoorden op elk werkstation apart
4. Gebruik dezelfde gebruikersnamen maar eventueel andere wachtwoorden per PC

**Toekomstige verbetering**: Centrale gebruikersidentiteit via de API (gepland voor latere fase).

---

## Deel 6: Sync Configureren

### Sync inschakelen

Op elk werkstation in `.env.local`:

```env
GARAGEFLOW_SYNC_ENABLED=true
GARAGEFLOW_API_URL=https://api.jouwdomein.nl
GARAGEFLOW_API_KEY=dezelfde-sleutel-als-server
```

### Hoe sync werkt

1. Gebruiker werkt in de app → data opgeslagen in lokale SQLite
2. Wijzigingen worden automatisch in de SyncQueue gezet
3. Elke 60 seconden stuurt de BackgroundSyncService wijzigingen naar de API
4. De API slaat ze op in PostgreSQL
5. Andere werkstations ontvangen de wijzigingen bij hun volgende sync cyclus

### Wat synchroniseert

| Entity | Sync | Richting |
|---|---|---|
| Klanten | Ja | Push + Pull |
| Voertuigen | Ja | Push + Pull |
| Onderhoud | Ja | Push + Pull |
| Keuringen | Ja | Push + Pull |
| Herinneringen | Ja | Push + Pull |
| Gebruikers | Nee | Lokaal per apparaat |
| Onderdelen | Nee | Lokaal (toekomstig) |
| Instellingen | Nee | Lokaal per apparaat |

### Sync volgorde (FK afhankelijkheden)

```
1. Klanten (geen afhankelijkheden)
2. Voertuigen (afhankelijk van klant)
3. Onderhoud, Keuringen, Herinneringen (afhankelijk van voertuig)
```

### Offline werken

- De app werkt **altijd**, ook zonder internet
- Wijzigingen worden lokaal opgeslagen en wachten in de SyncQueue
- Zodra er weer verbinding is, worden ze automatisch gesynchroniseerd

---

## Deel 7: Productie Checklist

### Server

- [ ] PostgreSQL database aangemaakt met sterk wachtwoord
- [ ] API gedeployed op Coolify of vergelijkbaar
- [ ] HTTPS ingeschakeld (Let's Encrypt)
- [ ] `GARAGEFLOW_API_KEY` ingesteld op een sterke, unieke waarde
- [ ] `/health` endpoint bereikbaar
- [ ] Firewall: alleen poort 443 (HTTPS) open voor API
- [ ] PostgreSQL **niet** publiek toegankelijk

### Elk werkstation

- [ ] GarageFlow geïnstalleerd (installer of ZIP)
- [ ] `.env.local` aangemaakt met correcte API URL en API key
- [ ] `GARAGEFLOW_SYNC_ENABLED=true` ingesteld
- [ ] App gestart en ingelogd
- [ ] Standaard wachtwoorden gewijzigd
- [ ] Sync getest: wijziging op werkstation 1 verschijnt op werkstation 2

### Beveiliging

- [ ] Standaard wachtwoorden gewijzigd (admin123, tech123, receptie123)
- [ ] API key is sterk en uniek (bijv. 32+ tekens)
- [ ] `.env.local` niet gedeeld via onveilige kanalen
- [ ] PostgreSQL wachtwoord is sterk
- [ ] Geen PostgreSQL credentials op de desktop

---

## Deel 8: Back-up Strategie

### Lokale back-up (per werkstation)

De app heeft een ingebouwde back-up functie:
- Ga naar **Back-up** in het menu
- Klik **Back-up aanmaken**
- Bestand wordt opgeslagen in de `backups/` map

### Cloud back-up (via sync)

Wanneer sync is ingeschakeld:
- Alle gesynchroniseerde data staat in PostgreSQL
- PostgreSQL kan apart gebackupt worden op de server
- Via Coolify: automatische PostgreSQL dumps instellen

### Aanbevolen schema

| Type | Frequentie | Locatie |
|---|---|---|
| Lokale back-up | Dagelijks | Werkstation `backups/` map |
| PostgreSQL dump | Dagelijks | Server / externe opslag |
| Volledige server backup | Wekelijks | Externe opslag |

---

## Deel 9: Updates Uitrollen

### Desktop app updaten

1. Publiceer de nieuwe versie:
   ```bash
   dotnet publish GarageFlow.Wpf -c Release -r win-x64 --self-contained true -o ./publish
   ```
2. Maak een nieuwe installer of ZIP
3. Distribueer naar werkstations
4. Op elk werkstation:
   - Sluit de oude app
   - Installeer de nieuwe versie (overschrijft bestanden)
   - `.env.local` en `garageflow.db` blijven behouden
   - Migraties worden automatisch toegepast bij opstarten

### API updaten

1. Push de nieuwe code naar je Git repository
2. In Coolify: klik **Redeploy**
3. De API start opnieuw en past automatisch migraties toe

---

## Deel 10: Probleemoplossing

### App start niet

1. Controleer of `.env.local` bestaat in de app map
2. Bekijk de logs in de `logs/` map
3. Verwijder `garageflow.db` om een verse database te krijgen (let op: data gaat verloren)

### Sync werkt niet

1. Controleer `GARAGEFLOW_SYNC_ENABLED=true` in `.env.local`
2. Controleer of de API bereikbaar is: `https://api.jouwdomein.nl/health`
3. Controleer of de API key overeenkomt op werkstation en server
4. Bekijk de logs voor sync foutmeldingen
5. Test met: `curl -H "X-Api-Key: JOUW_KEY" https://api.jouwdomein.nl/api/sync/status`

### Database fout na update

EF Core migraties worden automatisch toegepast. Als er een fout is:
1. Maak een back-up van `garageflow.db`
2. Verwijder `garageflow.db`
3. Start de app opnieuw (verse database + seed data)
4. Sync haalt data terug van de cloud (als sync eerder actief was)

### Meerdere werkstations zien niet dezelfde data

1. Controleer of sync op beide werkstations is ingeschakeld
2. Wacht minimaal 60 seconden (sync interval)
3. Controleer de logs op sync fouten
4. Controleer of beide werkstations dezelfde API URL en API key gebruiken

---

## Snel Overzicht

| Onderdeel | Commando / Actie |
|---|---|
| API lokaal testen | `docker-compose up -d` |
| API deployen | Dockerfile via Coolify |
| Desktop publiceren | `dotnet publish GarageFlow.Wpf -c Release -r win-x64 --self-contained true -o ./publish` |
| Installer maken | Inno Setup met `publish/` map |
| Eerste start | `GarageFlow.exe` → login `admin` / `admin123` |
| Sync aanzetten | `.env.local` → `GARAGEFLOW_SYNC_ENABLED=true` |
| Health check | `https://api.jouwdomein.nl/health` |
| Logs bekijken | `logs/` map in app directory |
