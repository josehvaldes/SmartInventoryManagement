import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { BrowserRouter } from 'react-router'
import { msalInstance } from './utils/msalInstance.ts';
import { MsalProvider } from '@azure/msal-react'
const isMockingEnabled = import.meta.env.DEV && import.meta.env.VITE_ENABLE_MOCKS === 'true';

async function enableMocking() {
  if (isMockingEnabled) {
    const { worker } = await import('./mocks/browser');

    await worker.start({ onUnhandledFrame: 'warn' });
  }
}

async function createAppToRender() 
{
  await msalInstance.initialize();
  createRoot(document.getElementById('root')!).render(
    <StrictMode>
      <MsalProvider instance={msalInstance}>
        <BrowserRouter >
          <App />
        </BrowserRouter>
      </MsalProvider>
    </StrictMode>,
  )
}

enableMocking().then(() => {
  createAppToRender();
});
