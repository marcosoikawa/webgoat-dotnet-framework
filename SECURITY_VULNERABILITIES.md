# Security Vulnerabilities Documentation

This document outlines the security vulnerabilities found in WebGoat.NET during code review. WebGoat.NET is an educational application designed to teach web security, so these vulnerabilities are intentionally present for learning purposes.

## Critical Vulnerabilities

### 1. SQL Injection
**Files:** `WebGoat/App_Code/DB/SqliteDbProvider.cs`, `WebGoat/App_Code/DB/MySqlDbProvider.cs`

**Vulnerability:** The `GetEmailByName` method uses string concatenation to build SQL queries, making it vulnerable to SQL injection attacks.

**Vulnerable Code:**
```csharp
public DataSet GetEmailByName(string name)
{
    string sql = "select firstName, lastName, email from Employees where firstName like '" + name + "%' or lastName like '" + name + "%'";
    // ... rest of method
}
```

**Attack Vector:** An attacker could input: `'; DROP TABLE Employees; --` to execute malicious SQL.

**Fix:** Use parameterized queries:
```csharp
public DataSet GetEmailByName(string name)
{
    string sql = "select firstName, lastName, email from Employees where firstName like @name + '%' or lastName like @name + '%'";
    using (SqliteConnection connection = new SqliteConnection(_connectionString))
    {
        connection.Open();
        SqliteDataAdapter da = new SqliteDataAdapter(sql, connection);
        da.SelectCommand.Parameters.AddWithValue("@name", name);
        // ... rest of method
    }
}
```

### 2. Cross-Site Scripting (XSS)
**Files:** `WebGoat/Content/ReflectedXSS.aspx.cs`, `WebGoat/Content/StoredXSS.aspx.cs`

**Vulnerability:** User input is displayed without proper HTML encoding, allowing script injection.

**Vulnerable Code (Reflected XSS):**
```csharp
void LoadCity(String city)
{
    DataSet ds = du.GetOffice(city);
    lblOutput.Text = "Here are the details for our " + city + " Office";
    // ... rest of method
}
```

**Vulnerable Code (Stored XSS):**
```csharp
void LoadComments()
{
    foreach (DataRow row in ds.Tables[0].Rows)
    {
        comments += "<strong>Email:</strong>" + row["email"] + "<br/>";
        comments += "<strong>Comment:</strong><br/>" + row["comment"] + "<br/>";
    }
    lblComments.Text = comments;
}
```

**Fix:** Use HTML encoding:
```csharp
// For Reflected XSS
lblOutput.Text = "Here are the details for our " + Server.HtmlEncode(city) + " Office";

// For Stored XSS
comments += "<strong>Email:</strong>" + Server.HtmlEncode(row["email"].ToString()) + "<br/>";
comments += "<strong>Comment:</strong><br/>" + Server.HtmlEncode(row["comment"].ToString()) + "<br/>";
```

### 3. Information Disclosure
**File:** `WebGoat/Content/ExploitDebug.aspx.cs`

**Vulnerability:** Sensitive system information is exposed through exception messages.

**Vulnerable Code:**
```csharp
protected void btnGo_Click(object sender, EventArgs e)
{
    StringBuilder strBuilder = new StringBuilder();
    strBuilder.AppendFormat("Current Dir: {0}", Environment.CurrentDirectory);
    strBuilder.AppendFormat("UserName: {0}", Environment.UserName);
    strBuilder.AppendFormat("Machine Name: {0}", Environment.MachineName);
    strBuilder.AppendFormat("OS Version: {0}", Environment.OSVersion);
    throw new Exception(strBuilder.ToString());
}
```

**Attack Vector:** Attackers can gather system information to plan further attacks.

**Fix:** Don't expose sensitive information in error messages. Use generic error messages and log details securely.

## High-Risk Vulnerabilities

### 4. Weak Random Number Generation
**Files:** `WebGoat/App_Code/WeakRandom.cs`, `WebGoat/App_Code/VeryWeakRandom.cs`

**Vulnerability:** Predictable random number generation algorithms.

**Vulnerable Code:**
```csharp
public uint Next(uint min, uint max)
{
    unchecked
    {
        _seed = _seed * _seed + _seed;
    }
    return _seed % (max - min) + min;
}
```

