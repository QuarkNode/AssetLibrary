# Asset Library

A web application for managing 3D part and asset files: upload models, track revisions, and keep part data (materials, part numbers, status) organized in one place. Built with ASP.NET Core MVC and SQL Server.

## Features

1. **User Accounts:** Users can create accounts so that everything they manage is stored under their own account.
2. **Projects:** Create, view, edit and delete projects that group related parts.
3. **Assets/Parts:** Upload, edit, view and delete asset files inside a project.
4. **File Upload:** Supported 3D formats are `.stl`, `.obj` and `.fbx`, checked with server-side validation.
5. **Revisions:** Every upload is stored as a new revision of an asset, giving users a history of their work. Each revision is downloadable.
6. **Trash:** Deleted items go to the trash, where they can be reviewed and restored. <!-- TODO: keep this line only if soft delete and restore work -->

### Upload Rules

- Allowed extensions: `.stl`, `.obj`, `.fbx`
- Maximum file size: 75MB
- Uploaded files are saved to `wwwroot/uploads/` at runtime. This folder is created automatically and is not part of the repository.

## Tech Stack

| Area | Technology |
| --- | --- |
| Framework | ASP.NET Core MVC (.NET 10) |
| Database | SQL Server |
| ORM | Entity Framework Core (code-first migrations) |
| Auth | ASP.NET Core Identity |
| Front end | Razor Views, Bootstrap, jQuery Validation |

## Project Structure

```
AssetLibrary/
|-- AssetLibrary.Web/        # Controllers, Views, wwwroot, validation attributes, Program.cs
|-- AssetLibrary.Data/       # DbContext, entities, configurations, migrations
|-- AssetLibrary.Services/   # Service contracts, implementations, ViewModels
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server
- Git

NuGet packages are restored automatically when the solution is built. The packages used are:

| Package | Project |
| --- | --- |
| Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore | AssetLibrary.Data |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | AssetLibrary.Data |
| Microsoft.EntityFrameworkCore.SqlServer | AssetLibrary.Data |
| Microsoft.EntityFrameworkCore.Tools | AssetLibrary.Data, AssetLibrary.Web |
| System.Security.Cryptography.Pkcs | AssetLibrary.Data |

### Setup

1. Download the project with **Code > Download ZIP** and extract it.

2. Configure the connection string in `appsettings.json` of the `AssetLibrary.Web` project:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb);Database=AssetLibrary;Trusted_Connection=True;Encrypt=False"
   }
   ```

3. Create the database. In Visual Studio open **Tools > NuGet Package Manager > Package Manager Console**, then set:

   - **Default project:** `AssetLibrary.Data`
   - **Startup project:** `AssetLibrary.Web` (Solution Explorer > right-click > Set as Startup Project)

   and run:

   ```powershell
   Update-Database
   ```

4. Run the app (press **Ctrl+F5** in Visual Studio, or run `dotnet run --project AssetLibrary.Web`).

## Test Credentials

| Role | Email | Password |
| --- | --- | --- |
| User | user@example.com | Password123! |

You can also create your own account from the Register page.

## Usage

1. Register or log in.
2. Create a project.
3. Add an asset to the project and upload a 3D file.
4. Upload new files to the same asset to create further revisions.
5. Edit or delete projects and assets from their detail pages.

## Sample Files

Small test models are included in `AssetLibrary.Web/SeedData/Models/`. Use them to try the upload feature without needing your own 3D files.

## Database

Main entities: `User`, `Project`, `Asset`, `Revision`, `Material`, `Status`.

<img width="1225" height="875" alt="ER Diagram" src="https://github.com/user-attachments/assets/1558407c-c47c-4491-aea8-051eec476317" />

## Screenshots

<img width="1909" height="872" alt="Website Screenshot" src="https://github.com/user-attachments/assets/c8a5c032-9ddd-4415-9bf9-fe48a5f474f6" />

## Known Limitations

- Upload validation currently checks the file extension only, not the file contents.
- Uploaded files are stored in `wwwroot/uploads/`, so they can be downloaded by anyone who knows the file URL.

## Roadmap

| Feature | Description | Completed |
| --- | --- | :---: |
| 3D Viewer | View a 3D asset in the browser: rotate, zoom and pan. | ❌ |
| Tags | Custom tag system for projects. | ❌ |
| Search | Search field and filters for projects, assets and tags. | ❌ |
| Content validation | Check file headers (magic bytes) on upload. | ❌ |

## Author

Chris, built for the ASP.NET Fundamentals course.
