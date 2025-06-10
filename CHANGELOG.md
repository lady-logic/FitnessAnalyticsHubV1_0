# Changelog
All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [1.3.0] - 2025-06-10

### 🎉 Major Features
- **NEW: Dashboard Interface** - Complete dashboard implementation with activity analytics and visualizations
- Interactive charts and metrics for fitness data analysis
- Responsive design for desktop and mobile devices

### ✨ Added
- Activity overview with key performance indicators
- Visual charts for tracking progress over time
- Modern UI components with intuitive navigation

### 🛠️ Technical
- Implemented new dashboard architecture

## [1.2.0] - 2025-06-07
### 🚀 Added
- **AutoMapper integration** for consistent object mapping
- **CancellationToken support** across all repository operations
- **DatabaseConfiguration helper** for centralized database setup
- **Dedicated entity configuration classes** for better organization

### 🔄 Changed
- **Entity configurations** moved to separate IEntityTypeConfiguration classes
- **Database configuration** consolidated to single location
- **Manual property mapping** replaced with AutoMapper profiles

### 📚 Technical Improvements
- Reduced code duplication through AutoMapper
- Improved code organization with separated configurations
- Enhanced async operation support with cancellation tokens
- Better maintainability through centralized mapping logic

### 🔧 Internal
- Refactored ApplicationDbContext.OnModelCreating()
- Updated InfrastructureServiceRegistration for consolidated DB config
- Enhanced EfRepository with cancellation token support

## [1.1.0] - 2024-06-01
### 🚀 Added
- **Global Exception Handling Middleware** for centralized error handling
- **Domain-specific exceptions** for better error categorization:
  - `ActivityNotFoundException` for missing activities
  - `AthleteNotFoundException` for missing athletes
  - `StravaServiceException` family for Strava API errors
- **Structured error responses** with type, message, status code, and timestamp
- **Comprehensive unit tests** for all exception classes and middleware

### 🔄 Changed
- **BREAKING**: Controllers no longer handle exceptions directly
- **BREAKING**: Services now throw specific exceptions instead of returning null
- **Improved**: All try-catch blocks removed from controllers
- **Enhanced**: Service methods now fail fast with meaningful exceptions

### 🧹 Removed
- Manual exception handling in controllers
- Generic `Exception` usage in services
- FluentAssertions dependency in favor of standard xUnit assertions

### 🐛 Fixed
- Inconsistent error responses across different endpoints
- Generic error messages that didn't provide context
- Missing exception handling in several service methods

### 📚 Technical Details
- Added `GlobalExceptionHandlingMiddleware` in WebApi layer
- Created exception hierarchy in Domain layer
- Updated all controller and service tests
- Implemented consistent HTTP status code mapping

### 🔧 Breaking Changes
**For API consumers:**
- Error response format changed from simple strings to structured JSON
- HTTP status codes are now more specific (404 for not found, 401 for auth issues)

**For developers:**
- Services no longer return null for missing entities
- All domain exceptions must be caught by middleware
- Controller methods simplified (no more try-catch blocks)

### 📖 Migration Guide
1. Update API clients to expect new error response format
2. Remove any custom exception handling that relies on old patterns
3. Update service method calls to handle new exception types
