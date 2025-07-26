# Copilot Instructions for Returnly Web Application

<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

## Project Overview

This is a web application version of the Returnly Indian tax calculation desktop application. The project consists of:

- **Backend**: ASP.NET Core Web API (.NET 8) in the `ReturnlyWebApi` folder
- **Frontend**: React with TypeScript in the `returnly-web` folder

## Key Features

1. **Form16 PDF Processing**: Upload and extract tax data from Form16 documents
2. **Tax Calculations**: Calculate taxes using Indian tax slabs for both old and new regimes
3. **ITR Form Generation**: Generate ITR-1 and ITR-2 forms based on user data
4. **Tax Regime Comparison**: Compare old vs new tax regime benefits
5. **Responsive Web Interface**: Modern, mobile-friendly UI

## Architecture Guidelines

### Backend (ASP.NET Core Web API)
- Use Clean Architecture principles
- Implement proper error handling and validation
- Use dependency injection for services
- Follow RESTful API design patterns
- Implement file upload for PDF processing
- Use Entity Framework Core for data persistence (if needed)

### Frontend (React + TypeScript)
- Use functional components with hooks
- Implement proper state management (Context API or Redux if needed)
- Use Material-UI or similar for consistent UI components
- Implement responsive design for mobile compatibility
- Handle file uploads with progress indicators
- Use proper TypeScript types for API responses

## Business Logic

The core tax calculation and ITR generation logic should be ported from the original WPF application:
- Tax calculation services
- ITR form generation services
- Form16 data processing
- Indian tax law compliance (AY 2024-25)

## Security Considerations

- Implement proper authentication and authorization
- Validate all file uploads
- Sanitize user inputs
- Use HTTPS for all communications
- Implement rate limiting for API endpoints

## Code Quality

- Follow C# and TypeScript coding standards
- Write unit tests for business logic
- Use proper error handling and logging
- Implement proper validation on both client and server
- Document complex business logic with comments

## Dependencies to Consider

### Backend
- PdfPig for PDF processing
- FluentValidation for request validation
- Serilog for logging
- Swashbuckle for API documentation

### Frontend
- Material-UI for components
- React Router for navigation
- Axios for API calls
- React Hook Form for form handling
- React Query for server state management

When generating code, prioritize:
1. Code readability and maintainability
2. Proper error handling
3. Security best practices
4. Performance optimization
5. User experience considerations
