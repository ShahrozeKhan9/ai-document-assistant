# AI Document Assistant

Upload PDFs and ask questions about them. Answers come with citations to the page they came from.

Built with **ASP.NET Core 8 Web API**, **Entity Framework Core + SQL Server**, **Qdrant** (vector search), **Google Gemini**, and a **React + TypeScript** frontend.

> Status: in progress. Phase 1 (upload, text extraction, chunking) is done.

## Roadmap

- [x] **Phase 1:** PDF upload, page-by-page text extraction, overlapping chunking, SQL Server storage
- [ ] **Phase 2:** embeddings + Qdrant, question answering with Gemini, source citations
- [ ] **Phase 3:** React chat UI with streaming answers
- [ ] **Phase 4:** JWT auth, Docker Compose, GitHub Actions CI, live deployment

## How it works (Phase 1)

```
PDF upload ──► save file to /storage
           ──► PdfPig extracts text page by page
           ──► TextChunker splits each page into ~1000-char chunks (200-char overlap),
               ending at sentence or word boundaries
           ──► Document + Chunks saved to SQL Server (one transaction)
```

Chunks keep their page number so later answers can cite "page 4".

## Project structure

```
src/DocAssistant.Api/
├── Controllers/   DocumentsController: HTTP endpoints only, no business logic
├── Services/      PdfTextExtractor, TextChunker, DocumentService
├── Data/          AppDbContext (EF Core)
├── Models/        Document, DocumentChunk (database entities)
├── DTOs/          What the API returns (never expose entities directly)
└── Options/       Strongly typed settings from appsettings.json
```

## Run locally

Requirements: .NET 8 SDK, SQL Server (or SQL Server Express / LocalDB).

Set `ConnectionStrings:Default` in `appsettings.json` for your SQL Server, for example `Server=.\\SQLEXPRESS;...` for SQL Express or `Server=(localdb)\\MSSQLLocalDB;...` for LocalDB.

```bash
cd src/DocAssistant.Api
dotnet tool install --global dotnet-ef      # once
dotnet restore
dotnet ef database update
dotnet run --launch-profile http
```

Swagger opens at `http://localhost:5080/swagger`. On Windows you can also double-click `run.bat`.

## API

| Method | Route | What it does |
| --- | --- | --- |
| POST | `/api/documents` | Upload a PDF (form field `file`) |
| GET | `/api/documents` | List documents |
| GET | `/api/documents/{id}` | Document details |
| GET | `/api/documents/{id}/chunks` | Chunks the document was split into |
| DELETE | `/api/documents/{id}` | Delete document, chunks and stored file |
