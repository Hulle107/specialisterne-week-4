# Klassediagram

## Indholdsfortegnelse

- [Klassediagram](#klassediagram)
  - [Indholdsfortegnelse](#indholdsfortegnelse)
  - [DownloadEntry](#downloadentry)
  - [ExcelReader](#excelreader)
  - [Downloader](#downloader)
  - [StatusLogger](#statuslogger)
  - [Storage](#storage)
  - [Program](#program)
  - [Overview](#overview)

## DownloadEntry

```mermaid

classDiagram

  class DownloadEntry {
    - string BRNum
    - string PrimaryUrl
    - string AlternativeUrl
  }

```

## ExcelReader

```mermaid

classDiagram

  class ExcelReader {
    + List~DownloadEntry~ Read(string filepath)
  }

```

## Downloader

```mermaid

classDiagram

  class Downloader {
    - Storage StorageInstance
    - StatusLogger StatusLoggerInstance
    + bool Download(DownloadEntry report)
  }

```

## StatusLogger

```mermaid

classDiagram

  class StatusLogger {
    + StatusLogger()
    + void LogStatus(DownloadEntry report, bool success)
  }

```

## Storage

```mermaid

classDiagram

  class Storage {
    - string LocalPath
    - string NASPath
    - NetworkCredential NASCredential
    + Storage(string localPath, string nasPath, NetworkCredential nasCredential)
    + void UploadToNAS()
  }

```

## Program

```mermaid

classDiagram

  class Program {
    - Downloader DownloaderInstance
    + void Main(string[] args)
  }

```

## Overview

```mermaid

flowchart LR

  Program --> ExcelReader
  Program --> Downloader
  Downloader --> StatusLogger
  Downloader --> Storage
  ExcelReader --> DownloadEntry

```