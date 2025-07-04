<%@ Page Title="Security Comparison Demo" Language="C#" MasterPageFile="~/Resources/Master-Pages/Site.Master" AutoEventWireup="true" CodeBehind="SecurityComparison.aspx.cs" Inherits="OWASP.WebGoat.NET.SecurityComparison" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContentPlaceHolder" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContentPlaceholder" runat="server">
    <h2>Security Comparison Demo</h2>
    <p>This page demonstrates the difference between vulnerable and secure implementations of common functionality.</p>
    
    <div style="background-color: #f0f0f0; padding: 15px; margin: 10px 0; border-left: 4px solid #d9534f;">
        <h3>SQL Injection Comparison</h3>
        <p><strong>Test SQL Injection:</strong> Try entering: <code>'; DROP TABLE Employees; --</code></p>
        
        <table>
            <tr>
                <td>Search Name:</td>
                <td><asp:TextBox ID="txtNameSearch" runat="server" Text="John" /></td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Button ID="btnVulnerable" runat="server" Text="Vulnerable Search" OnClick="btnVulnerable_Click" BackColor="#d9534f" ForeColor="White" />
                    <asp:Button ID="btnSecure" runat="server" Text="Secure Search" OnClick="btnSecure_Click" BackColor="#5cb85c" ForeColor="White" />
                </td>
            </tr>
        </table>
        
        <asp:GridView ID="grdResults" runat="server" AutoGenerateColumns="true" />
        <asp:Label ID="lblSqlError" runat="server" ForeColor="Red" />
    </div>
    
    <div style="background-color: #f0f0f0; padding: 15px; margin: 10px 0; border-left: 4px solid #f0ad4e;">
        <h3>Hash Comparison</h3>
        <p><strong>Test Message Hashing:</strong> Compare weak vs. secure hash algorithms</p>
        
        <table>
            <tr>
                <td>Message:</td>
                <td><asp:TextBox ID="txtMessage" runat="server" Text="Hello World" /></td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Button ID="btnWeakHash" runat="server" Text="Weak Hash" OnClick="btnWeakHash_Click" BackColor="#d9534f" ForeColor="White" />
                    <asp:Button ID="btnSecureHash" runat="server" Text="Secure Hash" OnClick="btnSecureHash_Click" BackColor="#5cb85c" ForeColor="White" />
                </td>
            </tr>
        </table>
        
        <asp:Label ID="lblHashResults" runat="server" />
    </div>
    
    <div style="background-color: #f0f0f0; padding: 15px; margin: 10px 0; border-left: 4px solid #5bc0de;">
        <h3>Random Number Comparison</h3>
        <p><strong>Test Random Number Generation:</strong> Compare weak vs. secure random number generators</p>
        
        <table>
            <tr>
                <td>Min:</td>
                <td><asp:TextBox ID="txtMin" runat="server" Text="1" /></td>
            </tr>
            <tr>
                <td>Max:</td>
                <td><asp:TextBox ID="txtMax" runat="server" Text="100" /></td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Button ID="btnWeakRandom" runat="server" Text="Weak Random" OnClick="btnWeakRandom_Click" BackColor="#d9534f" ForeColor="White" />
                    <asp:Button ID="btnSecureRandom" runat="server" Text="Secure Random" OnClick="btnSecureRandom_Click" BackColor="#5cb85c" ForeColor="White" />
                </td>
            </tr>
        </table>
        
        <asp:Label ID="lblRandomResults" runat="server" />
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="HelpContentPlaceholder" runat="server">
    <h3>About This Demo</h3>
    <p>This page demonstrates the security differences between vulnerable and secure implementations:</p>
    <ul>
        <li><strong>SQL Injection:</strong> Shows how parameterized queries prevent SQL injection attacks</li>
        <li><strong>Hash Functions:</strong> Compares weak custom hash with secure SHA-256</li>
        <li><strong>Random Numbers:</strong> Shows predictable vs. cryptographically secure random generation</li>
    </ul>
    <p><strong>Educational Purpose:</strong> This comparison helps understand why secure coding practices are essential.</p>
</asp:Content>