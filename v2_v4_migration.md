# Backblaze B2 Native API: Changelog from v2 to v4

This document outlines all significant changes made to the Backblaze B2 Native API between versions 2 and 4, focusing on new features, modified endpoints, and compatibility considerations.

## Version 4: Multi-Bucket Application Keys (April 29, 2025)

### Major Features
- Added support for Multi-Bucket Application Keys that grant access to multiple specific buckets within an account

### Modified API Endpoints

#### b2_authorize_account
- **Response Structure Changes:**
  - Removed fields: `bucketId`, `bucketName`, and `infoType` from the response
  - Added new `allowed` field containing:
    - `buckets`: Array of objects with bucket `id` and `name` fields
    - `capabilities`: List of granted capabilities
    - `namePrefix`: File name prefix restrictions (if any)
  - **Compatibility Notes:** 
    - Multi-Bucket Application Keys work only with v4 b2_authorize_account
    - Keys created with v3 b2_create_key will work with v4 b2_authorize_account
    - Keys created with v4 b2_create_key with no bucket restrictions will work with v3 b2_authorize_account

#### b2_create_key
- **Request Changes:**
  - Added `bucketIds` field: Array of bucket IDs to grant access to
  - Removed `bucketId` field (replaced by `bucketIds`)
- **Response Changes:**
  - Added `bucketIds` field in response
  - Removed `bucketId` field from response

#### b2_delete_key
- **Response Changes:**
  - Added `bucketIds` field in response
  - Removed `bucketId` field from response
- **Compatibility Notes:**
  - Only works with Multi-Bucket Application Keys when using v4
  - Returns error if earlier API versions are called with Multi-Bucket Application Keys

#### b2_list_keys
- **Response Changes:**
  - Added `bucketIds` field in response
  - Removed `bucketId` field from response
- **Compatibility Notes:**
  - Only Multi-Bucket Application Keys will be returned with v4
  - Earlier API versions will only return non-Multi-Bucket Application Keys

## Version 3: b2_authorize_account (September 23, 2021)

### Major Features
- Extended b2_authorize_account to work with multiple API suites, including B2 Storage and Partner APIs

### Modified API Endpoints

#### b2_authorize_account
- **Response Structure Changes:**
  - Added data structure that groups information by API suite
  - For each enabled API suite, provides information needed to call those APIs
  - Added new `apiInfo` structure containing:
    - `storageApi`: Information for B2 Native API
    - `groupsApi`: Information for Partner API
    - Additional API suites as applicable

## Version 2: Object Lock (May 11, 2021)

### Major Features
- Added Object Lock capabilities to view information about locked objects

### Modified API Endpoints

#### b2_list_file_names
- **Response Changes:**
  - Added `fileRetention` field with object lock retention information
  - Added `legalHold` field with legal hold information

#### b2_list_file_versions
- **Response Changes:**
  - Added `fileRetention` field with object lock retention information 
  - Added `legalHold` field with legal hold information

### New API Endpoints
- **b2_update_file_retention**: Modifies Object Lock retention settings for existing files
- **b2_update_file_legal_hold**: Sets or removes legal hold on a file

## Version 2: Server-Side Encryption (March 5, 2021)

### Major Features
- Added Server-Side Encryption support with both SSE-B2 and SSE-C modes
- Added two options for encryption with AES-256: SSE-B2 (Backblaze-managed keys) and SSE-C (Customer-managed keys)

### Modified API Endpoints

#### b2_list_file_names
- **Response Changes:**
  - Added `serverSideEncryption` field with encryption information

#### b2_list_file_versions
- **Response Changes:**
  - Added `serverSideEncryption` field with encryption information

#### b2_upload_file and b2_copy_file
- **Request Changes:**
  - Added encryption headers:
    - `X-Bz-Server-Side-Encryption`: Request SSE-B2 encryption
    - `X-Bz-Server-Side-Encryption-Customer-Algorithm`, `X-Bz-Server-Side-Encryption-Customer-Key`, `X-Bz-Server-Side-Encryption-Customer-Key-Md5`: Request SSE-C encryption

#### b2_create_bucket and b2_update_bucket
- **Request Changes:**
  - Added `defaultServerSideEncryption` field to set default encryption settings for bucket

#### b2_get_bucket_info
- **Response Changes:**
  - Added `defaultServerSideEncryption` field showing bucket encryption settings

## Version 2: App Key Workaround (Sept 13, 2018)

### Major Features
- Clean up of workaround introduced in August 9, 2018 release
- Returns exactly what is requested in a query without implicit filters based on app key

### Modified API Endpoints

#### b2_authorize_account
- **Response Changes:**
  - Added `bucketName` when app key is restricted to a bucket (already had `bucketId`)
  - Removed `minimumPartSize` field (replaced by `recommendedPartSize` and `absoluteMinimumPartSize`)

#### b2_hide_file
- **Response Changes:**
  - Added `accountId` and `bucketId` fields

#### b2_list_file_names
- **Response Changes:** 
  - Added `accountId` and `bucketId` fields
  - Removed `size` field (replaced by `contentLength`)

#### b2_list_file_versions
- **Response Changes:**
  - Added `accountId` and `bucketId` fields
  - Removed `size` field (replaced by `contentLength`)

#### b2_list_unfinished_large_files
- **Response Changes:**
  - Added `action` field (always "upload")
  - Added `contentLength` field (always null)
  - Added `contentSha1` field (always null)

#### b2_start_large_file
- **Response Changes:**
  - Added `action` field (always "upload")
  - Added `contentLength` field (always null)
  - Added `contentSha1` field (always null)

#### b2_update_bucket
- **Response Changes:**
  - Added `action` field (always "start")
  - Added `contentLength` field (0)
  - Added `contentSha1` field ("none") 