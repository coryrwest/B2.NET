# B2.NET

B2.NET is a C# client for the Backblaze B2 Cloud Storage service through its REST API.

B2.NET is still in early Alpha, so use it in production at your own risk.

## Features

*  Full implmentation of the B2 REST API.
*  Full Async support (Planned)

## NuGet

B2.NET will eventually be available as a NuGet package.

## Install

Use the NuGet package, or just stick the B2.NET.ddl in your project.

## Bucket Types
```
allPrivate
allPublic
```

## Examples
```csharp
var options = new B2Options() {
	AccountId = "YOUR ACCOUNT ID",
	ApplicationKey = "YOUR APPLICATION KEY",
	BucketId = "OPTIONAL BUCKET ID"
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
var bucketList = client.GetBucketList().Result;
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
var bucketList = client.CreateBucket("BUCKETNAME", "OPTIONAL_BUCKET_TYPE").Result;
// [
//   { BucketId: "",
//     BucketName: "",
//     BucketType: "" }
// ]
```

### Update a Bucket
```csharp
var client = new B2Client(options);
options = client.Authorize().Result;
var bucketList = client.UpdateBucket("BUCKETID", "BUCKETYPE").Result;
// [
//   { BucketId: "",
//     BucketName: "",
//     BucketType: "" }
// ]
```

### Delete a Bucket
```csharp
var client = new B2Client(options);
options = client.Authorize().Result;
var bucketList = client.DeleteBucket("BUCKETID").Result;
// [
//   { BucketId: "",
//     BucketName: "",
//     BucketType: "" }
// ]
```

## Release Notes

0.1.0 Initial Alpha release.