**Fix:** Use cryptographically secure random number generators:
```csharp
using System.Security.Cryptography;

public class SecureRandom
{
    private static readonly RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
    
    public static uint Next(uint min, uint max)
    {
        byte[] bytes = new byte[4];
        rng.GetBytes(bytes);
        uint value = BitConverter.ToUInt32(bytes, 0);
        return (value % (max - min)) + min;
    }
}
```

### 5. Weak Message Digest
**File:** `WebGoat/App_Code/WeakMessageDigest.cs`

**Vulnerability:** Custom weak hashing algorithm that's easily reversible.

**Vulnerable Code:**
```csharp
public static byte GenByte(string word)
{
    int val = 0;
    foreach(char c in word)
        val += (byte) c;
    bVal = (byte) (val % (127 - 32 -1) + 33);
    return bVal;
}
```

**Fix:** Use standard cryptographic hash functions:
```csharp
using System.Security.Cryptography;

public static string GenerateSecureHash(string input)
{
    using (SHA256 sha256 = SHA256.Create())
    {
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
```

## Medium-Risk Vulnerabilities

### 6. Configuration Security Issues
**File:** `WebGoat/Web.config`

**Vulnerabilities:**
- Debug mode enabled in production
- Custom errors disabled
- HTTP-only cookies disabled
- SSL not required
- Hardcoded credentials

**Vulnerable Configuration:**
```xml
<compilation debug="true" targetFramework="4.8">
<customErrors mode="Off"/>
<httpCookies httpOnlyCookies="false" requireSSL="false"/>
<credentials passwordFormat="Clear">
    <user name="admin" password="admin"/>
</credentials>
```

**Fix:**
```xml
<compilation debug="false" targetFramework="4.8">
<customErrors mode="On" defaultRedirect="~/Error.aspx"/>
<httpCookies httpOnlyCookies="true" requireSSL="true"/>
<!-- Remove hardcoded credentials, use secure authentication -->
```

### 7. Input Validation Issues
**File:** `WebGoat/App_Code/ConfigFile.cs`

**Vulnerability:** No input validation or error handling for file operations.

**Vulnerable Code:**
```csharp
public void Load()
{
    foreach (string line in File.ReadAllLines(_filePath))
    {
        // No validation or error handling
        if (line.Length == 0) continue;
        // ... processing without validation
    }
}
```

**Fix:** Add proper validation and error handling:
```csharp
public void Load()
{
    if (!File.Exists(_filePath))
        throw new FileNotFoundException("Configuration file not found", _filePath);
        
    try
    {
        foreach (string line in File.ReadAllLines(_filePath))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            // Validate line format before processing
            if (line.Contains('=') && !line.StartsWith("#"))
            {
                // Process valid configuration line
            }
        }
    }
    catch (Exception ex)
    {
        throw new InvalidOperationException("Error loading configuration file", ex);
    }
}
```

## Additional Security Concerns

### 8. SQL Injection in Security Questions
**File:** `WebGoat/App_Code/DB/SqliteDbProvider.cs`

**Vulnerability:** Another SQL injection in `GetSecurityQuestionAndAnswer` method.

**Vulnerable Code:**
```csharp
string sql = "select SecurityQuestions.question_text, CustomerLogin.answer from CustomerLogin, " + 
    "SecurityQuestions where CustomerLogin.email = '" + email + "' and CustomerLogin.question_id = " +
    "SecurityQuestions.question_id;";
```

**Fix:** Use parameterized queries for all database operations.

### 9. Session Management Issues
**File:** `WebGoat/Web.config`

**Vulnerability:** Long session timeout and insecure session configuration.

**Vulnerable Configuration:**
```xml
<sessionState mode="InProc" cookieless="false" timeout="20000"/>
```

**Fix:**
```xml
<sessionState mode="InProc" cookieless="false" timeout="20" regenerateExpiredSessionId="true"/>
```

## Recommendations

1. **For Educational Use:** Keep vulnerabilities but add clear documentation and examples of secure alternatives.

2. **For Production:** Never deploy this application in a production environment without addressing all vulnerabilities.

3. **Security Testing:** Use this application to practice identifying and exploiting common web vulnerabilities.

4. **Secure Coding Training:** Use the "Fixed" versions of methods as examples of secure coding practices.

5. **Regular Updates:** Keep security vulnerability examples current with modern attack techniques.

## Conclusion

This code review identified multiple critical security vulnerabilities typical of web applications. While these are intentionally present for educational purposes, understanding these vulnerabilities and their fixes is crucial for secure application development.