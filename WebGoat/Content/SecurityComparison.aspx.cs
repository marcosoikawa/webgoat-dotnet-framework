using System;
using System.Data;
using System.Web.UI;
using OWASP.WebGoat.NET.App_Code;
using OWASP.WebGoat.NET.App_Code.DB;

namespace OWASP.WebGoat.NET
{
    public partial class SecurityComparison : System.Web.UI.Page
    {
        private IDbProvider du = Settings.CurrentDbProvider;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblSqlError.Text = string.Empty;
                lblHashResults.Text = string.Empty;
                lblRandomResults.Text = string.Empty;
            }
        }

        protected void btnVulnerable_Click(object sender, EventArgs e)
        {
            try
            {
                // Use the vulnerable method
                DataSet ds = du.GetEmailByName(txtNameSearch.Text);
                
                if (ds != null && ds.Tables.Count > 0)
                {
                    grdResults.DataSource = ds.Tables[0];
                    grdResults.DataBind();
                    lblSqlError.Text = "<br/><span style='color: red;'>VULNERABLE: This search used string concatenation and is susceptible to SQL injection!</span>";
                }
                else
                {
                    grdResults.DataSource = null;
                    grdResults.DataBind();
                    lblSqlError.Text = "<br/><span style='color: red;'>No results found (or SQL injection prevented by database)</span>";
                }
            }
            catch (Exception ex)
            {
                lblSqlError.Text = "<br/><span style='color: red;'>Error: " + Server.HtmlEncode(ex.Message) + "</span>";
                grdResults.DataSource = null;
                grdResults.DataBind();
            }
        }

        protected void btnSecure_Click(object sender, EventArgs e)
        {
            try
            {
                // Use the secure method
                DataSet ds = du.GetEmailByNameSecure(txtNameSearch.Text);
                
                if (ds != null && ds.Tables.Count > 0)
                {
                    grdResults.DataSource = ds.Tables[0];
                    grdResults.DataBind();
                    lblSqlError.Text = "<br/><span style='color: green;'>SECURE: This search used parameterized queries and is protected from SQL injection!</span>";
                }
                else
                {
                    grdResults.DataSource = null;
                    grdResults.DataBind();
                    lblSqlError.Text = "<br/><span style='color: green;'>No results found (secure search)</span>";
                }
            }
            catch (Exception ex)
            {
                lblSqlError.Text = "<br/><span style='color: red;'>Error: " + Server.HtmlEncode(ex.Message) + "</span>";
                grdResults.DataSource = null;
                grdResults.DataBind();
            }
        }

        protected void btnWeakHash_Click(object sender, EventArgs e)
        {
            try
            {
                string message = txtMessage.Text;
                string weakHash = WeakMessageDigest.GenerateWeakDigest(message);
                
                lblHashResults.Text = "<br/><strong>Weak Hash Result:</strong><br/>" +
                    "<span style='color: red;'>Message: " + Server.HtmlEncode(message) + "</span><br/>" +
                    "<span style='color: red;'>Weak Hash: " + Server.HtmlEncode(weakHash) + "</span><br/>" +
                    "<span style='color: red;'>⚠️ WARNING: This hash is easily reversible and predictable!</span>";
            }
            catch (Exception ex)
            {
                lblHashResults.Text = "<br/><span style='color: red;'>Error: " + Server.HtmlEncode(ex.Message) + "</span>";
            }
        }

        protected void btnSecureHash_Click(object sender, EventArgs e)
        {
            try
            {
                string message = txtMessage.Text;
                string secureHash = SecureMessageDigest.GenerateSecureHash(message);
                
                // Also demonstrate salted hash
                string salt;
                string saltedHash = SecureMessageDigest.GenerateSecureHashWithRandomSalt(message, out salt);
                
                lblHashResults.Text = "<br/><strong>Secure Hash Results:</strong><br/>" +
                    "<span style='color: green;'>Message: " + Server.HtmlEncode(message) + "</span><br/>" +
                    "<span style='color: green;'>SHA-256 Hash: " + Server.HtmlEncode(secureHash) + "</span><br/>" +
                    "<span style='color: green;'>Salt: " + Server.HtmlEncode(salt) + "</span><br/>" +
                    "<span style='color: green;'>Salted Hash: " + Server.HtmlEncode(saltedHash) + "</span><br/>" +
                    "<span style='color: green;'>✅ SECURE: This hash is cryptographically secure!</span>";
            }
            catch (Exception ex)
            {
                lblHashResults.Text = "<br/><span style='color: red;'>Error: " + Server.HtmlEncode(ex.Message) + "</span>";
            }
        }

        protected void btnWeakRandom_Click(object sender, EventArgs e)
        {
            try
            {
                uint min = uint.Parse(txtMin.Text);
                uint max = uint.Parse(txtMax.Text);
                
                WeakRandom weakRandom = new WeakRandom();
                
                // Generate 10 numbers to show the pattern
                string results = "<br/><strong>Weak Random Results:</strong><br/>";
                results += "<span style='color: red;'>10 'random' numbers: ";
                
                for (int i = 0; i < 10; i++)
                {
                    uint number = weakRandom.Next(min, max);
                    results += number + " ";
                }
                
                results += "</span><br/>";
                results += "<span style='color: red;'>⚠️ WARNING: These numbers follow a predictable pattern!</span>";
                
                lblRandomResults.Text = results;
            }
            catch (Exception ex)
            {
                lblRandomResults.Text = "<br/><span style='color: red;'>Error: " + Server.HtmlEncode(ex.Message) + "</span>";
            }
        }

        protected void btnSecureRandom_Click(object sender, EventArgs e)
        {
            try
            {
                uint min = uint.Parse(txtMin.Text);
                uint max = uint.Parse(txtMax.Text);
                
                using (SecureRandom secureRandom = new SecureRandom())
                {
                    // Generate 10 numbers to show randomness
                    string results = "<br/><strong>Secure Random Results:</strong><br/>";
                    results += "<span style='color: green;'>10 cryptographically secure random numbers: ";
                    
                    for (int i = 0; i < 10; i++)
                    {
                        uint number = secureRandom.Next(min, max);
                        results += number + " ";
                    }
                    
                    results += "</span><br/>";
                    results += "<span style='color: green;'>✅ SECURE: These numbers are cryptographically random!</span>";
                    
                    lblRandomResults.Text = results;
                }
            }
            catch (Exception ex)
            {
                lblRandomResults.Text = "<br/><span style='color: red;'>Error: " + Server.HtmlEncode(ex.Message) + "</span>";
            }
        }
    }
}