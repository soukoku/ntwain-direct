# TWAIN Direct 1.1 Bridge/Sidecar Implementor's Guide

## Overview

TWAIN Direct is a RESTful API specification for scanner communication that modernizes the classic TWAIN protocol. This guide covers essential implementation details for building a **Bridge** (converts TWAIN drivers to TWAIN Direct) and **Sidecar** (HTTP service exposing TWAIN Direct API).

---

## Architecture Components

### Bridge
- Translates between classic TWAIN and TWAIN Direct protocols
- Communicates with TWAIN DSM (Data Source Manager) and DS (Data Source/Driver)
- Handles 32-bit/64-bit process separation (TWAIN drivers are often 32-bit only)
- Manages IPC between main process and TWAIN process

### Sidecar
- HTTP/HTTPS service exposing TWAIN Direct RESTful API
- Manages scanner sessions and state machines
- Handles image data streaming and multipart responses
- Implements Privet protocol for local device discovery

---

## Core API Endpoints

### Discovery & Information

#### `/privet/info` (GET)
**Purpose**: Device discovery and capabilities  
**Response**: `PrivetInfo`
```json
{
  "version": "3.0",
  "name": "Scanner Name",
  "id": "unique-device-id",
  "type": ["scanner"],
  "api": ["/privet/twaindirect/session"],
  "x-privet-token": "auth-token",
  "manufacturer": "Vendor Name",
  "model": "Model XYZ"
}
```

#### `/privet/infoex` (POST)
**Purpose**: Extended scanner information  
**Method**: `infoex`
```json
{
  "kind": "twainlocalscanner",
  "commandId": "uuid",
  "method": "infoex"
}
```

---

## Session Management

### Session States
TWAIN Direct defines a strict state machine:

| State | Description | Valid Transitions |
|-------|-------------|-------------------|
| `noSession` | No active session | ? `ready` (via createSession) |
| `ready` | Session created, waiting for commands | ? `capturing`, `closed` |
| `capturing` | Actively scanning images | ? `draining`, `ready` |
| `draining` | Finishing capture, delivering remaining images | ? `ready` |
| `closed` | Session ended | ? `noSession` |

### Session Workflow

```
1. createSession   ? Returns sessionId, state="ready"
2. sendTask        ? Configure scan parameters
3. startCapturing  ? Begin scanning, state="capturing"
4. waitForEvents   ? Long poll for imageBlocks events
5. readImageBlock  ? Retrieve image data + metadata
6. releaseImageBlocks ? Free memory after processing
7. stopCapturing   ? End scan, state="draining" or "ready"
8. closeSession    ? Clean up, state="closed"
```

### Key Methods

#### `createSession`
**Creates a new scanning session**
```json
{
  "kind": "twainlocalscanner",
  "commandId": "cmd-001",
  "method": "createSession"
}
```
**Response includes**: `sessionId`, `revision`, `state`

#### `sendTask`
**Configures scanner behavior with hierarchical task structure**
```json
{
  "kind": "twainlocalscanner",
  "method": "sendTask",
  "params": {
    "sessionId": "session-123",
    "task": {
      "actions": [{
        "action": "configure",
        "streams": [{
          "sources": [{
            "source": "any",  // or "adf", "flatbed", "feederFront"
            "pixelFormats": [{
              "pixelFormat": "bw1",  // or "gray8", "rgb24"
              "attributes": [{
                "attribute": "compression",
                "values": [{ "value": "group4" }]
              }, {
                "attribute": "resolution",
                "values": [{ "value": 300 }]
              }]
            }]
          }]
        }]
      }]
    }
  }
}
```

**Task Hierarchy**:
```
Task
 ?? Actions[]
     ?? Streams[]
         ?? Sources[]
             ?? PixelFormats[]
                 ?? Attributes[]
                     ?? Values[]
```

#### `startCapturing` / `stopCapturing`
**Control image acquisition**
```json
{
  "kind": "twainlocalscanner",
  "method": "startCapturing",
  "params": { "sessionId": "session-123" }
}
```

---

## Event-Driven Architecture

### `waitForEvents` (Long Polling)
**Critical for asynchronous operation**

