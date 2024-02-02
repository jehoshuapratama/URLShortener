# Link Shortener Web Application

## Overview

This is a simple link shortener web application built with ASP.NET Core MVC, C#, and Azure Cosmos DB for storage. The application allows users to shorten URLs and provides an optional expiry time for the shortened links.

## Features

- Shorten long URLs to compact and easy-to-share short links.
- Optional expiry time for links.
- Redirection from short links to the original URLs.
- Data storage in Azure Cosmos DB (NoSQL).
- Link Conflict management

## Prerequisites

Before running the application, make sure you have the following installed:

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Visual Studio](https://visualstudio.microsoft.com/) or a preferred code editor
- Azure Cosmos DB account and connection details (I already setup my Azure Cosmos DB account within this project, so do not need to create one)

## Getting Started

1. Clone the repository:

   ```bash
   git clone https://github.com/jehoshuapratama/link-shortener.git
   cd link-shortener

2. Open the URLShortener.sln in Visual Studio.
3. Run the project by pressing F5 on your laptop or simply press the run button.
