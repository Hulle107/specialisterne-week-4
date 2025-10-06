# 📋 Softwarekravspecifikation (SRS)

**Opgave:** Automatisk download af PDF-rapporter (GRI_2017_2020)<br>
**Version:** 1.0<br> 
**Kodesprog:** C#<br>
**Evt. frameworks:** C# Console Application (.NET)<br>
**Estimeret tid:** 1 uge<br>
**Dato:** 06-10-2025

## 📚 Indholdsfortegnelse

- [📋 Softwarekravspecifikation (SRS)](#-softwarekravspecifikation-srs)
  - [📚 Indholdsfortegnelse](#-indholdsfortegnelse)
  - [1. 🧭 Indledning](#1--indledning)
    - [1.1 🎯 Formål](#11--formål)
    - [1.2 📦 Omfang](#12--omfang)
    - [1.3 📚 Definitioner, forkortelser og akronymer](#13--definitioner-forkortelser-og-akronymer)
    - [1.4 🔗 Referencer](#14--referencer)
    - [1.5 🗺️ Oversigt over dokumentet](#15-️-oversigt-over-dokumentet)
  - [2. 🏗️ Overordnet beskrivelse](#2-️-overordnet-beskrivelse)
    - [2.1 🧩 Produktperspektiv](#21--produktperspektiv)
    - [2.2 ⚙️ Produktfunktioner](#22-️-produktfunktioner)
    - [2.3 👥 Brugerkarakteristika](#23--brugerkarakteristika)
    - [2.4 🚧 Begrænsninger](#24--begrænsninger)
    - [2.5 🔍 Antagelser](#25--antagelser)
  - [3. 📌 Specifikke krav](#3--specifikke-krav)
    - [3.1 ✅ Funktionskrav](#31--funktionskrav)
    - [3.2 🛡️ Ikke-funktionelle krav](#32-️-ikke-funktionelle-krav)
    - [3.3 🔌 Grænseflader](#33--grænseflader)
  - [4. 📎 Bilag](#4--bilag)
  - [5. ✍️ Godkendelse](#5-️-godkendelse)

## 1. 🧭 Indledning

### 1.1 🎯 Formål

At udvikle et stabilt C#-konsolprogram til automatisk download af PDF-rapporter fra en Excel-liste med to alternative URL’er. Programmet skal være hurtigere og mere pålideligt end den eksisterende Python-løsning.

### 1.2 📦 Omfang

- Download PDF’er fra kolonne `Primær URL`
- Hvis `Primær URL` fejler, forsøg linket i `Alternativ URL`
- Rapporterne navngives ud fra `BRNummer`
- Der genereres en kolonne med status: `"Downloadet"` eller `"Ikke downloadet"`
- Filerne overføres til en NAS-mappe (eller andet delt netværksdrev)
- Koden skal afleveres, da kunden forventer at bruge løsningen fremadrettet

### 1.3 📚 Definitioner, forkortelser og akronymer

| Term | Forklaring |
| :--- | :--------- |
| NAS | Netværkslager, hvor rapporter skal uploades |
| Console App | Kommandolinjebaseret program i C# |
| BRNummer | Unik identifikation af rapporten (bruges som filnavn)<br><br>Kolonne: `A`<br>Navn: `BRNum` |
| Primær URL | Primær URL til rapporten<br><br>Kolonne: `AL`<br>Navn: `Pdf_URL` |
| Alternativ URL | Alternativ URL til rapporten<br><br>Kolonne: `AM`<br>Navn: `Report Html Address` |
| GRI_2017_2020 | Rapportkatalog til download |
| Metadata2006-2016 | Tidligere rapportdata (brugt som reference til statuskolonne) |

### 1.4 🔗 Referencer

- Vedhæftet:
  - [GRI_2017_2020.xlsx](./resources/GRI_2017_2020.xlsx)
  - [Metadata2006-2016.xlsx](./resources/Metadata2006_2016.xlsx)
  - Python-kode (eksisterende løsning), [download_files.py](./resources/download_files.py)

### 1.5 🗺️ Oversigt over dokumentet

Dette dokument beskriver det overordnede formål, funktionelle og tekniske krav samt leverancer.

## 2. 🏗️ Overordnet beskrivelse

### 2.1 🧩 Produktperspektiv

Programmet er et konsolprogram i C# (.NET 8+), der kører på en lokal Windows-maskine med adgang til netværksdrev og internet. Det er et enkeltstående program og kræver ingen database eller webserver.

### 2.2 ⚙️ Produktfunktioner

- Læser input fra Excel-fil (`GRI_2017_2020.xlsx`)
- Downloader PDF fra `Primær URL`
  - Ved fejl forsøges `Alternativ URL`
- Gemmer fil som `[BRNummer].pdf`
- Skriver status (`"Downloadet"` / `"Ikke downloadet"`) til ny kolonne
- Kopierer filer til NAS/netværkssti

### 2.3 👥 Brugerkarakteristika

Brugeren er en IT-kyndig person, som kan køre en konsolapplikation og arbejde med filer/netværk.

### 2.4 🚧 Begrænsninger

- Excel-filen skal være korrekt struktureret
- NAS-stien skal være tilgængelig og skrivebar
- Minimal GUI – programmet kører via kommandolinje

### 2.5 🔍 Antagelser

- Internetadgang er tilgængelig
- Excel-filer er i `.xlsx`-format
- Kunden vedligeholder URL-lister løbende

## 3. 📌 Specifikke krav

### 3.1 ✅ Funktionskrav

**Input:**

- Excel-fil `GRI_2017_2020.xlsx` med følgende kolonner:
  - BRNum (kolonne `A`)
  - Pdf_URL (kolonne `AL`)
  - Report Html Address (kolonne `AM`)

**CLI-parametre (valgfrit):**
- --input \<sti>: Angiver sti til inputfil
- --output \<sti>: Angiver sti til outputfil
- --nas \<sti>: Angiver sti til NAS/netværksdrev

**Hovedfunktioner:**

1. For hver række:
   - Prøv at hente PDF fra `Primær URL`
   - Hvis fejler → prøv fra `Alternativ URL`
   - Hvis download lykkes → gem som `[BRNummer].pdf`
   - Hvis begge fejler → spring over og log status
2. Gem status i ny kolonne (fx **DownloadStatus**) med værdi:
   - `"Downloadet"` hvis succes
   - `"Ikke downloadet"` hvis begge fejler
3. Overfør PDF-filer til NAS (via konfigureret netværkssti)
4. Gem opdateret Excel-fil som `GRI_2017_2020_resultat.xlsx`

### 3.2 🛡️ Ikke-funktionelle krav

- Hurtigere end eksisterende Python-løsning
- Tolerant over for netværksfejl (timeout, 404 osv.)
- Skal kunne genkøres uden at hente allerede downloadede filer
- Skal logge fremgang (f.eks. i konsollen eller en logfil)
- Programmet skal ignorere fejl fra enkelte rækker og fortsætte med næste
- Ved manglende internetforbindelse skal der gives en tydelig fejlmeddelelse i konsollen

### 3.3 🔌 Grænseflader

| Type | Beskrivelse |
| :--- | :---------- |
| 🧾 Input | Excel-fil `GRI_2017_2020.xlsx` |
| 📤 Output | - PDF-filer navngivet `[BRNummer].pdf`<br>- Excel med kolonne `"DownloadStatus"`<br>- Logfil (valgfrit) |
| 🌐 Netværk | NAS-sti konfigureret i f.eks. `appsettings.json` eller som argument |

## 4. 📎 Bilag

- [GRI_2017_2020.xlsx](./resources/GRI_2017_2020.xlsx) – rapportmetadata
- [Metadata2006-2016.xlsx](./resources/Metadata2006_2016.xlsx) – eksempel på tidligere statusstruktur
- [download_files.py](./resources/download_files.py) – tidligere løsning
- NAS-oplysninger (modtages ved aflevering)

## 5. ✍️ Godkendelse

| Navn | Rolle | Dato | Godkendt |
| :--- | :---- | :--: | :------: |
| [Kunde Navn] | Kunde | [Dato] | 🔲 |
| Delta Thiesen | IT-konsulent, Specialisterne | 06-10-2025 | ✅ |