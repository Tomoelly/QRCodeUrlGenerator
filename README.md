---

# QRCode URL Generator

A customizable console application for generating and managing unique QR code URLs. Designed for flexibility and ease of integration, this application allows users to preview generated URLs or insert them into a database. Fully commented in both English and Traditional Chinese for accessibility.

---

## Features

- **Random URL Generation**: Create unique QR code URLs with customizable length.
- **Database Integration**: Insert generated URLs into a database with a simple configuration.
- **Preview Functionality**: Preview generated URLs without database operations.
- **Bilingual Support**: Code comments and console messages are available in both Traditional Chinese and English.
- **Modular Design**: Easy to extend and integrate into other projects.

---

## Prerequisites

- .NET 6 or later
- Microsoft SQL Server or another compatible database
- Visual Studio or any C#-compatible IDE

---

## Setup

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/yourusername/QRCodeUrlGenerator.git
   cd QRCodeUrlGenerator
   ```

2. **Update Configuration**:
   - Navigate to `appsettings.json` and replace the `DefaultConnection` string with your database connection details:
     ```json
     {
       "ConnectionStrings": {
         "DefaultConnection": "Server=your-server-name;Database=your-database-name;User Id=your-username;Password=your-password;"
       }
     }
     ```

3. **Database Table Setup**:
   - Create the necessary table in your database:
     ```sql
     CREATE TABLE QRCodeTable (
         Id INT IDENTITY(1,1) PRIMARY KEY,
         FullUrl NVARCHAR(255) NOT NULL,
         Code NVARCHAR(16) NOT NULL UNIQUE,
         CreatedAt DATETIME NOT NULL
     );
     ```

4. **Build and Run the Application**:
   - Build the project in your IDE or using the command line:
     ```bash
     dotnet build
     ```
   - Run the application:
     ```bash
     dotnet run
     ```

---

## Usage

### **Main Menu**

1. **Preview Random URLs**:
   - Select `1` to generate and preview random URLs.
   - Enter the number of URLs you want to preview.
   
2. **Insert URLs into Database**:
   - Select `2` to generate and insert URLs into your database.
   - Enter the number of records to insert.
   
3. **Exit**:
   - Select `3` to terminate the program.

---

## Customization

### **Customize URL Format**

- Modify the URL format in `UrlGeneratorService.cs`:
  ```csharp
  FullUrl = $"https://yourdomain.com/QR/{code}"
  ```

### **Change Random Code Length**

- Update the length in the `RandomStringGenerator` class:
  ```csharp
  var newCode = RandomStringGenerator.GenerateRandomString(10);
  ```

---

## Example

### **Preview**
```plaintext
Select operation mode:
1. Generate and preview random URLs
2. Insert into database
3. Exit
> 1
Enter the number of random URLs to generate: 5
Generated random URLs:
https://yourdomain.com/QR/ABC123XYZ9
https://yourdomain.com/QR/MNO456UVW0
...
```

### **Insert**
```plaintext
Select operation mode:
1. Generate and preview random URLs
2. Insert into database
3. Exit
> 2
Enter the number of records to insert: 10
Inserting 10 records...
Data insertion completed.
```

---

## License

This project is licensed under the MIT License. See the LICENSE file for details.

---

## Notes

- Ensure your database is properly configured before running the application.
- This application is for generating URLs only. It does not create QR code images.
- Contributions and suggestions are welcome!

---

## Additional Notes for GitHub

1. **Add a `.gitignore` File**:
   - Ensure sensitive files like `appsettings.json` are not committed. Add it to `.gitignore` after providing an example file (`appsettings.example.json`).

2. **Set a License**:
   - Use a license like MIT for open-source contributions.

3. **Include a Description**:
   - In the GitHub repository description, mention:
     > A customizable console application for generating and managing unique QR code URLs.

4. **Enable Discussions**:
   - Allow users to ask questions or provide feedback on the project.

---

## Post-Publishing Tasks

- Test the repository with a clean clone to ensure all instructions are clear and functional.
- Share the repository link in your portfolio or resume to showcase your skills.

---
