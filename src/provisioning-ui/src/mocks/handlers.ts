import { delay, http, HttpResponse } from 'msw';

const ENDPOINT_LATENCY_MS = {
    health: { base: 120, jitter: 80 },
    homepage: { base: 450, jitter: 300 },
    tenants: { base: 800, jitter: 500 },
} as const;

async function simulateLatency(
    endpoint: keyof typeof ENDPOINT_LATENCY_MS
): Promise<void> {
    const { base, jitter } = ENDPOINT_LATENCY_MS[endpoint];
    const randomized = base + Math.floor(Math.random() * (jitter + 1));
    await delay(randomized);
}

export const handlers = [
    http.get('http://localhost:5174/api/v1/tenantrequests', async () => {
        await simulateLatency('tenants');
        return HttpResponse.json({}, { status: 200 });
    })
]