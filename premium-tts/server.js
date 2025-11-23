import express from 'express';
import cors from 'cors';
import dotenv from 'dotenv';
import textToSpeech from '@google-cloud/text-to-speech';
import rateLimit from 'express-rate-limit';

dotenv.config();

const app = express();
const PORT = process.env.PORT || 3100;

app.use(cors());
app.use(express.json({ limit: '2mb' }));

// Rate limit to avoid abuse during development/testing
const ttsLimiter = rateLimit({
  windowMs: 15 * 60 * 1000,
  max: 60,
  message: 'Too many text-to-speech requests, please try again later.',
  standardHeaders: true,
  legacyHeaders: false
});

// Initialize Google Cloud Text-to-Speech client (mirrors AI Quiz Builder logic)
let ttsClient = null;
try {
  if (process.env.GOOGLE_CLOUD_KEY_BASE64) {
    const credentials = JSON.parse(
      Buffer.from(process.env.GOOGLE_CLOUD_KEY_BASE64, 'base64').toString('utf-8')
    );
    ttsClient = new textToSpeech.TextToSpeechClient({ credentials });
    console.log('✅ Google Cloud TTS initialized (base64 env)');
  } else if (process.env.GOOGLE_CLOUD_KEY_JSON) {
    const credentials = JSON.parse(process.env.GOOGLE_CLOUD_KEY_JSON);
    ttsClient = new textToSpeech.TextToSpeechClient({ credentials });
    console.log('✅ Google Cloud TTS initialized (JSON env)');
  } else {
    ttsClient = new textToSpeech.TextToSpeechClient({ keyFilename: './google-cloud-key.json' });
    console.log('✅ Google Cloud TTS initialized (local file)');
  }
} catch (error) {
  console.error('❌ Failed to initialize Google Cloud TTS:', error.message);
}

app.get('/health', (_req, res) => {
  res.json({
    status: 'ok',
    hasTtsClient: !!ttsClient
  });
});

// Premium voice endpoint
app.post('/api/text-to-speech', ttsLimiter, async (req, res) => {
  try {
    const { text, voiceName, speakingRate, pitch } = req.body || {};

    if (!text) {
      return res.status(400).json({ error: 'Text is required' });
    }

    if (text.length > 1000) {
      return res.status(400).json({ error: 'Text too long (max 1000 characters)' });
    }

    if (!ttsClient) {
      return res.status(503).json({ error: 'Google Cloud TTS not configured' });
    }

    const request = {
      input: { text },
      voice: {
        languageCode: 'en-US',
        name: voiceName || 'en-US-Neural2-F', // warm/friendly female voice
        ssmlGender: 'FEMALE'
      },
      audioConfig: {
        audioEncoding: 'MP3',
        speakingRate: typeof speakingRate === 'number' ? speakingRate : 1.0,
        pitch: typeof pitch === 'number' ? pitch : 4.0, // higher pitch for kid-like tone
        volumeGainDb: 0.0
      }
    };

    const [response] = await ttsClient.synthesizeSpeech(request);
    const audioBase64 = response.audioContent.toString('base64');

    console.log(`✅ TTS request: ${text.length} chars`);

    res.json({
      audio: audioBase64,
      contentType: 'audio/mpeg'
    });
  } catch (error) {
    console.error('❌ Text-to-Speech error:', error);
    res.status(500).json({ error: 'Failed to synthesize speech' });
  }
});

app.listen(PORT, () => {
  console.log(`🚀 Premium TTS server running on port ${PORT}`);
});
