# B2.NET
[![ghit.me](https://ghit.me/badge.svg?repo=coryrwest/B2.NET)](https://ghit.me/repo/coryrwest/B2.NET)

B2.NET aims to be a C# client for the [Backblaze B2 Cloud Storage](https://secure.backblaze.com/b2/) service.

B2.NET is still in early Alpha, so use it in production at your own risk.

[B2 Documentation](https://www.backblaze.com/b2/docs/)

## Features

*  99% implementation of the B2 REST API
*  Large file upload support (planned)
*  UFT-8 and Url Encoding support (planned)
*  Full Async support
*  Full test coverage
*  Targets .NET 4.5

## NuGet

```
Install-Package B2Net
```

## Install

Stick the B2Net.ddl in your project.

## Usage
```csharp
// the B2Client will default to the bucketId provided here
// for all subsequent calls if you set PersistBucket to true.
var options = new B2Options() {
	AccountId = "YOUR ACCOUNT ID",
	ApplicationKey = "YOUR APPLICATION KEY",
	BucketId = "OPTIONAL BUCKET ID",
	PersistBucket = true/false
};
var client = new B2Client(Options);
```

### Authorize
```csharp
// the returned options object will contain the authorizationToken
// necessary for subsequent calls to the B2 API.
var options = client.Authorize().Result;
```

### List Buckets
```csharp
var client = new B2Client(options);
options = client.Authorize().Result;
var bucketList = client.Buckets.GetList().Result;
// [
//   { BucketId: "",
//     BucketName: "",
//     BucketType: "" }
// ]
```

### Create a Bucket
```csharp
var client = new B2Client(options);
options = client.Authorize().Result;
var bucket = client.Buckets.Create("BUCKETNAME", "OPTIONAL_BUCKET_TYPE").Result;
// { BucketId: "",
//   BucketName: "",
//   BucketType: "" }
```

### Update a Bucket
```csharp
var client = new B2Client(options);
options = client.Authorize().Result;
var bucket = client.Buckets.Update("BUCKETID", "BUCKETYPE").Result;
// { BucketId: "",
//   BucketName: "",
//   BucketType: "" }
```

#### Bucket Types
```
allPrivate
allPublic
```

### Delete a Bucket
```csharp
var client = new B2Client(options);
options = client.Authorize().Result;
var bucket = client.Buckets.Delete("BUCKETID").Result;
// { BucketId: "",
//   BucketName: "",
//   BucketType: "" }
```

### Get a list of files
```csharp
var client = new B2Client(options);
options = client.Authorize().Result;
var fileList = client.Files.GetList("BUCKETID", "FILENAME").Result;
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

### Upload a file
```csharp
var client = new B2Client(options);
options = client.Authorize().Result;
var file = client.Files.Upload("FILEDATABYTES", "FILENAME", "BUCKETID", "FILEINFOATTRS").Result;
// { FileId: "",
//   FileName: "",
//   ContentLength: "", 
//   ContentSHA1: "", 
//   ContentType: "",
//   FileInfo: Dictionary<string,string> }
```

### Download a file by id
```csharp
var client = new B2Client(options);
options = client.Authorize().Result;
var file = client.Files.DownloadById("FILEID").Result;
// { FileId: "",
//   FileName: "",
//   ContentLength: "", 
//   ContentSHA1: "", 
//   ContentType: "",
//   FileData: byte[],
//   FileInfo: Dictionary<string,string> }
```

### Download a file by name
```csharp
var client = new B2Client(options);
options = client.Authorize().Result;
var file = client.Files.DownloadName("FILENAME", "BUCKETNAME").Result;
// { FileId: "",
//   FileName: "",
//   ContentLength: "", 
//   ContentSHA1: "", 
//   ContentType: "",
//   FileData: byte[],
//   FileInfo: Dictionary<string,string> }
```

### Get versions for a file
```csharp
var client = new B2Client(options);
options = client.Authorize().Result;
var file = client.Files.GetVersions("FILENAME", "FILEID").Result;
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

### Delete a file version
```csharp
var client = new B2Client(options);
options = client.Authorize().Result;
var file = client.Files.Delete("FILEID", "FILENAME").Result;
// { FileId: "",
//   FileName: ""}
```

### Hide a file version
```csharp
var client = new B2Client(options);
options = client.Authorize().Result;
var file = client.Files.Hide("FILEID", "BUCKETID").Result;
// { FileId: "",
//   FileName: "",
//   Action: "",
//   Size: "",
//   UploadTimestamp: ""}
```

### Get info for a file
```csharp
var client = new B2Client(options);
options = client.Authorize().Result;
var file = client.Files.GetInfo("FILEID").Result;
// { FileId: "",
//   FileName: "",
//   ContentSHA1: "",
//   BucketId: "",
//   ContentLength: "",
//   ContentType: "",
//   FileInfo: Dictionary<string,string> }
```

## Release Notes

0.1.5 File info attributes support.
0.1.0 Initial Alpha release.

