# Simple HTTP Server in C#

A somewhat minimal HTTP server implemented in C#.  

- Handles basic HTTP requests  
- Can serve static files  
- Includes a small Python script that can be used instead of tools like `curl` (with obvious limitations)  

## Requirements
- .NET 9.0  
- C# 13

## Security Concerns
- The file function is unsafe
- The function to create and compress files is unsafe