```json
{
  "kind": "twainlocalscanner",
  "method": "waitForEvents",
  "params": {
    "sessionId": "session-123",
    "sessionRevision": 5
  }
}
```

**Implementation Requirements**:
- Hold connection open until events occur or timeout (typically 30 seconds)
- Return immediately if events exist for revisions > `sessionRevision`
- Increment `revision` on every state change
- Support concurrent waitForEvents from multiple clients

**Event Types**:
- `imageBlocks` - New images available
- `statusChanged` - Scanner status changed
- `sessionClosed` - Session terminated
- `scanDone` - Capture complete

---

## Image Block Management

### `readImageBlock`
**Retrieves scanned image data**

```json
{
  "kind": "twainlocalscanner",
  "method": "readImageBlock",
  "params": {
    "sessionId": "session-123",
    "imageBlockNum": 0,
    "withMetadata": true
  }
}
```

**Response**: Multipart MIME with:
1. **Part 1**: JSON metadata
```json
{
  "metadata": {
    "address": {
      "imageNumber": 1,
      "sheetNumber": 1,
      "source": "feederFront",
      "pixelFormatName": "gray8"
    },
    "image": {
      "compression": "group4",
      "pixelFormat": "bw1",
      "pixelWidth": 2550,
      "pixelHeight": 3300,
      "resolution": 300,
      "size": 125432
    },
    "status": {
      "success": true,
      "detected": "nominal"
    }
  }
}
```

2. **Part 2**: Raw image data (PDF/raster, TIFF, JPEG, PNG)

### `readImageBlockMetadata`
**Retrieves only metadata** (no image data) - faster for workflow decisions

### `releaseImageBlocks`
**CRITICAL**: Must be called to free memory
```json
{
  "params": {
    "sessionId": "session-123",
    "imageBlockNum": 0,
    "lastImageBlockNum": 5
  }
}
```

**Implementation**:
- Track which blocks are "in use"
- Only release when client confirms receipt
- Implement memory limits and backpressure

---

## Task Configuration Deep Dive

### Common Attributes

| Attribute | Values | Description |
|-----------|--------|-------------|
| `compression` | `none`, `group4`, `jpeg` | Image compression |
| `resolution` | Integer (DPI) | Scan resolution |
| `cropping` | Object with coordinates | Physical cropping |
| `automaticDeskew` | `on`, `off` | Auto-straighten |
| `automaticColorDetect` | `on`, `off` | Auto B&W/Gray/Color |
| `brightness` | -1000 to 1000 | Brightness adjustment |
| `contrast` | -1000 to 1000 | Contrast adjustment |
| `threshold` | 0-255 | B&W threshold |
| `doubleFeedDetection` | `on`, `off` | Detect multi-feeds |
| `numberOfSheets` | Integer | Max sheets to scan |

### Source Types
- `any` - Bridge selects best available
- `adf` - Automatic Document Feeder (both sides)
- `adfFront` - ADF front side only
- `adfRear` - ADF rear side only
- `flatbed` - Flatbed scanner
- `feederFront` / `feederRear` - Generic feeder

### Pixel Formats
- `bw1` - 1-bit black & white
- `gray8` - 8-bit grayscale
- `gray16` - 16-bit grayscale
- `rgb24` - 24-bit color
- `rgb48` - 48-bit color

---

## Error Handling

### Standard Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `invalidSessionId` | 404 | Session doesn't exist |
| `invalidState` | 409 | Operation not valid in current state |
| `noSession` | 404 | No session created |
| `busy` | 503 | Scanner busy with another operation |
| `paperJam` | 503 | Paper jam detected |
| `paperEmpty` | 503 | Feeder empty |
| `doubleFeed` | 503 | Double feed detected |
| `coverOpen` | 503 | Scanner cover open |
| `timeout` | 408 | Operation timed out |
| `communicationError` | 503 | Device communication failed |
| `invalidXPrivetToken` | 401 | Authentication failed |

### Error Response Format
```json
{
  "kind": "twainlocalscanner",
  "commandId": "cmd-001",
  "method": "startCapturing",
  "results": {
    "success": false,
    "session": {
      "sessionId": "session-123",
      "state": "ready",
      "status": {
        "success": false,
        "detected": "paperJam"
      }
    }
  }
}
```

