# Deployment Guide

## Overview
This document provides guidelines for deploying the EVM Management System.

## Prerequisites
- .NET 8.0 Runtime
- SQL Server Database
- Web Server (IIS, Kestrel, etc.)
- SSL Certificate (for production)

## Deployment Steps

### 1. Build the Application
- Build the solution in Release mode
- Ensure all dependencies are included

### 2. Database Setup
- Run database migrations
- Configure connection strings
- Set up initial data if needed

### 3. Configuration
- Update appsettings.json with production values
- Configure environment variables
- Set up logging

### 4. Deploy
- Copy files to deployment location
- Configure web server
- Set up reverse proxy if needed

### 5. Post-Deployment
- Verify endpoints are accessible
- Test authentication
- Monitor logs

## Environment Variables
- ASPNETCORE_ENVIRONMENT
- ConnectionStrings__DefaultConnection
- JWT settings
- Other service configurations

## Notes
- Always backup database before deployment
- Test in staging environment first
- Monitor application after deployment

