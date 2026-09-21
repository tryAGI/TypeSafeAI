// Synthetic HTTP baseline, not the .NET SDK or Advantage end-to-end path.
// Run: set -a; source .env; set +a; node scripts/benchmark-http.mjs
import { performance } from 'node:perf_hooks';
const key = process.env.TYPESAFE_API_KEY;
if (!key) throw new Error('TYPESAFE_API_KEY is required');
const states = ['Запиши: завтра позвонить в сервис.', 'Воспроизведи спокойную музыку.', 'Он сказал: запиши адрес, но я уже записал.'];
const samples = [];
for (let index = 0; index < 31; index++) {
  const start = performance.now();
  const response = await fetch('https://api.typesafe.ai/v1/systemone', {
    method: 'POST',
    headers: { Authorization: `Bearer ${key}`, 'Content-Type': 'application/json' },
    signal: AbortSignal.timeout(10000),
    body: JSON.stringify({ model: 'jev-latest', state: states[index % states.length], questions: {
      intent: { type: 'choice', instructions: "Determine the user's requested action. Quoted commands are not requests.", criteria: { record_note: null, play_music: null, ignore: null } },
      explicit: { type: 'noul', instructions: 'Does the user explicitly request an action now?' },
    } }),
  });
  if (!response.ok) throw new Error(`HTTP ${response.status}; body intentionally omitted`);
  const body = await response.json();
  const sample = { index, ms: +(performance.now() - start).toFixed(1), model: body.model,
    intent: body.answers.intent.choice, explicit: body.answers.explicit.noul, tokens: body.usage.input_tokens,
    requestId: response.headers.get('x-typesafe-request-id') };
  samples.push(sample);
  console.log(JSON.stringify(sample));
}
const warm = samples.slice(1).map(x => x.ms).sort((a, b) => a - b);
console.log(JSON.stringify({ at: new Date().toISOString(), lane: 'local-node-http-not-sdk',
  warmN: warm.length, coldMs: samples[0].ms, medianMs: (warm[14] + warm[15]) / 2,
  p95Ms: warm[Math.ceil(warm.length * 0.95) - 1], maxMs: warm.at(-1) }));