---

## Status Detection

### `detected` Values
Must be reported accurately for proper application behavior:

| Status | Meaning | When to Use |
|--------|---------|-------------|
| `nominal` | Normal operation | Default success state |
| `success` | Operation succeeded | Generic success |
| `adfEmpty` / `feederEmpty` | No paper | After last sheet |
| `paperJam` | Jam detected | Hardware reported jam |
| `paperDoubleFeed` | Multi-feed | Double feed sensor triggered |
| `coverOpen` | Cover open | Cover sensor active |
| `endOfJob` | Scan complete | All requested images delivered |
| `nextAction` | Waiting for user | Requires user intervention |

---

## Image Format Support

### PDF/raster (Recommended)
- Multi-page support in single file
- Efficient for B&W with Group4 compression
- Specification: PDF/raster 1.0
- MIME: `application/pdf`

### TIFF
- Single or multi-page
- Supports all pixel formats
- Various compression schemes
- MIME: `image/tiff`

### JPEG
- Color and grayscale only
- Lossy compression
- MIME: `image/jpeg`

### PNG
- All formats supported
- Lossless compression
- MIME: `image/png`

---

## Security & Authentication

### X-Privet-Token
- Returned in `/privet/info`
- Must be included in all command headers
- Prevents unauthorized access on local network
- Implement token rotation for security

```http
POST /privet/twaindirect/session
X-Privet-Token: abc123def456
Content-Type: application/json
```

---

## Implementation Checklist

### Bridge Implementation
- [ ] TWAIN DSM/DS initialization and cleanup
- [ ] 32-bit/64-bit process bridging (if needed)
- [ ] Capability negotiation with TWAIN driver
- [ ] Image transfer (native/buffered/file modes)
- [ ] Status code translation (TWAIN ? TWAIN Direct)
- [ ] Handle driver quirks and vendor-specific behavior
- [ ] Memory management for image data

### Sidecar Implementation
- [ ] HTTP/HTTPS server with CORS support
- [ ] Session state machine enforcement
- [ ] Long polling for waitForEvents
- [ ] Multipart MIME responses for readImageBlock
- [ ] Image block memory management and release
- [ ] Concurrent session handling (if supported)
- [ ] Proper error code mapping
- [ ] Privet discovery support
- [ ] X-Privet-Token generation and validation

### Task Translation
- [ ] Parse hierarchical task JSON
- [ ] Map attributes to TWAIN capabilities
- [ ] Handle unsupported attributes gracefully
- [ ] Return task reply with actual applied values
- [ ] Support vendor-specific extensions

---

## Best Practices

### Performance
1. **Use image streaming**: Don't buffer entire documents
2. **Implement backpressure**: Limit in-flight imageBlocks
3. **Release blocks promptly**: Avoid memory exhaustion
4. **Compress appropriately**: Group4 for B&W, JPEG for color

### Reliability
1. **Validate state transitions**: Reject invalid operations
2. **Handle driver failures**: Map to appropriate error codes
3. **Timeout long operations**: Don't hang indefinitely
4. **Clean up resources**: Close sessions on client disconnect

### Client Experience
1. **Fast waitForEvents**: Return quickly when events exist
2. **Accurate status detection**: Report paper out, jams correctly
3. **Meaningful error messages**: Help diagnose issues
4. **Consistent behavior**: Match TWAIN Direct specification

---

## Common Pitfalls

1. **Forgetting to increment revision**: Breaks waitForEvents polling
2. **Not handling releaseImageBlocks**: Memory leaks
3. **Blocking waitForEvents**: Use async/threading properly
4. **Wrong MIME boundaries**: Breaks multipart parsing
5. **State machine violations**: Client confusion and errors
6. **Missing X-Privet-Token validation**: Security issue
7. **Not draining after stopCapturing**: Losing images in flight
8. **Incorrect detected values**: Application workflow breaks

---

## Testing Strategy

### Unit Tests
- State machine transitions
- Task parsing and validation
- Error code mapping
- Image metadata generation

### Integration Tests
- Full scan workflow (create ? task ? capture ? read ? release ? close)
- Multiple concurrent sessions
- Error recovery (jam, timeout, etc.)
- Long polling behavior
- Multipart response parsing

