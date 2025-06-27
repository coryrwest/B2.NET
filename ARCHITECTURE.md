
# B2.NET Architecture and Style Guide

## Overview

B2.NET is a C# client library for Backblaze B2 Cloud Storage service. It provides a structured, object-oriented approach to interact with the B2 API, handling authentication, request generation, and response parsing.

## Project Structure

The project is organized into the following main components:

- **Core Client Classes**: Main entry point for API interactions
- **Service-specific Classes**: Specialized classes for different B2 services
- **HTTP Layer**: Request generators and response parsers
- **Models**: Data transfer objects and domain models
- **Utilities**: Helper methods and extensions

## Architecture

### Client Architecture

1. **Entry Point: B2Client**
   - Main class that users interact with
   - Initializes and manages authentication
   - Provides access to service-specific functionality through properties

2. **Service Classes**
   - Specialized classes for different B2 services:
     - `Buckets`: Bucket management operations
     - `Files`: File operations
     - `LargeFiles`: Large file operations
   - Each implements a corresponding interface (IBuckets, IFiles, ILargeFiles)

3. **Options and State Management**
   - `B2Options`: Central configuration and state management
   - Stores authentication tokens, API URLs, and other settings
   - Passed between components to maintain state

### HTTP Layer

1. **Request Generation**
   - Request generators create HttpRequestMessage objects
   - Organized by service area (Auth, Bucket, File, etc.)
   - Base request generator provides common functionality

2. **Response Parsing**
   - `ResponseParser`: Converts HTTP responses to model objects
   - Handles error checking and JSON deserialization

3. **HttpClient Management**
   - Single HttpClient instance managed by HttpClientFactory
   - Configured with appropriate headers and timeout settings

## Code Conventions

### API Call Pattern

API calls follow a consistent pattern:

1. **Authentication Check**
   - Refresh authorization if needed
   - Determine bucket ID if applicable

2. **Request Generation**
   - Create appropriate HttpRequestMessage
   - Set headers, content, and other request properties

3. **HTTP Request Execution**
   - Send request using HttpClient
   - Handle cancellation tokens for async operations

4. **Response Processing**
   - Parse response using ResponseParser
   - Convert JSON to appropriate model objects
   - Handle errors and exceptions

### Error Handling

1. **Exception Types**
   - `AuthorizationException`: Authentication errors
   - `NotAuthorizedException`: Permission issues
   - `B2Exception`: General B2 API errors
   - Standard .NET exceptions for other errors

2. **Error Response Processing**
   - Errors are checked in the ResponseParser
   - B2 error responses are converted to appropriate exceptions

### Asynchronous Programming

1. **Task-based Asynchronous Pattern**
   - All API calls are asynchronous
   - Methods return Task or Task<T>
   - Support for cancellation tokens

2. **Async/Await Usage**
   - Consistent use of async/await throughout the codebase
   - Proper exception propagation in async methods

## Design Patterns

1. **Dependency Injection**
   - Services receive dependencies through constructors
   - Promotes testability and loose coupling

2. **Factory Pattern**
   - HttpClientFactory creates and configures HttpClient instances
   - Request generators create specialized HTTP requests

3. **Interface-based Design**
   - Core functionality exposed through interfaces
   - Enables mocking and testing

4. **Functional Approach for Authorization**
   - Authorization function passed to services
   - Allows for refreshing tokens when needed

## Threading Considerations

The library is not thread-safe. There is a global state object (`B2Options`) that is passed around to hold various configuration values, which can lead to race conditions if multiple threads access it simultaneously.


## Limitations

1. No support for application key management
2. Missing support for certain B2 API features (see missing_features.md)
3. Not thread-safe
4. Limited retry logic for failed requests 