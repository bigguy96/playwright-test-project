# Playwright NTLM Demo

This project demonstrates how to use **C# Playwright** to automate testing of an internal .NET MVC application with **NTLM authentication**.  
The setup is designed to be **self-contained** and portable across machines.

---

## 🚀 Getting Started

### 1. Prerequisites
- [.NET 6 SDK or later](https://dotnet.microsoft.com/en-us/download)
- PowerShell (Windows PowerShell 5.1 or PowerShell 7+)
- Visual Studio 2022 (optional, but recommended for development)

---

## 🔧 Initial Setup

Install the Playwright CLI globally (only needed once):

```powershell
dotnet tool install --global Microsoft.Playwright.CLI
playwright install
```

---

### 2. Clone and Set Up

If you cloned this repo using **Visual Studio 2022**, you can run the setup script directly from the built-in terminal:

1. Open the solution in Visual Studio.  
2. Go to **View > Terminal** or press `` Ctrl+` `` to open the terminal.  
3. Make sure you are in the **repo root directory** (where `setup.ps1` is located).  
4. Run:
   ```powershell
   ./setup.ps1
   ```

---

## 🚀 Running the Setup Script

From the **repo root**, you can run the script using either **Windows PowerShell** or **PowerShell Core**:

### Windows PowerShell (Default in Visual Studio Terminal)
```powershell
powershell -ExecutionPolicy Bypass -File .\setup.ps1
```