### Hardware Tests
- Various TWAIN drivers
- Different scanner models
- Duplex scanning
- Large documents (>100 pages)
- All pixel formats and compressions
- Error conditions (no paper, cover open, etc.)

---

## Reference Implementation Notes

Based on the provided codebase:

**DTO Classes** (`NTwain.Sidecar.Dtos`):
- Well-structured request/response models
- Use C# records for immutability
- JSON property name mapping with `[JsonPropertyName]`
- Strong typing with required fields

**Constants** (`Constants.cs`):
- Centralized error codes, states, methods
- Makes validation and comparison consistent
- Self-documenting API surface

**Session Management**:
- Track `sessionId`, `revision`, `state`
- Maintain imageBlocks list
- Support `doneCapturing` and `endOfJob` flags

**Bridge Architecture** (`DSBridge32/64`):
- Separate processes for 32-bit TWAIN drivers
- IPC using SimpleIpc library
- Request/response pattern with `DSRequest`/`DSResponse`

---

## Specification Compliance

Ensure adherence to:
- **TWAIN Direct 1.1 Specification** (official document)
- **Privet Protocol** (for discovery)
- **HTTP/1.1** (for RESTful API)
- **Multipart MIME** (RFC 2046)
- **PDF/raster 1.0** (for image format)

---

## Resources

- **TWAIN Direct Specification**: https://www.twaindirect.org/
- **TWAIN Working Group**: https://www.twain.org/
- **PDF/raster Specification**: Included with TWAIN Direct docs
- **This Codebase**: Reference implementation for .NET

---

## Quick Start Template

```csharp
// 1. Implement session manager
public class SessionManager
{
    private Dictionary<string, ScanSession> _sessions = new();
    
    public SessionInfo CreateSession()
    {
        var sessionId = Guid.NewGuid().ToString();
        var session = new ScanSession(sessionId);
        _sessions[sessionId] = session;
        
        return new SessionInfo 
        {
            SessionId = sessionId,
            State = SessionStates.Ready,
            Revision = 0
        };
    }
}

// 2. Implement state machine
public class ScanSession
{
    private string _state = SessionStates.Ready;
    private int _revision = 0;
    
    public void StartCapturing()
    {
        if (_state != SessionStates.Ready)
            throw new InvalidStateException();
            
        _state = SessionStates.Capturing;
        _revision++;
    }
}

// 3. Implement image block streaming
public class ImageBlockManager
{
    private Queue<ImageBlock> _blocks = new();
    private HashSet<int> _releasedBlocks = new();
    
    public void AddBlock(ImageBlock block) 
    {
        _blocks.Enqueue(block);
        RaiseImageBlocksEvent();
    }
    
    public void ReleaseBlocks(int first, int last)
    {
        for (int i = first; i <= last; i++)
            _releasedBlocks.Add(i);
    }
}

// 4. Implement long polling
public async Task<WaitForEventsResponse> WaitForEvents(
    string sessionId, int sessionRevision, CancellationToken ct)
{
    var session = GetSession(sessionId);
    
    // Wait for revision change or timeout
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    cts.CancelAfter(TimeSpan.FromSeconds(30));
    
    try 
    {
        await session.WaitForRevisionChange(sessionRevision, cts.Token);
        return new WaitForEventsResponse 
        {
            Results = new WaitForEventsResults
            {
                Success = true,
                Events = session.GetEventsSince(sessionRevision)
            }
        };
    }
    catch (OperationCanceledException)
    {
        // Timeout - return empty events
        return new WaitForEventsResponse 
        {
            Results = new WaitForEventsResults
            {
                Success = true,
                Events = Array.Empty<SessionEvent>()
            }
        };
    }
}
```

---

## Conclusion

Implementing a TWAIN Direct Bridge/Sidecar requires:
1. **Deep understanding** of state machine and event model
2. **Careful memory management** for image blocks
3. **Proper async handling** for long polling
4. **Accurate error mapping** from TWAIN to TWAIN Direct
5. **Robust testing** across different hardware

This guide provides the essential patterns and pitfalls. Always refer to the official specification for authoritative details.
