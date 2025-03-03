# Catalog App

This repository contains two applications:

**1. Console Application**: Imports products and categories from a CSV file into the database.
**2. Web Application**: Provides RESTful endpoint to retrieve product information.

---

## **Prerequisites**
Ensure you have the following installed:
- [.NET 8.0 SDK](https://dotnet.microsoft.com/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/)

---

## **Project Structure**
```
CatalogApp/
│-- CatalogConsoleApp/    # Console application
│-- CatalogWebApp/        # Web API project
│-- Catalog.Repository/   # Database and repositories
│-- Catalog.Service/      # Business logic services
│-- Catalog.Tests/        # Unit tests
│-- README.md             # Documentation
```

---

## **Setting Up the Database**

**Update Connection String**:
Modify `appsettings.json` in `CatalogWebApp` and `CatalogConsoleApp`:
```json
"ConnectionStrings": {
  "CatalogDB": "Server=YOUR_SERVER;Database=CatalogDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;"
}
```

**Apply Migrations**:
Run the following command from the **solution directory**:
```sh
dotnet ef database update --project Catalog.Repository --startup-project CatalogConsoleApp
```

---

## **Running the Console App**

The console app imports data from a CSV file.

### **Steps:**
```sh
cd CatalogConsoleApp
dotnet run
```

### **Expected Behavior:**
Reads the CSV file provided by the user.
Validates and filters unique categories/products.
Inserts valid data into the database.
Logs errors, summaries, invalid records to a respective log files in logs folder.

---

## **Running the Web API**

### **Steps:**
```sh
cd CatalogWebApp
dotnet run
```

### **Swagger UI:**
Once running, access the Swagger UI for API testing at:
```
http://localhost:44331/swagger
```

### **Available API Endpoints:**
| HTTP Method | Endpoint | Description |
|------------|---------|-------------|
| **GET** | `/api/products/GetProducts?page=1&pageSize=10` | Retrieves paginated products |

---

## **Unit Testing**
To run unit tests, execute:
```sh
cd Catalog.Tests
dotnet test
```

---

## **Troubleshooting**

- **Database Connection Issues:** Ensure SQL Server is running and the connection string is correct.
- **CSV Import Fails:** Check if the file format is correct (`.csv`) and data is properly formatted using ',' as delimiter.
- **API Not Working:** Ensure the API is running and Swagger is accessible at `http://localhost:44331/swagger`.