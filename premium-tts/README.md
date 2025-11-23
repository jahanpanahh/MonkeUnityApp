# Premium TTS (Google Cloud)

Small Express server that proxies Google Cloud Text-to-Speech using the same setup as the AI Quiz Builder project. It returns base64-encoded MP3 audio with a kid-like Neural2 voice so the Unity client can stream/play it.

## Setup

1) Copy the provided `google-cloud-key.json` from AI Quiz Builder into this folder (already done).
2) Install dependencies:

```bash
npm install
```

3) Run the server:

```bash
npm run dev
```

The default port is `3100`. Configure a different port via `PORT` env var if needed.

### Env options

- `PORT`: Port to listen on (default `3100`).
- `GOOGLE_CLOUD_KEY_BASE64`: Base64 string of the service account key (overrides local file).
- `GOOGLE_CLOUD_KEY_JSON`: Raw JSON string of the service account key (fallback if base64 not provided).

### Endpoint

`POST /api/text-to-speech`

Body:

```json
{
  "text": "Hello Monke!",
  "voiceName": "en-US-Neural2-F", // optional
  "speakingRate": 1.0,            // optional
  "pitch": 4.0                    // optional
}
```

Response:

```json
{
  "audio": "<base64 mp3>",
  "contentType": "audio/mpeg"
}
```
