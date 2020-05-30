# O365AuditLogExtract [![Build Status](https://github.com/prdpsvs/O365AuditLogExtract/workflows/CI/badge.svg?branch=master)](https://github.com/prdpsvs/O365AuditLogExtract)

O365 AuditLog extract solution contains implementation of ETL process to extract activities from Office 365 using O365 Management Activity Api and ingest these activities to Azure SQL Database.

## Steps to deploy O365AuditLogExtract solution

1. Update CI.yaml file to generate outputs to deploy O365AuditLogExtract solution
2. Deploy following Resources as infrastructure to Azure
    * Azure Web App (Asp.Net MVC 4.7 or Latest)
    * Application Insights
    * Azure SQL Server with Database
    * Azure Key Vault
    * Create App Registration to access secrets from Azure Key Vault
    * Create App Registration and add Application Id & Client Secret of App Registration to Azure Key Vault as secrets. Expose O365 Management Activity Api permissions to read activities using O365 Management Activity Api.

4. Create Action Flow to deploy generated outputs from CI.yaml file
    * Update appsettings.json with Environment variables and secrets
    * Deploy package file generated from CI workflow
    
## Network Diagram 

[VNET with Service Endpoints](/Networking Diagram with VNET & Service Endpoint.png)



