using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OWASP.WebGoat.NET.App_Code
{
    public class ConfigFile
    {
        private string _filePath;
        
        private IDictionary<string, string> _settings = new Dictionary<string, string>();
        private IDictionary<string, string> _settingComments = new Dictionary<string, string>();
               
        private UTF8Encoding _encoding = new UTF8Encoding();
            
        private const char SPLIT_CHAR = '=';
        
        public ConfigFile(string fileName)
        {
            _filePath = fileName;
        }
            
        //TODO: Obviously no checks for problems, so when you get time do it like bhudda.
        public void Load()
        {
            // VULNERABLE: No input validation, error handling, or file existence checks
            string comment = string.Empty;
            
            //It's all or nothing here buddy.
            foreach (string line in File.ReadAllLines(_filePath))
            {
                
                if (line.Length == 0)
                    continue;
                
                if (line[0] == '#')
                {
                    comment = line;
                    continue;
                }
                 
                string[] tokens = line.Split(SPLIT_CHAR);
                
                if (tokens.Length >=2)
                {
                    string key = tokens[0].ToLower();
                    _settings[key] = tokens[1];
                    
                    if (!string.IsNullOrEmpty(comment))
                        _settingComments[key] = comment;
                }   
            
                comment = string.Empty;        
            }
        }

        // SECURE VERSION: Proper validation and error handling
        public void LoadSecure()
        {
            // Input validation
            if (string.IsNullOrEmpty(_filePath))
                throw new ArgumentException("File path cannot be null or empty");
            
            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"Configuration file not found: {_filePath}");
            
            try
            {
                string comment = string.Empty;
                
                foreach (string line in File.ReadAllLines(_filePath))
                {
                    // Skip empty lines
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    
                    // Handle comments
                    if (line.TrimStart().StartsWith("#"))
                    {
                        comment = line;
                        continue;
                    }
                     
                    // Validate line format
                    if (!line.Contains(SPLIT_CHAR))
                    {
                        comment = string.Empty;
                        continue;
                    }
                    
                    string[] tokens = line.Split(new[] { SPLIT_CHAR }, 2); // Split into max 2 parts
                    
                    if (tokens.Length == 2 && !string.IsNullOrWhiteSpace(tokens[0]))
                    {
                        string key = tokens[0].Trim().ToLower();
                        string value = tokens[1]?.Trim() ?? string.Empty;
                        
                        // Validate key doesn't contain invalid characters
                        if (key.IndexOfAny(new[] { '\r', '\n', '\t' }) >= 0)
                        {
                            comment = string.Empty;
                            continue;
                        }
                        
                        _settings[key] = value;
                        
                        if (!string.IsNullOrEmpty(comment))
                            _settingComments[key] = comment;
                    }   
                
                    comment = string.Empty;        
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException($"Access denied to configuration file: {_filePath}", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"Error reading configuration file: {_filePath}", ex);
            }
        }
            
        public void Save()
        {
            // VULNERABLE: No error handling or validation
            using (FileStream stream = File.Create(_filePath))
            {
                byte[] data = ToByteArray();
                
                stream.Write(data, 0, data.Length);
            }
        }

        // SECURE VERSION: Proper error handling and validation
        public void SaveSecure()
        {
            // Input validation
            if (string.IsNullOrEmpty(_filePath))
                throw new ArgumentException("File path cannot be null or empty");
            
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                
                byte[] data = ToByteArray();
                
                // Validate data before writing
                if (data == null || data.Length == 0)
                    throw new InvalidOperationException("No data to save");
                
                // Atomic write - write to temp file first, then rename
                string tempPath = _filePath + ".tmp";
                
                using (FileStream stream = File.Create(tempPath))
                {
                    stream.Write(data, 0, data.Length);
                    stream.Flush();
                }
                
                // Replace original file with temp file
                if (File.Exists(_filePath))
                    File.Delete(_filePath);
                    
                File.Move(tempPath, _filePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException($"Access denied to configuration file: {_filePath}", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"Error writing configuration file: {_filePath}", ex);
            }
        }
            
        private byte[] ToByteArray()
        {
            StringBuilder builder = new StringBuilder();
                
            foreach (var pair in _settings)
            {
                if (_settingComments.ContainsKey(pair.Key))
                {
                    builder.Append(_settingComments[pair.Key]);
                    builder.AppendLine();
                }
                
                builder.AppendFormat("{0}={1}", pair.Key, pair.Value);
                builder.AppendLine();
            }
                
            return _encoding.GetBytes(builder.ToString());
        }
            
        public string Get(string key)
        {
            key = key.ToLower();
            
            if (_settings.ContainsKey(key))
                return _settings[key];
                    
            return string.Empty;
        }
            
        public void Set(string key, string value)
        {
            _settings[key.ToLower()] = value;
        }

        public void Remove(string key)
        {
            _settings.Remove(key.ToLower());
        }
    }
}

