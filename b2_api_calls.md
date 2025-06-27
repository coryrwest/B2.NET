# Backblaze B2 Native API Calls

Below is a list of the main B2 Native API calls, their accepted parameters, and features, based on the official documentation.

---

## Authentication
- **b2_authorize_account**
  - **Parameters:** applicationKeyId, applicationKey
  - **Features:** Returns account info, API URLs, and an authorization token for subsequent requests.

## Key Management
- **b2_create_key**
  - **Parameters:** accountId, capabilities, keyName, validDurationInSeconds, bucketId, namePrefix
  - **Features:** Create a new application key with specific permissions.
- **b2_delete_key**
  - **Parameters:** applicationKeyId
  - **Features:** Delete an application key.
- **b2_list_keys**
  - **Parameters:** accountId, maxKeyCount, startApplicationKeyId
  - **Features:** List application keys for the account.

## Bucket Management
- **b2_create_bucket**
  - **Parameters:** accountId, bucketName, bucketType, bucketInfo, corsRules, lifecycleRules, defaultRetention, fileLockEnabled
  - **Features:** Create a new bucket with optional settings.
- **b2_delete_bucket**
  - **Parameters:** accountId, bucketId
  - **Features:** Delete a bucket.
- **b2_list_buckets**
  - **Parameters:** accountId, bucketId, bucketName
  - **Features:** List all buckets for the account.
- **b2_update_bucket**
  - **Parameters:** accountId, bucketId, bucketType, bucketInfo, corsRules, lifecycleRules, defaultRetention, ifRevisionIs
  - **Features:** Update bucket settings.
- **b2_get_bucket_notification_rules**
  - **Parameters:** bucketId
  - **Features:** Lists bucket event notification rules alphabetically by rule name.
- **b2_set_bucket_notification_rules**
  - **Parameters:** bucketId, eventNotificationRules
  - **Features:** Replace the event notification rules of an existing bucket.

## File Management
- **b2_list_file_names**
  - **Parameters:** bucketId, startFileName, maxFileCount, prefix, delimiter
  - **Features:** List files in a bucket.
- **b2_list_file_versions**
  - **Parameters:** bucketId, startFileName, startFileId, maxFileCount, prefix, delimiter
  - **Features:** List all versions of files in a bucket.
- **b2_get_file_info**
  - **Parameters:** fileId
  - **Features:** Get metadata for a file.
- **b2_hide_file**
  - **Parameters:** bucketId, fileName
  - **Features:** Hide a file (makes it invisible to list operations).
- **b2_delete_file_version**
  - **Parameters:** fileName, fileId
  - **Features:** Delete a specific version of a file.

## File Upload/Download
- **b2_get_upload_url**
  - **Parameters:** bucketId
  - **Features:** Get a URL and auth token for uploading files.
- **b2_upload_file**
  - **Parameters:** uploadUrl, uploadAuthToken, file data, fileName, contentType, contentSha1, fileInfo, serverSideEncryption
  - **Features:** Upload a file.
- **b2_get_download_authorization**
  - **Parameters:** bucketId, fileNamePrefix, validDurationInSeconds, b2ContentDisposition, b2ContentLanguage, b2Expires, b2CacheControl, b2ContentEncoding, b2ContentType
  - **Features:** Get a download authorization token for private files.
- **b2_download_file_by_id**
  - **Parameters:** fileId, Range (header)
  - **Features:** Download a file by its ID.
- **b2_download_file_by_name**
  - **Parameters:** bucketName, fileName, Range (header)
  - **Features:** Download a file by its name.

## Large File Management
- **b2_start_large_file**
  - **Parameters:** bucketId, fileName, contentType, fileInfo
  - **Features:** Initiate a large file upload.
- **b2_get_upload_part_url**
  - **Parameters:** fileId
  - **Features:** Get a URL and auth token for uploading a part of a large file.
- **b2_upload_part**
  - **Parameters:** uploadUrl, uploadAuthToken, part data, partNumber, contentLength, contentSha1
  - **Features:** Upload a part of a large file.
- **b2_finish_large_file**
  - **Parameters:** fileId, partSha1Array
  - **Features:** Complete a large file upload.
- **b2_cancel_large_file**
  - **Parameters:** fileId
  - **Features:** Cancel a large file upload.
- **b2_list_parts**
  - **Parameters:** fileId, startPartNumber, maxPartCount
  - **Features:** List uploaded parts for a large file.
- **b2_list_unfinished_large_files**
  - **Parameters:** bucketId, namePrefix, startFileId, maxFileCount
  - **Features:** Lists information about large file uploads that have been started but not finished or canceled.
- **b2_copy_part**
  - **Parameters:** sourceFileId, largeFileId, partNumber, range
  - **Features:** Copy a part from an existing file to a large file being uploaded.

## File Copy/Retention/Legal Hold
- **b2_copy_file**
  - **Parameters:** sourceFileId, fileName, destinationBucketId, metadataDirective, contentType, fileInfo, range
  - **Features:** Copy a file within B2.
- **b2_update_file_retention**
  - **Parameters:** fileId, fileName, retention, bypassGovernance
  - **Features:** Update file retention settings.
- **b2_update_file_legal_hold**
  - **Parameters:** fileId, fileName, legalHold
  - **Features:** Set or remove a legal hold on a file.

---

**Note:**
- All API calls require an Authorization header with a valid token (except for b2_authorize_account).
- Many calls accept optional parameters for advanced features (e.g., CORS, lifecycle, retention, encryption).
- For full details, see the [Backblaze B2 Native API Documentation](https://www.backblaze.com/apidocs/introduction-to-the-b2-native-api).
