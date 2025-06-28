## Missing Features in B2.NET vs B2 Native API

### Summary

The following features from the Backblaze B2 Native API are **not implemented** in this project:

1. **Application Key Management**
   - Endpoints: `b2_list_keys`, `b2_create_key`, `b2_delete_key`, `b2_update_key`
   - No support for listing, creating, deleting, or updating application keys.
2. **Bucket Notification Rules**
   - Endpoints: `b2_get_bucket_notification_rules`, `b2_set_bucket_notification_rules`
   - No support for retrieving or setting bucket event notification rules.
3. **Standalone Lifecycle Rule APIs**
   - If the API provides endpoints for lifecycle rules outside of bucket create/update, these are not implemented.
4. **Other Advanced/Administrative Endpoints**
   - No support for billing, user management, or advanced account settings if present in the API.

### Feature Coverage Table

| API Feature                | Status         | Notes                                                      |
|----------------------------|---------------|------------------------------------------------------------|
| Authentication             | Implemented   | Full support                                               |
| Bucket Management          | Implemented   | Full support                                               |
| File Management            | Implemented   | Full support                                               |
| Large File Management      | Implemented   | Full support                                               |
| Lifecycle Rules            | Implemented   | Only via bucket create/update                              |
| CORS Rules                 | Implemented   | Only via bucket create/update                              |
| Application Key Management | **Missing**   | No support for list/create/delete/update keys              |
| Bucket Notification Rules  | **Missing**   | No support for get/set bucket notification rules           |
| Standalone Lifecycle APIs  | **Missing**   | No support for lifecycle rules outside bucket operations   |
| Admin/Advanced APIs        | **Missing**   | No support for billing, user management, or advanced account settings |

---

For details, see the [Backblaze B2 Native API Documentation](https://www.backblaze.com/apidocs/introduction-to-the-b2-native-api).

For information about the architecture and design patterns used in this project, see the [B2.NET Architecture and Style Guide](ARCHITECTURE.md).
