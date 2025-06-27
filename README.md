# B2.NET
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

B2.NET is a C# client for the [Backblaze B2 Cloud Storage](https://secure.backblaze.com/b2/) service.

While the core of B2.NET is mature you should still consider this library in Beta, so use it in production at your own risk.

[B2 Documentation](https://www.backblaze.com/b2/docs/)

## Features

*  Full implementation of the B2 REST API (except Keys management)
*  Suport for uploading Streams
*  Support for file FriendlyURL's (this functionality is not part of the supported B2 api and may break at any time)
*  UFT-8 and Url Encoding support
*  Fully Async
*  Full test coverage
*  Targets .NET 4.5 and .NET Standard 1.5
*  You cannot manage keys with this library, just use already existing ones.


> [!WARNING]
> This library is not thread-safe currently. There is a global state object that is passed around to hold various configuration values.

## Install
[nuget package](https://www.nuget.org/packages/B2Net/)

```
Install-Package B2Net
```

## Guide
*  [Buckets](#buckets)
*  [Files](#files)
*  [Large Files](#largefile)
*  [Errors](#errors)

## Usage
```csharp
// the B2Client will default to the bucketId provided here
// for all subsequent calls if you set PersistBucket to true.
var options = new B2Options() {
	KeyId = "YOUR APPLICATION KEYID",
	ApplicationKey = "YOUR APPLICATION KEY",
	BucketId = "OPTIONAL BUCKET ID",
	PersistBucket = true/false
};
var client = new B2Client(options);
```

> [!NOTE]  
> If you are experiencing an SSL error when initialting requests try adding the below snippet just before client creation. (thanks
@JM63)
> Error: "The request was aborted: Could not create SSL/TLS secure channel"
> Code Snippet:
> -------------
> ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
> | SecurityProtocolType.Tls11
> | SecurityProtocolType.Tls12
> | SecurityProtocolType.Ssl3;


### Authorize
The `options` object returned from the Authorize method will contain the authorizationToken necessary for subsequent
calls to the B2 API. This will automatically be handled by the library when necessary. You do not have to keep this object around.
The `options` object requires KeyId and ApplicationKey to authorize.

**NOTE:** Authorization Tokens automatically expire after 24 hours, in some cases long-lived clients can experience a token timeout. As of v.0.8.0 the client will attempt to get a new token if it detects that it has en expired token. If you want to disable dthis functionality, pass the `NoTokenRefresh` bool in your B2Options object.

#### authorizeOnInitialize
If you set `authorizeOnInitialize` to false the B2Client will not call the B2 API until you tell it to. This requires that you call Initialize() before making any other calls with the client. The client WILL NOT automatically call this if needed, you MUST call it yourself.

#### Application Keys
Application Keys are supported for use, but you cannot manage them with this library. If you want to use an application key you must specify your KeyId, and ApplicationKey for the application key that you want to use.

```csharp
// This is the prefered way of authenticating. (Use this method if you want to use PersistBucket)
var client = new B2Client(options, authorizeOnInitialize = true);
// OR (Use this method if you want to use PersistBucket)
var client = new B2Client(B2Client.Authorize(options));
// OR
var client = new B2Client(B2Client.Authorize("KEYID", "APPLICATIONKEY"));
// OR
var client = new B2Client("KEYID", "APPLICATIONKEY", "OPTIONAL REQUESTTIMEOUT");
```

Once authenticated the capabilities of your Key are available on your `B2Client` `Capabilities` property:
```csharp
var bucketId = client.Capabilities.BucketName;
var capabilities = client.Capabilities.Capabilities;
// See here for available capabilities: [https://www.backblaze.com/b2/docs/application_keys.html](https://www.backblaze.com/b2/docs/application_keys.html)
```
**NOTE:** You must call Authorize or have an Authorized client instance before you can access Capabilities. This is because only Backblaze knows the capabilities of a key and that information is returned in the Authorization response.

### Options Object
```json
{
  "accountId": string,
  "keyId": string,
  "applicationKey": string,
  "bucketId": string,
  "persistBucket": false,
  "noTokenRefresh": false,
  "apiUrl": string,
  "s3ApiUrl": string,
  "downloadUrl": string,
  "authorizationToken": string,
  "capabilities": [
	{
	  "bucketName": string,
	  "bucketId": string,
	  "namePrefix": string,
	  "capabilities": string[]
	}
  ],
  "uploadAuthorizationToken": string,
  "recommendedPartSize": 0,
  "absoluteMinimumPartSize": 0,
  "minimumPartSize": 0,
  "requestTimeout": 0,
  "authenticated": false,
  "authTokenExpiration": "0001-01-01T00:00:00"
}
```
| Key | Description |
| --- | --- |
| AccountId | ID of your B2 Account, populated from Authorize |
| KeyId | KeyId for your application key or Master Key |
| ApplicationKey | Key text for your application or Master key |
| BucketId | BucketId you are operating on. Use for PersistBucket |
| PersistBucket | Will persist the BucketId specified across requests |
| NoTokenRefresh | Will display the automatic token refresh |
| ApiUrl | URL for the BackBlaze API, populated on Authorize |
| S3ApiURL | URL for the BackBlaze S3 compatible API, populated on Authorize |
| DownloadURL | URL used for downloading files from B2, populated on Authorize |
| AuthorizationToken | Auth Token for the current client instance, expires in 24 hours |
| Capabilities | List of capabilities of your key |
| UploadAuthorizationToken | Auth token used for uploads, populated and updated on file upload |
| RecommendedPartSize | Recommended size from B2 for file uploads, populated on Authorize |
| AbsoluteMinimumPartSize | Minimum size from B2 for file uploads, populated on Authorize |
| MinimumPartSize | Minimum size from B2 for file uploads, populated on Authorize |
| RequestTimeout | Timeout, in seconds, for the HttpClient that calls the B2 API under the hood |
| Authenticated | Whether the client is authenticated, populated on Authorize |
|| Used because a client can be initialized without being authorized |
| AuthTokenExpiration | When the auth token will expire, used internally for determining when a refresh should occur |


### <a name="buckets"></a>Buckets
#### List Buckets
```csharp
var client = new B2Client("KEYID", "APPLICATIONKEY");
var bucketList = await client.Buckets.GetList();
// [
//   { BucketId: "",
//     BucketName: "",
//     BucketType: "",
//     BucketInfo: Dictionary<string,string>,
//     LifecycleRules: List<B2BucketLifecycleRule>,
//     CORSRules: List<B2CORSRule>,
//     Revision: int }
// ]
```

#### Create a Bucket
```csharp
var client = new B2Client("KEYID", "APPLICATIONKEY");
var bucket = await client.Buckets.Create("BUCKETNAME", "OPTIONAL_BUCKET_TYPE");
// { BucketId: "",
//   BucketName: "",
//   BucketType: "",
//   BucketInfo: Dictionary<string,string>,
//   LifecycleRules: List<B2BucketLifecycleRule>,
//   CORSRules: List<B2CORSRule>,
//   Revision: int }
```

#### Create a Bucket with options
```csharp
var client = new B2Client("KEYID", "APPLICATIONKEY");
var bucket = await client.Buckets.Create("BUCKETNAME", new B2BucketOptions() {
	CacheControl = 300,
	FileLockEnabled = false,
	LifecycleRules = new System.Collections.Generic.List<B2BucketLifecycleRule>() {
		new B2BucketLifecycleRule() {
			DaysFromHidingToDeleting = 30,
			DaysFromUploadingToHiding = null,
			FileNamePrefix = ""
		}
	},
	CORSRules = new List<B2CORSRule>() {
		new B2CORSRule() {
			CorsRuleName = "corsRule",
			AllowedOrigins = new string[],
			AllowedOperations = new string[],
			AllowedHeaders = new string[],
			ExposeHeaders = new string[],
			MaxAgeSeconds = 1200
		}
	},
	DefaultRetention = new B2DefaultRetention() {
		Mode = (RetentionMode.governance || RetentionMode.compliance),
		Period = new Period() {
			Duration = 30,
			Unit = string
		}
	}
});
// { BucketId: "",
//   BucketName: "",
//   BucketType: "",
//   BucketInfo: Dictionary<string,string>,
//   LifecycleRules: List<B2BucketLifecycleRule>,
//   CORSRules: List<B2CORSRule>,
//   Revision: int }
```

#### Update a Bucket
```csharp
var client = new B2Client("KEYID", "APPLICATIONKEY");
var bucket = await client.Buckets.Update("BUCKETID", "BUCKETYPE");
// { BucketId: "",
//   BucketName: "",
//   BucketInfo: Dictionary<string,string>,
//   LifecycleRules: List<B2BucketLifecycleRule>,
//   CORSRules: List<B2CORSRule>,
//   Revision: int }
```

#### Update a Bucket with options
```csharp
var client = new B2Client("KEYID", "APPLICATIONKEY");
var bucket = await client.Buckets.Update(new B2BucketOptions() {
	CacheControl = 300,
	LifecycleRules = new List<B2BucketLifecycleRule>() {
		new B2BucketLifecycleRule() {
			DaysFromHidingToDeleting = 30,
			DaysFromUploadingToHiding = null,
			FileNamePrefix = ""
		}
	},
	CORSRules = new List<B2CORSRule>() {
		new B2CORSRule() {
			CorsRuleName = "corsRule",
			AllowedOrigins = new string[],
			AllowedOperations = new string[],
			AllowedHeaders = new string[],
			ExposeHeaders = new string[],
			MaxAgeSeconds = 1200
		}
	},
	DefaultRetention = new B2DefaultRetention() {
		Mode = (RetentionMode.governance || RetentionMode.compliance),
		Period = new Period() {
			Duration = 30,
			Unit = string
		}
	}
}, "BUCKETID");

// { BucketId: "",
//   BucketName: "",
//   BucketType: "",
//   BucketInfo: Dictionary<string,string>,
//   LifecycleRules: List<B2BucketLifecycleRule>,
//   CORSRules: List<B2CORSRule>,
//   Revision: int }
```

##### Bucket Types
```
allPrivate
allPublic
```

#### Delete a Bucket
```csharp
var client = new B2Client("KEYID", "APPLICATIONKEY");
var bucket = await client.Buckets.Delete("BUCKETID");
// { BucketId: "",
//   BucketName: "",
//   BucketType: "" }
```

### <a name="files"></a>Files
#### Get a list of files
```csharp
var client = new B2Client("KEYID", "APPLICATIONKEY");
var fileList = await client.Files.GetList("BUCKETID", "FILENAME");
// Using optional prefix and delimiter
var fileList = await client.Files.GetList("BUCKETID", "FILENAME", prefix: "PREFIX", delimiter: "DELIMITER");
// {
//   NextFileName: "",
//   [
//     { FileId: "",
//       FileName: "",
//       ContentLength: "",
//       ContentSHA1: "",
//       ContentType: "" }
//   ]
// }
```

#### Upload a file
```csharp
var client = new B2Client("KEYID", "APPLICATIONKEY");
var uploadUrl = await client.Files.GetUploadUrl("BUCKETID");
var file = await client.Files.Upload("FILEDATABYTES", "FILENAME", "CONTENTTYPE", uploadUrl, "AUTORETRY", "BUCKETID", "FILEINFOATTRS");
// { FileId: "",
//   FileName: "",
//   Action: "",
//   ContentLength: "",
//   ContentSHA1: "",
//   ContentMD5: "",
//   ContentType: "",
//   FileRetention: <FileRetention>,
//   LegalHold: <LegalHold>,
//   LegalHoldBool: bool,
//   FileInfo: Dictionary<string,string> }
```

#### Upload a file via Stream
Please note that there are certain limitations when using Streams for upload. Firstly, If you want to use SHA1 hash verification on your uploads
you will have to append the SHA1 to the end of your data stream. The library will not do this for you. It is up to you to decide how to get the SHA1
based on the type of stream you have and how you are handling it. Secondly, you may disable SHA1 verification on the upload by setting the `dontSHA`
flag to true.
```csharp
var client = new B2Client("KEYID", "APPLICATIONKEY");
var uploadUrl = await client.Files.GetUploadUrl("BUCKETID");
var file = await client.Files.Upload("FILESTREAM", "FILENAME", "CONTENTTYPE", uploadUrl, "AUTORETRY", dontSHA, "BUCKETID", "FILEINFOATTRS");
// { FileId: "",
//   FileName: "",
//   Action: "",
//   ContentLength: "",
//   ContentSHA1: "",
//   ContentMD5: "",
//   ContentType: "",
//   FileRetention: <FileRetention>,
//   LegalHold: <LegalHold>,
//   LegalHoldBool: bool,
//   FileInfo: Dictionary<string,string> }
```

#### Download a file by id
```csharp
var client = new B2Client("KEYID", "APPLICATIONKEY");
var file = await client.Files.DownloadById("FILEID");
// { FileId: "",
//   FileName: "",
//   Size: 0,
//   ContentSHA1: "",
//   FileData: byte[],
//   FileInfo: Dictionary<string,string> }
```

#### Download a file by name
```csharp
var client = new B2Client("KEYID", "APPLICATIONKEY");
var file = await client.Files.DownloadName("FILENAME", "BUCKETNAME");
// { FileId: "",
//   FileName: "",
//   Size: 0,
//   ContentSHA1: "",
//   FileData: byte[],
//   FileInfo: Dictionary<string,string> }
```

#### Copy a file by id
```csharp
var client = new B2Client("KEYID", "APPLICATIONKEY");
var file = await client.Files.Copy("FILEID", "NEWFILENAME");
// { FileId: "",
//   FileName: "",
//   Action: "",
//   ContentLength: "",
//   ContentSHA1: "",
//   ContentMD5: "",
//   ContentType: "",
//   FileRetention: <FileRetention>,
//   LegalHold: <LegalHold>,
//   LegalHoldBool: bool,
//   FileInfo: Dictionary<string,string> }
```

#### Replace a file by id
```csharp
var client = new B2Client("KEYID", "APPLICATIONKEY");
var file = await client.Files.Copy("FILEID", "NEWFILENAME", B2MetadataDirective.REPLACE, "CONTENT/TYPE");
// { FileId: "",
//   FileName: "",
//   Action: "",
//   ContentLength: "",
//   ContentSHA1: "",
//   ContentMD5: "",
//   ContentType: "",
//   FileRetention: <FileRetention>,
//   LegalHold: <LegalHold>,
//   LegalHoldBool: bool,
//   FileInfo: Dictionary<string,string> }
```

#### Get versions for a file
```csharp
var client = new B2Client("KEYID", "APPLICATIONKEY");
var file = await client.Files.GetVersions("FILENAME", "FILEID");
// {
//   NextFileName: "",
//   NextFileId: "",
//   [
//     { FileId: "",
//       FileName: "",
//       Action: "",
//       Size: "",
//       UploadTimestamp: "" }
//   ]
// }
```

#### Delete a file version
```csharp
var client = new B2Client("KEYID", "APPLICATIONKEY");
var file = await client.Files.Delete("FILEID", "FILENAME");
// { FileId: "",
//   FileName: ""}
```

#### Hide a file version
```csharp
var client = new B2Client("KEYID", "APPLICATIONKEY");
var file = await client.Files.Hide("FILEID", "BUCKETID");
// { FileId: "",
//   FileName: "",
//   Action: "",
//   Size: "",
//   UploadTimestamp: ""}
```

#### Get info for a file
```csharp
var client = new B2Client("KEYID", "APPLICATIONKEY");
var file = client.Files.GetInfo("FILEID").Result;
// { FileId: "",
//   FileName: "",
//   Action: "",
//   ContentLength: "",
//   ContentSHA1: "",
//   ContentMD5: "",
//   ContentType: "",
//   FileRetention: <FileRetention>,
//   LegalHold: <LegalHold>,
//   LegalHoldBool: bool,
//   FileInfo: Dictionary<string,string> }
```


#### Get a download authorization token
```csharp
var client = new B2Client("KEYID", "APPLICATIONKEY");
var downloadAuth = await client.Files.GetDownloadAuthorization("FILENAMEPREFIX", "VALIDDURATION", "BUCKETNAME", "CONTENTDISPOSITION");
// { FileNamePrefix: "",
//   BucketId: "",
//   AuthorizationToken: "" }
```

### <a name="largefile"></a>Large File API
See the Large File tests for usage details.


### <a name="errors"></a>Errors
Certain errors returned by B2 point to a temporary error with the upload or download of a file.
If one of these errors are encountered during an upload or download then the B2Exception that
is returned will have the `ShouldRetryRequest` flag marked true. This is an indication that you
should retry the request if you are so inclined.

## Release Notes

*  0.7.5  Stream uploading.
*  0.7.4  Content-Type setting on Upload.
*  0.7.3  Thread-safeish HttpClient.
*  0.7.2  Async the authorize method, Added initialize path that does not call the API until told.
*  0.7.1  Updated documentation and API for removal of AccountId, which is no longer needed. (thanks @seertenedos)
*  0.7.0  Fixed bug with encoding file names with /, All B2Client constructors will auto authorize with Backblaze, Capabilities surfaced to property on the B2Client, File copy API added.
*  0.6.1  Made Capabilities on the B2 Client read only, as they define what an application key can do and should not be mutable.
*  0.6.0  Preliminary support for the v2 Keys API
*  0.5.32 Fixed bug preventing the use of Keys API keys
*  0.5.31 Fixed upload bug introduced in 0.5.21
*  0.5.3  Fixed incorrect property names for B2UploadPart and added GetDownloadAuthorization
*  0.5.21 Fixed bug with formatting POST requests
*  0.5.2  Implemented Interfaces for easier testing (from @mattdewn) and fixed tests
*  0.5.0  Large file management apis (version mix up as well)
*  0.4.9  static Authorize
*  0.4.6  Configureable HttpClient timeout
*  0.4.5  File prefix and delimiter support
*  0.4.0  Added Large File support
*  0.2.5  Support for .NET 4.5 and .NET Standard 1.5 and FriendlyURL's for files
*  0.1.92 Fixed Lifecycle Rules null issue.
*  0.1.9  Added Lifecycle Rules and Cache Control to bucket creation and updating.
*  0.1.81 Switch targeting to netstandard1.3 and updated B2 endpoint
*  0.1.7  Merged changes for file name encoding and maxFileCount from tomvoros
*  0.1.6  Started URL Encoding file names.
*  0.1.5  File info attributes support.
*  0.1.0  Initial Alpha release.


## Running the tests
From the src directory run
```
dotnet pack -o ..\tests\
```

From the tests directory run
```
dotnet restore
dotnet test
```
