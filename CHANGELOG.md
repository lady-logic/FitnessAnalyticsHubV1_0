# Changelog
All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [1.3.0] - 2025-06-14
### 🤖 Added
- **HuggingFace AI Integration** for intelligent fitness analytics
- **Gemini AI Integration** for enhanced workout analysis and insights
- **AIAssistantClientService** with HttpClient-based communication
- **Workout Analysis AI Endpoints** powered by Meta-Llama-3.1-8B-Instruct model
- **Intelligent fallback system** for API rate limiting and errors
- **Health checks** for AI service availability monitoring
- **AI-powered markdown formatting** for structured response display

### 🔄 Changed
- **Enhanced workout analysis** with AI-powered insights and trends
- **Improved user experience** with motivational AI responses
- **Service architecture** extended with AI microservice communication
- **AI Insights Panel** redesigned with structured content presentation
- **Dashboard UI** enhanced with formatted AI response display

### 🎨 UI/UX Improvements
- **Structured AI Response Display** with headers, lists, and emphasis
- **Performance Score Visualization** with color-coded progress bars
- **Enhanced Typography** for AI insights with improved readability
- **Responsive Design** for AI content on mobile devices
- **Visual Hierarchy** improvements in AI insights panel

### 🔧 Technical Improvements
- Integrated HuggingFace Inference API for natural language processing
- Implemented proper error handling for AI service failures
- Added dependency injection configuration for AI services
- Created robust HTTP client setup with base address configuration
- Established microservice communication patterns
- **Advanced markdown parsing** for AI response formatting
- **CSS styling system** for structured AI content display
- **Error boundary implementation** for AI service failures

### 🛠️ Infrastructure
- **AI Microservice** configured on port 5169
- **Service-to-service communication** via HTTP API calls
- **Endpoint routing** for `/api/WorkoutAnalysis/analyze/huggingface`
- **Cross-service error handling** with graceful degradation
- **Frontend AI service integration** with proper TypeScript interfaces

### 📊 Features
- **Performance Insights**: Intelligent progression tracking
- **Training Recommendations**: Personalized coaching suggestions
- **Interactive AI Analysis**: Click-to-analyze individual activities
- **Real-time Performance Scoring**: Immediate feedback on workouts
- **Structured Insights Display**: Headers, bullet points, and emphasis formatting

### 🔧 Internal
- Fixed endpoint URL routing issues (404 → 200 responses)
- Implemented proper JSON serialization for AI requests
- Added comprehensive logging for AI service interactions
- Created fallback response generation for service availability
- **Enhanced error handling** for frontend AI integration
- **Improved loading states** for AI analysis operations
- **TypeScript interfaces** for AI service communication
- **Markdown sanitization** and formatting pipeline

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
